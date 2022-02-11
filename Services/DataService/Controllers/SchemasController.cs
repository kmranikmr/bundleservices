using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using AutoMapper;
using DataAccess.DTO;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using RestSharp;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SchemasController : ControllerBase
    {
        private readonly IRepository _repository;

        private readonly IMapper _mapper;

        private readonly ILogger<SchemasController> _logger;

        private string _queryServiceString = null;
        public SchemasController(IRepository repo, IMapper mapper, ILogger<SchemasController> logger, IOptions<ConnectionStringsConfig> optionsAccessor)
        {
            _repository = repo;
            _mapper = mapper;
            _logger = logger;
            _queryServiceString = optionsAccessor?.Value?.QueryServiceConnection;
        }

        // GET: api/Schemas/1/1
        [HttpGet("{userId}/{projectId}")]
        public async Task<ActionResult<SchemaDTO[]>> GetProjectSchemas([FromRoute] int userId, [FromRoute] int projectId)
        {
            var projectSchema = await _repository.GetSchemasAsync(userId, projectId, true);

            var result = _mapper.Map<ProjectSchema[], SchemaDTO[]>(projectSchema);

            return result;
        }

        // GET: api/Schemas/1/5
        [HttpGet("{userId}/{schemaId}")]
        public async Task<IActionResult> GetProjectSchema([FromRoute] int userId, [FromRoute]int schemaId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var projectSchema = await _repository.GetSchemaAsync(schemaId, true);

            if (projectSchema == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(_mapper.Map<ProjectSchema, SchemaDTO>(projectSchema));
            }           
        }

        ///
        // GET: api/Schemas/1/5
        [HttpGet("[action]/{userId}/{schemaId}/{isWorkflow:bool=false}")]
        public async Task<IActionResult> ExploreProjectSchema([FromRoute] int userId, [FromRoute]int schemaId, [FromRoute]bool isWorkflow)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!isWorkflow)
            {
                var projectSchema = await _repository.GetSchemaAsync(schemaId, true);

                if (projectSchema == null)
                {
                    return NotFound();
                }
                else
                {
                    var ret = _mapper.Map<ProjectSchema, SchemaDTO>(projectSchema);
                    List<string> modelInf = new List<string>();
                    foreach (var model in ret.SchemaModels)
                    {

                        modelInf.Add(model.ModelName);
                        modelInf.Add("Field Count :" + model.ModelMetadatas.Count().ToString());
                    }
                    return Ok(new { ret.SchemaName, modelInf });
                }
            }
            else
            {
                var version = await _repository.GetWorkflowVersion(userId, schemaId);

                if (version == null)
                {
                    return NotFound();
                }
                List<string> modelInf = new List<string>();
                foreach ( var attempt in version.WorkflowSessionAttempts)
                {
                    modelInf.Add(version.OutputModelName + "["+attempt.CreatedOn+"]");
                    modelInf.Add("Field Count :" + version.WorkflowModelMetadatas.Count().ToString());
                }
                string VersionName = "Version:" + version.VersionNumber;
                return Ok(new { VersionName , modelInf });
            }
        }

        /// <summary>
        /// schema summary
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        [HttpGet("{projectId}/{schemaId}/{isWorkflow:bool=false}")]
        public async Task<IActionResult> GetProjectSchemaSummary( [FromRoute]int projectId,  [FromRoute]int schemaId, bool isWorkflow)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var projectSchema = await _repository.GetSchemaAsync(schemaId, true);

            if (projectSchema == null)
            {
                return NotFound();
            }
            else
            {
                ProjectSchemaSummary schemaSummary = new ProjectSchemaSummary();
                schemaSummary.SchemaName = projectSchema.SchemaName;
                schemaSummary.SchemaId = projectSchema.SchemaId;
                schemaSummary.NumberOfModels = projectSchema.SchemaModels.Count;
                var key = projectSchema.ProjectFiles.Where(x => x.ProjectId == schemaId && x.SchemaId == schemaId).Select(x => x.SourceType).Distinct();
                var value = projectSchema.ProjectFiles.Where(x => x.ProjectId == schemaId && x.SchemaId == schemaId).Count();
                foreach (SourceType stype in key)
                {
                    if (!schemaSummary.SourcetoNumberOfFiles.ContainsKey(key.ToString()))
                    {
                        schemaSummary.SourcetoNumberOfFiles.Add(key.ToString(), 1);
                    }
                    else
                    {
                        schemaSummary.SourcetoNumberOfFiles[key.ToString()] += 1;
                    }
                }
                return Ok(_mapper.Map<ProjectSchema, SchemaDTO>(projectSchema));
            }
        }

        [HttpGet("[action]/{projectId}/{schemaId}/{modelId}/{isWorkflow:bool=false}")]
        public async Task<IActionResult> ExploreModelSummary([FromRoute]int projectId, [FromRoute]int schemaId, [FromRoute]int modelId, [FromHeader] string authorization, bool isWorkflow)
        {
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
           // var accessToken1 = await HttpContext.("access_token");
           var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token");
            var client = new RestClient();
            var url = $"{_queryServiceString}:6004/api/Search";
            var requestRest = new RestRequest(url, Method.POST, DataFormat.Json);
            if ( isWorkflow)
            {
                url += "/true";
            }
            if (!isWorkflow)
            {
                var projectSchema = await _repository.GetSchemaAsync(schemaId, true);

                if (projectSchema == null)
                {
                    return NotFound();
                }
                string schemaName = projectSchema.SchemaName;
                schemaName = schemaName.Replace(" ", "");
                var schemaModels = await _repository.GetModelsAsync(userId, schemaId);
                var model = schemaModels.Where(x => x.ModelId == modelId).FirstOrDefault();
                string query = $"select * from {schemaName}.{model.ModelName} limit 10";


                var body = new QueryRequest { QueryString = query, AllRecords = false, SearchDestination = DestinationType.RDBMS, PageFilter = "rowid", PageIndex = 0, PageSize = 10, ProjectId = projectId };
                requestRest.AddHeader("Authorization", authorization);
                requestRest.AddJsonBody(body);
                IRestResponse response = await client.ExecuteAsync(requestRest);
                List<List<string>> list = new List<List<string>>();
                if (response != null)
                {
                    var res = response.Content;
                    var result = JsonConvert.DeserializeObject<SearchResult>(res);
                    if (result != null)
                    {
                        if (result.Header2 != null && result.Results != null && result.Results.Count <= 10)
                        {
                            if (result.Header2.Count == result.Results[0].Count)
                            {
                                int index = 0;
                                foreach (var iteminList in result.Results)
                                {
                                    var l = new List<string>();
                                    index = 0;
                                    foreach (string header in result.Header2)
                                    {
                                        l.Add($"{header}:{iteminList[index]}");
                                        index++;
                                    }
                                    list.Add(l);
                                }
                            }
                        }

                    }
                }

                //var list = model.ModelMetadatas.Select(x => new { ColumnName = x.ColumnName, DataType = x.DataType }).ToList();
                // var list = new List<string> { { "Field1: 123" }, { "Field2: 345" }, { "Field3: 567" } };
                if (model != null)//new List<string>{ { "Field1: 123" }, { "Field2: 345" }, { "Field3: 567" } });
                    return Ok(list);//new List<string> { { "Field1: 123" }, { "Field2: 345" }, { "Field3: 567" }}
                else
                    return NotFound();
            }
            else
            {
                var outputTable = _repository.GetWorkflowOutputTableName(projectId, schemaId);
                if ( outputTable != null)
                {
                    if ( !string.IsNullOrEmpty(outputTable.Result.TableName) )
                    {
                        string query = $"select *,1 as rowid from {outputTable.Result.TableName} limit 10";
                        var body = new QueryRequest { QueryString = query, AllRecords = false, SearchDestination = DestinationType.RDBMS, PageFilter = "rowid", PageIndex = 0, PageSize = 10, ProjectId = projectId };
                        requestRest.AddHeader("Authorization", authorization);
                        requestRest.AddJsonBody(body);
                        IRestResponse response = await client.ExecuteAsync(requestRest);
                        List<List<string>> list = new List<List<string>>();
                        if (response != null)
                        {
                            var res = response.Content;
                            var result = JsonConvert.DeserializeObject<SearchResult>(res);
                            if (result != null)
                            {
                                if (result.Header2 != null && result.Results != null && result.Results.Count <= 10)
                                {
                                    if (result.Header2.Count == result.Results[0].Count)
                                    {
                                        int index = 0;
                                        foreach (var iteminList in result.Results)
                                        {
                                            var l = new List<string>();
                                            index = 0;
                                            foreach (string header in result.Header2)
                                            {
                                                l.Add($"{header}:{iteminList[index]}");
                                                index++;
                                            }
                                            list.Add(l);
                                        }
                                    }
                                }

                            }
                        }
                        if (list.Count > 0 )//new List<string>{ { "Field1: 123" }, { "Field2: 345" }, { "Field3: 567" } });
                            return Ok(list);//new List<string> { { "Field1: 123" }, { "Field2: 345" }, { "Field3: 567" }}
                        else
                            return NotFound();
                    }
                }
                return Ok(StatusCodes.Status204NoContent);
            }
        }
        // POST: api/Schemas
        [HttpPost("{userId}/{projectId}")]
        public async Task<IActionResult> PostProjectSchema([FromRoute] int userId, int projectId, [FromBody] SchemaDTO schemaDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingProject = await _repository.GetProjectAsync( userId, projectId);

                if (existingProject != null)
                {
                    var projectSchema = _mapper.Map<ProjectSchema>(schemaDTO);

                    projectSchema.ProjectId = projectId;

                    projectSchema.UserId = userId;

                    _repository.Add(projectSchema);

                    await _repository.SaveChangesAsync();

                    return CreatedAtAction("GetProjectSchema", new { id = userId, schemaId = projectSchema.SchemaId }, projectSchema);
                }
                else
                {
                    return this.StatusCode(StatusCodes.Status204NoContent, "Project does not exist.");
                }
            }
            catch (Exception ex)
            {

                return NotFound();
            }
        }        
    }
}