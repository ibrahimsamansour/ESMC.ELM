using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class ProjectEntityConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects", t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.ProjectId);

            builder.Property(x => x.ProjectCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => x.ProjectCode)
                .IsUnique();

            builder.Property(x => x.ProjectName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.CustomerName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Country)
                .HasMaxLength(100);

            builder.Property(x => x.ContractNumber)
                .HasMaxLength(100);

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();
        }
    }
}