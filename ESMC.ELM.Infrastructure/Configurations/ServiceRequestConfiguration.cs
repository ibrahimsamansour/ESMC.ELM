using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Persistence.Configurations
{
    public class ServiceRequestConfiguration
        : IEntityTypeConfiguration<ServiceRequest>
    {
        public void Configure(
            EntityTypeBuilder<ServiceRequest> builder)
        {
            builder.ToTable(
                "ServiceRequests",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.ServiceRequestId);

            builder.Property(x => x.ServiceRequestNumber)
                .IsRequired()
                .HasMaxLength(80);

            builder.Property(x => x.ProjectId)
                .IsRequired();

            builder.Property(x => x.MeterSerialNumber)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.CustomerReference)
                .HasMaxLength(200);

            builder.Property(x => x.ReceivedDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.CustomerComplaint)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.ConditionOnReceipt)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.WarrantyStatus)
                .HasMaxLength(50);

            builder.Property(x => x.Priority)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(40);

            builder.Property(x => x.ReturnDate)
                .HasColumnType("date");

            builder.Property(x => x.Notes)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MeterModel)
                .WithMany()
                .HasForeignKey(x => x.MeterModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FirmwareRelease)
                .WithMany()
                .HasForeignKey(x => x.FirmwareReleaseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductionBatch)
                .WithMany()
                .HasForeignKey(x => x.ProductionBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}