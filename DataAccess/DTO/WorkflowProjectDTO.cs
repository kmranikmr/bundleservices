using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(WorkflowProject))]
    public class WorkflowProjectDTO
    {
        public string ExternalProjectName { get; set; }
        public int WorkflowProjectId { get; set; }
        public  DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string RecentVersionNumber { get; set; }
        public int WorkflowServerTypeId { get; set; }
        public string Description { get; set; }
       
    }
}
