﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Resources;

namespace ModulesRegistry.Services.Implementations;

public sealed class LayoutService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider, ILogger<LayoutService> logger)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly ITimeProvider TimeProvider = timeProvider;
    private readonly ILogger<LayoutService> Logger = logger;

    public async Task<Layout?> GetLayoutAsync(ClaimsPrincipal? principal, int layoutId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Layouts.Where(l => l.Id == layoutId)
                .Include(l => l.Meeting).ThenInclude(m => m.OrganiserGroup).ThenInclude(og => og.GroupMembers.Where(gm => gm.IsGroupAdministrator || gm.IsDataAdministrator))
                .Include(l => l.PrimaryModuleStandard).ThenInclude(pms => pms.Scale)
                .FirstOrDefaultAsync();
        }
        return null;
    }

    public async Task<int> ModulesRegisteredCountAsync(ClaimsPrincipal? principal, int meetingId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.LayoutModules.AsNoTracking()
                .Where(ls => ls.LayoutParticipant.Layout.MeetingId == meetingId)
                .CountAsync();
        }
        return -1;
    }

    public async Task<IEnumerable<LayoutStation>> GetStationsAsync(ClaimsPrincipal? principal, int layoutId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.LayoutStations.AsNoTracking()
                .Include(ls => ls.LayoutParticipant).ThenInclude(lp => lp.MeetingParticipant).ThenInclude(mp => mp.Meeting)
                .Include(ls => ls.Station).ThenInclude(s => s.StationTracks)
                .Include(ls => ls.OtherCountry)
                .Include(ls => ls.Station).ThenInclude(s => s.Region).ThenInclude(s => s.Country)
                .Where(ls => ls.LayoutParticipant.LayoutId == layoutId)
                .ToListAsync();
        }
        return [];
    }

    public async Task<LayoutStation?> GetLayoutStationAsync(ClaimsPrincipal? principal, int? stationId)
    {
        if (principal.IsAuthenticated() && stationId.HasValue)
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.LayoutStations.AsNoTracking().FirstOrDefaultAsync(ls => ls.StationId == stationId);
        }
        return null;
    }

    public async Task<IEnumerable<LayoutStation>> GetParticipantsLayoutStations(ClaimsPrincipal? principal, int layoutId, int personId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.LayoutStations
                .Include(ls => ls.Station).ThenInclude(s => s.PrimaryModule)
                .Where(ls => ls.LayoutParticipant.LayoutId == layoutId && ls.LayoutParticipant.PersonId == personId)
                .ToReadOnlyListAsync();
        }
        return [];
    }

    public async Task<IEnumerable<LayoutVehicle>> GetLayoutVehicles(int layoutId, bool onlyTractionUnits = false)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.LayoutVehicles
            .Where(lv => lv.LayoutId == layoutId && (onlyTractionUnits == false || lv.IsTractionUnit == true))
            .ToReadOnlyListAsync();
    }
    public async Task<(int Count, string Message, Layout? Entity)> SaveAsync(ClaimsPrincipal? principal, Layout entity)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            if (!entity.ModuleRegistrationClosingDate.HasValue) entity.ModuleRegistrationClosingDate = entity.RegistrationClosingDate;
            var existing = await dbContext.Layouts.SingleOrDefaultAsync(l => l.Id == entity.Id);
            if (existing is null)
            {
                dbContext.Layouts.Add(entity);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(entity);
            }
            else
            {
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                if (dbContext.Entry(entity).State == EntityState.Unchanged) return (-1).SaveResult(existing);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(entity);
            }

        }
        return (0, Resources.Strings.NotAuthorized, entity);
    }

    public Task<IEnumerable<AvailableModule>> GetsAvailableModules(ClaimsPrincipal? principal, int layoutId, int personId) =>
        GetAvaliableModules2("GetAvailableModules", principal, layoutId, personId);

    public Task<IEnumerable<AvailableModule>> GetBorrowableModules(ClaimsPrincipal? principal, int layoutId, int personId) =>
        GetAvaliableModules2("GetBorrowableModules", principal, layoutId, personId);

    private async Task<IEnumerable<AvailableModule>> GetAvaliableModules2(string storedProcedureName, ClaimsPrincipal? principal, int layoutId, int personId)
    {
        var result = new List<AvailableModule>(200);
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            using var connection = dbContext.Database.GetDbConnection() as SqlConnection;
            if (connection is not null)
            {
                var command = new SqlCommand(storedProcedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 120
                };
                command.Parameters.AddWithValue("@LayoutId", layoutId);
                command.Parameters.AddWithValue("@PersonId", personId);
                try
                {
                    connection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    var resourceManager = new ResourceManager(typeof(Resources.Strings));
                    while (await reader.ReadAsync())
                    {
                        result.Add(reader.MapAvailableModule());
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "Failed {storedProcedureName} for layout id={layoutId} and person id = {personId}", storedProcedureName, layoutId, personId);
                    throw;
                }
            }
        }
        return result;
    }


    // THIS IS NOT READY YET
#pragma warning disable IDE0051 // Remove unused private members
    private async Task<IEnumerable<RegisteredModule>> GetRegisteredModulesAsync(ClaimsPrincipal? principal, int layoutId, int personId)
#pragma warning restore IDE0051 // Remove unused private members
    {
        const string storedProcedureName = "RegisteredModules";
        var result = new List<RegisteredModule>(200);
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            using var connection = dbContext.Database.GetDbConnection() as SqlConnection;
            if (connection is not null)
            {
                var command = new SqlCommand(storedProcedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 120
                };
                command.Parameters.AddWithValue("@LayoutId", layoutId);
                command.Parameters.AddWithValue("@PersonId", personId);
                try
                {
                    connection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    var resourceManager = new ResourceManager(typeof(Resources.Strings));
                    while (await reader.ReadAsync())
                    {
                        result.Add(reader.MapRegisteredModule());
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "Failed {storedProcedureName} for layout id={layoutId} and person id = {personId}", storedProcedureName, layoutId, personId);
                    throw;
                }
            }
        }
        return result;
    }

    public async Task<IEnumerable<LayoutModule>> GetLayoutModulesAsync(ClaimsPrincipal? principal, int layoutId, int personId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.LayoutModules
                .Include(ls => ls.LayoutStation).ThenInclude(ls => ls.Station)
                .Include(m => m.Module).ThenInclude(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                .Include(m => m.Module).ThenInclude(m => m.ModuleOwnerships).ThenInclude(mo => mo.Group)
                .Include(lp => lp.LayoutParticipant).ThenInclude(mp => mp.MeetingParticipant).ThenInclude(mp => mp.Person)
                .Where(lm => lm.LayoutParticipant.LayoutId == layoutId && lm.LayoutParticipant.PersonId == personId && lm.LayoutParticipant.LayoutModules.Any(lm => lm.Module.ModuleOwnerships.Any(mo => mo.PersonId == personId || mo.GroupId > 0)))
                .ToListAsync();
        }
        return [];
    }


    /// <summary>
    /// Adds <see cref="LayoutModule">modules</see> and <see cref="LayoutService">stations</see> in a <see cref="ModulePackage"/> to a <see cref="Layout"/>
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="participantId"></param>
    /// <param name="layoutId"></param>
    /// <param name="package"></param>
    /// <returns></returns>
    public async Task<(int Count, string Message)> AddPackageModulesAsync(ClaimsPrincipal? principal, int layoutParticipantId, ModulePackage package)
    {
        if (principal.IsAuthenticated())
        {
            var result = package.Modules.Count();
            using var dbContext = Factory.CreateDbContext();
            var participant = await dbContext.LayoutParticipants
                .SingleOrDefaultAsync(lp => lp.Id == layoutParticipantId);
            if (participant is null) return (-1, Resources.Strings.NotFound);
            foreach (var module in package.Modules)
            {
                var existing = await dbContext.LayoutModules
                    .AsNoTracking()
                    .SingleOrDefaultAsync(lm => lm.ModuleId == module.ModuleId && lm.LayoutParticipantId == layoutParticipantId);
                if (existing is null)
                {
                    var layoutModule = new LayoutModule { ModuleId = module.ModuleId, LayoutParticipantId = participant.Id, RegisteredTime = TimeProvider.Now };
                    if (module.StationId.HasValue)
                    {
                        var moduleStation = await dbContext.Stations.AsNoTracking().SingleOrDefaultAsync(s => s.Id == module.StationId);
                        if (moduleStation is not null)
                        {
                            var existingLayoutStation = await dbContext.LayoutStations.SingleOrDefaultAsync(ls => ls.StationId == moduleStation.Id && ls.LayoutParticipant.Id == layoutParticipantId);
                            if (existingLayoutStation is null)
                            {
                                var layoutStation = new LayoutStation { StationId = module.StationId.Value, LayoutParticipantId = layoutParticipantId };
                                dbContext.LayoutStations.Add(layoutStation);
                                layoutModule.LayoutStation = layoutStation;
                            }
                        }
                    }
                    participant.LastModifiedDateTime = TimeProvider.Now;
                    dbContext.LayoutModules.Add(layoutModule);
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    result--;
                }
            }
            return result == 0 ? (-1, Resources.Strings.NoModification) : (1, Resources.Strings.Saved);
        }
        return (0, Resources.Strings.NotAuthorized);
    }


    public async Task<(int Count, string Message)> RemoveModuleWithStationAsync(ClaimsPrincipal? principal, int layoutModuleId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.LayoutModules
                .Include(lm => lm.LayoutStation).ThenInclude(ls => ls.LayoutModules).ThenInclude(lm=> lm.LayoutParticipant)
                .SingleOrDefaultAsync(lm => lm.Id == layoutModuleId);

            if (existing is null) return (-1, Resources.Strings.NoModification);

            //var stationIsReferredFromOtherLayoutModule = existing.LayoutStation is not null && existing.LayoutStation.LayoutModules.Any(lm => lm.Id != existing.Id);

            //if (!stationIsReferredFromOtherLayoutModule && existing.LayoutStation is not null)
            //{
            //    dbContext.LayoutStations.Remove(existing.LayoutStation);
            //    var stationRemoved = await dbContext.SaveChangesAsync();
            //}
            existing.LayoutParticipant.LastModifiedDateTime = TimeProvider.Now;
            dbContext.LayoutModules.Remove(existing); // If there is a layout station associated with this module, all modules referring that layout station are removed. See database table LayoutModule trigger.
            var result = await dbContext.SaveChangesAsync();
            return result.DeleteResult();
        }
        return principal.NotAuthorized<LayoutModule>();
    }

}
