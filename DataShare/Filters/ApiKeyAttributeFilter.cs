using Common.Utils;
using Microsoft.AspNetCore.Mvc.Filters;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataShareService.Filters
{
    public class ApiKeyAttributeFilter : ActionFilterAttribute
    {
        public const string ApiKeyHeaderName = "api-key";
        public void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine("OnactionExecuted");
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
            {
                Console.WriteLine("unauth");
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
            string hasheUserKey = Api.GenerateHashedKey(potentialApiKey);
            //var tmpSource = ASCIIEncoding.ASCII.GetBytes(potentialApiKey);
            //var tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
            var url = $"http://ec2basedservicealb-760561316.us-east-1.elb.amazonaws.com:6002/api/UserApi/Validate";//change this
            Console.WriteLine("found key");
            var restClient = new RestClient(url);
            var requestTable = new RestRequest(Method.POST);
            requestTable.AddHeader("Accept", "application/json");
            requestTable.AddHeader("api-key", hasheUserKey);
            // var json = JsonConvert.SerializeObject(context.HttpContext.Request.Path.Value);
            /// requestTable.AddParameter("application/json", json, ParameterType.RequestBody);

            IRestResponse responseTable = restClient.Execute(requestTable);
            if (responseTable == null)
            {
                Console.WriteLine("responseTable null");
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
            if (!responseTable.Content.Contains("1") && !responseTable.Content.Contains("true"))
            {
                Console.WriteLine(responseTable.Content);
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            
        }
    }
}
