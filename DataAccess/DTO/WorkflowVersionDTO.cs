using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(WorkflowVersion))]
    public class WorkflowVersionDTO
    {
        public int WorkflowVersionId { get; set; }
        public int WorkflowProjectId { get; set; }
        public int UserId { get; set; }
        public int ExternalWorkflowId { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string WorkflowJson { get; set; }
        public bool? IsPublished { get; set; }  
        public string UploadedPath { get; set; }
        public string WorkflowPropertyJson { get; set; }
        public string Status { get; set; }
        public bool? IsActive { get; set; }

    }
}
