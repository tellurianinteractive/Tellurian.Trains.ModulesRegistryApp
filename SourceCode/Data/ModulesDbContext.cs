#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public partial class ModulesDbContext : DbContext
{
    //public ModulesDbContext() { }
    public ModulesDbContext(DbContextOptions<ModulesDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Cargo> Cargos { get; set; }
    public virtual DbSet<CargoDirection> CargoDirections { get; set; }
    public virtual DbSet<CargoReadyTime> CargoReadyTimes { get; set; }
    public virtual DbSet<CargoRelation> CargoRelations { get; set; }
    public virtual DbSet<QuantityUnit> CargoUnits { get; set; }
    public virtual DbSet<Country> Countries { get; set; }
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
    public virtual DbSet<Meeting> Meetings { get; set; }
    public virtual DbSet<MeetingParticipant> MeetingParticipants { get; set; }
    public virtual DbSet<Module> Modules { get; set; }
    public virtual DbSet<ModuleExit> ModuleExits { get; set; }
    public virtual DbSet<ModuleGableType> ModuleGableTypes { get; set; }
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "Finnish_Swedish_CI_AS");

        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.ToTable("Cargo");

            entity.Property(e => e.DefaultClasses).HasMaxLength(10);
            entity.Property(e => e.NhmCode).HasColumnName("NHMCode");

            entity.Property(e => e.EN)
                .HasMaxLength(50)
                .HasColumnName("EN");

            entity.Property(e => e.DA)
                .HasMaxLength(50)
                .HasColumnName("DA");

            entity.Property(e => e.DE)
                .HasMaxLength(50)
                .HasColumnName("DE");

            entity.Property(e => e.NL)
                 .HasMaxLength(50)
                 .HasColumnName("NL");

            entity.Property(e => e.NO)
                 .HasMaxLength(50)
                 .HasColumnName("NO");

            entity.Property(e => e.PL)
                 .HasMaxLength(50)
                 .HasColumnName("PL");

            entity.Property(e => e.SV)
                .HasMaxLength(50)
                .HasColumnName("SV");

            entity.HasOne<NHM>()
                .WithMany()
                .HasForeignKey(e => e.NhmCode);
        });

        modelBuilder.Entity<CargoDirection>(entity =>
        {
            entity.ToTable("CargoDirection");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.ShortName)
                .IsRequired()
                .HasMaxLength(4);
        });

        modelBuilder.Entity<CargoReadyTime>(entity =>
        {
            entity.ToTable("CargoReadyTime");

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.ShortName)
                .IsRequired()
                .HasMaxLength(10);
        });

        modelBuilder.Entity<CargoRelation>(entity =>
        {
            entity.ToTable("CargoRelation");

            entity.HasOne(d => d.OperatingDay)
                .WithMany()
                .HasForeignKey(d => d.OperatingDayId);

            entity.HasOne(d => d.SupplierStationCustomerCargo)
               .WithMany()
               .HasForeignKey(d => d.SupplierStationCustomerCargoId);

            entity.HasOne(d => d.ConsumerStationCustomerCargo)
                 .WithMany()
                 .HasForeignKey(d => d.ConsumerStationCustomerCargoId);
        });

        modelBuilder.Entity<QuantityUnit>(entity =>
        {
            entity.ToTable("CargoUnit");

            entity.Property(e => e.Designation)
                .IsRequired()
                .HasMaxLength(6);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(12);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Country");

            entity.Property(e => e.DomainSuffix)
                .IsRequired()
                .HasMaxLength(2)
                .IsFixedLength(true);

            entity.Property(e => e.EnglishName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Languages)
                .HasMaxLength(10);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Document");

            entity.Property(e => e.FileExtension)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength(true);
        });

        modelBuilder.Entity<ExternalStation>(entity =>
        {
            entity.ToTable("ExternalStation");

            entity.Property(e => e.Note).HasMaxLength(20);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Signature)
                .IsRequired()
                .HasMaxLength(6);

            entity.HasOne(d => d.Region)
                .WithMany(p => p.ExternalStations)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ExternalStationCustomer>(entity =>
        {
            entity.ToTable("ExternalStationCustomer");

            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.ExternalStation)
                .WithMany(p => p.ExternalStationCustomers)
                .HasForeignKey(d => d.ExternalStationId);
        });

        modelBuilder.Entity<ExternalStationCustomerCargo>(entity =>
        {
            entity.ToTable("ExternalStationCustomerCargo");

            entity.Property(e => e.SpecialCargoName).HasMaxLength(20);

            entity.Property(e => e.SpecificWagonClass).HasMaxLength(10);

            entity.HasOne(e => e.Cargo)
                .WithMany()
                .HasForeignKey(d => d.CargoId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Direction)
                .WithMany()
                .HasForeignKey(d => d.DirectionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ExternalStationCustomer)
                .WithMany(p => p.ExternalStationCustomerCargos)
                .HasForeignKey(d => d.ExternalStationCustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OperatingDay)
                .WithMany()
                .HasForeignKey(d => d.OperatingDayId);

            entity.HasOne(e => e.QuantityUnit)
                .WithMany()
                .HasForeignKey(d => d.QuantityUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("Group");

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.CityName).HasMaxLength(50);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ShortName).HasMaxLength(10);

            entity.HasOne(d => d.Country)
                .WithMany(p => p.Groups)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Group_Country");

            entity.HasOne(d => d.GroupDomain)
                .WithMany(p => p.Groups)
                .HasForeignKey(d => d.GroupDomainId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GroupDomain>(entity => entity.ToTable("GroupDomain"));

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.ToTable("GroupMember");

            entity.HasOne(d => d.Group)
                .WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.GroupId);

            entity.HasOne(d => d.Person)
                .WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Layout>(entity =>
        {
            entity.Property(e => e.Note)
                .HasMaxLength(50);

            entity.ToTable("Layout");

            entity.HasOne(d => d.Meeting)
                .WithMany(p => p.Layouts)
                .HasForeignKey(d => d.MeetingId);

            entity.HasOne(d => d.ResponsibleGroup)
                .WithMany()
                .HasForeignKey(d => d.ResponsibleGroupId);

            entity.HasOne(d => d.PrimaryModuleStandard)
                .WithMany()
                .HasForeignKey(d => d.PrimaryModuleStandardId);
        });

        modelBuilder.Entity<LayoutLine>(entity =>
        {
            entity.ToTable("LayoutLine");

            entity.HasOne(e => e.FromLayoutStation)
                .WithMany(e => e.StartingLines)
                .HasForeignKey(e => e.FromLayoutStationId);

            entity.HasOne(e => e.FromStationExit)
                .WithMany()
                .HasForeignKey(e => e.FromStationExitId);

            entity.HasOne(e => e.ToLayoutStation)
                .WithMany(e => e.EndingLines)
                .HasForeignKey(e => e.ToLayoutStationId);

            entity.HasOne(e => e.ToStationExit)
              .WithMany()
              .HasForeignKey(e => e.ToStationExitId);

        });

        modelBuilder.Entity<LayoutModule>(entity =>
        {
            entity.Property(e => e.Note)
                 .HasMaxLength(50);

            entity.ToTable("LayoutModule");

            entity.HasOne(e => e.LayoutParticipant)
                 .WithMany(e => e.LayoutModules)
                 .HasForeignKey(e => e.LayoutParticipantId);

            entity.HasOne(e => e.Module)
                  .WithMany()
                  .HasForeignKey(e => e.ModuleId);

            entity.HasOne(e => e.LayoutLine)
               .WithMany(e => e.Lines)
               .HasForeignKey(e => e.LayoutLineId);

            entity.HasOne(e => e.LayoutStation)
                .WithMany(e => e.LayoutModules)
                .HasForeignKey(e => e.LayoutStationId);

            entity.HasOne(e => e.LayoutParticipant)
                .WithMany(e => e.LayoutModules)
                .HasForeignKey(e => e.LayoutParticipantId);

        });

        modelBuilder.Entity<LayoutParticipant>(entity =>
        {
            entity.ToTable("LayoutParticipant");

            entity.HasOne(e => e.Person)
                .WithMany()
                .HasForeignKey(e => e.PersonId);

            entity.HasOne(e => e.MeetingParticipant)
                .WithMany()
                .HasForeignKey(e => e.MeetingParticipantId);

            entity.HasOne(e => e.Layout)
                .WithMany(e => e.LayoutParticipants)
                .HasForeignKey(e => e.LayoutId);

        });

        modelBuilder.Entity<LayoutStation>(entity =>
        {
            entity.ToTable("LayoutStation");

            entity.Property(e => e.OtherName)
                 .HasMaxLength(50);

            entity.Property(e => e.OtherSignature)
                  .HasMaxLength(5);

            entity.HasOne(e => e.LayoutParticipant)
                 .WithMany(e => e.LayoutStations)
                 .HasForeignKey(e => e.LayoutParticipantId);

            entity.HasMany(e => e.Regions)
                .WithMany(e => e.LayoutStations);

            entity.HasOne(e => e.Station)
                 .WithOne()
                 .HasForeignKey<LayoutStation>(e => e.StationId);

            entity.HasOne(e => e.OtherCountry)
                .WithOne()
                .HasForeignKey<LayoutStation>(e => e.OtherCountryId);

        });

        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToTable("Meeting");

            entity.HasOne(d => d.OrganiserGroup)
                .WithMany()
                .HasForeignKey(d => d.OrganiserGroupId);
        });

        modelBuilder.Entity<MeetingParticipant>(entity =>
        {
            entity.ToTable("MeetingParticipant");

            entity.HasOne(d => d.Meeting)
                .WithMany(d => d.Participants)
                .HasForeignKey(d => d.MeetingId);

            entity.HasOne(d => d.Person)
                .WithMany()
                .HasForeignKey(d => d.PersonId);

            entity.HasMany(d => d.Layouts)
                .WithOne()
                .HasForeignKey(d => d.LayoutId);
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

            entity.HasOne(d => d.GableType)
                .WithMany()
                .HasForeignKey(d => d.GableTypeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ModuleGableType>(entity =>
        {
            entity.ToTable("ModuleGableType");

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
            entity.ToTable("Station");

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Signature)
                .IsRequired()
                .HasMaxLength(5);

            entity.HasOne(d => d.Region)
                .WithMany(p => p.Stations)
                .HasForeignKey(d => d.RegionId);
        });

        modelBuilder.Entity<StationCustomer>(entity =>
        {
            entity.ToTable("StationCustomer");

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

        _ = modelBuilder.Entity<StationCustomerCargo>(entity =>
          {
              entity.ToTable("StationCustomerCargo");

              entity.Property(e => e.QuantityUnitId).HasDefaultValueSql("((4))");

              entity.Property(e => e.SpecialCargoName).HasMaxLength(20);

              entity.Property(e => e.TrackOrArea).HasMaxLength(10);

              entity.Property(e => e.TrackOrAreaColor)
                  .HasMaxLength(10)
                  .IsFixedLength(true);

              entity.Property(e => e.SpecificWagonClass).HasMaxLength(10);

              entity.HasOne(e => e.Cargo)
                  .WithMany()
                  .HasForeignKey(d => d.CargoId)
                  .OnDelete(DeleteBehavior.ClientSetNull);

              entity.HasOne(d => d.Direction)
                  .WithMany()
                  .HasForeignKey(d => d.DirectionId)
                  .OnDelete(DeleteBehavior.ClientSetNull);

              entity.HasOne(d => d.OperatingDay)
                  .WithMany()
                  .HasForeignKey(d => d.OperatingDayId);

              entity.HasOne(d => d.ReadyTime)
                  .WithMany()
                  .HasForeignKey(d => d.ReadyTimeId)
                  .OnDelete(DeleteBehavior.ClientSetNull);

              entity.HasOne(d => d.StationCustomer)
                  .WithMany(p => p.StationCustomerCargos)
                  .HasForeignKey(d => d.StationCustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

              entity.HasOne(e => e.QuantityUnit)
                  .WithMany()
                  .HasForeignKey(d => d.QuantityUnitId)
                  .OnDelete(DeleteBehavior.ClientSetNull);
          });

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
