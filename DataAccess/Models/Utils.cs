using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;
namespace DataAccess.Models
{
    public static class Utils
    {

        public static async Task<bool> CallCreateView(string query, string viewName, string authorization)
        {
            var url = $"http://localhost:6004/api/Search/CreateView";//change this
            var restClient = new RestClient(url);
            var requestTable = new RestRequest(Method.POST);
            requestTable.AddHeader("Accept", "application/json");
            requestTable.AddHeader("Authorization", authorization);
            requestTable.AddJsonBody(new { query, viewName });
            IRestResponse responseTable = await restClient.ExecuteAsync(requestTable);
            if ( responseTable.Content == "false")
            {
                return false;
            }
            return true;
        }
        public static async Task<string> GetMappedQuery(string query, int projectId, string authorization)
        {
            var url = $"http://localhost:6004/api/Search/MappedQuery/false";//change this
            var restClient = new RestClient(url);
            var requestTable = new RestRequest(Method.POST);
            requestTable.AddHeader("Accept", "application/json");
            // requestTable.AddJsonBody(new { query, projectId });
            var json = JsonConvert.SerializeObject(new { query, projectId });
            requestTable.AddHeader("Authorization", authorization);
            requestTable.AddParameter("application/json", json, ParameterType.RequestBody);

            IRestResponse responseTable = await restClient.ExecuteAsync(requestTable);
            if (responseTable != null)
            {
                return responseTable.Content;
            }
            return "";
        }

        public static string GetShortUrl()
        {
            string URL = "";
            List<int> numbers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            List<char> characters = new List<char>()
                    {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '-', '_'};

            // Create one instance of the Random  
            Random rand = new Random();
            // run the loop till I get a string of 10 characters  
            for (int i = 0; i < 11; i++)
            {
                // Get random numbers, to get either a character or a number...  
                int random = rand.Next(0, 3);
                if (random == 1)
                {
                    // use a number  
                    random = rand.Next(0, numbers.Count);
                    URL += numbers[random].ToString();
                }
                else
                {
                    // Use a character  
                    random = rand.Next(0, characters.Count);
                    URL += characters[random].ToString();
                }
            }
            return URL;
        }
         public static string DetectDelimiter(StreamReader reader)
        {
            // assume one of following delimiters
            var possibleDelimiters = new List<string> { ",", ";", "\t", "|" };

            var headerLine = reader.ReadLine();

            // reset the reader to initial position for outside reuse
            // Eg. Csv helper won't find header line, because it has been read in the Reader
            reader.BaseStream.Position = 0;
            reader.DiscardBufferedData();

            foreach (var possibleDelimiter in possibleDelimiters)
            {
                if (headerLine.Contains(possibleDelimiter))
                {
                    return possibleDelimiter;
                }
            }

            return possibleDelimiters[0];
        }
        public static SearchResult GetFilesPreview(string[] filePath)
        {
            StringBuilder builder = new StringBuilder(1000);
            SearchResult result = new SearchResult();
            result.Results = new List<List<string>>();
            bool isCsv = false;
            foreach (var path in filePath)
            {
                Console.WriteLine(path);
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
                using (BufferedStream bs = new BufferedStream(fs))
                using (TextReader tr = new StreamReader(bs))
                {
                    int count = 0;
                    
                    if (path.Contains(".csv"))
                    {
                     
                        var delimiter = DetectDelimiter(tr as StreamReader);
                       
                        using (var parser = new CsvParser(tr, System.Globalization.CultureInfo.CurrentCulture))
                        {
                            parser.Configuration.Delimiter = delimiter;
                            while (true)
                            {
                                var row = parser.Read();
                                if (row == null || count == 10) break;
                                if ( count == 0 )
                                {
                                    result.Header2 = new List<string>(row);
                                      result.Header = new List<HeaderData>();
                                    result.Header.Add(new HeaderData { Header = delimiter });

                                }
                                else
                                {
                                    result.Results.Add(new List<string>(row));
                                }
                                count++;
                            }
                        }
                        Console.WriteLine(count);
                        /* using (var parser = new CsvParser(tr, System.Globalization.CultureInfo.CurrentCulture))
                        {
                            parser.Configuration.Delimiter = delimiter;
                            while (true)
                            {
                                var row = parser.Read();
                                if (row == null || count == 10) break;
                                if ( count == 0 )
                                {
                                    result.Header2 = new List<string>(row);
                                }
                                else
                                {
                                    result.Results.Add(new List<string>(row));
                                }
                                count++;
                            }
                        }*/
                    }
                }
            }
            return result;
        }
    }
}
