using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowVersion
    {
        public WorkflowVersion()
        {
            WorkflowAutomationStates = new HashSet<WorkflowAutomationState>();
            WorkflowAutomations = new HashSet<WorkflowAutomation>();
            WorkflowModelMetadatas = new HashSet<WorkflowModelMetadata>();
            WorkflowMonitors = new HashSet<WorkflowMonitor>();
            WorkflowOutputModels = new HashSet<WorkflowOutputModel>();
            WorkflowSearchHistories = new HashSet<WorkflowSearchHistory>();
            WorkflowSessionAttempts = new HashSet<WorkflowSessionAttempt>();
            WorkflowStateModelMaps = new HashSet<WorkflowStateModelMap>();
            WorkflowTests = new HashSet<WorkflowTest>();
        }

        public int WorkflowVersionId { get; set; }
        public int WorkflowProjectId { get; set; }
        public int? LastWorkflowSessionAttemptId { get; set; }
        public int UserId { get; set; }
        public int ExternalProjectId { get; set; }
        public int? ExternalWorkflowId { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string WorkflowJson { get; set; }
        public bool? IsPublished { get; set; }
        public string UploadedPath { get; set; }
        public string WorkflowPropertyJson { get; set; }
        public string OutputModelName { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public WorkflowSessionAttempt LastWorkflowSessionAttempt { get; set; }
        public WorkflowProject WorkflowProject { get; set; }
        public ICollection<WorkflowAutomationState> WorkflowAutomationStates { get; set; }
        public ICollection<WorkflowAutomation> WorkflowAutomations { get; set; }
        public ICollection<WorkflowModelMetadata> WorkflowModelMetadatas { get; set; }
        public ICollection<WorkflowMonitor> WorkflowMonitors { get; set; }
        public ICollection<WorkflowOutputModel> WorkflowOutputModels { get; set; }
        public ICollection<WorkflowSearchHistory> WorkflowSearchHistories { get; set; }
        public ICollection<WorkflowSessionAttempt> WorkflowSessionAttempts { get; set; }
        public ICollection<WorkflowStateModelMap> WorkflowStateModelMaps { get; set; }
        public ICollection<WorkflowTest> WorkflowTests { get; set; }
    }
}
