using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class ProductionOrderEntityConfiguration
        : IEntityTypeConfiguration<ProductionOrder>
    {
        public void Configure(
            EntityTypeBuilder<ProductionOrder> builder)
        {
            builder.ToTable("ProductionOrders", t =>
                t.ExcludeFromMigrations());

            builder.HasKey(x => x.ProductionOrderId);

            builder.Property(x => x.ProductionOrderId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ProductionOrderNumber)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(x => x.ProjectId)
                .IsRequired();

            builder.Property(x => x.OrderDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.PlannedQuantity)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.RequestedStartDate)
                .HasColumnType("date");

            builder.Property(x => x.RequestedCompletionDate)
                .HasColumnType("date");

            builder.Property(x => x.ActualCompletionDate)
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

            builder.HasIndex(x => x.ProductionOrderNumber)
                .IsUnique();
        }
    }
}