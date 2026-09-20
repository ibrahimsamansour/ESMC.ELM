using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class PostMaintenanceResultConfiguration
        : IEntityTypeConfiguration<PostMaintenanceResult>
    {
        public void Configure(EntityTypeBuilder<PostMaintenanceResult> builder)
        {
            builder.ToTable(
                "PostMaintenanceResults",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.PostMaintenanceResultId);

            builder.Property(x => x.PostMaintenanceResultId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ServiceRequestId)
                .IsRequired();

            builder.Property(x => x.ResultDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.FinalStatus)
                .IsRequired()
                .HasMaxLength(40);

            builder.Property(x => x.FinalCondition)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.CurrentConfigurationReference)
                .HasMaxLength(200);

            builder.Property(x => x.FirmwareChanged)
                .IsRequired();

            builder.Property(x => x.ConfigurationChanged)
                .IsRequired();

            builder.Property(x => x.ComponentsChangedSummary)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.FunctionalStatus)
                .HasMaxLength(200);

            builder.Property(x => x.CommunicationStatus)
                .HasMaxLength(200);

            builder.Property(x => x.MeterReadingStatus)
                .HasMaxLength(200);

            builder.Property(x => x.CalibrationStatus)
                .HasMaxLength(200);

            builder.Property(x => x.ReadyForReturn)
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasColumnType("nvarchar(max)");

            // One Post-Maintenance Result per Service Request
            builder.HasIndex(x => x.ServiceRequestId)
                .IsUnique();

            builder.HasOne(x => x.ServiceRequest)
                .WithMany()
                .HasForeignKey(x => x.ServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CurrentFirmwareRelease)
                .WithMany()
                .HasForeignKey(x => x.CurrentFirmwareReleaseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}