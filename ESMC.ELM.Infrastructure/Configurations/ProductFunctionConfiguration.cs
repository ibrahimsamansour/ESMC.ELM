using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class ProductFunctionConfiguration
        : IEntityTypeConfiguration<ProductFunction>
    {
        public void Configure(
            EntityTypeBuilder<ProductFunction> builder)
        {
            builder.ToTable(
                "ProductFunctions",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.ProductFunctionId);

            builder.Property(x => x.FunctionCode)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(x => x.FunctionName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Category)
                .HasMaxLength(100);

            builder.Property(x => x.Purpose)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Description)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.OwnerUserId);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CreatedByUserId);

            builder.Property(x => x.UpdatedAt);

            builder.Property(x => x.UpdatedByUserId);

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.Property(x => x.DeletedAt);

            builder.Property(x => x.DeletedByUserId);
        }
    }
}