using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class FunctionVersionConfiguration
        : IEntityTypeConfiguration<FunctionVersion>
    {
        public void Configure(
            EntityTypeBuilder<FunctionVersion> builder)
        {
            builder.ToTable(
                "FunctionVersions",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.FunctionVersionId);

            builder.Property(x => x.Revision)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.BehaviorDescription)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.Inputs)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Outputs)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Preconditions)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.TriggerConditions)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.ProcessingLogic)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.ErrorConditions)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Parameters)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Dependencies)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.DLMSObjects)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Commands)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.SecurityRequirements)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.CommunicationRequirements)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.StandardsReferences)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.ChangeFromPrevious)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.ReasonForChange)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.ApprovalDate)
                .HasColumnType("date");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.HasOne(x => x.ProductFunction)
                .WithMany()
                .HasForeignKey(x => x.ProductFunctionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}