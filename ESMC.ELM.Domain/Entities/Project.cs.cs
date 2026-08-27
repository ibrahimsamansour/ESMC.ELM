using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMC.ELM.Domain.Entities
{
    public class Project
    {
        public long ProjectId { get; set; }

        public string ProjectCode { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string? Country { get; set; }

        public string? ContractNumber { get; set; }

        public int? ContractQuantity { get; set; }

        public Guid? ProjectManagerUserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }
    }
}