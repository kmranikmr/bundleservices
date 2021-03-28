using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataShareService.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestSharp;

namespace DataShareService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueryController : ControllerBase
    {
        private string postgresdatabase = "";
        public QueryController(IOptions<DatabaseConfig> doptions )
        {
            postgresdatabase = doptions.Value.postgres;
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
                var url = $"http://localhost:6002/api/UserApi/CheckSharedData/{queryname}";//change this
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
                        url = $"http://127.0.0.1:7891/{postgresdatabase}/public/{queryname}";//change this
                    }
                    else
                    {
                        if (everything.Contains("page"))
                        {
                            url = $"http://127.0.0.1:7891/{postgresdatabase}/public/{queryname}?{everything}";
                            //query?_page=3&_page_size=3
                           // url = url.Replace("query?", "?");
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
                return null;
            }
        }
    }
}