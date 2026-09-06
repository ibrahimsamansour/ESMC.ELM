using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class TestResultConfiguration
        : IEntityTypeConfiguration<TestResult>
    {
        public void Configure(EntityTypeBuilder<TestResult> builder)
        {
            builder.ToTable(
                "TestResults",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.TestResultId);

            builder.Property(x => x.Result)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.ActualResult)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Comments)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.EvidencePath)
                .HasMaxLength(1000);

            builder.Property(x => x.ExecutionDate)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.TestRun)
                .WithMany()
                .HasForeignKey(x => x.TestRunId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TestCase)
                .WithMany()
                .HasForeignKey(x => x.TestCaseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}