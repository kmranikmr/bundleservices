using System;
using System.Collections.Generic;

namespace FileUploadService.Model
{
    public partial class SourceType
    {
        public SourceType()
        {
            ProjectFiles = new HashSet<ProjectFile>();
        }

        public int SourceTypeId { get; set; }
        public string SourceTypeName { get; set; }

        public ICollection<ProjectFile> ProjectFiles { get; set; }
    }
}
