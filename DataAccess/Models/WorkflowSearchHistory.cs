using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowSearchHistory
    {
        public WorkflowSearchHistory()
        {
            WorkflowSearchGraphs = new HashSet<WorkflowSearchGraph>();
        }

        public int WorkflowSearchHistoryId { get; set; }
        public string WorkflowSearchHistoryName { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UserId { get; set; }
        public int WorkflowProjectId { get; set; }
        public int WorkflowVersionId { get; set; }
        public string SearchQuery { get; set; }
        public string ResolvedSearchQuery { get; set; }
        public string Md5 { get; set; }
        public string FriendlyName { get; set; }
        public DateTime LastExecutedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual WorkflowProject WorkflowProject { get; set; }
        public virtual WorkflowVersion WorkflowVersion { get; set; }
        public virtual ICollection<WorkflowSearchGraph> WorkflowSearchGraphs { get; set; }
    }
}
