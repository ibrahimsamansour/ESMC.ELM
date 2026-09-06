using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class TestRunConfiguration
        : IEntityTypeConfiguration<TestRun>
    {
        public void Configure(
            EntityTypeBuilder<TestRun> builder)
        {
            builder.ToTable(
                "TestRuns",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.TestRunId);

            builder.Property(x => x.TestRunCode)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(x => x.TestType)
                .HasMaxLength(40)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.Environment)
                .HasMaxLength(200);

            builder.Property(x => x.BuildVersion)
                .HasMaxLength(100);

            builder.Property(x => x.StartDate)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

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

            builder.HasOne(x => x.ProjectConfiguration)
                .WithMany()
                .HasForeignKey(x => x.ProjectConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FirmwareRelease)
                .WithMany()
                .HasForeignKey(x => x.FirmwareReleaseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}