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
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
            string hasheUserKey = Api.GenerateHashedKey(potentialApiKey);
            //var tmpSource = ASCIIEncoding.ASCII.GetBytes(potentialApiKey);
            //var tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
            var url = $"http://localhost:6002/api/UserApi/Validate";//change this
            var restClient = new RestClient(url);
            var requestTable = new RestRequest(Method.POST);
            requestTable.AddHeader("Accept", "application/json");
            requestTable.AddHeader("api-key", hasheUserKey);
            // var json = JsonConvert.SerializeObject(context.HttpContext.Request.Path.Value);
            /// requestTable.AddParameter("application/json", json, ParameterType.RequestBody);

            IRestResponse responseTable = restClient.Execute(requestTable);
            if (responseTable == null)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
            if (!responseTable.Content.Contains("1") && !responseTable.Content.Contains("true"))
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            
        }
    }
}
