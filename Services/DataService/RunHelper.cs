using DataAccess.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataService
{
    public class RunHelper
    {

        public RunHelper()
        {

        }

        public async Task<bool> Run( int  userId, int projectWorkflowId, int workflowVersionId, IRepository repository, int[] jobIds)
        {
            Console.WriteLine($"Run|GetWorkflowVersion Start ");
            var versionInfo = await repository.GetWorkflowVersion(userId, workflowVersionId);
            Console.WriteLine($"Run|GetWorkflowVersion Done  ");
            var attempt = await repository.AddWorkflowAttempt(projectWorkflowId, userId, workflowVersionId);
          
            if (attempt != null)
            {
                Console.WriteLine($"Run|AddWorkflowAttempt Done {attempt.WorkflowSessionAttemptId} ");
                var client = new RestClient($"http://idapt.duckdns.org:8080/workflow/{versionInfo.ExternalWorkflowId}/run");//$"https://h6661pykt9.execute-api.us-east-1.amazonaws.com/dev/workflow/{versionInfo.ExternalWorkflowId}/run");
                var requestRest = new RestRequest(Method.POST);
                requestRest.AddHeader("Accept", "application/json");
                //requestRest.AddJsonBody(workflow);
                var jsonObj = new
                {
                    workflowId = versionInfo.ExternalWorkflowId.ToString(),
                    revision = versionInfo.VersionNumber.ToString(),
                    workflowAttemptId = attempt.WorkflowSessionAttemptId,
                    sessionIds = string.Join(",", jobIds)
                };
                Console.WriteLine($"Run|Json Body ExternalWorkflowID {versionInfo.ExternalWorkflowId} ");
                Console.WriteLine($"Run|Json Body revision {versionInfo.VersionNumber} ");
                Console.WriteLine($"Run|Json Body workflowAttemptId {attempt.WorkflowSessionAttemptId} ");
                var json = JsonConvert.SerializeObject(jsonObj);
                requestRest.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(requestRest);

                Console.WriteLine($"Run|Json Body ExecuteAsync Content{response.Content} ");
                //we will get the server atemptid
                //we will try get that externbal attempt id back...use current workflowproject/versionid and local attemptid to update with tht externnal attemptid
                RestSessionAttempt reponseObject = JsonConvert.DeserializeObject<RestSessionAttempt>(response.Content);
                if (reponseObject != null)
                {
                    Console.WriteLine($"Run|Json Body Deserialiized {reponseObject.id}");
                }
                else
                {
                    Console.WriteLine($"Run|Json Body Deserialiized Null");
                }
                if (reponseObject != null)
                {
                    Console.WriteLine($"Run|reponseObject not null");
                    int.TryParse(reponseObject.id, out int aId);
                    attempt.ExternalAttemptId = aId;
                    attempt.ExternalWorkflowId = (int)versionInfo.ExternalWorkflowId;

                    await repository.SaveChangesAsync();
                    Thread.Sleep(1000);
                    Console.WriteLine($"Run|UpdateWorkflowAttempt with procesing");
                    var updatedAttempt = await repository.UpdateWorkflowAttempt(aId, (int)versionInfo.ExternalWorkflowId, userId, "Processing", "");
                    Console.WriteLine($"Run|UpdateWorkflowAttempt with procesing done");
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}
