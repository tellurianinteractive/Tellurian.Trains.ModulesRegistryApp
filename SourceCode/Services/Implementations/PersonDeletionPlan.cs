using ModulesRegistry.Resources;

namespace ModulesRegistry.Services.Implementations;

/// <summary>
/// Everything that goes with a person when the person is deleted (decided 2026-11-21), loaded tracked so that the plan shown on
/// the delete page is the one applied:
/// <list type="bullet">
/// <item>Deleted: the user account, group memberships, meeting and layout participations (the triggers delete their registered
/// modules and stations), modules the person owns alone with their exits, documents and stations (the triggers delete the
/// stations' customers and layout registrations), vehicles and wiFRED throttles.</item>
/// <item>Changed: a shared module loses the person's share and the other owners' shares grow in proportion, an assistant role is
/// removed, a station that other modules still refer to stays with them, and a layout with the person as contact gets none.</item>
/// </list>
/// Everything is written through the change tracker, so the change log records it, except what the database triggers and
/// cascades delete below the deleted rows.
/// </summary>
internal sealed class PersonDeletionPlan
{
    private PersonDeletionPlan(Person person, bool deletesPerson)
    {
        Person = person;
        DeletesPerson = deletesPerson;
    }

    public Person Person { get; }
    /// <summary>False for a plan that deletes only one of the person's modules (<see cref="LoadForModuleAsync"/>).</summary>
    public bool DeletesPerson { get; }
    public List<string> Obstacles { get; } = [];
    public List<GroupMember> GroupMembers { get; private set; } = [];
    public List<MeetingParticipant> MeetingParticipants { get; private set; } = [];
    public List<LayoutParticipant> LayoutParticipants { get; private set; } = [];
    public List<Layout> LayoutsAsContact { get; private set; } = [];
    public List<Vehicle> Vehicles { get; private set; } = [];
    public List<WiFredThrottle> WiFredThrottles { get; private set; } = [];
    public List<Module> ModulesDeleted { get; } = [];
    public List<ModuleOwnership> SharesRemoved { get; } = [];
    public List<ModuleOwnership> AssistantsRemoved { get; } = [];
    public List<Station> StationsDeleted { get; } = [];
    public List<(Station Station, int? PrimaryModuleId)> StationsKept { get; } = [];
    /// <summary>Deleted modules that are, or whose deleted station is, registered for a meeting that has not ended.</summary>
    public HashSet<int> RegisteredModuleIds { get; } = [];

    /// <param name="currentPersonId">The user's own person, who may not delete themselves.</param>
    /// <param name="today">Meetings ending today or later are not over.</param>
    public static async Task<PersonDeletionPlan?> LoadAsync(ModulesDbContext dbContext, int personId, int currentPersonId, DateTime today, CancellationToken cancellationToken = default)
    {
        var person = await FindPersonAsync(dbContext, personId, cancellationToken);
        if (person is null) return null;

        var plan = new PersonDeletionPlan(person, deletesPerson: true)
        {
            GroupMembers = await dbContext.GroupMembers.Include(gm => gm.Group).Where(gm => gm.PersonId == personId).ToListAsync(cancellationToken),
            MeetingParticipants = await dbContext.MeetingParticipants.Include(mp => mp.Meeting).Where(mp => mp.PersonId == personId).ToListAsync(cancellationToken),
            LayoutParticipants = await dbContext.LayoutParticipants.Where(lp => lp.PersonId == personId || lp.MeetingParticipant.PersonId == personId).ToListAsync(cancellationToken),
            LayoutsAsContact = await dbContext.Layouts.Include(l => l.Meeting).Where(l => l.ContactPersonId == personId).ToListAsync(cancellationToken),
            Vehicles = await dbContext.Vehicles.Where(v => v.OwningPersonId == personId).ToListAsync(cancellationToken),
            WiFredThrottles = await dbContext.WiFredThrottles.Where(w => w.OwningPersonId == personId).ToListAsync(cancellationToken),
        };
        await plan.AddOwnershipsAsync(dbContext, cancellationToken);
        await plan.AddStationsAsync(dbContext, cancellationToken);
        plan.AddPersonObstacles(currentPersonId, today.Date);
        await plan.AddModuleObstaclesAsync(dbContext, today.Date, cancellationToken);
        return plan;
    }

    /// <summary>
    /// Deletes one module that the person owns alone, before the person is deleted, with its station unless another module refers
    /// to it. Null when the person doesn't own the module alone.
    /// </summary>
    public static async Task<PersonDeletionPlan?> LoadForModuleAsync(ModulesDbContext dbContext, int personId, int moduleId, DateTime today, CancellationToken cancellationToken = default)
    {
        var person = await FindPersonAsync(dbContext, personId, cancellationToken);
        if (person is null) return null;
        var plan = new PersonDeletionPlan(person, deletesPerson: false);
        await plan.AddOwnershipsAsync(dbContext, cancellationToken, moduleId);
        if (plan.ModulesDeleted.Count == 0) return null;
        await plan.AddStationsAsync(dbContext, cancellationToken);
        await plan.AddModuleObstaclesAsync(dbContext, today.Date, cancellationToken);
        return plan;
    }

    public PersonDeletion AsPersonDeletion() => new()
    {
        Person = Person,
        Obstacles = Obstacles,
        UserSignInAddress = Person.User?.EmailAddress,
        Groups = [.. GroupMembers.Select(gm => gm.Group.FullName).Order()],
        Meetings = [.. MeetingParticipants.OrderBy(mp => mp.Meeting.StartDate).Select(mp => MeetingName(mp.Meeting))],
        ModulesDeleted = [.. ModulesDeleted
            .Select(m => new PersonDeletion.OwnedModule(m.Id, Name(m), m.Station?.FullName, RegisteredModuleIds.Contains(m.Id)))
            .OrderBy(m => m.Name)],
        StationsDeleted = [.. StationsDeleted.Select(s => s.FullName).Order()],
        Vehicles = Vehicles.Count,
        WiFredThrottles = WiFredThrottles.Count,
        ModulesShared = Names(SharesRemoved.Select(mo => mo.Module)),
        ModulesAssisted = Names(AssistantsRemoved.Select(mo => mo.Module)),
        StationsKept = [.. StationsKept.Select(s => s.Station.FullName).Order()],
        LayoutsAsContact = [.. LayoutsAsContact.OrderBy(l => l.Meeting.StartDate).Select(l => l.ShortName.HasValue ? $"{MeetingName(l.Meeting)}, {l.ShortName}" : MeetingName(l.Meeting))],
    };

    /// <summary>
    /// Changes first, in their own save, so that no deleted module or station is referred to when the deletes run; then the
    /// deletes. One transaction, so nothing is left half deleted.
    /// </summary>
    public async Task<int> ApplyAsync(ModulesDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var module in ModulesDeleted) module.StationId = null;
        foreach (var (station, primaryModuleId) in StationsKept) station.PrimaryModuleId = primaryModuleId;
        foreach (var station in StationsDeleted) station.PrimaryModuleId = null;
        foreach (var layout in LayoutsAsContact) layout.ContactPersonId = null;
        foreach (var share in SharesRemoved) GiveShareToOtherOwners(share);
        var count = await dbContext.SaveChangesAsync(cancellationToken);

        var documentIds = ModulesDeleted.SelectMany(m => m.DocumentIds())
            .Concat(StationsDeleted.Where(s => s.PdfInstructionId.HasValue).Select(s => s.PdfInstructionId!.Value))
            .ToList();
        dbContext.ModuleOwnerships.RemoveRange(SharesRemoved.Concat(AssistantsRemoved));
        dbContext.LayoutParticipants.RemoveRange(LayoutParticipants);
        dbContext.MeetingParticipants.RemoveRange(MeetingParticipants);
        dbContext.GroupMembers.RemoveRange(GroupMembers);
        dbContext.Vehicles.RemoveRange(Vehicles);
        dbContext.WiFredThrottles.RemoveRange(WiFredThrottles);
        dbContext.Stations.RemoveRange(StationsDeleted);
        foreach (var module in ModulesDeleted)
        {
            dbContext.ModuleOwnerships.RemoveRange(module.ModuleOwnerships);
            dbContext.ModuleExits.RemoveRange(module.ModuleExits);
        }
        dbContext.Modules.RemoveRange(ModulesDeleted);
        await DocumentService.RemoveDocumentsAsync(dbContext, documentIds);
        if (DeletesPerson)
        {
            dbContext.People.Remove(Person);
            if (Person.User is not null) dbContext.Users.Remove(Person.User);
        }
        count += await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return count;
    }

    /// <param name="moduleId">Only this module, and only when the person owns it alone.</param>
    private async Task AddOwnershipsAsync(ModulesDbContext dbContext, CancellationToken cancellationToken, int? moduleId = null)
    {
        var ownerships = await dbContext.ModuleOwnerships
            .Include(mo => mo.Module).ThenInclude(m => m.ModuleOwnerships)
            .Include(mo => mo.Module).ThenInclude(m => m.ModuleExits)
            .Include(mo => mo.Module).ThenInclude(m => m.Station)
            .Where(mo => mo.PersonId == Person.Id && (moduleId == null || mo.ModuleId == moduleId))
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        foreach (var ownership in ownerships)
        {
            var isOwnedAlone = ownership.OwnedShare > 0 && !OtherOwners(ownership).Any();
            if (isOwnedAlone)
            {
                if (!ModulesDeleted.Contains(ownership.Module)) ModulesDeleted.Add(ownership.Module);
            }
            else if (!moduleId.HasValue)
            {
                if (ownership.OwnedShare <= 0) AssistantsRemoved.Add(ownership);
                else SharesRemoved.Add(ownership);
            }
        }
    }

    /// <summary>
    /// A deleted module's station is deleted too, unless a module that isn't deleted refers to it; then it stays, with such a
    /// module as its primary module if the primary module is deleted.
    /// </summary>
    private async Task AddStationsAsync(ModulesDbContext dbContext, CancellationToken cancellationToken)
    {
        if (ModulesDeleted.Count == 0) return;
        var deletedModuleIds = ModulesDeleted.Select(m => m.Id).ToList();
        var stationIds = ModulesDeleted.Where(m => m.StationId.HasValue).Select(m => m.StationId!.Value).ToList();
        var stations = await dbContext.Stations
            .Where(s => stationIds.Contains(s.Id) || (s.PrimaryModuleId.HasValue && deletedModuleIds.Contains(s.PrimaryModuleId.Value)))
            .ToListAsync(cancellationToken);
        foreach (var station in stations)
        {
            var remainingModuleId = await dbContext.Modules
                .Where(m => m.StationId == station.Id && !deletedModuleIds.Contains(m.Id))
                .OrderBy(m => m.Id)
                .Select(m => (int?)m.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (remainingModuleId.HasValue)
            {
                var isPrimaryDeleted = station.PrimaryModuleId.HasValue && deletedModuleIds.Contains(station.PrimaryModuleId.Value);
                StationsKept.Add((station, isPrimaryDeleted ? remainingModuleId : station.PrimaryModuleId));
            }
            else
            {
                StationsDeleted.Add(station);
            }
        }
    }

    /// <summary>
    /// The person themselves, or a registration for a meeting that has not ended. Membership in the group organising such a meeting
    /// doesn't prevent the delete (decided 2026-11-21): shared modules go to the other owners first, and modules owned alone are
    /// checked in <see cref="AddModuleObstaclesAsync"/>.
    /// </summary>
    private void AddPersonObstacles(int currentPersonId, DateTime today)
    {
        if (Person.Id == currentPersonId) Obstacles.Add(Strings.YouMayNotDeleteYourself);
        foreach (var participant in MeetingParticipants.Where(mp => mp.CancellationTime is null && mp.Meeting.EndDate >= today))
        {
            Obstacles.Add(string.Format(Strings.PersonIsRegisteredForUpcomingMeeting, MeetingName(participant.Meeting)));
        }
    }

    /// <summary>A module, or its station, registered for a meeting that has not ended may not be deleted; it may be transferred.</summary>
    private async Task AddModuleObstaclesAsync(ModulesDbContext dbContext, DateTime today, CancellationToken cancellationToken)
    {
        var deletedModuleIds = ModulesDeleted.Select(m => m.Id).ToList();
        var registeredModules = await dbContext.LayoutModules.AsNoTracking()
            .Where(lm => deletedModuleIds.Contains(lm.ModuleId) && lm.LayoutParticipant.Layout.Meeting.EndDate >= today)
            .Select(lm => lm.ModuleId)
            .Distinct()
            .ToListAsync(cancellationToken);
        RegisteredModuleIds.UnionWith(registeredModules);
        var deletedStationIds = StationsDeleted.Select(s => s.Id).ToList();
        var registeredStations = await dbContext.LayoutStations.AsNoTracking()
            .Where(ls => deletedStationIds.Contains(ls.StationId) && ls.LayoutParticipant.Layout.Meeting.EndDate >= today)
            .Select(ls => ls.StationId)
            .Distinct()
            .ToListAsync(cancellationToken);
        RegisteredModuleIds.UnionWith(ModulesDeleted.Where(m => m.StationId.HasValue && registeredStations.Contains(m.StationId.Value)).Select(m => m.Id));
        foreach (var module in ModulesDeleted.Where(m => RegisteredModuleIds.Contains(m.Id)).OrderBy(Name))
        {
            var isStationRegistered = module.StationId.HasValue && registeredStations.Contains(module.StationId.Value);
            Obstacles.Add(isStationRegistered ? $"{module.Station.FullName}: {Strings.StationIsRegisteredForUpcomingMeeting}" : $"{Name(module)}: {Strings.ModuleIsRegisteredForUpcomingMeeting}");
        }
    }

    // Also the person's user when it is marked as deleted (the query filter on users).
    private static Task<Person?> FindPersonAsync(ModulesDbContext dbContext, int personId, CancellationToken cancellationToken) =>
        dbContext.People.IgnoreQueryFilters()
            .Include(p => p.Country)
            .Include(p => p.User)
            .SingleOrDefaultAsync(p => p.Id == personId, cancellationToken);

    private static IEnumerable<ModuleOwnership> OtherOwners(ModuleOwnership ownership) =>
        ownership.Module.ModuleOwnerships.Where(mo => mo.Id != ownership.Id && mo.OwnedShare > 0);

    /// <summary>The other owners keep their proportions to each other and together own the whole module.</summary>
    private static void GiveShareToOtherOwners(ModuleOwnership share)
    {
        var others = OtherOwners(share).ToList();
        var total = others.Sum(mo => mo.OwnedShare);
        foreach (var other in others) other.OwnedShare = others.Count == 1 ? 1 : other.OwnedShare / total;
    }

    private static string MeetingName(Meeting meeting) => $"{meeting.Name} {meeting.StartDate.Year}";

    private static string Name(Module module) => module.ConfigurationLabel.HasValue ? $"{module.FullName} {module.ConfigurationLabel}" : module.FullName;

    private static List<string> Names(IEnumerable<Module> modules) => [.. modules.Select(Name).Distinct().Order()];
}