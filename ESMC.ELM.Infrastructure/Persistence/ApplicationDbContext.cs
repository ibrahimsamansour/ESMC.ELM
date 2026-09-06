using ESMC.ELM.Domain.Entities;
using ESMC.ELM.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ESMC.ELM.Infrastructure.Persistence
{
    public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<MeterModel> MeterModels { get; set; } = null!;
        public DbSet<FirmwareRelease> FirmwareReleases { get; set; } = null!;
        public DbSet<CommunicationInterface> CommunicationInterfaces { get; set; } = null!;
        public DbSet<ProjectConfiguration> ProjectConfigurations { get; set; } = null!;
        public DbSet<ProductFunction> ProductFunctions { get; set; } = null!;
        public DbSet<FunctionVersion> FunctionVersions { get; set; } = null!;
        public DbSet<FunctionKnowledgeFile> FunctionKnowledgeFiles { get; set; } = null!;

        public DbSet<MeterModelCommunicationInterface>
            MeterModelCommunicationInterfaces
        { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}