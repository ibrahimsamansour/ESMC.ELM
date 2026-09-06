using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class CommunicationInterfaceConfiguration
        : IEntityTypeConfiguration<CommunicationInterface>
    {
        public void Configure(
            EntityTypeBuilder<CommunicationInterface> builder)
        {
            builder.ToTable(
                "CommunicationInterfaces",
                t => t.ExcludeFromMigrations());

            builder.HasKey(c => c.CommunicationInterfaceId);

            builder.Property(c => c.Name)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(c => c.Description)
                .HasMaxLength(300);
        }
    }
}