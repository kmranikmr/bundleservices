using System;
using System.Collections.Generic;

namespace FileUploadService.Models
{
    public partial class ProjectFile
    {
        public int ProjectFileId { get; set; }
        public int? ProjectId { get; set; }
        public int UserId { get; set; }
        public int SourceTypeId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string SourceConfiguration { get; set; }
        public DateTime UploadDate { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual SourceType SourceType { get; set; }
    }
}
