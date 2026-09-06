using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class MeterModelConfiguration : IEntityTypeConfiguration<MeterModel>
    {
        public void Configure(EntityTypeBuilder<MeterModel> builder)
        {
            builder.ToTable(
                "MeterModels",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.MeterModelId);

            builder.Property(x => x.ModelCode)
                .IsRequired()
                .HasMaxLength(80);

            builder.Property(x => x.CommercialName)
                .HasMaxLength(200);

            builder.Property(x => x.ModelFamily)
                .HasMaxLength(100);

            builder.Property(x => x.MeterType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.PhaseType)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.ConnectionType)
                .HasMaxLength(50);

            builder.Property(x => x.ActiveAccuracyClass)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.ReactiveAccuracyClass)
                .HasMaxLength(30);

            builder.Property(x => x.ReferenceVoltage)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.BasicCurrent)
                .HasPrecision(18, 3);

            builder.Property(x => x.MaximumCurrent)
                .HasPrecision(18, 3);

            builder.Property(x => x.Frequency)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.ProductStatus)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.Description)
                .HasColumnType("nvarchar(max)");
        }
    }
}