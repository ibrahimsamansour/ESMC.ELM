using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class ServiceInspectionConfiguration
        : IEntityTypeConfiguration<ServiceInspection>
    {
        public void Configure(
            EntityTypeBuilder<ServiceInspection> builder)
        {
            builder.ToTable(
                "ServiceInspections",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.ServiceInspectionId);

            builder.Property(x => x.InspectionDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.InspectionType)
                .IsRequired()
                .HasMaxLength(160);

            builder.Property(x => x.ObservedProblem)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Diagnosis)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.RootCause)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.FailureCategory)
                .HasMaxLength(200);

            builder.Property(x => x.InspectionResult)
                .HasMaxLength(400);

            builder.Property(x => x.Notes)
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.ServiceRequest)
                .WithMany()
                .HasForeignKey(x => x.ServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AffectedFunction)
                .WithMany()
                .HasForeignKey(x => x.AffectedFunctionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}