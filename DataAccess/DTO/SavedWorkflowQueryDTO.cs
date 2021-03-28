using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(WorkflowSearchHistory))]
    public class SavedWorkflowQueryDTO
    {
        public string SearchQuery { get; set; }
        public string WorkflowName{get; set;}
        public int WorkflowProjectId { get; set; }
        public int WriterId { get; set; }
        public int WorkflowVersionId { get; set; }
        public string Version { get; set; }
        public int UserId { get; set; }
        public int WorkflowSearchHistoryId { get; set; }
    }
}
