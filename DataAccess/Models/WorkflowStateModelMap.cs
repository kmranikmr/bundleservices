using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowStateModelMap
    {
        public int WorkflowStateModelMapId { get; set; }
        public int WorkflowAutomationStateId { get; set; }
        public int WorkflowVersionId { get; set; }
        public int? ModelId { get; set; }
        public int? WorkflowOutputModelId { get; set; }
        public int SessionId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual SchemaModel Model { get; set; }
        public virtual WorkflowAutomationState WorkflowAutomationState { get; set; }
        public virtual WorkflowOutputModel WorkflowOutputModel { get; set; }
        public virtual WorkflowVersion WorkflowVersion { get; set; }
    }
}
