namespace ESMC.ELM.Web.Models
{
    public class DashboardViewModel
    {
        // =====================================================
        // KPI COUNTERS
        // =====================================================

        public int ActiveProjects { get; set; }

        public int MeterModels { get; set; }

        public int Requirements { get; set; }

        public int OpenDefects { get; set; }

        public int TestRuns { get; set; }

        public int ProductionBatches { get; set; }

        public int OpenServiceRequests { get; set; }

        public int EngineeringTools { get; set; }


        // =====================================================
        // OPERATIONAL SUMMARY
        // =====================================================

        public int ProductionOrders { get; set; }

        public int ServiceRequests { get; set; }

        public int TotalDefects { get; set; }

        public int TotalProjects { get; set; }


        // =====================================================
        // RECENT PROJECTS
        // =====================================================

        public List<DashboardProjectItem> RecentProjects { get; set; }
            = new();


        // =====================================================
        // RECENT SERVICE REQUESTS
        // =====================================================

        public List<DashboardServiceRequestItem> RecentServiceRequests { get; set; }
            = new();
    }


    public class DashboardProjectItem
    {
        public long ProjectId { get; set; }

        public string ProjectCode { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }


    public class DashboardServiceRequestItem
    {
        public long ServiceRequestId { get; set; }

        public string ServiceRequestNumber { get; set; } = string.Empty;

        public string MeterSerialNumber { get; set; } = string.Empty;

        public string ProjectCode { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public DateTime ReceivedDate { get; set; }
    }
}