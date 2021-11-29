using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class SearchHistory
    {
        public SearchHistory()
        {
            SearchGraphs = new HashSet<SearchGraph>();
        }

        public int SearchHistoryId { get; set; }
        public string SearchHistoryName { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int WriterId { get; set; }
        public string SearchQuery { get; set; }
        public string ResolvedSearchQuery { get; set; }
        public string Md5 { get; set; }
        public DateTime LastExecutedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string FirendlyName { get; set; }
        public Project Project { get; set; }
        public Writer Writer { get; set; }
        public ICollection<SearchGraph> SearchGraphs { get; set; }
    }
}
