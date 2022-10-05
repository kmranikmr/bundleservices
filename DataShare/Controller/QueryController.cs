using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataShareService.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DataShareService.Controller
{
    public class ConnectionStringsConfig
    {

        public string MilServiceConnection { get; set; }

    }

    [Route("api/[controller]")]
    [ApiController]
    public class QueryController : ControllerBase
    {
        private string postgresdatabase = "";
        private string _server;
        public QueryController(IOptions<DatabaseConfig> doptions )
        {
            _server = "";
            postgresdatabase = doptions.Value.postgres;
            _server = doptions.Value.workflowServer;
        }
        //[ApiKeyAuth]
        //[HttpGet("[action]")]
        //public Task<ActionResult<string>> Test()//(/*[FromRoute]string queryname*/ [FromHeader(Name = "api-key")] string apiKey)
        //{
        //    int ggg = 0;
        //    return null;
        ////}
        /// <summary>
        ///
        /// </summary>
        /// <param name="queryname"></param>
        /// <param name="apiKey"></param>
        /// <param name="everything"></param>
        /// <returns></returns>
        [ApiKeyAuth]
         [HttpGet("[action]/{queryname}/{everything}/{isWorkflow:bool=false}")]
        public async Task<ActionResult<string>>UserData([FromRoute]string queryname,  [FromHeader(Name = "api-key")] string apiKey, string everything)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //string queryname = "kkk";
                Console.WriteLine("query controller");
                var url = $"http://{_server}:6002/api/UserApi/CheckSharedData/{queryname}";//change this
                var restClient = new RestClient(url);
                var requestTable = new RestRequest(Method.GET);
                requestTable.AddHeader("Accept", "application/json");
                requestTable.AddHeader("api-key", apiKey.ToString());
                Console.WriteLine(apiKey);
                // var json = JsonConvert.SerializeObject(api);
                //  requestTable.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse responseTable = await restClient.ExecuteAsync(requestTable);
                if (responseTable.Content.Contains("false") || responseTable == null )
                {
                    Console.WriteLine(url);
                    return "";
                }
                else
                {
                    if (everything == "_all")
                    {
                        url = $"http://idapt.duckdns.org:7891/{postgresdatabase}/public/{queryname}";//change this
                    }
                    else
                    {
                        if (everything.Contains("page"))
                        {
                            url = $"http://idapt.duckdns.org:7891/{postgresdatabase}/public/{queryname}?{everything}";
                            //query?_page=3&_page_size=3
                           // url = url.Replace("query?", "?");
                        }
                        else
                        {
                            url = $"http://idapt.duckdns.org:7891/{postgresdatabase}/public/{queryname}?{everything}";
                        }
                        //url = $"http://127.0.0.1:7891/nwdi_ts/public/{queryname}";
                    }
                    Console.WriteLine(url);
                    restClient = new RestClient(url);
                    requestTable = new RestRequest(Method.GET);
                    requestTable.AddHeader("Accept", "application/json");
                    //  requestTable.AddHeader("api-key", apiKey.ToString());
                    responseTable = await restClient.ExecuteAsync(requestTable);
                    return responseTable.Content;
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex);
                return null;
            }
        }

        //post 

        [ApiKeyAuth]
        [HttpPost("[action]/{queryname}/{everything}/{isWorkflow:bool=false}")]
        public async Task<ActionResult<string>> PostData([FromRoute] string queryname, [FromHeader(Name = "api-key")] string apiKey, string everything, [FromBody] dynamic bdy)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //string queryname = "kkk";
                //Console.WriteLine("query controller");
                //var url = $"http://localhost:6002/api/UserApi/CheckSharedData/{queryname}";//change this
                // var restClient = new RestClient(url);
                // var requestTable = new RestRequest(Method.GET);
                //requestTable.AddHeader("Accept", "application/json");
                //requestTable.AddHeader("api-key", apiKey.ToString());
                Console.WriteLine(apiKey);
                // var json = JsonConvert.SerializeObject(api);
                //  requestTable.AddParameter("application/json", json, ParameterType.RequestBody);
                //IRestResponse responseTable = await restClient.ExecuteAsync(requestTable);
                //if (responseTable.Content.Contains("false") || responseTable == null)
                //{
                //    Console.WriteLine(url);
                //    return "";
                //}
                //else
                if(queryname.Contains("similarity"))
                {
                    string [] half = everything.Split('/');
                    if (half.Length > 0 && half[0] != "")
                    {
                        string dataurl = $"http://idapt.duckdns.org:6012/similarity/{half[0]}";
                        dynamic dataparsed = bdy;// JObject.Parse(bdy);
                        Console.WriteLine(dataparsed.chief_complaints);
                        var jsonOj = new

                        {
                            type = "test",
                            chief_complaints = dataparsed.chief_complaints,
                            blood_tests = dataparsed.blood_tests,
                            blood_results = dataparsed.blood_results,
                            lookup_api = dataparsed.lookup_api,
                             field_name = dataparsed.field_name
                        };
                        var json = JsonConvert.SerializeObject(jsonOj);
                        var restClient = new RestClient(dataurl);
                        var requestTable = new RestRequest(Method.POST);
                        requestTable.AddHeader("Accept", "application/json");
                        requestTable.AddParameter("application/json", json, ParameterType.RequestBody);
                        var response = await restClient.ExecuteAsync(requestTable);
                        if (response != null)
                        {
                            return response.Content;
                        }
                        return null;
                    }
                    return null;
                }
                else
                {
                    //checkeverything has predict then call predict using post
                    string dataurl = $"http://idapt.duckdns.org:7891/{postgresdatabase}/public/{queryname}";
                    var url = $"{_server}:6011/predict";
                    var jsonOj = new
             
                    {
                        urlmodel = dataurl,
                        data = bdy
                    };
                    var json = JsonConvert.SerializeObject(jsonOj);
                    var restClient = new RestClient(url);
                    var requestTable = new RestRequest(Method.POST);
                    requestTable.AddHeader("Accept", "application/json");
                    requestTable.AddParameter("application/json", json, ParameterType.RequestBody);
                    var response  = await restClient.ExecuteAsync(requestTable);
                    if (response != null)
                    {
                        return response.Content;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}
