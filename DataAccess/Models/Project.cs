using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Project
    {
        public Project()
        {
            Jobs = new HashSet<Job>();
            ModelMetadatas = new HashSet<ModelMetadata>();
            ProjectAutomations = new HashSet<ProjectAutomation>();
            ProjectFiles = new HashSet<ProjectFile>();
            ProjectReaders = new HashSet<ProjectReader>();
            ProjectSchemas = new HashSet<ProjectSchema>();
            ProjectUsers = new HashSet<ProjectUser>();
            ProjectWriters = new HashSet<ProjectWriter>();
            SchemaModels = new HashSet<SchemaModel>();
            SearchHistories = new HashSet<SearchHistory>();
        }

        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastAccessedOn { get; set; }
        public bool IsFavorite { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Job> Jobs { get; set; }
        public virtual ICollection<ModelMetadata> ModelMetadatas { get; set; }
        public virtual ICollection<ProjectAutomation> ProjectAutomations { get; set; }
        public virtual ICollection<ProjectFile> ProjectFiles { get; set; }
        public virtual ICollection<ProjectReader> ProjectReaders { get; set; }
        public virtual ICollection<ProjectSchema> ProjectSchemas { get; set; }
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }
        public virtual ICollection<ProjectWriter> ProjectWriters { get; set; }
        public virtual ICollection<SchemaModel> SchemaModels { get; set; }
        public virtual ICollection<SearchHistory> SearchHistories { get; set; }
    }
}
