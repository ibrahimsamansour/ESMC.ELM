using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class EngineeringToolVersionMeterModelConfiguration
        : IEntityTypeConfiguration<EngineeringToolVersionMeterModel>
    {
        public void Configure(
            EntityTypeBuilder<EngineeringToolVersionMeterModel> builder)
        {
            builder.ToTable(
                "EngineeringToolVersionMeterModels",
                t => t.ExcludeFromMigrations());

            // Composite Primary Key
            builder.HasKey(x => new
            {
                x.EngineeringToolVersionId,
                x.MeterModelId
            });

            builder.Property(x => x.EngineeringToolVersionId)
                .IsRequired();

            builder.Property(x => x.MeterModelId)
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // Tool Version relationship
            builder.HasOne(x => x.EngineeringToolVersion)
                .WithMany(x => x.MeterModels)
                .HasForeignKey(x => x.EngineeringToolVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Meter Model relationship
            builder.HasOne(x => x.MeterModel)
                .WithMany()
                .HasForeignKey(x => x.MeterModelId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}