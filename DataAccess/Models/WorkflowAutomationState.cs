using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowAutomationState
    {
        public WorkflowAutomationState()
        {
            WorkflowAutomations = new HashSet<WorkflowAutomation>();
            WorkflowStateModelMaps = new HashSet<WorkflowStateModelMap>();
        }

        public int WorkflowAutomationStateId { get; set; }
        public int WorkflowVersionId { get; set; }
        public bool? StateStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public WorkflowVersion WorkflowVersion { get; set; }
        public ICollection<WorkflowAutomation> WorkflowAutomations { get; set; }
        public ICollection<WorkflowStateModelMap> WorkflowStateModelMaps { get; set; }
    }
}
