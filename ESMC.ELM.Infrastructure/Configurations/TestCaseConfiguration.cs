using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class TestCaseConfiguration
        : IEntityTypeConfiguration<TestCase>
    {
        public void Configure(
            EntityTypeBuilder<TestCase> builder)
        {
            builder.ToTable(
                "TestCases",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.TestCaseId);

            builder.Property(x => x.TestCaseCode)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(x => x.TestType)
                .HasMaxLength(40)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.Objective)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Preconditions)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.TestSteps)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.TestData)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.ExpectedResult)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Requirement)
                .WithMany()
                .HasForeignKey(x => x.RequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductFunction)
                .WithMany()
                .HasForeignKey(x => x.ProductFunctionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}