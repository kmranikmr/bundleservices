using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowSessionAttempt
    {
        public WorkflowSessionAttempt()
        {
            WorkflowSessionLogs = new HashSet<WorkflowSessionLog>();
            WorkflowVersions = new HashSet<WorkflowVersion>();
        }

        public int WorkflowSessionAttemptId { get; set; }
        public int WorkflowVersionId { get; set; }
        public int UserId { get; set; }
        public int? WorkflowProjectId { get; set; }
        public int ExternalProjectId { get; set; }
        public int ExternalWorkflowId { get; set; }
        public int ExternalAttemptId { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? EndDate { get; set; }
        public int WorkflowStatusTypeId { get; set; }
        public int? WorkflowAutomationId { get; set; }
        public string Result { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public WorkflowProject WorkflowProject { get; set; }
        public WorkflowStatusType WorkflowStatusType { get; set; }
        public WorkflowVersion WorkflowVersion { get; set; }
        public ICollection<WorkflowSessionLog> WorkflowSessionLogs { get; set; }
        public ICollection<WorkflowVersion> WorkflowVersions { get; set; }
    }
}
