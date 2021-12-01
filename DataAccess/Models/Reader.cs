using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class Reader
    {
        public Reader()
        {
            ProjectAutomations = new HashSet<ProjectAutomation>();
            ProjectFiles = new HashSet<ProjectFile>();
            ProjectReaders = new HashSet<ProjectReader>();
        }

        public int ReaderId { get; set; }
        public int ReaderTypeId { get; set; }
        public int UserId { get; set; }
        public string ReaderConfiguration { get; set; }
        public string ConfigurationName { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ReaderType ReaderType { get; set; }
        public virtual ICollection<ProjectAutomation> ProjectAutomations { get; set; }
        public virtual ICollection<ProjectFile> ProjectFiles { get; set; }
        public virtual ICollection<ProjectReader> ProjectReaders { get; set; }
    }
}
