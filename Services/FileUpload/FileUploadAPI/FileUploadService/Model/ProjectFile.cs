using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadService.Model
{


    public class ProjectFile
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
    }

    public class ProjectFileResponse
    {
        public int UserId { get; set; }
        public int FileId { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedTime { get; set; } 
        public string Status { get; set; }
    }

}
