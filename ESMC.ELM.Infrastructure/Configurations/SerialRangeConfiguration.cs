using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class SerialRangeConfiguration
        : IEntityTypeConfiguration<SerialRange>
    {
        public void Configure(EntityTypeBuilder<SerialRange> builder)
        {
            builder.ToTable(
                "SerialRanges",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.SerialRangeId);

            builder.Property(x => x.SerialRangeId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ProductionBatchId)
                .IsRequired();

            builder.Property(x => x.SerialFrom)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.SerialTo)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.RecipientCompanyId);

            builder.HasOne(x => x.ProductionBatch)
                .WithMany()
                .HasForeignKey(x => x.ProductionBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RecipientCompany)
                .WithMany()
                .HasForeignKey(x => x.RecipientCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}