using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class DefectConfiguration
        : IEntityTypeConfiguration<Defect>
    {
        public void Configure(EntityTypeBuilder<Defect> builder)
        {
            builder.ToTable(
                "Defects",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.DefectId);

            builder.Property(x => x.DefectCode)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.Source)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Severity)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.ReproductionSteps)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.ExpectedBehavior)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.ActualBehavior)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.RootCause)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.FixDescription)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.VerificationStatus)
                .HasMaxLength(30);

            builder.Property(x => x.ReportedDate)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TestResult)
                .WithMany()
                .HasForeignKey(x => x.TestResultId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MeterModel)
                .WithMany()
                .HasForeignKey(x => x.MeterModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FirmwareRelease)
                .WithMany()
                .HasForeignKey(x => x.FirmwareReleaseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductFunction)
                .WithMany()
                .HasForeignKey(x => x.ProductFunctionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FunctionVersion)
                .WithMany()
                .HasForeignKey(x => x.FunctionVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FixFirmwareRelease)
                .WithMany()
                .HasForeignKey(x => x.FixFirmwareReleaseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}