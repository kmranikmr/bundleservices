using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DataAccess.DTO;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace DataService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkflowTestController : ControllerBase
    {

        private readonly IRepository _repository;

        private readonly IMapper _mapper;

        private string _connectionstring = null;

        private readonly ILogger<WorkflowTestController> _logger;

        private string _workflowConnectionString = null;

        public WorkflowTestController(IRepository repo, IMapper mapper, IOptions<ConnectionStringsConfig> optionsAccessor, ILogger<WorkflowTestController> logger)
        {
            _repository = repo;
            _mapper = mapper;
            _connectionstring = optionsAccessor.Value.DefaultConnection;
            _logger = logger;
            _workflowConnectionString = optionsAccessor.Value.WorkflowConnection;
        }

        [HttpGet("{workflowProjecId}/{workflowVersionId}")]
        public async Task<ActionResult<WorkflowTestDTO[]>> GetWorkflowTests(int workflowProjecId, int workflowVersionId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var workflowTests = await _repository.GetWorkflowTest(userId, workflowProjecId, workflowVersionId);
                var workflowTestDTo= _mapper.Map<WorkflowTestDTO[]>(workflowTests);
                return workflowTestDTo;
            }
            catch (Exception ex)
            {
                return Ok(false);
            }
        }

        private async void backgroundWorker1_DoWork(int userId, int workflowTestId, int workflowVersionId, int projectWorkflowId, IRepository _repository)// object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            string connectionString = _connectionstring;
            // Console.WriteLine("backgroind");
            var options = SqlServerDbContextOptionsExtensions.UseSqlServer<DAPDbContext>(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
            var dbContext = new DAPDbContext(options);
            IRepository repo = new Repository(dbContext, null);

            Thread.Sleep(1000);

            var pubResult = await repo.UpdateWorkflowTestResult(workflowTestId, workflowTestId, userId, "processing", "");

            Thread.Sleep(1000);
            pubResult = await repo.UpdateWorkflowTestResult(workflowTestId, workflowTestId, userId, "success", "no issues found");
          

        }

        [HttpPost("[action]/{projectWorkflowId}/{workflowVersionId}/{isSample:bool=false}")]
        public async Task<IActionResult> Test([FromRoute] int projectWorkflowId, int workflowVersionId, [FromBody]  WorkflowTestDTO workflowTestDTO, bool isSample = false)
        {
            bool mock = false;
            Console.WriteLine("Test");
            if (!ModelState.IsValid)
            {
                 Console.WriteLine("Test - ModelState Not Valid");
                return BadRequest(ModelState);
            }
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

           
             Console.WriteLine("Test - User ID ");

            //version == 0 then first create

            var workflowTest = _mapper.Map<WorkflowTest>(workflowTestDTO);

             Console.WriteLine("Test - MapworkflowDTo");
            if (workflowVersionId == 0)
            {
                var workflowVersion = new WorkflowVersion
                {
                    UserId = userId,
                    WorkflowProjectId = projectWorkflowId,
                    WorkflowJson = workflowTestDTO.WorkflowJson,
                    WorkflowPropertyJson = workflowTestDTO.WorkflowPropertyJson
                };

               Console.WriteLine("Test - workflowversionid is 0 ");
                var workflowVer = await _repository.AddWorkFlowVersion(userId, projectWorkflowId, workflowVersion);
                workflowTestDTO.WorkflowVersionId = workflowVer.WorkflowVersionId;
                workflowTestDTO.VersionNumber = workflowVer.VersionNumber;
                workflowTestDTO.UserId = userId;
                
            }


            //create a test
            workflowTest = _repository.AddWorkflowTest(userId, projectWorkflowId, workflowTestDTO.WorkflowVersionId, workflowTestDTO.WorkflowJson, workflowTestDTO.WorkflowPropertyJson);
             Console.WriteLine("Test - create a Test");
            Thread.Sleep(1000);
            //publish

            var workflowProject = await _repository.GetWorkflowProject(userId, projectWorkflowId);
            string Workflow = "";
             Console.WriteLine("Test -Getworkfllow Project");
            if (!mock)
            {


                List<ProjectQueryDetails> ModelList;
                Workflow = NodeRepository.PrepareWorkflowJson(workflowTest.WorkflowTestId, workflowProject.ExternalProjectName + "_Test",
                                   workflowTest.WorkflowPropertyJson, userId, projectWorkflowId, workflowTest.WorkflowVersionId, _repository, out ModelList, true, isSample);
                 Console.WriteLine("Test - prepareworkflowjson done");
            }


            if (mock)
            {
                workflowTest = await _repository.UpdateWorkflowTestPublish(workflowTest.WorkflowTestId, projectWorkflowId, projectWorkflowId, workflowTest.WorkflowTestId, workflowTest.WorkflowTestId);
                Thread.Sleep(2000);
                //workflowTest.ExternalAttemptId = workflowTest.WorkflowTestId;
                //workflowTest.Result = "Test Created";
                //workflowTest.WorkflowStatusTypeId = 1;
                //workflowTest.UserId = userId;
                //await _repository.SaveChangesAsync();
                //await _repository.UpdateWorkflowTest(userId, workflowTest.WorkflowTestId, projectWorkflowId, projectWorkflowId, workflowTest.WorkflowTestId);
                //Thread.Sleep(1000);
                var workflowtestNew = _mapper.Map<WorkflowTestDTO>(workflowTest);
                Thread.Sleep(1000);

                BackgroundWorker bg = new BackgroundWorker();
                bg.DoWork += (obj, e) => backgroundWorker1_DoWork(userId, workflowTest.WorkflowTestId, workflowVersionId, projectWorkflowId, _repository);
                bg.RunWorkerAsync();
                return Ok(workflowtestNew);
            }
            else
            {
                string workflowUrl = "http://192.168.1.11:8080/workflow";
                if (_workflowConnectionString != null)
                    workflowUrl = _workflowConnectionString;
                Console.WriteLine(workflowUrl+":8080/workflow");
                var client = new RestClient(workflowUrl+":8080/workflow"); //($"https://h6661pykt9.execute-api.us-east-1.amazonaws.com/dev/workflow");
                var requestRest = new RestRequest(Method.POST);
                requestRest.AddHeader("Accept", "application/json");
                //var newJsonBody = new {
                //    workflow = Workflow,
                //    testRun = "true"
                //};
                //var json = JsonConvert.SerializeObject(newJsonBody);

                requestRest.AddParameter("application/json", Workflow, ParameterType.RequestBody);
                _logger.LogInformation($"Publishing|ExecuteAsync Start");
                IRestResponse response = await client.ExecuteAsync(requestRest);
                Console.WriteLine($"Publishing|ExecuteAsync Done{response.Content}");
                PublishResponse reponseObject = JsonConvert.DeserializeObject<PublishResponse>(response.Content);
                if (reponseObject.id != null)
                {
                    int.TryParse(reponseObject.id, out int wProjectid);
                    int.TryParse(reponseObject.workflowId, out int workflowID);
                    workflowTest = await _repository.UpdateWorkflowTestPublish(workflowTest.WorkflowTestId, projectWorkflowId, wProjectid, workflowID);

                }
                //run
            

                client = new RestClient($"{workflowUrl}:8080/workflow/{workflowTest.ExternalWorkflowId}/run");//$"https://h6661pykt9.execute-api.us-east-1.amazonaws.com/dev/workflow/{versionInfo.ExternalWorkflowId}/run");
                requestRest = new RestRequest(Method.POST);
                requestRest.AddHeader("Accept", "application/json");
                requestRest.AddQueryParameter("testRun", "true");
                //requestRest.AddJsonBody(workflow);
                var jsonObj = new
                {
                    workflowId = workflowTest.ExternalWorkflowId.ToString(),
                    revision = workflowTest.WorkflowTestId.ToString(),
                    workflowAttemptId = workflowTest.WorkflowTestId,
                    sessionIds = ""
                };
                Console.WriteLine($"Run|Json Body ExternalWorkflowID {workflowTest.ExternalWorkflowId} ");
                Console.WriteLine($"Run|Json Body revision {workflowTest.WorkflowTestId} ");

                var json = JsonConvert.SerializeObject(jsonObj);
                requestRest.AddParameter("application/json", json, ParameterType.RequestBody);
                response = await client.ExecuteAsync(requestRest);

                Console.WriteLine($"Run|Json Body ExecuteAsync Content{response.Content} ");
                //we will get the server atemptid
                //we will try get that externbal attempt id back...use current workflowproject/versionid and local attemptid to update with tht externnal attemptid
                RestSessionAttempt responseObject = JsonConvert.DeserializeObject<RestSessionAttempt>(response.Content);
                if (responseObject != null)
                {
                    Console.WriteLine($"Run|Json Body Deserialiized {responseObject.id}");
                }
                else
                {
                    Console.WriteLine($"Run|Json Body Deserialiized Null");
                }
                if (responseObject != null)
                {
                    Console.WriteLine($"Run|reponseObject not null");
                    int.TryParse(responseObject.id, out int aId);
                    workflowTest.ExternalAttemptId = aId;
                    workflowTest.Result = "Processing";
                    workflowTest.WorkflowStatusTypeId = 2;
                    await _repository.SaveChangesAsync();
                    Thread.Sleep(1000);
                    var workflowtestNew = _mapper.Map<WorkflowTestDTO>(workflowTest);
                    Console.WriteLine($"Run|UpdateWorkflowAttempt with procesing done");
                    return Ok(workflowtestNew);
                }
                else
                {
                    return Ok(false);
                }
            }


        }
    }
}
