using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class ExecutionProject
    {
        public string id;
        public string name;
    }
    public class ExecutionWorkflow
    {
        public string name;
        public string id;
    }
    public class RestSessionAttempt
    {
        public  string id { get; set; }
        public string sessionUuid { get; set; }
        public  string sessionId { get; set; }
        [JsonProperty(PropertyName = "Project")]
        public ExecutionProject project { get; set; }
        [JsonProperty(PropertyName = "Workflow")]
        public  ExecutionWorkflow workflow{ get; set; }
    }

    public class IngestedData
    {
        public int ProjectId { get; set; }
        public int SchemaId { get; set; }
        public int ModelId { get; set; }
        public int jobId { get; set; }
        public int UserId { get; set; }
    }

}
