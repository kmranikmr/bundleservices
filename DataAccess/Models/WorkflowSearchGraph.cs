using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowSearchGraph
    {
        public int WorkflowSearchGraphId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int WorkflowSearchHistoryId { get; set; }
        public string GraphDescription { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public WorkflowSearchHistory WorkflowSearchHistory { get; set; }
    }
}
