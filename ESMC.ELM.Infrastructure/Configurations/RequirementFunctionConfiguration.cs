using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Configurations
{
    public class RequirementFunctionConfiguration
        : IEntityTypeConfiguration<RequirementFunction>
    {
        public void Configure(
            EntityTypeBuilder<RequirementFunction> builder)
        {
            builder.ToTable(
                "RequirementFunctions",
                t => t.ExcludeFromMigrations());

            builder.HasKey(x => new
            {
                x.RequirementId,
                x.ProductFunctionId
            });

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.HasOne(x => x.Requirement)
                .WithMany()
                .HasForeignKey(x => x.RequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductFunction)
                .WithMany()
                .HasForeignKey(x => x.ProductFunctionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}