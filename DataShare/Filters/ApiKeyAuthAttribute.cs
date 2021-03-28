using Common.Utils;
using Microsoft.AspNetCore.Mvc.Filters;
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
        
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //before
            if ( !context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey) )
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
           // var tmpSource = ASCIIEncoding.ASCII.GetBytes(potentialApiKey);
           // var tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
            string hasheUserKey = Api.GenerateHashedKey(potentialApiKey);
           // bool test = Api.MatchHashedKey(hasheUserKey, "$s2$16384$8$1$sQL6ul0n7aJb / aeozbvqzCQUDh55wL1kwYEQsNO81G8 =$0ayJAYu3vbBSXjLtPK8hmw1DFlATV0REStJvqy9mGI0=");
            var url = $"http://localhost:6002/api/UserApi/Validate";//change this
            var restClient = new RestClient(url);
            var requestTable = new RestRequest(Method.POST);
            requestTable.AddHeader("Accept", "application/json");
            requestTable.AddHeader("api-key", potentialApiKey);
           // var json = JsonConvert.SerializeObject(context.HttpContext.Request.Path.Value);
           /// requestTable.AddParameter("application/json", json, ParameterType.RequestBody);
            
            IRestResponse responseTable = await restClient.ExecuteAsync(requestTable);
            if ( responseTable == null)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
            if ( !responseTable.Content.Contains("1") && !responseTable.Content.Contains("true"))
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
            await next();
        }
    }
}
