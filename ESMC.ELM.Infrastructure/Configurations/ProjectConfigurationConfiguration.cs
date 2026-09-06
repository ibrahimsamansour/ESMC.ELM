using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class ProjectConfigurationConfiguration
        : IEntityTypeConfiguration<ProjectConfiguration>
    {
        public void Configure(
            EntityTypeBuilder<ProjectConfiguration> builder)
        {
            builder.ToTable(
                "ProjectConfigurations",
                t => t.ExcludeFromMigrations());

            builder.HasKey(p => p.ProjectConfigurationId);

            builder.Property(p => p.ConfigurationCode)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(p => p.ConfigurationName)
                .HasMaxLength(200);

            builder.Property(p => p.PlannedQuantity)
                .IsRequired();

            builder.Property(p => p.MeterProfile)
                .HasMaxLength(200);

            builder.Property(p => p.CommunicationProfile)
                .HasMaxLength(200);

            builder.Property(p => p.DLMSConfigurationVersion)
                .HasMaxLength(100);

            builder.Property(p => p.ParameterizationVersion)
                .HasMaxLength(100);

            builder.Property(p => p.EncryptionKeysVersion)
                .HasMaxLength(100);

            builder.Property(p => p.LabelVersion)
                .HasMaxLength(100);

            builder.Property(p => p.PackagingVersion)
                .HasMaxLength(100);

            builder.Property(p => p.EffectiveFrom)
                .HasColumnType("date");

            builder.Property(p => p.EffectiveTo)
                .HasColumnType("date");

            builder.Property(p => p.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(p => p.ChangeReason)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.ApprovalDate)
                .HasColumnType("date");

            builder.Property(p => p.EngineeringNotes)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.IsDeleted)
                .IsRequired();

            builder.HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.MeterModel)
                .WithMany()
                .HasForeignKey(p => p.MeterModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.FirmwareRelease)
                .WithMany()
                .HasForeignKey(p => p.FirmwareReleaseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}