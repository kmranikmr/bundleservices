using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(Project))]
    public class ProjectTreeDTO
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string ProjectDescription { get; set; }

        public SchemaTreeDTO[] ProjectSchemas { get; set; }

        public ProjectTreeDTO()
        {

        }
    }
    [AutoMap(typeof(ProjectSchema))]

    public class SchemaTreeDTO
    {
        public int SchemaId { get; set; }

        public string SchemaName { get; set; }

        public ModelTreeDTO[] SchemaModels { get; set; }

        public SchemaTreeDTO()
        {

        }
    }
    [AutoMap(typeof(SchemaModel))]
    public class ModelTreeDTO
    {
        public int ModelID { get; set; }

        public string ModelName { get; set; }

        public ModelMetadataDTO[] ModelMetadatas { get; set; }

        public ModelTreeDTO()
        {

        }
    }

   


}
