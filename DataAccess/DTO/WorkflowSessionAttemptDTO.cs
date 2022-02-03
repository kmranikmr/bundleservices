using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(WorkflowSessionAttempt))]
    public class WorkflowSessionAttemptDTO
    {
        public int WorkflowSessionAttemptId { get; set; }
        public int WorkflowVersionId { get; set; }
        public int UserId { get; set; }
        public int? WorkflowProjectId { get; set; }
        public int ExternalProjectId { get; set; }
        public int ExternalWorkflowId { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? EndDate { get; set; }
        public int WorkflowStatusTypeId { get; set; }
        public string Result { get; set; }
        public int ExternalAttemptId { get; set; }

    }
}
