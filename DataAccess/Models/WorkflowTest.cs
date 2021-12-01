using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowTest
    {
        public int WorkflowTestId { get; set; }
        public int WorkflowProjectId { get; set; }
        public int WorkflowVersionId { get; set; }
        public int UserId { get; set; }
        public int ExternalProjectId { get; set; }
        public int? ExternalWorkflowId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string WorkflowJson { get; set; }
        public string WorkflowPropertyJson { get; set; }
        public int ExternalAttemptId { get; set; }
        public int WorkflowStatusTypeId { get; set; }
        public string Result { get; set; }
        public string LogData { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual WorkflowProject WorkflowProject { get; set; }
        public virtual WorkflowStatusType WorkflowStatusType { get; set; }
        public virtual WorkflowVersion WorkflowVersion { get; set; }
    }
}
