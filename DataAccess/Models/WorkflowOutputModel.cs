using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowOutputModel
    {
        public WorkflowOutputModel()
        {
            WorkflowModelMetadatas = new HashSet<WorkflowModelMetadata>();
            WorkflowMonitors = new HashSet<WorkflowMonitor>();
            WorkflowStateModelMaps = new HashSet<WorkflowStateModelMap>();
        }

        public int WorkflowOutputModelId { get; set; }
        public int WorkflowProjectId { get; set; }
        public int WorkflowVersionId { get; set; }
        public string TableName { get; set; }
        public string DisplayName { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public WorkflowProject WorkflowProject { get; set; }
        public WorkflowVersion WorkflowVersion { get; set; }
        public ICollection<WorkflowModelMetadata> WorkflowModelMetadatas { get; set; }
        public ICollection<WorkflowMonitor> WorkflowMonitors { get; set; }
        public ICollection<WorkflowStateModelMap> WorkflowStateModelMaps { get; set; }
    }
}
