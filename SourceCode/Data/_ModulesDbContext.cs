#nullable disable

using Microsoft.EntityFrameworkCore;
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
    public virtual DbSet<CargoRelation> CargoRelations { get; set; }
    public virtual DbSet<CargoQuantityUnit> CargoUnits { get; set; }
    public virtual DbSet<Country> Countries { get; set; }
    public virtual DbSet<CountryStatistics> CountriesStatistics { get; set; }
    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<ExternalStation> ExternalStations { get; set; }
    public virtual DbSet<ExternalStationCustomer> ExternalStationCustomers { get; set; }
    public virtual DbSet<ExternalStationCustomerCargo> ExternalStationCustomerCargos { get; set; }
    public virtual DbSet<Group> Groups { get; set; }
    public virtual DbSet<GroupDomain> GroupDomains { get; set; }
    public virtual DbSet<GroupMember> GroupMembers { get; set; }
    public virtual DbSet<Layout> Layouts { get; set; }
    public virtual DbSet<LayoutModule> LayoutModules { get; set; }
    public virtual DbSet<LayoutParticipant> LayoutParticipants { get; set; }
    public virtual DbSet<LayoutStation> LayoutStations { get; set; }
    public virtual DbSet<LayoutVehicle> LayoutVehicles { get; set; }
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
    public virtual DbSet<Operator> Operators { get; set; }
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
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "Finnish_Swedish_CI_AS");

        modelBuilder.MapCargo();
        modelBuilder.MapCargoDirection();
        modelBuilder.MapCargoPackagingUnit();
        modelBuilder.MapCargoQuantityUnit();
        modelBuilder.MapCargoReadyTime();
        modelBuilder.MapCargoRelation();
        modelBuilder.MapCountry();
        modelBuilder.MapCountryStatistics();
        modelBuilder.MapDocument();
        modelBuilder.MapExternalStation();
        modelBuilder.MapExternalStationCustomer();
        modelBuilder.MapExternalStationCustomerCargo();
        modelBuilder.MapGroup();
        modelBuilder.MapGroupDomain();
        modelBuilder.MapGroupMember();
        modelBuilder.MapLayout();
        modelBuilder.MapLayoutModule();
        modelBuilder.MapLayoutParticipant();
        modelBuilder.MapLayoutStation();
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
        modelBuilder.MapOperator();

        modelBuilder.MapVehicle();
        modelBuilder.MapVehicleFeature();
        modelBuilder.MapVehicleInteroperability();
        modelBuilder.MapVehicleOperator();
        modelBuilder.MapWiFredThrottle();






        
        

        

        

       

       
        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("Person");
            entity.HasQueryFilter(p => p.DeletedTimestamp.HasValue == false);

            entity.Property(e => e.CityName).HasMaxLength(50);

            entity.Property(e => e.EmailAddresses).HasMaxLength(50);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.FremoOwnerSignature).HasMaxLength(10);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.MiddleName).HasMaxLength(50);

            entity.HasOne(d => d.Country)
                .WithMany(p => p.People)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User)
                .WithOne(p => p.Person)
                .HasForeignKey<Person>(d => d.UserId);
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("Property");

            entity.HasIndex(e => new { e.Name, e.Value }, "IX_Property_Unique")
                .IsUnique();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.ToTable("Region");

            entity.Property(e => e.BackColor)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.ColourName)
                .HasMaxLength(10);

            entity.Property(e => e.EnglishName)
                .HasMaxLength(50);

            entity.Property(e => e.ForeColor)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.LocalName)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.Country)
                .WithMany(p => p.Regions)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(s => s.RepresentativeExternalStation)
                .WithOne()
                .HasForeignKey<Region>(s => s.RepresentativeExternalStationId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Scale>(entity =>
        {
            entity.ToTable("Scale");

            entity.Property(e => e.ShortName)
                .IsRequired()
                .HasMaxLength(10);
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.ToTable("Station", tb => tb.HasTrigger("DeleteStation"));

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Signature)
                .IsRequired()
                .HasMaxLength(5);

            entity.HasOne(d => d.Region)
                .WithMany(p => p.Stations)
                .HasForeignKey(d => d.RegionId);

            entity.HasOne(e => e.PrimaryModule)
                .WithMany()
                .HasForeignKey(e => e.PrimaryModuleId);
        });

        modelBuilder.Entity<StationCustomer>(entity =>
        {
            entity.ToTable("StationCustomer", tb => tb.HasTrigger("DeleteStationCustomer"));

            entity.Property(e => e.Comment)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.TrackOrArea).HasMaxLength(50);

            entity.Property(e => e.TrackOrAreaColor)
                .HasMaxLength(10)
                .IsFixedLength(true);

            entity.HasOne(d => d.Station)
                .WithMany(p => p.StationCustomers)
                .HasForeignKey(d => d.StationId);
        });

        modelBuilder.MapStationCustomerWaybill();

        modelBuilder.MapStationCustomerCargo();

        modelBuilder.Entity<StationTrack>(entity =>
        {
            entity.ToTable("StationTrack");

            entity.Property(e => e.Designation)
                .IsRequired()
                .HasMaxLength(5);

            entity.Property(e => e.UsageNote).HasMaxLength(20);

            entity.HasOne(d => d.Station)
                .WithMany(p => p.StationTracks)
                .HasForeignKey(d => d.StationId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasQueryFilter(u => u.DeletedTimestamp.HasValue == false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
