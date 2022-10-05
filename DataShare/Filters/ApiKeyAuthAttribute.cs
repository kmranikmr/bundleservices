using AutoMapper.Configuration;
using Common.Utils;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataShareService.Filters
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        public const string ApiKeyHeaderName = "api-key";
        public string workflowserver = "";
        //public ApiKeyAuthAttribute(IOptions<DatabaseConfig> doptions)
        //{
        //    workflowserver = doptions.Value.workflowServer;
        //}
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)//, IOptions<DatabaseConfig> doptions)
        {
            //before
            Console.WriteLine("onactionexeution2");
            if ( !context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey) )
            {
                Console.WriteLine("api key nt got");
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
           // var tmpSource = ASCIIEncoding.ASCII.GetBytes(potentialApiKey);
           // var tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
            Console.WriteLine("let through");
            string hasheUserKey = Api.GenerateHashedKey(potentialApiKey);
            Console.WriteLine(potentialApiKey);
            var memoryCache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
            if(memoryCache.TryGetValue("workflowServer", out string connVal))
            {
                workflowserver = connVal;
            }
            // bool test = Api.MatchHashedKey(hasheUserKey, "$s2$16384$8$1$sQL6ul0n7aJb / aeozbvqzCQUDh55wL1kwYEQsNO81G8 =$0ayJAYu3vbBSXjLtPK8hmw1DFlATV0REStJvqy9mGI0=");
            var url = $"http://{workflowserver}:6002/api/UserApi/Validate";//change this
            var restClient = new RestClient(url);
            var requestTable = new RestRequest(Method.POST);
            requestTable.AddHeader("Accept", "application/json");
            requestTable.AddHeader("api-key", potentialApiKey);
           // var json = JsonConvert.SerializeObject(context.HttpContext.Request.Path.Value);
           /// requestTable.AddParameter("application/json", json, ParameterType.RequestBody);
            
            IRestResponse responseTable = await restClient.ExecuteAsync(requestTable);
            if ( responseTable == null)
            {
                Console.WriteLine("unauth - responsetable null");
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
            if ( !responseTable.Content.Contains("1") && !responseTable.Content.Contains("true"))
            {
                Console.WriteLine(responseTable.Content);
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
            await next();
        }
    }
}
