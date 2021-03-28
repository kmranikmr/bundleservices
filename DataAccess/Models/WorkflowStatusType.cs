using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowStatusType
    {
        public WorkflowStatusType()
        {
            WorkflowSessionAttempts = new HashSet<WorkflowSessionAttempt>();
            WorkflowTests = new HashSet<WorkflowTest>();
        }

        public int WorkflowStatusTypeId { get; set; }
        public string WorkflowStatusTypeName { get; set; }

        public ICollection<WorkflowSessionAttempt> WorkflowSessionAttempts { get; set; }
        public ICollection<WorkflowTest> WorkflowTests { get; set; }
    }
}
