using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class EngineeringToolConfiguration
        : IEntityTypeConfiguration<EngineeringTool>
    {
        public void Configure(EntityTypeBuilder<EngineeringTool> builder)
        {
            builder.ToTable(
                "EngineeringTools",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.EngineeringToolId);

            builder.Property(x => x.EngineeringToolId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ToolCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ToolName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.ToolType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Vendor)
                .HasMaxLength(150);

            builder.Property(x => x.Description)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.HasIndex(x => x.ToolCode)
                .IsUnique();

            builder.HasMany(x => x.Versions)
                .WithOne(x => x.EngineeringTool)
                .HasForeignKey(x => x.EngineeringToolId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}