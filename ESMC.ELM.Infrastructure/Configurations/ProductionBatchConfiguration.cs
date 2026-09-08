using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class ProductionBatchConfiguration
        : IEntityTypeConfiguration<ProductionBatch>
    {
        public void Configure(
            EntityTypeBuilder<ProductionBatch> builder)
        {
            builder.ToTable(
                "ProductionBatches",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.ProductionBatchId);

            builder.Property(x => x.ProductionBatchId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.BatchNumber)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(x => x.ProductionOrderId)
                .IsRequired();

            builder.Property(x => x.ProjectConfigurationId)
                .IsRequired();

            builder.Property(x => x.BatchQuantity)
                .IsRequired();

            builder.Property(x => x.ProductionDate)
                .HasColumnType("date");

            builder.Property(x => x.CompletionDate)
                .HasColumnType("date");

            builder.Property(x => x.ProductionLine)
                .HasMaxLength(100);

            builder.Property(x => x.BatchStatus)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.BatchDecision)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.DecisionDate)
                .HasColumnType("date");

            builder.Property(x => x.DecisionBy)
                .HasMaxLength(200);

            builder.Property(x => x.RejectionReason)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Notes)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.HasOne(x => x.ProductionOrder)
                .WithMany()
                .HasForeignKey(x => x.ProductionOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProjectConfiguration)
                .WithMany()
                .HasForeignKey(x => x.ProjectConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.BatchNumber)
                .IsUnique();
        }
    }
}