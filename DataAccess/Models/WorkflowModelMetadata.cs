using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowModelMetadata
    {
        public int WorkflowModelMetadataId { get; set; }
        public int WorkflowVersionId { get; set; }
        public int WorkflowOutputModelId { get; set; }
        public string ColumnName { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public WorkflowOutputModel WorkflowOutputModel { get; set; }
        public WorkflowVersion WorkflowVersion { get; set; }
    }
}
