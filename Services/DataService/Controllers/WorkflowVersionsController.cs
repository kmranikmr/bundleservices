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
using System.ComponentModel;
using Microsoft.Extensions.Options;
using RestSharp;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace DataService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkflowVersionsController : ControllerBase
    {
        private readonly IRepository _repository;

        private readonly IMapper _mapper;

        private string _connectionstring = null;

        private readonly ILogger<WorkflowVersionsController> _logger;

        private string _workflowConnectionString = null;

        public WorkflowVersionsController(IRepository repo, IMapper mapper, IOptions<ConnectionStringsConfig> optionsAccessor, ILogger<WorkflowVersionsController> logger)
        {
            _repository = repo;
            _mapper = mapper;
            _connectionstring = optionsAccessor.Value.DefaultConnection;
            _logger = logger;
            _workflowConnectionString = optionsAccessor.Value.WorkflowConnection;
        }

        //// GET: api/WorkflowVersion
        [HttpGet("{workflowProjecId}")]
        public async Task<ActionResult<WorkflowVersionDTO[]>> GetWorkflowVersions( int workflowProjecId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
               
                var result = await _repository.GetWorkflowVersions(userId, workflowProjecId, false, true );
                var workflowVersionDTOs = _mapper.Map<WorkflowVersionDTO[]>(result);
                //loop for now
                for ( int i = 0; i < workflowVersionDTOs.Length; i++)
                {
                    if (result[i].LastWorkflowSessionAttempt != null)
                    {
                        workflowVersionDTOs[i].Status = result[i].LastWorkflowSessionAttempt.Result;
                    }
                }
                if (workflowVersionDTOs == null)
                {
                    return this.StatusCode(StatusCodes.Status204NoContent, "");
                }
                else
                {
                    return workflowVersionDTOs;
                }
            }
            catch(Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,"");
            }
        }
        ///POST workflow project
        ///worflow version add or update..if workflowversionid is known ( existing) then update
          // POST: api/Project
        [HttpPost()]//"[action]"
        public async Task<IActionResult> PostWorkflowVersion( [FromBody] WorkflowVersionDTO project)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var workflowVersion = _mapper.Map<WorkflowVersion>(project);
            if (workflowVersion != null)
            {
                workflowVersion.UserId = userId;
                
                WorkflowVersion workflowVer = null;
                if (workflowVersion.WorkflowVersionId > 0 )
                {
                    workflowVer = await _repository.UpdateWorkFlowVersion(userId, workflowVersion.WorkflowProjectId, workflowVersion, workflowVersion.WorkflowVersionId);
                }
                else
                {
                    workflowVer = await _repository.AddWorkFlowVersion(userId, workflowVersion.WorkflowProjectId, workflowVersion );

                    project.WorkflowVersionId = workflowVer.WorkflowVersionId;
                    project.VersionNumber = workflowVer.VersionNumber;
                    project.CreatedOn = workflowVer.CreatedOn;
                    project.UpdatedOn = workflowVer.UpdatedOn;
                }

                workflowVer.UserId = userId;
                return CreatedAtAction("GetWorkflowVersions", new { id = workflowVer.WorkflowVersionId }, project);

            }
            return StatusCode(StatusCodes.Status404NotFound);
        }

        ///DELETE to be added
        ///
        /// 
        [HttpDelete("{workflowProjectId}/{workflowVersionId}")]
        public async Task<IActionResult> DeleteProject([FromRoute] int workflowProjectId, [FromRoute] int workflowVersionId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            bool result = await _repository.DeleteWorkflowVersion(userId, workflowProjectId, workflowVersionId);

            if (result == false)
            {
                return NotFound();
            }
            else
            {
                return Ok(workflowVersionId);
            }
        }
        /// <summary>
        /// Update versions table with externalprojectid, externalworkflowid
        /// </summary>
        /// <param name="projectWorkflowId"></param>
        /// <param name="workflowVersionId"></param>
        /// <param name="externalProjectId"></param>
        /// <param name="externalWorkflowVersionId"></param>
        /// <returns></returns>
        [HttpPost("[action]/{projectWorkflowId}/{workflowVersionId}/{externalProjectId}/{externalWorkflowVersionId}")]
        public async Task<IActionResult> UpdateWorkflowVersion([FromRoute] int projectWorkflowId, [FromRoute] int workflowVersionId, 
                                                                    [FromRoute] int externalProjectId, [FromRoute] int externalWorkflowVersionId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            bool result = await _repository.UpdateWorkflowVersion(workflowVersionId, projectWorkflowId, externalProjectId, externalWorkflowVersionId);
            if (result == false)
            {
                return NotFound();
            }
            else
            {
                return Ok(workflowVersionId);
            }
        }

        [HttpPost("[action]/{projectWorkflowId}/{workflowVersionId}/{published}")]
        public async Task<IActionResult> SetWorkflowPublished([FromRoute] int projectWorkflowId, [FromRoute] int workflowVersionId, [FromRoute]int published)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            bool result = await _repository.SetWorkflowPublished(workflowVersionId, projectWorkflowId, published);
            if (result == false)
            {
                return NotFound();
            }
            else
            {
                return Ok(result);
            }
        }
        private async void backgroundWorker1_DoWork(int workflowVersionId, int projectWorkflowId, IRepository _repository)// object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string connectionString = _connectionstring;
           // Console.WriteLine("backgroind");
            var options = SqlServerDbContextOptionsExtensions.UseSqlServer<DAPDbContext>(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
            var dbContext = new DAPDbContext(options);
            IRepository repo = new Repository(dbContext, null);
         //   bool result = await _repository.UpdateWorkflowVersion(workflowVersionId, projectWorkflowId, projectWorkflowId, workflowVersionId);

            await Task.Delay(1000);
           // int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            bool pubResult = await repo.SetWorkflowPublished(workflowVersionId, projectWorkflowId, 1);
           /// Console.WriteLine("backgroind");
        }

        private async void backgroundWorker2_DoWork(int userId, int workflowVersionId, int projectWorkflowId, int WorkflowSessionAttemptId, IRepository _repository)// object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string connectionString = _connectionstring;
            // Console.WriteLine("backgroind");
            var options = SqlServerDbContextOptionsExtensions.UseSqlServer<DAPDbContext>(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
            var dbContext = new DAPDbContext(options);
            IRepository repo = new Repository(dbContext, null);
            //   bool result = await _repository.UpdateWorkflowVersion(workflowVersionId, projectWorkflowId, projectWorkflowId, workflowVersionId);

            await Task.Delay(1000);
            
            var pubResult = await repo.UpdateWorkflowAttempt(WorkflowSessionAttemptId, projectWorkflowId, userId, workflowVersionId, "processing");

            await Task.Delay(10000);
            pubResult = await repo.UpdateWorkflowAttempt(WorkflowSessionAttemptId, projectWorkflowId, userId, workflowVersionId, "success");
            await Task.Delay(1000);
            await repo.UpdateWorkflowVersionLastAttemptId(workflowVersionId, projectWorkflowId, WorkflowSessionAttemptId);
            string log = @"{\""result\"": \""had issues\""}";
            var logged = await repo.UpdateWorkflowAttemptLog(WorkflowSessionAttemptId, projectWorkflowId, userId, workflowVersionId, log);
            /// Console.WriteLine("backgroind");
        }

        [HttpPost("[action]/{projectWorkflowId}/{workflowVersionId}")]
        public async Task<IActionResult> Publish([FromRoute] int projectWorkflowId, [FromRoute] int workflowVersionId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
             try{
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            //convert workflow 
            Console.WriteLine($"Publishing|GetWorkflowProject|{projectWorkflowId}");
            var workflowProject = _repository.GetWorkflowProject(userId, projectWorkflowId);
            Console.WriteLine($"Publishing|GetWorkflowProject Done|{projectWorkflowId}");
            var workflowVersion = await _repository.GetWorkflowVersion(userId, workflowVersionId);
            Console.WriteLine($"Publishing|GetWorkflowVersion Done|{workflowVersionId}");
            //setoutputTables so we can get the names and set them
            //var outputTableModel = await _repository.AddWorkflowOutputTable(projectWorkflowId, workflowVersionId, userId);
            //if (outputTableModel != null )
            //{
            //    await _repository.UpdateWorkflowOutputTable(outputTableModel.WorkflowOutputModelId, projectWorkflowId, workflowVersionId
            //}
            Console.WriteLine("workflowserver"+_workflowConnectionString);

            List<ProjectQueryDetails> ModelList;
            var workflow = NodeRepository.PrepareWorkflowJson(workflowVersion.VersionNumber, workflowProject.Result.ExternalProjectName, workflowVersion.WorkflowPropertyJson, userId, projectWorkflowId, workflowVersionId, _repository, out ModelList);
            Console.WriteLine($"Publishing|PrepareWorkflowJson Done");
            Console.WriteLine(workflow);
            List<int> ModelIdsUsed = new List<int>();
            if ( ModelList != null )
            {
                foreach ( var modeldetail in ModelList)
                {
                    if (modeldetail.UseProjectId)
                    {
                        var schemaModel = await _repository.GetModel(userId, modeldetail.ProjectId, modeldetail.SchemaName, modeldetail.ModelName);
                        if ( schemaModel != null)
                        {
                            Console.WriteLine($"Publishing|ModelI found-1- {schemaModel.ModelId}");
                            ModelIdsUsed.Add(schemaModel.ModelId);
                        }
                    }
                    else
                    {
                        var schemaModel = await _repository.GetModel(userId, modeldetail.ProjectName, modeldetail.SchemaName, modeldetail.ModelName);
                        if (schemaModel != null)
                        {
                            Console.WriteLine($"Publishing|ModelI found-2- {schemaModel.ModelId}");
                            ModelIdsUsed.Add(schemaModel.ModelId);
                        }
                    }
                    
                }
                Console.WriteLine("ModelsIdsUsed " + ModelIdsUsed.Count );
                if ( ModelIdsUsed.Count > 0)
                {
                    var ret = await _repository.AddWorkflowMonitor(userId, projectWorkflowId, workflowVersionId, ModelIdsUsed);
                    Console.WriteLine($"Publishing|workflow monitor models added Done {ret}"  );
                }
            }

            Console.WriteLine("Server Send Steps");
            //simulate excution service work time
            //  bool result = await _repository.UpdateWorkflowVersion( workflowVersionId, projectWorkflowId, projectWorkflowId, workflowVersionId);
            string workflowUrl = "http://localhost:8080/workflow";
            if (_workflowConnectionString != null)
            {
                workflowUrl = _workflowConnectionString;
            }
            Console.WriteLine("workflowserver"+_workflowConnectionString);
            var client = new RestClient(workflowUrl+":8080/workflow"); //($"https://h6661pykt9.execute-api.us-east-1.amazonaws.com/dev/workflow");
            var requestRest = new RestRequest(Method.POST);
            requestRest.AddHeader("Accept", "application/json");
            //requestRest.AddJsonBody(workflow);
            requestRest.AddParameter("application/json", workflow, ParameterType.RequestBody);
            Console.WriteLine($"Publishing|ExecuteAsync Start");
            IRestResponse response = await client.ExecuteAsync(requestRest);
            Console.WriteLine($"Publishing|ExecuteAsync Done{response.Content}");
            PublishResponse reponseObject = JsonConvert.DeserializeObject<PublishResponse>(response.Content);
            if (reponseObject.id != null)
            {
                int.TryParse(reponseObject.id, out int wProjectid);
                int.TryParse(reponseObject.workflowId , out int workflowID);
                bool result = await _repository.UpdateWorkflowVersion(workflowVersionId, projectWorkflowId, wProjectid, workflowID);
                bool pubResult = await _repository.SetWorkflowPublished(workflowVersionId, projectWorkflowId, 1);
            }
            else
            {
                Console.WriteLine($"Publishing|reponseObject.id NULL");
            }
            //BackgroundWorker bg = new BackgroundWorker();
            //bg.DoWork += (obj, e) => backgroundWorker1_DoWork(workflowVersionId,projectWorkflowId, _repository);
            //bg.RunWorkerAsync();

            return Ok(true);
           }catch(Exception ex)
	   {
                Console.WriteLine("Exception " + ex);
             return Ok(false); 
           }

        }


        [HttpPost("[action]/{projectWorkflowId}/{workflowVersionId}")]
        public async Task<IActionResult> Run([FromRoute] int projectWorkflowId, [FromRoute] int workflowVersionId, [FromHeader] string authorization)
        {
            bool mock = false;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            //simulate excution service work time
            Console.WriteLine($"Run|GetWorkflowVersion Start ");
           var versionInfo = await _repository.GetWorkflowVersion(userId, workflowVersionId);
           Console.WriteLine($"Run|GetWorkflowVersion Done  ");
           var attempt = await _repository.AddWorkflowAttempt(projectWorkflowId, userId, workflowVersionId);
            Console.WriteLine($"Run|AddWorkflowAttempt Done {attempt.WorkflowSessionAttemptId} ");

            if (mock)
            {
                if (attempt != null)
                {
                    BackgroundWorker bg = new BackgroundWorker();
                    bg.DoWork += (obj, e) => backgroundWorker2_DoWork(userId, workflowVersionId, projectWorkflowId, attempt.WorkflowSessionAttemptId, _repository);
                    bg.RunWorkerAsync();

                    return Ok(true);
                }
                else
                {
                    return BadRequest(false);
                }
            }
            else
            {
                if (attempt != null)
                {
                    var client = new RestClient($"{_workflowConnectionString}:8080/workflow/{versionInfo.ExternalWorkflowId}/run");//$"https://h6661pykt9.execute-api.us-east-1.amazonaws.com/dev/workflow/{versionInfo.ExternalWorkflowId}/run");
                    var requestRest = new RestRequest(Method.POST);
                    requestRest.AddHeader("Accept", "application/json");
                    //requestRest.AddJsonBody(workflow);
                    var jsonObj = new
                    {
                        workflowId = versionInfo.ExternalWorkflowId.ToString(),
                        revision = versionInfo.VersionNumber.ToString(),
                        workflowAttemptId = attempt.WorkflowSessionAttemptId,
                        
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
                       
                        await _repository.SaveChangesAsync();
                        Thread.Sleep(1000);
                        Console.WriteLine($"Run|UpdateWorkflowAttempt with procesing");
                        var updatedAttempt = await _repository.UpdateWorkflowAttempt(aId, (int)versionInfo.ExternalWorkflowId, userId, "Processing", "");
                        Console.WriteLine($"Run|UpdateWorkflowAttempt with procesing done");
                        return Ok(true);
                    }
                    else
                    {
                        return Ok(false);
                    }
                   
                }
                else
                {
                    return BadRequest(false);
                }
            }

        }

        [HttpPost("[action]/{projectId}/]/{workflowId}")]//[action]
        public async Task<ActionResult<WorkflowVersionDTO[]>> StopSchedule([FromRoute] int projectId, [FromRoute] int workflowId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            try
            {
                var workflow = await _repository.GetWorkflowVersion(userId, workflowId);
                if (workflow == null) return StatusCode(StatusCodes.Status404NotFound);
                var client = new RestClient();
                string url = "http://idapt.duckdns.org:65432/api/schedules";
                var requestRest = new RestRequest(url, Method.GET, DataFormat.Json);
                IRestResponse response = await client.ExecuteAsync(requestRest);
                var res = response.Content;
                var result = JsonConvert.DeserializeObject<Schedules>(res);
                var schedule = result.schedules.Where(x => x.workflow.id == workflow.ExternalWorkflowId.ToString() && workflow.ExternalProjectId.ToString() == x.project.id).FirstOrDefault();
                if (schedule != null)
                {
                    url = $"http://idapt.duckdns.org:65432/api/schedule/{schedule.id}/disable";
                    requestRest = new RestRequest(url, Method.POST, DataFormat.Json);
                    response = await client.ExecuteAsync(requestRest);
                    var ret = await _repository.DisableWorkflowVersion(workflow.WorkflowVersionId, workflow.WorkflowProjectId);
                    if (workflow.ExternalWorkflowId != null)
                    {
                        ret = await _repository.DisableWorkflowVersionAttempt((int)workflow.ExternalWorkflowId, workflow.ExternalProjectId);
                    }

                    var versionsData = await _repository.GetWorkflowVersions(userId, projectId, false, true);
                    var workflowVersionDTOs = _mapper.Map<WorkflowVersionDTO[]>(versionsData);
                    if (workflowVersionDTOs == null)
                    {
                        return this.StatusCode(StatusCodes.Status204NoContent, "");
                    }
                    else
                    {
                        return workflowVersionDTOs;
                    }
                }

            }catch(Exception ex)
            {
                return this.StatusCode(StatusCodes.Status204NoContent, "");
            }
            return this.StatusCode(StatusCodes.Status204NoContent, "");
            //deseriliazation
            //get scheuyleid = go through array and look fpor prohject id annd workflowid...linq
            //call disable to disable all
            //some repo call to update isactive = false
            //return workflowdto fr that projectid
        }



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
    }
