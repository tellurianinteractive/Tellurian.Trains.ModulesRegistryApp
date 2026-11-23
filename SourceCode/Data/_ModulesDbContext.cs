#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;
#pragma warning disable IDE0290 // Use primary constructor

public partial class ModulesDbContext : DbContext
{
    //public ModulesDbContext() { }
    public ModulesDbContext(DbContextOptions<ModulesDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Cargo> Cargos { get; set; }
    public virtual DbSet<CargoDirection> CargoDirections { get; set; }
    public virtual DbSet<CargoPackagingUnit> CargoPackagingUnits { get; set; }
    public virtual DbSet<CargoReadyTime> CargoReadyTimes { get; set; }
    public virtual DbSet<CargoQuantityUnit> CargoUnits { get; set; }
    public virtual DbSet<ChangeLog> ChangeLogs { get; set; }
    public virtual DbSet<Country> Countries { get; set; }
    public virtual DbSet<CountryStatistics> CountriesStatistics { get; set; }
    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<ExternalStation> ExternalStations { get; set; }
    public virtual DbSet<ExternalStationCustomer> ExternalStationCustomers { get; set; }
    public virtual DbSet<ExternalStationCustomerCargo> ExternalStationCustomerCargos { get; set; }
    public virtual DbSet<Group> Groups { get; set; }
    public virtual DbSet<GroupDomain> GroupDomains { get; set; }
    public virtual DbSet<GroupLayoutModule> GroupLayoutModules { get; set; }
    public virtual DbSet<GroupMember> GroupMembers { get; set; }
    public virtual DbSet<Layout> Layouts { get; set; }
    public virtual DbSet<LayoutModule> LayoutModules { get; set; }
    public virtual DbSet<LayoutParticipant> LayoutParticipants { get; set; }
    public virtual DbSet<LayoutStation> LayoutStations { get; set; }
    public virtual DbSet<ListboxItem> ListboxItems { get; set; }
    public virtual DbSet<Meeting> Meetings { get; set; }
    public virtual DbSet<MeetingAdministrator> MeetingAdministrators { get; set; }
    public virtual DbSet<MeetingParticipant> MeetingParticipants { get; set; }
    public virtual DbSet<Module> Modules { get; set; }
    public virtual DbSet<ModuleEndProfile> ModuleEndProfiles { get; set; }
    public virtual DbSet<ModuleExit> ModuleExits { get; set; }
    public virtual DbSet<ModuleOwnership> ModuleOwnerships { get; set; }
    public virtual DbSet<ModuleStandard> ModuleStandards { get; set; }
    public virtual DbSet<NHM> NhmCodes { get; set; }
    public virtual DbSet<OperatingBasicDay> OperatingBasicDays { get; set; }
    public virtual DbSet<OperatingDay> OperatingDays { get; set; }
    public virtual DbSet<Person> People { get; set; }
    public virtual DbSet<Property> Properties { get; set; }
    public virtual DbSet<Region> Regions { get; set; }
    public virtual DbSet<Scale> Scales { get; set; }
    public virtual DbSet<Station> Stations { get; set; }
    public virtual DbSet<StationCustomer> StationCustomers { get; set; }
    public virtual DbSet<StationCustomerCargo> StationCustomerCargos { get; set; }
    public virtual DbSet<StationCustomerWaybill> StationCustomerWaybills { get; set; }
    public virtual DbSet<StationTrack> StationTracks { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Vehicle> Vehicles { get; set; }
    public virtual DbSet<VehicleFeature> VehicleFeatures { get; set; }
    public virtual DbSet<VehicleOperator> VehicleOperators { get; set; }
    public virtual DbSet<WiFredThrottle> WiFredThrottles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Name=ConnectionStrings:TimetablePlanningDatabase",
                options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        }
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
        // Indexes are declared in each entity's mapping as they are in the SQL project, so EF doesn't add one for every foreign key.
        configurationBuilder.Conventions.Remove<ForeignKeyIndexConvention>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "Finnish_Swedish_CI_AS");

        modelBuilder.MapCargo();
        modelBuilder.MapCargoDirection();
        modelBuilder.MapCargoPackagingUnit();
        modelBuilder.MapCargoQuantityUnit();
        modelBuilder.MapCargoReadyTime();
        modelBuilder.MapChangeLog();
        modelBuilder.MapCountry();
        modelBuilder.MapCountryStatistics();
        modelBuilder.MapDocument();
        modelBuilder.MapExternalStation();
        modelBuilder.MapExternalStationCustomer();
        modelBuilder.MapExternalStationCustomerCargo();
        modelBuilder.MapGroup();
        modelBuilder.MapGroupDomain();
        modelBuilder.MapGroupLayoutModule();
        modelBuilder.MapGroupMember();
        modelBuilder.MapLayout();
        modelBuilder.MapLayoutModule();
        modelBuilder.MapLayoutParticipant();
        modelBuilder.MapLayoutStation();
        modelBuilder.MapListboxItem();
        modelBuilder.MapMeeting();
        modelBuilder.MapMeetingAdministrator();
        modelBuilder.MapMeetingParticipant();
        modelBuilder.MapModule();
        modelBuilder.MapModuleEndProfile();
        modelBuilder.MapModuleExit();
        modelBuilder.MapModuleOwnership();
        modelBuilder.MapModuleStandard();
        modelBuilder.MapNHM();
        modelBuilder.MapOperatingBasicDay();
        modelBuilder.MapOperationDay();
        modelBuilder.MapPerson();
        modelBuilder.MapProperty();
        modelBuilder.MapRegion();
        modelBuilder.MapScale();
        modelBuilder.MapStation();
        modelBuilder.MapStationCustomer();
        modelBuilder.MapStationCustomerCargo();
        modelBuilder.MapStationCustomerWaybill();
        modelBuilder.MapStationTrack();
        modelBuilder.MapUser();
        modelBuilder.MapVehicle();
        modelBuilder.MapVehicleFeature();
        modelBuilder.MapVehicleInteroperability();
        modelBuilder.MapVehicleOperator();
        modelBuilder.MapWiFredThrottle();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}