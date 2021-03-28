using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(WorkflowServerType))]
    public class WorkflowServerTypeDTO
    {
        public int WorkflowServerTypeId { get; set; }
        public string WorkflowServerTypeName { get; set; }
    }
}
