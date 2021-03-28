using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    public class UserKeyDTO
    {
        public UserKeyDTO(string apiKey)
        {
            ApiKey = apiKey;
        }
        public string ApiKey { get; set; }
    }

    [AutoMap(typeof(UserApiKey))]
    public class UserApiKeyDTO
    {
        public int UserId { get; set; }
        public string ApiKey { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string Scope { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
