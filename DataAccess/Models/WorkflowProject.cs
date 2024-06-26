﻿using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class WorkflowProject
    {
        public WorkflowProject()
        {
            WorkflowAutomations = new HashSet<WorkflowAutomation>();
            WorkflowMonitors = new HashSet<WorkflowMonitor>();
            WorkflowOutputModels = new HashSet<WorkflowOutputModel>();
            WorkflowSearchHistories = new HashSet<WorkflowSearchHistory>();
            WorkflowSessionAttempts = new HashSet<WorkflowSessionAttempt>();
            WorkflowSessionLogs = new HashSet<WorkflowSessionLog>();
            WorkflowTests = new HashSet<WorkflowTest>();
            WorkflowVersions = new HashSet<WorkflowVersion>();
        }

        public int WorkflowProjectId { get; set; }
        public int UserId { get; set; }
        public int ExternalProjectId { get; set; }
        public string ExternalProjectName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string RecentVersionNumber { get; set; }
        public int? WorkflowServerTypeId { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual WorkflowServerType WorkflowServerType { get; set; }
        public virtual ICollection<WorkflowAutomation> WorkflowAutomations { get; set; }
        public virtual ICollection<WorkflowMonitor> WorkflowMonitors { get; set; }
        public virtual ICollection<WorkflowOutputModel> WorkflowOutputModels { get; set; }
        public virtual ICollection<WorkflowSearchHistory> WorkflowSearchHistories { get; set; }
        public virtual ICollection<WorkflowSessionAttempt> WorkflowSessionAttempts { get; set; }
        public virtual ICollection<WorkflowSessionLog> WorkflowSessionLogs { get; set; }
        public virtual ICollection<WorkflowTest> WorkflowTests { get; set; }
        public virtual ICollection<WorkflowVersion> WorkflowVersions { get; set; }
    }
}
