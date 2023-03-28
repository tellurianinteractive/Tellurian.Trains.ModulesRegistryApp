#nullable disable

using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

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
    public virtual DbSet<LayoutLine> LayoutLines { get; set; }
    public virtual DbSet<LayoutModule> LayoutModules { get; set; }
    public virtual DbSet<LayoutParticipant> LayoutParticipants { get; set; }
    public virtual DbSet<LayoutStation> LayoutStations { get; set; }
    public virtual DbSet<ListboxItem> ListboxItems { get; set; }    
    public virtual DbSet<Meeting> Meetings { get; set; }
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
        modelBuilder.MapLayoutLine();
        modelBuilder.MapLayoutModule();
        modelBuilder.MapLayoutParticipant();
        modelBuilder.MapLayoutStation();

       

       

        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToTable("Meeting", tb => tb.HasTrigger("DeleteMeeting"));

            entity.HasOne(d => d.OrganiserGroup)
                .WithMany()
                .HasForeignKey(d => d.OrganiserGroupId);

            entity.HasOne(d => d.GroupDomain)
                .WithMany()
                .HasForeignKey(d => d.GroupDomainId);

            entity.HasMany(d => d.Participants)
                .WithOne(d => d.Meeting)
                .HasForeignKey(d => d.MeetingId);
        });

        modelBuilder.Entity<MeetingParticipant>(entity =>
        {
            entity.ToTable("MeetingParticipant", tb => tb.HasTrigger("DeleteMeetingParticipant"));

            entity.HasOne(d => d.Meeting)
                .WithMany(d => d.Participants)
                .HasForeignKey(d => d.MeetingId);

            entity.HasOne(d => d.Person)
                .WithMany()
                .HasForeignKey(d => d.PersonId);

            entity.HasMany(d => d.LayoutParticipations)
                .WithOne(d => d.MeetingParticipant)
                .HasForeignKey(d => d.MeetingParticipantId);
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.ToTable("Module");

            entity.Property(e => e.ConfigurationLabel).HasMaxLength(10);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Note).HasMaxLength(255);

            entity.Property(e => e.NumberOfThroughTracks).HasDefaultValueSql("((1))");

            entity.Property(e => e.PackageLabel).HasMaxLength(10);

            entity.Property(e => e.Theme).HasMaxLength(50);

            entity.HasOne(d => d.DwgDrawing)
                .WithMany()
                .HasForeignKey(d => d.DwgDrawingId);

            entity.HasOne(d => d.SkpDrawing)
                .WithMany()
                .HasForeignKey(d => d.SkpDrawingId);

            entity.HasOne(d => d.PdfDocumentation)
                .WithMany()
                .HasForeignKey(d => d.PdfDocumentationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Scale)
                .WithMany(p => p.Modules)
                .HasForeignKey(d => d.ScaleId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Station)
                  .WithMany(p => p.Modules)
                  .HasForeignKey(d => d.StationId)
                  .OnDelete(DeleteBehavior.SetNull);
        });


        modelBuilder.Entity<ModuleEndProfile>(entity =>
        {
            entity.ToTable("ModuleEndProfile");

            entity.Property(e => e.Designation)
                .IsRequired();

            entity.HasOne(d => d.Scale)
                .WithMany()
                .HasForeignKey(d => d.ScaleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.PdfDocument)
                .WithMany()
                .HasForeignKey(d => d.PdfDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ModuleExit>(entity =>
        {
            entity.ToTable("ModuleExit");

            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.Module)
                .WithMany(p => p.ModuleExits)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.EndProfile)
                .WithMany()
                .HasForeignKey(d => d.EndProfileId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<ModuleOwnership>(entity =>
        {
            entity.ToTable("ModuleOwnership");

            entity.Property(e => e.GroupId)
                .HasComment("Owning organisation (if null, a Person must own it)");

            entity.Property(e => e.OwnedShare)
                .HasDefaultValueSql("((1))")
                .HasComment("The ownerships share as 1/this value.");

            entity.Property(e => e.PersonId)
                .HasComment("Owning person (if null, an Organisation must own it)");

            entity.HasOne(d => d.Group)
                .WithMany(p => p.ModuleOwnerships)
                .HasForeignKey(d => d.GroupId);

            entity.HasOne(d => d.Module)
                .WithMany(p => p.ModuleOwnerships)
                .HasForeignKey(d => d.ModuleId);

            entity.HasOne(d => d.Person)
                .WithMany(p => p.ModuleOwnerships)
                .HasForeignKey(d => d.PersonId);
        });

        modelBuilder.Entity<ModuleStandard>(entity =>
        {
            entity.ToTable("ModuleStandard");

            entity.Property(e => e.AcceptedNorm)
                .HasMaxLength(255);

            entity.Property(e => e.Couplings)
                .HasMaxLength(20);

            entity.Property(e => e.Electricity)
                .HasMaxLength(20);

            entity.Property(e => e.PreferredTheme)
                .HasMaxLength(20);

            entity.Property(e => e.ShortName)
                .HasMaxLength(10);

            entity.Property(e => e.TrackSystem)
                .HasMaxLength(20);

            entity.Property(e => e.Wheelset)
                .HasMaxLength(50);

            entity.HasOne(d => d.Scale)
                .WithMany(p => p.ModuleStandards)
                .HasForeignKey(d => d.ScaleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<NHM>(entity => entity.ToTable("NHM"));

        modelBuilder.Entity<OperatingBasicDay>(entity =>
        {
            entity.HasKey(e => new { e.OperatingDayId, e.BasicDayId });

            entity.ToTable("OperatingBasicDay");

            entity.HasOne(d => d.BasicDay)
                .WithMany(p => p.OperatingBasicDayBasicDays)
                .HasForeignKey(d => d.BasicDayId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OperatingDay)
                .WithMany(p => p.OperatingBasicDayOperatingDays)
                .HasForeignKey(d => d.OperatingDayId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<OperatingDay>(entity =>
        {
            entity.ToTable("OperatingDay");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ShortName)
                .IsRequired()
                .HasMaxLength(10);
        });

        modelBuilder.Entity<Operator>(entity =>
        {
            entity.ToTable("Operator");

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Signature)
                .IsRequired()
                .HasMaxLength(6);

            entity.HasOne(c => c.PrimaryOperatingCountry)
                .WithMany()
                .HasForeignKey(e => e.PrimaryOperatingCountryId);
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("Person");

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

            entity.HasIndex(e => e.EmailAddress, "IX_User_EmailAddress")
                .IsUnique();

            entity.Property(e => e.EmailAddress)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.HashedPassword)
                .HasMaxLength(255);

            entity.Property(e => e.RegistrationTime)
                .HasDefaultValueSql("(getdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
