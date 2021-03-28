using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public partial class UserApiKey
    {
        public int UserApiKeyId { get; set; }
        public int UserId { get; set; }
        public string ApiKey { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string Scope { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
