using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class RequirementConfiguration
        : IEntityTypeConfiguration<Requirement>
    {
        public void Configure(EntityTypeBuilder<Requirement> builder)
        {
            builder.ToTable(
                "Requirements",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.RequirementId);

            builder.Property(x => x.RequirementCode)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(x => x.RequirementType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.CustomerRequirement)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.TechnicalInterpretation)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.Source)
                .HasMaxLength(300);

            builder.Property(x => x.Priority)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.AcceptanceCriteria)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}