using System;
using System.Collections.Generic;

namespace DataAccess.Models
{

    public class PromptLayerData
    {
        public Dictionary<string, Dictionary<string, List<PromptNode>>> PromptNodes { get; set; }
        public Dictionary<string, List<string>> PromptLinks { get; set; }
    }

    public class PromptNode
    {
        public string Type { get; set; }
        public string Usage { get; set; }
        public int Id { get; set; }
        public string Template { get; set; }
        public string[] VarInput { get; set; }
    }
}
