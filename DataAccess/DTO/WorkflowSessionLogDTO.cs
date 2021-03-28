using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(WorkflowSessionLog))]
    public class WorkflowSessionLogDTO
    {
        public int WorkflowSessionLogId { get; set; }
        public int? WorkflowSessionAttemptId { get; set; }
        public int WorkflowVersionId { get; set; }
        public int? WorkflowProjectId { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public string LogData { get; set; }
    }
}
