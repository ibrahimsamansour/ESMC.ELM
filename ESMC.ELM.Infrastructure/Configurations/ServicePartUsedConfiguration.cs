using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class ServicePartUsedConfiguration
        : IEntityTypeConfiguration<ServicePartUsed>
    {
        public void Configure(EntityTypeBuilder<ServicePartUsed> builder)
        {
            builder.ToTable(
                "ServicePartsUsed",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.ServicePartUsedId);

            builder.Property(x => x.ServicePartUsedId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ServiceRepairId)
                .IsRequired();

            builder.Property(x => x.PartName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.PartNumber)
                .HasMaxLength(100);

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.Property(x => x.OldPartSerialNumber)
                .HasMaxLength(100);

            builder.Property(x => x.NewPartSerialNumber)
                .HasMaxLength(100);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.HasOne(x => x.ServiceRepair)
                .WithMany()
                .HasForeignKey(x => x.ServiceRepairId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}