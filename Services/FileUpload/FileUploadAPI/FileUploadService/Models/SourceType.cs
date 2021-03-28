using System;
using System.Collections.Generic;

namespace FileUploadService.Models
{
    public partial class SourceType
    {
        public SourceType()
        {
            ProjectFile = new HashSet<ProjectFile>();
        }

        public int SourceTypeId { get; set; }
        public string SourceTypeName { get; set; }

        public virtual ICollection<ProjectFile> ProjectFile { get; set; }
    }
}
