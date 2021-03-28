using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(WorkflowTest))]
    public class WorkflowTestDTO
    {
        public int WorkflowTestId { get; set; }
        public int VersionNumber { get; set; }
        public string Status { get; set; }
        public int WorkflowProjectId { get; set; }
        public int WorkflowVersionId { get; set; }
        public int UserId { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string WorkflowJson { get; set; }
        public string WorkflowPropertyJson { get; set; }
        public int WorkflowStatusTypeId { get; set; }
        public string Result { get; set; }
        public string LogData { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

    }
}
