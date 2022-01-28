using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(UserSharedUrl))]
    public partial class UserSharedUrlDTO
    {
        public int UserSharedUrlId { get; set; }
        public int UserId { get; set; }
        public int? SearchHistoryId { get; set; }
        public int? WorkflowSearchHistoryId { get; set; }
        public string SharedUrl { get; set; }
        public string FriendlyName { get; set; }
    }
    
}
