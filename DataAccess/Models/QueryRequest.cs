using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public enum DestinationType
    {
        None,
        RDBMS,
        Mongo,
        ElasticSearch,
        csv

    }
    public class QueryRequest
    {
        public string QueryString { get; set; }
        public DestinationType SearchDestination { get; set; }
        public List<string> ModelNames { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string PageFilter { get; set; }

        public string PageFilterType { get; set; }
        public string PageOffset { get; set; }
        public bool AllRecords { get; set; }
        public List<string> Columns { set; get; }

        public int ProjectId { get; set; }
        public bool isPreview { get; set; }
    }
    public class HeaderData
    {
        public string Header { get; set; }
        public string DataType { get; set; }
    }
    public class SearchResult
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public List<List<string>> Results { get; set; }
        public int ElapsedMilliseconds { get; set; }
        public string PreviousStart { get; set; }
        public string NextStart { get; set; }
        public List<HeaderData> Header { get; set; }
        public List<string> Header2 { get; set; }
    }

}
