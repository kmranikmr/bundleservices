using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(SearchHistory))]
    public class SavedProjectQueryDTO
    {
        public string SearchQuery { get; set; }
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public int WriterId { get; set; }
        public int UserId { get; set; }
        public int SearchHistoryId { get; set; }
    }

}
