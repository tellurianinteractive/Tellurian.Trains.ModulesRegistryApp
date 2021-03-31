using Microsoft.EntityFrameworkCore;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class ModulesDbContext : DbContext
    {
        public ModulesDbContext(DbContextOptions<ModulesDbContext> options)
     : base(options)
        {
        }

        public virtual DbSet<Cargo> Cargos { get; set; }
        public virtual DbSet<CargoDirection> CargoDirections { get; set; }
        public virtual DbSet<CargoReadyTime> CargoReadyTimes { get; set; }
        public virtual DbSet<CargoRelation> CargoRelations { get; set; }
        public virtual DbSet<CargoUnit> CargoUnits { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<ExternalStation> ExternalStations { get; set; }
        public virtual DbSet<ExternalStationCustomer> ExternalStationCustomers { get; set; }
        public virtual DbSet<ExternalStationCustomerCargo> ExternalStationCustomerCargos { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<GroupMember> GroupMembers { get; set; }
        public virtual DbSet<Layout> Layouts { get; set; }
        public virtual DbSet<Meeting> Meetings { get; set; }
        public virtual DbSet<Module> Modules { get; set; }
        public virtual DbSet<ModuleExit> ModuleEntries { get; set; }
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
                optionsBuilder.UseSqlServer("Name=ConnectionStrings:TimetablePlanningDatabase");
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
                    .IsRequired()
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

                entity.HasOne(d => d.ConsumerStationCustomerCargo)
                    .WithMany(p => p.CargoRelationConsumerStationCustomerCargos)
                    .HasForeignKey(d => d.ConsumerStationCustomerCargoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CargoRelation_CargoCustomer_Consumer");

                entity.HasOne(d => d.OperatingDay)
                    .WithMany(p => p.CargoRelations)
                    .HasForeignKey(d => d.OperatingDayId)
                    .HasConstraintName("FK_CargoRelation_OperatingDay");

                entity.HasOne(d => d.Operator)
                    .WithMany(p => p.CargoRelations)
                    .HasForeignKey(d => d.OperatorId)
                    .HasConstraintName("FK_CargoRelation_Operator");

                entity.HasOne(d => d.SupplierStationCustomerCargo)
                    .WithMany(p => p.CargoRelationSupplierStationCustomerCargos)
                    .HasForeignKey(d => d.SupplierStationCustomerCargoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CargoRelation_CargoCustomer_Supplier");
            });

            modelBuilder.Entity<CargoUnit>(entity =>
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
                    .HasMaxLength(10)
                    .HasComment("A semicolon separated list of two-letter language codes.");
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

                entity.Property(e => e.Category).HasMaxLength(20);

                entity.Property(e => e.CountyName).HasMaxLength(50);

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.MunicipalityName)
                    .HasMaxLength(10)
                    .IsFixedLength(true);

                entity.Property(e => e.Signature)
                    .IsRequired()
                    .HasMaxLength(6);

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.ExternalStations)
                    .HasForeignKey(d => d.RegionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExternalStation_Region");
            });

            modelBuilder.Entity<ExternalStationCustomer>(entity =>
            {
                entity.ToTable("ExternalStationCustomer");

                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.ExternalStation)
                    .WithMany(p => p.ExternalStationCustomers)
                    .HasForeignKey(d => d.ExternalStationId)
                    .HasConstraintName("FK_ExternalStationCustomer_ExternalStation");
            });

            modelBuilder.Entity<ExternalStationCustomerCargo>(entity =>
            {
                entity.ToTable("ExternalStationCustomerCargo");

                entity.Property(e => e.SpecialCargoName).HasMaxLength(20);

                entity.HasOne<Cargo>()
                    .WithMany()
                    .HasForeignKey(d => d.CargoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExternalStationCustomerCargo_Cargo");

                entity.HasOne(d => d.Direction)
                    .WithMany(p => p.ExternalStationCustomerCargos)
                    .HasForeignKey(d => d.DirectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExternalStationCustomerCargo_CargoDirection");

                entity.HasOne(d => d.ExternalStationCustomer)
                    .WithMany(p => p.ExternalStationCustomerCargos)
                    .HasForeignKey(d => d.ExternalStationCustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExternalStationCustomerCargo_StationCustomer");

                entity.HasOne(d => d.OperatingDay)
                    .WithMany(p => p.ExternalStationCustomerCargos)
                    .HasForeignKey(d => d.OperatingDayId)
                    .HasConstraintName("FK_ExternalStationCustomerCargo_OperatingDay");

                entity.HasOne<CargoUnit>()
                    .WithMany()
                    .HasForeignKey(d => d.QuantityUnitId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExternalStationCustomerCargo_CargoUnit");
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
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Group_Country");
            });

            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.ToTable("GroupMember");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.GroupMembers)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("FK_GroupMember_Group");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.GroupMembers)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GroupMember_Person");
            });

            modelBuilder.Entity<Layout>(entity =>
            {
                entity.Property(e => e.Note)
                    .HasMaxLength(50);


                entity.ToTable("Layout");

                entity.HasOne(d => d.Meeting)
                    .WithMany(p => p.Layouts)
                    .HasForeignKey(d => d.MeetingId)
                    .HasConstraintName("FK_Layout_Meeting");

                entity.HasOne(d => d.ResponsibleGroup)
                    .WithMany()
                    .HasForeignKey(d => d.ResponsibleGroupId)
                    .HasConstraintName("FK_Layout_Group");

                entity.HasOne(d => d.PrimaryModuleStandard)
                    .WithMany()
                    .HasForeignKey(d => d.PrimaryModuleStandardId)
                    .HasConstraintName("FK_Layout_ModuleStandard");

            });

            modelBuilder.Entity<Meeting>(entity =>
            {
                entity.ToTable("Meeting");

                entity.HasOne(d => d.OrganiserGroup)
                    .WithMany()
                    .HasForeignKey(d => d.OrganiserGroupId)
                    .HasConstraintName("FK_Meeting_Group");
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
                    .HasForeignKey(d => d.DwgDrawingId)
                    .HasConstraintName("FK_Module_DwgDocument");

                entity.HasOne(d => d.PdfDocumentation)
                    .WithMany()
                    .HasForeignKey(d => d.PdfDocumentationId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Module_PdfDocument");

                entity.HasOne(d => d.Scale)
                    .WithMany(p => p.Modules)
                    .HasForeignKey(d => d.ScaleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Module_Scale");

                entity.HasOne(d => d.Station)
                      .WithMany(p => p.Modules)
                      .HasForeignKey(d => d.StationId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Module_Station");
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
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ModuleExit_Module");

                entity.HasOne(d => d.GableProperty)
                    .WithMany(p => p.ModuleGables)
                    .HasForeignKey(d => d.GablePropertyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ModuleExit_Property");
            });

            modelBuilder.Entity<ModuleOwnership>(entity =>
            {
                entity.ToTable("ModuleOwnership");

                entity.Property(e => e.GroupId).HasComment("Owning organisation (if null, a Person must own it)");

                entity.Property(e => e.OwnedShare)
                    .HasDefaultValueSql("((1))")
                    .HasComment("The ownerships share as 1/this value.");

                entity.Property(e => e.PersonId).HasComment("Owning person (if null, an Organisation must own it)");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.ModuleOwnerships)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("FK_ModuleOwnership_Group");

                entity.HasOne(d => d.Module)
                    .WithMany(p => p.ModuleOwnerships)
                    .HasForeignKey(d => d.ModuleId)
                    .HasConstraintName("FK_ModuleOwnership_Module");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.ModuleOwnerships)
                    .HasForeignKey(d => d.PersonId)
                    .HasConstraintName("FK_ModuleOwnership_Person");
            });

            modelBuilder.Entity<ModuleStandard>(entity =>
            {
                entity.ToTable("ModuleStandard");

                entity.Property(e => e.AcceptedNorm).HasMaxLength(255);

                entity.Property(e => e.Couplings).HasMaxLength(20);

                entity.Property(e => e.Electricity).HasMaxLength(20);

                entity.Property(e => e.PreferredTheme).HasMaxLength(20);

                entity.Property(e => e.ShortName).HasMaxLength(10);

                entity.Property(e => e.TrackSystem).HasMaxLength(20);

                entity.Property(e => e.Wheelset).HasMaxLength(50);

                entity.HasOne(d => d.Scale)
                    .WithMany(p => p.ModuleStandards)
                    .HasForeignKey(d => d.ScaleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ModuleStandard_Scale");
            });

            modelBuilder.Entity<NHM>(entity =>
            {
                entity.ToTable("NHM");
            });

            modelBuilder.Entity<OperatingBasicDay>(entity =>
            {
                entity.HasKey(e => new { e.OperatingDayId, e.BasicDayId })
                    .HasName("PK_OperatingDayId_BasicDayId");

                entity.ToTable("OperatingBasicDay");

                entity.HasOne(d => d.BasicDay)
                    .WithMany(p => p.OperatingBasicDayBasicDays)
                    .HasForeignKey(d => d.BasicDayId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OperatingBasicDay_BasicDays");

                entity.HasOne(d => d.OperatingDay)
                    .WithMany(p => p.OperatingBasicDayOperatingDays)
                    .HasForeignKey(d => d.OperatingDayId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OperatingBasicDay_OperatingDays");
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
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Person_Country");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Person)
                    .HasForeignKey<Person>(d => d.UserId)
                    .HasConstraintName("FK_Person_User");
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
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.EnglishName)
                    .IsRequired()
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
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Region_Country");
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
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("FK_Station_Region");
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
                    .HasForeignKey(d => d.StationId)
                    .HasConstraintName("FK_StationCustomer_Station");
            });

            modelBuilder.Entity<StationCustomerCargo>(entity =>
            {
                entity.ToTable("StationCustomerCargo");

                entity.Property(e => e.QuantityUnitId).HasDefaultValueSql("((4))");

                entity.Property(e => e.SpecialCargoName).HasMaxLength(20);

                entity.Property(e => e.TrackOrArea).HasMaxLength(10);

                entity.Property(e => e.TrackOrAreaColor)
                    .HasMaxLength(10)
                    .IsFixedLength(true);

                entity.HasOne<Cargo>()
                    .WithMany()
                    .HasForeignKey(d => d.CargoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CargoCustomer_Cargo");

                entity.HasOne(d => d.Direction)
                    .WithMany(p => p.StationCustomerCargos)
                    .HasForeignKey(d => d.DirectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CargoCustomer_CargoDirection");

                entity.HasOne(d => d.OperatingDay)
                    .WithMany(p => p.StationCustomerCargos)
                    .HasForeignKey(d => d.OperatingDayId)
                    .HasConstraintName("FK_CargoCustomer_OperatingDay");

                entity.HasOne(d => d.ReadyTime)
                    .WithMany(p => p.StationCustomerCargos)
                    .HasForeignKey(d => d.ReadyTimeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CargoCustomer_CargoReadyTime");

                entity.HasOne(d => d.StationCustomer)
                    .WithMany(p => p.StationCustomerCargos)
                    .HasForeignKey(d => d.StationCustomerId)
                    .HasConstraintName("FK_CargoCustomer_StationCustomer");
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
                    .HasForeignKey(d => d.StationId)
                    .HasConstraintName("FK_StationTrack_Station");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.EmailAddress, "IX_User_EmailAddress")
                    .IsUnique();

                entity.Property(e => e.EmailAddress)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.HashedPassword).HasMaxLength(255);

                entity.Property(e => e.RegistrationTime).HasDefaultValueSql("(getdate())");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
