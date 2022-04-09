using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using DataAccess.DTO;
using System.Security.Claims;
using RestSharp;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataService.Controllers
{
    public class ScheduleDetail
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class Schedule
    {
        public string id { get; set; }
        public ScheduleDetail project { get; set; }
        public ScheduleDetail workflow { get; set; }
        public DateTime nextRunTime { get; set; }
        public DateTime nextScheduleTime { get; set; }
        public DateTime disabledAt { get; set; }
    }

    public class Schedules
    {
        public Schedule[] schedules { get; set; }
    }

    public class Attempt
    {
        public string id { get; set; }
        [JsonProperty("params")]
        public object param { get; set; }
        public bool success { get; set; }
        public bool done { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime finishedAt { get; set; }
    }
    public class Detail
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class Session
    {
        public string id { get; set; }
        public Detail project { get; set; }
        public Detail workflow { get; set; }
        public string sessionUuid { get; set; }
        public DateTime sessionTime { get; set; }
        public Attempt lastAttempt { get; set; }
    }
    public class Sessions
    {
        public Session[] sessions { get; set; }
    }
    public class Error
    {
        public string stackTrace { get; set; }
        public string message { get; set; }
    }

    public class Task
    {
        public string id { get; set; }
        public string fullName { get; set; }
        public string parentId { get; set; }
        public object config { get; set; }
        public string[] upstreams { get; set; }
        public string state { get; set; }
        public string cancelRequested { get; set; }
        public object exportParams { get; set; }
        public object storeParams { get; set; }
        public object stateParams { get; set; }
        public string updatedAt { get; set; }
        public string retryAt { get; set; }
        public string startedAt { get; set; }
        public Error error { get; set; }
        public bool isGroup { get; set; }

    }
    public class Tasks
    {
        public Task[] tasks { get; set; }
    }

    //public class AttemptResult
    //{
    //    public DateTime AttemptTime { get; set; }
    //    public DateTime UpdatedAt { get; set; }
    //    public string Result { get; set; }
    //    public List<SessionResult> AttemptResults { get; set; }
    //}

    public class SessionResult
    {
        public DateTime sessionTime { get; set; }
        public DateTime AttemptTime { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Result { get; set; }
        public List<NodeResult> NodeResults { get; set; }
    }
    public class NodeResult
    {
        public int id { get; set; }
        public string NodeName { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Result { get; set; }
        public string DetailedMessage { get; set; }

    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkflowAttemptsController : ControllerBase
    {
        private readonly IRepository _repository;

        private readonly IMapper _mapper;

        private readonly ILogger<WorkflowAttemptsController> _logger;
        private string _queryServiceString = null;
        public WorkflowAttemptsController(IRepository repo, IMapper mapper, ILogger<WorkflowAttemptsController> logger, IOptions<ConnectionStringsConfig> optionsAccessor)
        {
            _repository = repo;
            _mapper = mapper;
            _logger = logger;
            _queryServiceString = optionsAccessor?.Value.QueryServiceConnection;
        }

        //// GET: api/WorkflowVersion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workflowProjecId"></param>
        /// <param name="workflowVersionId"></param>
        /// <returns></returns>
        [HttpGet("{workflowProjecId}/{workflowVersionId}")]
        public async Task<ActionResult<WorkflowSessionAttemptDTO[]>> GetWorkflowAttempts( int workflowProjecId, int workflowVersionId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var result = await _repository.GetWorkflowVersionAttempts(userId, workflowProjecId, workflowVersionId );
                var workflowAttemptDTOs = _mapper.Map<WorkflowSessionAttemptDTO[]>(result);
                if (workflowAttemptDTOs == null)
                {
                    return this.StatusCode(StatusCodes.Status204NoContent, "");
                }
                else
                {
                    return workflowAttemptDTOs;
                }
            }
            catch(Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,"");
            }
        }
        /// <summary>
        /// Get attempt logs
        /// </summary>
        /// <param name="workflowSessionAttemptId"></param>
        /// <param name="workflowProjecId"></param>
        /// <returns></returns>
        [HttpGet("[action]/{workflowSessionAttemptId}/{workflowProjecId}/{workflowVersionId}")]
        public async Task<ActionResult<WorkflowSessionLogDTO[]>> GetAttemptLog(int workflowSessionAttemptId, int workflowProjecId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var result = await _repository.GetWorkflowVersionLogs(userId, workflowProjecId, workflowSessionAttemptId);
                var workflowSessionDTOs = _mapper.Map<WorkflowSessionLogDTO[]>(result);
                return new OkObjectResult(workflowSessionDTOs);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "");
            }
        }

        /// <summary>
        /// post new worklow attempt
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>

        [HttpPost()]//"[action]"
        public async Task<IActionResult> PostWorkflowAttempt( [FromBody] WorkflowSessionAttemptDTO project)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var workflowAttempt = _mapper.Map<WorkflowSessionAttempt>(project);
            if (workflowAttempt != null)
            {
                workflowAttempt.UserId = userId;
                
               var workflowVer = await _repository.AddWorkflowAttempt( (int)workflowAttempt.WorkflowProjectId, userId, workflowAttempt.WorkflowVersionId);
                      
               project.WorkflowSessionAttemptId = workflowVer.WorkflowSessionAttemptId;

                return CreatedAtAction("GetWorkflowAttempts", new { id = workflowVer.WorkflowSessionAttemptId }, project);

            }
            return StatusCode(StatusCodes.Status404NotFound);
        }

        [HttpPost("{workflowProjectId}/{workflowVersionId}")]//[action]
        public async Task<IActionResult> PostWorkflowAttempt([FromRoute] int workflowProjectId, [FromRoute] int workflowVersionId)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var workflowAttempt = await _repository.AddWorkflowAttempt((int)workflowProjectId, userId, workflowVersionId);
            if (workflowAttempt != null)
            {
                return new OkObjectResult(workflowAttempt);
            }
            return StatusCode(StatusCodes.Status404NotFound);
        }
        ///
        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateWorkflowAttempt([FromBody] WorkflowSessionAttemptDTO project)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var workflowAttempt = _mapper.Map<WorkflowSessionAttempt>(project);
            if (workflowAttempt != null)
            {
               var attempt = await _repository.UpdateWorkflowAttempt(workflowAttempt.WorkflowSessionAttemptId, (int)workflowAttempt.WorkflowProjectId, userId, workflowAttempt.WorkflowVersionId, workflowAttempt.Result);
               workflowAttempt.Result = attempt.Result;
               workflowAttempt.EndDate = DateTime.Now;
               return CreatedAtAction("GetWorkflowAttempts", new { id = workflowAttempt.WorkflowSessionAttemptId }, project);

            }
            return StatusCode(StatusCodes.Status404NotFound);
        }
        /// <summary>
        /// Updateworkflow with result
        /// </summary>
        /// <param name="workflowSessionAttemptId"></param>
        /// <param name="workflowProjectId"></param>
        /// <param name="workflowVersionId"></param>
        /// <param name="Result"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("[action]/{workflowSessionAttemptId}/{workflowProjectId}/{workflowVersionId}/{Result}")]
        public async Task<IActionResult> UpdateWorkflowAttempt(int workflowSessionAttemptId, int workflowProjectId, int workflowVersionId , string Result)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var attempt = await _repository.UpdateWorkflowAttempt(workflowSessionAttemptId, workflowProjectId, userId, workflowVersionId, Result);
            if (attempt != null)
            {
                attempt.Result = attempt.Result;
                attempt.EndDate = DateTime.Now;
                var attemptDTO = _mapper.Map<WorkflowSessionAttemptDTO>(attempt);
                return CreatedAtAction("GetWorkflowAttempts", new { id = attempt.WorkflowSessionAttemptId }, attemptDTO );

            }
            return StatusCode(StatusCodes.Status404NotFound);
        }
        /// <summary>
        /// Post new attempt log 
        /// </summary>
        /// <param name="workflowSessionAttemptId"></param>
        /// <param name="workflowProjectId"></param>
        /// <param name="workflowVersionId"></param>
        /// <param name="Log"></param>
        /// <returns></returns>
        [AllowAnonymous] 
        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateWorkflowAttemptLog([FromRoute]int workflowSessionAttemptId, int workflowProjectId, int workflowVersionId, string Log)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var attempt = await _repository.UpdateWorkflowAttemptLog(workflowSessionAttemptId, workflowProjectId, userId, workflowVersionId, Log);
            if (attempt != null)
            {
                return new OkResult();
            }
            return StatusCode(StatusCodes.Status204NoContent);

        }


        [AllowAnonymous]
        [HttpPost("[action]/{externalAttemptId}/{externalWorkflowId}")]
        public async Task<IActionResult> UpdateWorkflowTestAttempt([FromRoute]int externalAttemptId, int externalWorkflowId, [FromBody] ResultData resultData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Console.WriteLine($"UpdateWorkflowTestAttempt { externalAttemptId } { externalWorkflowId} {resultData.Result}");
            var ret = await _repository.UpdateWorkflowTestResult(externalAttemptId, externalWorkflowId, 0, resultData.Result, resultData.Log);
            if (ret != null)
            Console.WriteLine($"UpdateWorkflowTestAttempt { externalAttemptId } { externalWorkflowId} {resultData.Result} {ret.Result} Done");
            else
                Console.WriteLine($"UpdateWorkflowTestAttempt { externalAttemptId } { externalWorkflowId} {resultData.Result} ret null");
            return new OkResult();
        }
        [AllowAnonymous]
        [HttpPost("[action]/{externalAttemptId}/{externalWorkflowId}")]
        public async Task<IActionResult> UpdateWorkflowAttempt([FromRoute]int externalAttemptId, int externalWorkflowId, [FromBody] ResultData resultData)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Console.WriteLine($"UpdateWorkflowAttempt { externalAttemptId } { externalWorkflowId} {resultData.Result}");
            var ret = await _repository.UpdateWorkflowAttempt(externalAttemptId, externalWorkflowId,  0,  resultData.Result, resultData.Log);
            //var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token");
            var tables = await _repository.GetWorkflowOutputTable((int)ret.WorkflowProjectId, ret.WorkflowVersionId, ret.UserId);
            
            var url = $"{_queryServiceString}/api/search/GetTableInfo";
            var restClient = new RestClient(url);
            var requestTable = new RestRequest(Method.POST);
             requestTable.AddHeader("Accept", "application/json");
            //requestTable.AddHeader("Content-Type", "application/json");
            if (tables == null || tables.Length == 0) return Ok("no tablle found");
            List<string> tableNames = new List<string>();
            foreach (var table in tables)
            {
                Console.WriteLine(table.TableName);
                tableNames.Add(table.TableName);
            }
            var jsonObj = new string[] { tables[0].TableName };
            var json = JsonConvert.SerializeObject(jsonObj);
            Console.WriteLine(json);
            //var requestTable = new RestRequest(url, Method.GET, DataFormat.Json);
            // var json = JsonConvert.SerializeObject(tableNames.ToArray());
           // requestTable.AddJsonBody(tableNames.ToArray());
            //Console.WriteLine(json);
            requestTable.AddParameter("application/json", json, ParameterType.RequestBody);
            // requestTable.AddHeader("Authorization", authorization);
            IRestResponse responseTable = await restClient.ExecuteAsync(requestTable);

            if (responseTable != null)
            {
                //Console.WriteLine(responseTable.Request.);
                Console.WriteLine(responseTable.Content);
                //updating modelmetadat
                var ListData = JsonConvert.DeserializeObject<TableInfo[]>(responseTable.Content);
                var tablesinfoList = ListData.ToList().GroupBy(x => x.TableName).ToList();
                bool retUpdate = false;
                foreach (var tableInfo in tablesinfoList)
                {
                    Console.WriteLine(tableInfo.Key);
                    var tableUsed = tables.ToList().Where(x => x.TableName == tableInfo.Key).SingleOrDefault();
                    if (tableUsed != null)
                    {
                        Console.WriteLine("tableUsed" + tableUsed.ToString());
                        retUpdate = await _repository.AddWorkflowModelMetaData(ret.UserId, ListData.ToList(), tableUsed.WorkflowVersionId, tableUsed.WorkflowOutputModelId);
                    }
                }

                if (retUpdate == true)
                {
                    return new OkResult();
                }
            }
            return new OkResult(); ;//StatusCode(StatusCodes.Status206PartialContent);

        }
        [AllowAnonymous]
        [HttpPost("[action]/{projectId}")]//[action]
        public async Task<IActionResult> ProjecTrigger([FromRoute] int ProjectId, [FromBody] IngestedData ingestedData)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            //check modelid against workflow_moniyor
            Console.WriteLine($"ProjecTrigger|{ProjectId} {ingestedData.SchemaId} {ingestedData.ModelId}");
            var workflowMonitor = await _repository.GetWorkflowMonitor(0, ingestedData.ModelId);
            if (workflowMonitor != null)
            {
                Console.WriteLine($"ProjecTrigger|GetWorkflowMonitor {ProjectId} {workflowMonitor.ModelId} {workflowMonitor.WorkflowMonitorId}");
            }
            if (workflowMonitor == null) return Ok("Not involved in workflow");
            //insert into state table to get state id if not runing 
            WorkflowAutomationState workflowState = await _repository.GetWorkflowAutomationState(workflowMonitor.WorkflowVersionId);
           
            if ((workflowState == null) || (workflowState != null && workflowState.StateStatus != true))
            {
                 workflowState = await _repository.AddWorkflowAutomationState(workflowMonitor.WorkflowVersionId);
            }
            if (workflowState != null)
            {
                Console.WriteLine($"ProjecTrigger|workflowState {ProjectId} {workflowState.StateStatus} {workflowState.WorkflowAutomationStateId}");
            }
            await _repository.AddWorkflowStateModelMap(workflowState.WorkflowAutomationStateId, workflowState.WorkflowVersionId, ingestedData.jobId, ingestedData.ModelId);
            var monitoredModels = await _repository.GetWorkflowMonitor(workflowState.WorkflowVersionId);
            var modelList = monitoredModels.Select(x => x.ModelId).ToList();

            bool checkModelsUsed = await _repository.CheckWorkflowStateModelMap(workflowState.WorkflowAutomationStateId, workflowState.WorkflowVersionId, ingestedData.jobId, modelList.ToArray());
            if (workflowState != null)
            {
                Console.WriteLine($"ProjecTrigger|CheckWorkflowStateModelMap {ProjectId} {checkModelsUsed} {workflowState.WorkflowAutomationStateId}");
            }
            //if all models are reported we can start an attempt
            if ( checkModelsUsed == true)
            {
               var jobs =  await _repository.GetJobIdfromWorkflowStateModelMap(workflowState.WorkflowAutomationStateId);
                //mak eentry into workflow automation table
                var automation = await _repository.AddWorkflowAutomation(workflowState.WorkflowAutomationStateId, workflowMonitor.WorkflowProjectId, workflowState.WorkflowVersionId);
                
                Console.WriteLine($"ProjecTrigger|AddWorkflowAutomation {ProjectId} {checkModelsUsed} {automation.WorkflowAutomationStateId}");
                
                //call RUN attempt
                RunHelper runner = new RunHelper();
                var ret = await runner.Run(ingestedData.UserId, workflowMonitor.WorkflowProjectId, workflowState.WorkflowVersionId, _repository, jobs);
                Console.WriteLine("Run Called");
                return StatusCode(StatusCodes.Status200OK);
            }
            return StatusCode(StatusCodes.Status404NotFound);
        }


        [AllowAnonymous]
        [HttpGet("[action]/{workflowTestId}")]//[action]
        public async Task<IActionResult> GetTestLog([FromRoute] int workflowTestId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                List<SessionResult> sessionResults = new List<SessionResult>();
                var workflowtest = await _repository.GetWorkflowTest(workflowTestId);
                if (workflowtest != null && workflowtest.ExternalAttemptId > 0)
                {
                    var client = new RestClient();
                    SessionResult sessionResult = new SessionResult();
                    sessionResult.AttemptTime = workflowtest.CreatedOn;
                    sessionResult.UpdatedAt = workflowtest.UpdatedOn;
                    sessionResult.NodeResults = new List<NodeResult>();
                    string urlTasks = "http://idapt.duckdns.org:65432/api/attempts/{0}/tasks";
                    int attemptId = workflowtest.ExternalAttemptId;
                    var requestRest = new RestRequest(string.Format(urlTasks, attemptId));
                    var response = await client.ExecuteAsync(requestRest);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var json = response.Content;//reader.ReadToEnd();
                        var tasks = JsonConvert.DeserializeObject<Tasks>(json);
                        int ind = 1;
                        sessionResult.sessionTime = workflowtest.CreatedOn;
                        foreach (var task in tasks.tasks)
                        {
                            if (task.isGroup == true) continue;
                            NodeResult nodeResult = new NodeResult();
                            DateTime.TryParse(task.startedAt, out DateTime t1);
                            nodeResult.StartedAt = t1;
                            DateTime.TryParse(task.updatedAt, out DateTime t2);
                            nodeResult.UpdatedAt = t2;
                            nodeResult.NodeName = task.fullName.Split('.').LastOrDefault();
                            nodeResult.id = ind++;
                            if (task.state.Contains("error"))
                            {
                                if (task.error != null)
                                {
                                    nodeResult.Result = "Error";
                                    sessionResult.Result = "Error";
                                    nodeResult.DetailedMessage = task.error.message;
                                }
                            }
                            else if (task.state.Contains("blocked"))
                            {
                                nodeResult.Result = "blocked";
                            }
                            else
                            {
                                if (task.fullName.Contains("failure-alert"))
                                {
                                    sessionResult.Result = "Failure";
                                    nodeResult.Result = "Notified";
                                }
                                else
                                {
                                    sessionResult.Result = "Success";
                                    nodeResult.Result = "Success";
                                }
                            }
                            sessionResult.NodeResults.Add(nodeResult);
                        }
                    }
                    sessionResults.Add(sessionResult);
                }
               return Ok(sessionResults.ToArray());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        [AllowAnonymous]
        [HttpGet("[action]/{externalprojectId}/{externalWorkflowId}/{externalAttemptId}")]//[action]
        public async Task<IActionResult> GetSessionLog([FromRoute] int externalprojectId, [FromRoute] int externalWorkflowId, [FromRoute] int externalAttemptId)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                string attempt_id = externalAttemptId.ToString();
                string urlsession = "http://idapt.duckdns.org:65432/api/projects/{0}/sessions";

                string urlTasks = "http://idapt.duckdns.org:65432/api/attempts/{0}/tasks";
                var client = new RestClient();
                var requestRest = new RestRequest(string.Format(urlsession, externalprojectId));
                IRestResponse response = client.Execute(requestRest);

                //using (System.IO.StreamReader reader = new StreamReader(@"sessions.json"))
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string json = response.Content;///reader.ReadToEnd();
                    var s = JsonConvert.DeserializeObject<Sessions>(json);

                    var ourSession = s.sessions.Where(x => x.workflow.id == externalWorkflowId.ToString() && x.lastAttempt.id == attempt_id);
                    List<SessionResult> sessionResults = new List<SessionResult>();
                    
                    foreach (var sess in ourSession)
                    {
                        SessionResult sessionResult = new SessionResult();
                        requestRest = new RestRequest(string.Format(urlTasks, sess.lastAttempt.id));
                        response = await client.ExecuteAsync(requestRest);
                        sessionResult.AttemptTime = sess.sessionTime;
                        sessionResult.UpdatedAt = sess.lastAttempt.finishedAt;



                        sessionResult.NodeResults = new List<NodeResult>();
                        //using (System.IO.StreamReader reader = new StreamReader(@"tasks.json"))
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            json = response.Content;//reader.ReadToEnd();
                            var tasks = JsonConvert.DeserializeObject<Tasks>(json);
                            int ind = 1;
                            sessionResult.sessionTime = sess.sessionTime;
                            foreach (var task in tasks.tasks)
                            {
                                if (task.isGroup == true) continue;
                                NodeResult nodeResult = new NodeResult();
                                DateTime.TryParse(task.startedAt, out DateTime t1);
                                nodeResult.StartedAt = t1;
                                DateTime.TryParse(task.updatedAt, out DateTime t2);
                                nodeResult.UpdatedAt = t2;
                                nodeResult.NodeName = task.fullName.Split('.').LastOrDefault();
                                nodeResult.id = ind++;
                                if (task.state.Contains("error"))
                                {
                                    if (task.error != null)
                                    {
                                        nodeResult.Result = "Error";
                                        sessionResult.Result = "Error";
                                        nodeResult.DetailedMessage = task.error.message;
                                    }
                                }
                                else if (task.state.Contains("blocked"))
                                {
                                    nodeResult.Result = "blocked";
                                }
                                else
                                {
                                    if (task.fullName.Contains("failure-alert"))
                                    {
                                        sessionResult.Result = "Failure";
                                        nodeResult.Result = "Notified";
                                    }
                                    else
                                    {
                                        sessionResult.Result = "Success";
                                        nodeResult.Result = "Success";
                                    }
                                }
                                sessionResult.NodeResults.Add(nodeResult);
                            }
                        }
                        sessionResults.Add(sessionResult);
                    }
                    return Ok(sessionResults.ToArray());
                }

                return StatusCode(StatusCodes.Status204NoContent);

            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }



            ///DELETE to be added
            ///
            /// 

            //// GET: api/WorkflowProjects/5
            //[HttpGet("{id}")]
            //public async Task<IActionResult> GetWorkflowProject([FromRoute] int id)
            //{
            //    if (!ModelState.IsValid)
            //    {
            //        return BadRequest(ModelState);
            //    }

            //    var workflowProject = await _context.WorkflowProjects.FindAsync(id);

            //    if (workflowProject == null)
            //    {
            //        return NotFound();
            //    }

            //    return Ok(workflowProject);
            //}

            //// PUT: api/WorkflowProjects/5
            //[HttpPut("{id}")]
            //public async Task<IActionResult> PutWorkflowProject([FromRoute] int id, [FromBody] WorkflowProject workflowProject)
            //{
            //    if (!ModelState.IsValid)
            //    {
            //        return BadRequest(ModelState);
            //    }

            //    if (id != workflowProject.WorkflowProjectId)
            //    {
            //        return BadRequest();
            //    }

            //    _context.Entry(workflowProject).State = EntityState.Modified;

            //    try
            //    {
            //        await _context.SaveChangesAsync();
            //    }
            //    catch (DbUpdateConcurrencyException)
            //    {
            //        if (!WorkflowProjectExists(id))
            //        {
            //            return NotFound();
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }

            //    return NoContent();
            //}

            //// POST: api/WorkflowProjects
            //[HttpPost]
            //public async Task<IActionResult> PostWorkflowProject([FromBody] WorkflowProject workflowProject)
            //{
            //    if (!ModelState.IsValid)
            //    {
            //        return BadRequest(ModelState);
            //    }

            //    _context.WorkflowProjects.Add(workflowProject);
            //    await _context.SaveChangesAsync();

            //    return CreatedAtAction("GetWorkflowProject", new { id = workflowProject.WorkflowProjectId }, workflowProject);
            //}

            //// DELETE: api/WorkflowProjects/5
            //[HttpDelete("{id}")]
            //public async Task<IActionResult> DeleteWorkflowProject([FromRoute] int id)
            //{
            //    if (!ModelState.IsValid)
            //    {
            //        return BadRequest(ModelState);
            //    }

            //    var workflowProject = await _context.WorkflowProjects.FindAsync(id);
            //    if (workflowProject == null)
            //    {
            //        return NotFound();
            //    }

            //    _context.WorkflowProjects.Remove(workflowProject);
            //    await _context.SaveChangesAsync();

            //    return Ok(workflowProject);
            //}

            //private bool WorkflowProjectExists(int id)
            //{
            //    return _context.WorkflowProjects.Any(e => e.WorkflowProjectId == id);
            //}
        }

    public class ResultData
    {
        public string Result{get;set;}
        public string Log { get; set; }
    }
}