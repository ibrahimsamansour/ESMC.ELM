using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class ProductionOrderConfigurationConfiguration
        : IEntityTypeConfiguration<ProductionOrderConfiguration>
    {
        public void Configure(
            EntityTypeBuilder<ProductionOrderConfiguration> builder)
        {
            builder.ToTable(
                "ProductionOrderConfigurations",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x =>
                x.ProductionOrderConfigurationId);

            builder.Property(x =>
                    x.ProductionOrderConfigurationId)
                .ValueGeneratedOnAdd();

            builder.Property(x =>
                    x.ProductionOrderId)
                .IsRequired();

            builder.Property(x =>
                    x.ProjectConfigurationId)
                .IsRequired();

            builder.Property(x =>
                    x.PlannedQuantity)
                .IsRequired();

            builder.Property(x =>
                    x.Notes)
                .HasMaxLength(500);

            builder.HasOne(x =>
                    x.ProductionOrder)
                .WithMany()
                .HasForeignKey(x =>
                    x.ProductionOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x =>
                    x.ProjectConfiguration)
                .WithMany()
                .HasForeignKey(x =>
                    x.ProjectConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new
            {
                x.ProductionOrderId,
                x.ProjectConfigurationId
            })
                .IsUnique();
        }
    }
}