using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(UserApiKeyLog))]
    public class UserApiKeyLogDTO
    {
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
