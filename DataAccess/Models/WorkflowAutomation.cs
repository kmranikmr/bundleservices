using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowAutomation
    {
        public int WorkflowAutomationId { get; set; }
        public int WorkflowProjectId { get; set; }
        public int WorkflowVersionId { get; set; }
        public int WorkflowAutomationStateId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public WorkflowAutomationState WorkflowAutomationState { get; set; }
        public WorkflowProject WorkflowProject { get; set; }
        public WorkflowVersion WorkflowVersion { get; set; }
    }
}
