using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class FirmwareReleaseConfiguration
        : IEntityTypeConfiguration<FirmwareRelease>
    {
        public void Configure(
            EntityTypeBuilder<FirmwareRelease> builder)
        {
            builder.ToTable(
                "FirmwareReleases",
                t => t.ExcludeFromMigrations());

            builder.HasKey(f => f.FirmwareReleaseId);

            builder.Property(f => f.Version)
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(f => f.ReleaseType)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(f => f.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(f => f.ChangeSummary)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(f => f.ReasonForChange)
                .HasColumnType("nvarchar(max)");

            builder.Property(f => f.Checksum)
                .HasMaxLength(256);

            builder.Property(f => f.FirmwareFilePath)
                .HasMaxLength(1000);

            builder.Property(f => f.ReleaseNotes)
                .HasColumnType("nvarchar(max)");

            builder.Property(f => f.ReleaseDate)
                .HasColumnType("date");

            builder.Property(f => f.ApprovalDate)
                .HasColumnType("date");

            builder.Property(f => f.CreatedAt)
                .IsRequired();

            builder.Property(f => f.IsDeleted)
                .IsRequired();

            builder.HasOne(f => f.MeterModel)
                .WithMany()
                .HasForeignKey(f => f.MeterModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.ParentFirmwareRelease)
                .WithMany()
                .HasForeignKey(f => f.ParentFirmwareReleaseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}