using System;
using System.Collections.Generic;

namespace FileUploadService.Model
{
    public partial class Project
    {
        public Project()
        {
            Jobs = new HashSet<Job>();
            ProjectFiles = new HashSet<ProjectFile>();
            ProjectReaders = new HashSet<ProjectReader>();
            ProjectSchemas = new HashSet<ProjectSchema>();
            ProjectUsers = new HashSet<ProjectUser>();
            ProjectWriters = new HashSet<ProjectWriter>();
        }

        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastAccessedOn { get; set; }
        public bool IsFavorite { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<Job> Jobs { get; set; }
        public ICollection<ProjectFile> ProjectFiles { get; set; }
        public ICollection<ProjectReader> ProjectReaders { get; set; }
        public ICollection<ProjectSchema> ProjectSchemas { get; set; }
        public ICollection<ProjectUser> ProjectUsers { get; set; }
        public ICollection<ProjectWriter> ProjectWriters { get; set; }
    }
}
