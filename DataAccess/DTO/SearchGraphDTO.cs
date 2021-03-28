using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DTO
{
    [AutoMap(typeof(Models.SearchGraph))]
    public class SearchGraphDTO
    {
        public int SearchGraphId { get; set; }

        public  string GraphDescription { get; set; }
    }

    public class GraphData
    {
        public string Xaxis { get; set; }
        public string Yaxis { get; set; }

    }
}
