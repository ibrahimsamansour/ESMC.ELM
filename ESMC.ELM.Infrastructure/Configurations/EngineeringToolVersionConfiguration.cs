using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class EngineeringToolVersionConfiguration
        : IEntityTypeConfiguration<EngineeringToolVersion>
    {
        public void Configure(EntityTypeBuilder<EngineeringToolVersion> builder)
        {
            builder.ToTable(
                "EngineeringToolVersions",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.EngineeringToolVersionId);

            builder.Property(x => x.EngineeringToolVersionId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.EngineeringToolId)
                .IsRequired();

            builder.Property(x => x.Version)
                .IsRequired()
                .HasMaxLength(80);

            builder.Property(x => x.ReleaseDate)
                .HasColumnType("date");

            builder.Property(x => x.ExecutableReference)
                .HasMaxLength(500);

            builder.Property(x => x.ReleaseNotes)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            // Same version cannot be duplicated
            // for the same Engineering Tool.
            builder.HasIndex(x => new
            {
                x.EngineeringToolId,
                x.Version
            })
            .IsUnique();

            builder.HasOne(x => x.EngineeringTool)
                .WithMany(x => x.Versions)
                .HasForeignKey(x => x.EngineeringToolId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.MeterModels)
                .WithOne(x => x.EngineeringToolVersion)
                .HasForeignKey(x => x.EngineeringToolVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Firmwares)
                .WithOne(x => x.EngineeringToolVersion)
                .HasForeignKey(x => x.EngineeringToolVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}