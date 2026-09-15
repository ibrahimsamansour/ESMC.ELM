using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class ServiceRepairConfiguration
        : IEntityTypeConfiguration<ServiceRepair>
    {
        public void Configure(EntityTypeBuilder<ServiceRepair> builder)
        {
            builder.ToTable(
                "ServiceRepairs",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.ServiceRepairId);

            builder.Property(x => x.ServiceRepairId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ServiceRequestId)
                .IsRequired();

            builder.Property(x => x.RepairDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.RepairEngineerName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.RepairType)
                .IsRequired()
                .HasMaxLength(60);

            builder.Property(x => x.ActionTaken)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.ConfigurationChanged)
                .IsRequired();

            builder.Property(x => x.RepairResult)
                .HasMaxLength(200);

            builder.Property(x => x.Notes)
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.ServiceRequest)
                .WithMany()
                .HasForeignKey(x => x.ServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FirmwareBefore)
                .WithMany()
                .HasForeignKey(x => x.FirmwareBeforeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FirmwareAfter)
                .WithMany()
                .HasForeignKey(x => x.FirmwareAfterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}