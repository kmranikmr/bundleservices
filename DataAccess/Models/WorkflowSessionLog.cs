using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowSessionLog
    {
        public int WorkflowSessionLogId { get; set; }
        public int? WorkflowSessionAttemptId { get; set; }
        public int ExternalProjectId { get; set; }
        public int? WorkflowProjectId { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public string LogData { get; set; }

        public WorkflowProject WorkflowProject { get; set; }
        public WorkflowSessionAttempt WorkflowSessionAttempt { get; set; }
    }
}
