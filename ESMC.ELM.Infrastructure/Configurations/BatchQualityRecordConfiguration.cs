using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class BatchQualityRecordConfiguration
        : IEntityTypeConfiguration<BatchQualityRecord>
    {
        public void Configure(
            EntityTypeBuilder<BatchQualityRecord> builder)
        {
            builder.ToTable(
                "BatchQualityRecords",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.BatchQualityRecordId);

            builder.Property(x => x.BatchQualityRecordId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ProductionBatchId)
                .IsRequired();

            builder.Property(x => x.InspectionType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.InspectionDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.Result)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.ReportReference)
                .HasMaxLength(300);

            builder.Property(x => x.Comments)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.ProductionBatch)
                .WithMany()
                .HasForeignKey(x => x.ProductionBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}