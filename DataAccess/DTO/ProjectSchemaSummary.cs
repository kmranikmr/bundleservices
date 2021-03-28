using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    public class ProjectSchemaSummary
    {
        public ProjectSchemaSummary()
        {
            SourcetoNumberOfFiles = new Dictionary<string, int>();
            WriterTypes = new List<string>();
        }

        public string SchemaName { get; set; }
        public int SchemaId { get; set; }
        public int NumberOfModels { get; set; }
        public Dictionary<string, int> SourcetoNumberOfFiles { get; set; }
        public List<string> WriterTypes { get; set; }
    }
}
