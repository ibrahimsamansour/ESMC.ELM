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
        public DbSet<Requirement> Requirements { get; set; } = null!;
        public DbSet<RequirementFunction> RequirementFunctions { get; set; } = null!;
        public DbSet<TestCase> TestCases { get; set; } = null!;
        public DbSet<TestRun> TestRuns { get; set; } = null!;
        public DbSet<TestResult> TestResults { get; set; } = null!;
        public DbSet<Defect> Defects { get; set; } = null!;
        public DbSet<ProductionOrder> ProductionOrders { get; set; }
        public DbSet<ProductionOrderConfiguration>
    ProductionOrderConfigurations
        { get; set; }
        public DbSet<ProductionBatch> ProductionBatches { get; set; }
        public DbSet<BatchQualityRecord> BatchQualityRecords { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<SerialRange> SerialRanges { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

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