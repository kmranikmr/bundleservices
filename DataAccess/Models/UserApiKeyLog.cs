using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class UserApiKeyLog
    {
        public int UserApiKeyLogId { get; set; }
        public int UserApiKeyId { get; set; }
        public int UserId { get; set; }
        public DateTime AccessedOn { get; set; }
        public string AccessedUrl { get; set; }
        public string AccessedUrlBody { get; set; }
        public string Metadata { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
