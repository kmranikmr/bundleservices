using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(SearchHistory))]
    public class SearchHistoryDTO
    {
        public string SearchQuery { get; set; }

        public int ProjectId { get; set; }
        public int SchemaVersionId { get; set; }
        public int WriterId { get; set; }

        public int UserId { get; set; }
        public int SearchHistoryId { get; set; }

        public SearchGraphDTO[] SearchGraphs { get; set; }
        public string FriendlyName { get; set; }
        public SearchHistoryDTO()
        {
           // List<SearchGraphDTO> ss = new List<SearchGraphDTO>();
           // ss.Add(new SearchGraphDTO() { GraphDescription = "tt" });
           // SearchGraphs = ss.ToArray();
        }
    }
}
