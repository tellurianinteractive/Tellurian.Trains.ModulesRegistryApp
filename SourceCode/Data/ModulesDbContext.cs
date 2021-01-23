using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class ModulesDbContext : DbContext
    {
        //public ModulesDbContext()
        //{
        //}

        public ModulesDbContext(DbContextOptions<ModulesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<ExternalStation> ExternalStations { get; set; }
        public virtual DbSet<ExternalStationCustomer> ExternalStationCustomers { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Module> Modules { get; set; }
        public virtual DbSet<ModuleOwnership> ModuleOwnerships { get; set; }
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<Station> Stations { get; set; }
        public virtual DbSet<StationCustomer> StationCustomers { get; set; }
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

            modelBuilder.Entity<Group>(entity =>
            {
                entity.ToTable("Group");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Purpose)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.ShortName).HasMaxLength(10);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Groups)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Group_Country");
            });

            modelBuilder.Entity<Module>(entity =>
            {
                entity.ToTable("Module");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FullName).HasMaxLength(50);

                entity.Property(e => e.NumberOfThroughTracks).HasDefaultValueSql("((1))");

                entity.Property(e => e.Scale).HasDefaultValueSql("((87))");

                entity.HasOne(d => d.Station)
                    .WithMany(p => p.Modules)
                    .HasForeignKey(d => d.StationId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Module_Station");
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

            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable("Person");

                entity.Property(e => e.CityName).HasMaxLength(50);

                entity.Property(e => e.EmailAddresses).HasMaxLength(50);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.MiddleName).HasMaxLength(50);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.People)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Person_Country1");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Person)
                    .HasForeignKey<Person>(d => d.UserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Person_User");
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

                entity.Property(e => e.TrackOrArea)
                    .HasMaxLength(10)
                    .IsFixedLength(true);

                entity.HasOne(d => d.Station)
                    .WithMany(p => p.StationCustomers)
                    .HasForeignKey(d => d.StationId)
                    .HasConstraintName("FK_StationCustomer_Station");
            });

            modelBuilder.Entity<StationTrack>(entity =>
            {
                entity.ToTable("StationTrack");

                entity.Property(e => e.Designation)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.UsageNote).HasMaxLength(50);

                entity.HasOne(d => d.Station)
                    .WithMany(p => p.StationTracks)
                    .HasForeignKey(d => d.StationId)
                    .HasConstraintName("FK_StationTrack_Station");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.EmailAddress).IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
