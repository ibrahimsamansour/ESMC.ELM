using ESMC.ELM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESMC.ELM.Infrastructure.Persistence.Configurations
{
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("Tasks", t => t.ExcludeFromMigrations());

            builder.HasKey(x => x.TaskId);

            builder.Property(x => x.TaskNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => x.TaskNumber)
                .IsUnique();

            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.StartDate)
                .HasColumnType("date");

            builder.Property(x => x.DueDate)
                .HasColumnType("date");

            builder.Property(x => x.Notes)
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MeterModel)
                .WithMany()
                .HasForeignKey(x => x.MeterModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Requirement)
                .WithMany()
                .HasForeignKey(x => x.RequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FunctionVersion)
                .WithMany()
                .HasForeignKey(x => x.FunctionVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EngineeringTool)
                .WithMany()
                .HasForeignKey(x => x.EngineeringToolId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}