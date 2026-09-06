using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class MeterModelCommunicationInterfaceConfiguration
        : IEntityTypeConfiguration<MeterModelCommunicationInterface>
    {
        public void Configure(
            EntityTypeBuilder<MeterModelCommunicationInterface> builder)
        {
            builder.ToTable(
                "MeterModelCommunicationInterfaces",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => new
            {
                x.MeterModelId,
                x.CommunicationInterfaceId
            });

            builder.HasOne(x => x.MeterModel)
                .WithMany()
                .HasForeignKey(x => x.MeterModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CommunicationInterface)
                .WithMany(c => c.MeterModelCommunicationInterfaces)
                .HasForeignKey(x => x.CommunicationInterfaceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}