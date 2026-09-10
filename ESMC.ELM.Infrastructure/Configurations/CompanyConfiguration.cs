using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class CompanyConfiguration
        : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable(
                "Companies",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.CompanyId);

            builder.Property(x => x.CompanyId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.CompanyCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.CompanyName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.CompanyType)
                .HasMaxLength(50);

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.HasIndex(x => x.CompanyCode)
                .IsUnique();
        }
    }
}