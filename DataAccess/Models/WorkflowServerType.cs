using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowServerType
    {
        public WorkflowServerType()
        {
            WorkflowProjects = new HashSet<WorkflowProject>();
        }

        public int WorkflowServerTypeId { get; set; }
        public string WorkflowServerTypeName { get; set; }

        public ICollection<WorkflowProject> WorkflowProjects { get; set; }
    }
}
