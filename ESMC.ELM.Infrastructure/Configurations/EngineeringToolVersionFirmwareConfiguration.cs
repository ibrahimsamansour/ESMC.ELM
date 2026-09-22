using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class EngineeringToolVersionFirmwareConfiguration
        : IEntityTypeConfiguration<EngineeringToolVersionFirmware>
    {
        public void Configure(
            EntityTypeBuilder<EngineeringToolVersionFirmware> builder)
        {
            builder.ToTable(
                "EngineeringToolVersionFirmwares",
                t => t.ExcludeFromMigrations());

            // Composite Primary Key
            builder.HasKey(x => new
            {
                x.EngineeringToolVersionId,
                x.MeterModelId,
                x.FirmwareReleaseId
            });

            builder.Property(x => x.EngineeringToolVersionId)
                .IsRequired();

            builder.Property(x => x.MeterModelId)
                .IsRequired();

            builder.Property(x => x.FirmwareReleaseId)
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // Tool Version
            builder.HasOne(x => x.EngineeringToolVersion)
                .WithMany(x => x.Firmwares)
                .HasForeignKey(x => x.EngineeringToolVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Meter Model
            builder.HasOne(x => x.MeterModel)
                .WithMany()
                .HasForeignKey(x => x.MeterModelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Firmware Release
            builder.HasOne(x => x.FirmwareRelease)
                .WithMany()
                .HasForeignKey(x => x.FirmwareReleaseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}