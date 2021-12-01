using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class ProjectFile
    {
        public ProjectFile()
        {
            Jobs = new HashSet<Job>();
        }

        public int ProjectFileId { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public int SourceTypeId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string SourceConfiguration { get; set; }
        public DateTime UploadDate { get; set; }
        public int? ReaderId { get; set; }
        public int? SchemaId { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Project Project { get; set; }
        public virtual Reader Reader { get; set; }
        public virtual ProjectSchema Schema { get; set; }
        public virtual SourceType SourceType { get; set; }
        public virtual ICollection<Job> Jobs { get; set; }
    }
}
