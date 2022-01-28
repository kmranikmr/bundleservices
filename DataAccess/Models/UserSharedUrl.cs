using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class UserSharedUrl
    {
        public int UserSharedUrlId { get; set; }
        public int UserId { get; set; }
        public int? SearchHistoryId { get; set; }
        public int? WorkflowSearchHistoryId { get; set; }
        public string SharedUrl { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
    
      
    }
}
