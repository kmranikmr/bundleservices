using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class SearchGraph
    {
        public int SearchGraphId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int SearchHistoryId { get; set; }
        public string GraphDescription { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual SearchHistory SearchHistory { get; set; }
    }
}
