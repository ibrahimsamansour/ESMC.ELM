using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class FunctionKnowledgeFileConfiguration
        : IEntityTypeConfiguration<FunctionKnowledgeFile>
    {
        public void Configure(
            EntityTypeBuilder<FunctionKnowledgeFile> builder)
        {
            builder.ToTable(
                "FunctionKnowledgeFiles",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.FunctionKnowledgeFileId);

            builder.Property(x => x.FileName)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.FileType)
                .HasMaxLength(100);

            builder.Property(x => x.FilePath)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.UploadedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.HasOne(x => x.ProductFunction)
                .WithMany()
                .HasForeignKey(x => x.ProductFunctionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FunctionVersion)
                .WithMany()
                .HasForeignKey(x => x.FunctionVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}