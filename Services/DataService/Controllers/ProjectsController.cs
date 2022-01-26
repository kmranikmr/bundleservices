using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using DataAccess.DTO;
using AutoMapper;
using System.Security.Claims;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace DataService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IRepository _repository;

        private readonly IMapper _mapper;

        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IRepository repo, IMapper mapper, ILogger<ProjectsController> logger)
        {
            _repository = repo;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Projects        
        [HttpGet("{id}")]       
        public async Task<ActionResult<ProjectDTO[]>> GetProjects([FromRoute]int userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var results = await _repository.GetAllProjectsByUserId(userId, false);

                if (!results.Any()) return NotFound("no project found for the user.");

                var projectDTOs = _mapper.Map<ProjectDTO[]>(results);
               
                foreach (ProjectDTO item in projectDTOs)
                {
                    var writers = await _repository.GetWritersInProject(userId, item.ProjectId);
                    var readers = await _repository.GetReadersInProject(userId, item.ProjectId);
                    var jobs = await _repository.GetJobsInProject(userId, item.ProjectId);
                    var writerString = string.Join(",", writers.Select(x => x.WriterType.WriterTypeName));
                    var readerString = string.Join(",", readers.Select(x => x.ReaderType.ReaderTypeName));
                    var schemas = await _repository.GetSchemasAsync(userId, item.ProjectId, false);
                    if (jobs.Any())
                    {                     
                        var processed = jobs.Where(j => j.JobStatusId == 3).Count();
                        var pending = jobs.Where(j => j.JobStatusId == 2 || j.JobStatusId == 1).Count();
                        var failed = jobs.Where(j => j.JobStatusId == 4).Count();
                        item.Summary = new List<ProjectStatusSummary>()
                        {
                            new ProjectStatusSummary(){ StatusName= "Processed", Count = processed},
                            new ProjectStatusSummary(){ StatusName= "Failed", Count = pending},
                            new ProjectStatusSummary(){ StatusName= "Pending", Count = failed},
                        };
                        item.ConfigSummary = new List<ProjectConfigSummary>();
                       
                        foreach ( var schema in schemas)
                        {
                            var projecSummary = new ProjectConfigSummary() { SchemaName = schema.SchemaName, ReaderType = readerString, WriterType = writerString };
                            item.ConfigSummary.Add(projecSummary);
                        }
                     

                    }
                    //item.Summary = new List<ProjectStatusSummary>()
                    //{
                    //    new ProjectStatusSummary(){ StatusName= "Processed", Count = 4},
                    //    new ProjectStatusSummary(){ StatusName= "Failed", Count = 3},
                    //    new ProjectStatusSummary(){ StatusName= "Pending", Count = 4},
                    //};
                    //item.ConfigSummary = new List<ProjectConfigSummary>()
                    //{
                    //    new ProjectConfigSummary(){SchemaName = "Schema1",ReaderType = "CSV",WriterType = "Mongo" },
                    //    new ProjectConfigSummary(){SchemaName = "Schema2",ReaderType = "Json",WriterType = "Postgres" }
                    //};
                }

                return projectDTOs;
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }          
        }

        // GET: api/Projects        
        [HttpGet("[action]/{isWorkflow:bool=false}")]
        public async Task<ActionResult<ProjectTreeDTO[]>> GetProjectTree( bool isWorkflow  )//add a new param..
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                if (!isWorkflow)
                {
                    Console.WriteLine("GetProjectTree");
                    var results = await _repository.GetAllProjectsByUserId(userId, false, true);
                    Console.WriteLine("GetProjectTree GetAllProjectsByUserId");
                    if (!results.Any()) return NotFound("no project found for the user.");

                    var projectDTOs = _mapper.Map<ProjectTreeDTO[]>(results);
                    Console.WriteLine("GetProjectTree mapper");
                    //if workflow projetid -> workflowprojectid , name
                    //SchemaId ->workflowversionid, version ->schemaname
                    //ModelID -> attemptid..modelname-> tablename  [datetime]...we will extend the queryrequest class to inlcude workflowversionid, attemptid..and query service wi
                    //limit the scope

                    return projectDTOs;
                }
                else
                {//
                    var results = await _repository.GetAllWorkflowProjectsByUserId(userId, true);
                    if (!results.Any()) return NotFound("no project found for the user.");
                    ProjectTreeDTO[] pTree = new ProjectTreeDTO[results.Length];
                    int pIndex = 0;
                    foreach ( var project in results)
                    {
                        int index = 0;
                        ProjectTreeDTO pDto = new ProjectTreeDTO
                        {
                            ProjectId = project.WorkflowProjectId,
                            ProjectName = project.ExternalProjectName,

                        };
                        pDto.ProjectSchemas = new SchemaTreeDTO[project.WorkflowVersions.Count];
                        foreach ( var version in project.WorkflowVersions)
                        {
                            SchemaTreeDTO sDto = new SchemaTreeDTO
                            {
                                SchemaId = version.WorkflowVersionId,
                                SchemaName = version.VersionNumber.ToString(),

                            };
                            var OutputModels = _repository.GetWorkflowOutputTable(project.WorkflowProjectId, version.WorkflowVersionId, userId, true);
                            if (OutputModels != null && OutputModels.Result != null && OutputModels.Result.Count() > 0)
                            {
                                sDto.SchemaModels = new ModelTreeDTO[OutputModels.Result.Count()];
                                int count = 0;
                                foreach (var modelTable in OutputModels.Result)
                                {
                                    ModelMetadataDTO[] modelmetadata = null;
                                    if ( modelTable.WorkflowModelMetadatas != null &&  modelTable.WorkflowModelMetadatas.Count > 0 )
                                    {
                                        modelmetadata = new ModelMetadataDTO[modelTable.WorkflowModelMetadatas.Count];
                                        int k = 0;
                                        foreach( var metadata in modelTable.WorkflowModelMetadatas)
                                        {
                                            ModelMetadataDTO meta = new ModelMetadataDTO
                                            {
                                                ColumnName = metadata.ColumnName,
                                                DataType = "--",
                                            };
                                            modelmetadata[k] = meta;
                                            k++;
                                        }
                                    }
                                    ModelTreeDTO mDto = new ModelTreeDTO
                                    {
                                        ModelID = modelTable.WorkflowOutputModelId,
                                        ModelName = modelTable.DisplayName ,//version.OutputModelName != null ? version.OutputModelName + "[" + attempt.WorkflowSessionAttemptId + "]" : "Output" + "[" + attempt.WorkflowSessionAttemptId + "]",
                                        //ModelMetadataDTO 
                                        ModelMetadatas = modelmetadata
                                    };
                                    if (!string.IsNullOrEmpty(mDto.ModelName))
                                    {
                                        sDto.SchemaModels[count] = mDto;
                                        count++;
                                    }
                                }
                            }
                            //if (version.WorkflowSessionAttempts.Count > 0)
                            //{
                            //    sDto.SchemaModels = new ModelTreeDTO[version.WorkflowSessionAttempts.Count];
                            //    int count = 0;
                            //    foreach (var attempt in version.WorkflowSessionAttempts)
                            //    {
                            //        ModelTreeDTO mDto = new ModelTreeDTO
                            //        {
                            //            ModelID = attempt.WorkflowSessionAttemptId,
                            //            ModelName = version.OutputModelName != null ?version.OutputModelName + "[" + attempt.WorkflowSessionAttemptId + "]" : "Output" + "[" + attempt.WorkflowSessionAttemptId + "]",

                            //        };
                            //        if (!string.IsNullOrEmpty(mDto.ModelName))
                            //        {
                            //            sDto.SchemaModels[count] = mDto;
                            //            count++;
                            //        }
                            //    }
                            //}
                            pDto.ProjectSchemas[index] = sDto;
                            index++;
                        }

                        pTree[pIndex] = pDto;
                        pIndex++;
                      
                    }
                    return pTree;
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="projectName"></param>
        /// <returns></returns>
        [HttpGet("[action]/{id}/{projectName}")]
        public async Task<int>GetProjectIdByName([FromRoute] int userId, [FromRoute] string projectName)
        {
            if (!ModelState.IsValid)
            {
                return -1;
            }
            var project = await _repository.GetProjectByName(projectName);
            if ( project != null )
            {
                return project.ProjectId;
            }
            return -1;
        }

        // GET: api/Projects/5
        [HttpGet("{id}/{projectId}/{isWorkflow:bool=false}")]
        public async Task<IActionResult> GetProject([FromRoute] int userId, [FromRoute] int projectId, bool isWorkflow )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!isWorkflow)
            {

                userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var project = await _repository.GetProjectAsync(userId, projectId, false);

                if (project == null)
                {
                    return NotFound();
                }

                var result = _mapper.Map<ProjectDTO>(project);

                return Ok(result);
            }
            else
            {
                userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var project = await _repository.GetWorkflowProject(userId, projectId, false);

                if (project == null)
                {
                    return NotFound();
                }
                ProjectDTO projectDTO = new ProjectDTO();
                projectDTO.ProjectName  = project.ExternalProjectName;
                projectDTO.ProjectId = project.WorkflowProjectId;
                projectDTO.CreatedOn = project.CreatedOn;
                projectDTO.ProjectDescription = project.Description;
               

                return Ok(projectDTO);
            }
        }

          // GET: api/Projects        
        [HttpGet("[action]/{projectId}/{isWorkflow:bool=false}")]
        public async Task<ActionResult<ProjectDTO>> GetProjectDetails([FromRoute]int projectId, bool isWorkflow)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var results = await _repository.GetAllProjectsByUserId(userId, false);

                if (!results.Any()) return NotFound("no project found for the user.");

                var projectDTOs = _mapper.Map<ProjectDTO[]>(results);

                var projectDTO = projectDTOs.Where(x => x.ProjectId == projectId).FirstOrDefault();
                if ( projectDTO != null )
                {
                    var writers = await _repository.GetWritersInProject(userId, projectId);
                    var readers = await _repository.GetReadersInProject(userId, projectId);
                    var jobs = await _repository.GetJobsInProject(userId, projectId);
                    var writerString = string.Join(",", writers.Select(x => x.WriterType.WriterTypeName));
                    var readerString = string.Join(",", readers.Select(x => x.ReaderType.ReaderTypeName));
                    var schemas = await _repository.GetSchemasAsync(userId, projectId, false);
                    if (jobs.Any())
                    {
                        var processed = jobs.Where(j => j.JobStatusId == 3).Count();
                        var pending = jobs.Where(j => j.JobStatusId == 2 || j.JobStatusId == 1).Count();
                        var failed = jobs.Where(j => j.JobStatusId == 4).Count();
                        projectDTO.Summary = new List<ProjectStatusSummary>()
                        {
                            new ProjectStatusSummary(){ StatusName= "Processed", Count = processed},
                            new ProjectStatusSummary(){ StatusName= "Failed", Count = pending},
                            new ProjectStatusSummary(){ StatusName= "Pending", Count = failed},
                        };
                        projectDTO.ConfigSummary = new List<ProjectConfigSummary>();

                        foreach (var schema in schemas)
                        {
                            var projecSummary = new ProjectConfigSummary() { SchemaName = schema.SchemaName, ReaderType = readerString, WriterType = writerString };
                            projectDTO.ConfigSummary.Add(projecSummary);
                        }


                    }
                }

                return projectDTO;
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("[action]/{projectId}")]
        public async Task<IActionResult> ExploreProject( [FromRoute] int projectId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var project = await _repository.GetProjectAsync(userId, projectId, false);

            if (project == null)
            {
                return NotFound();
            }

            var result = _mapper.Map<ProjectDTO>(project);

            return Ok(new { result.ProjectName, result.ProjectDescription, result.Summary, result.ConfigSummary});
        }

        // POST: api/Project
        [HttpPost("{id}")]
        public async Task<IActionResult> PostProject([FromRoute] int userId, [FromBody] CreateProject project)
        {
            

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
             userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var existingProject = _repository.GetProjectByName(project.ProjectName);

            if (existingProject.Result == null)
            {
                var newProject = new Project()
                {
                    ProjectName = project.ProjectName,
                    ProjectDescription = project.ProjectDescription,
                    CreatedBy = userId,
                    IsFavorite = false
                };

                _repository.Add(newProject);
                await _repository.SaveChangesAsync();

                return CreatedAtAction("GetProject", new { id = userId, projectId = newProject.ProjectId }, _mapper.Map<ProjectDTO>(newProject));
            }
            else
            {
                return this.StatusCode(StatusCodes.Status204NoContent, "Project name already in use.");
            }

            
        }

        
        [HttpPost("[action]/{id}/{projectId}/{flagId}")]
        public async Task<IActionResult> SetFavorite([FromRoute]int userId, [FromRoute]int projectId, [FromRoute]int flagId)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var project = await _repository.SetFavorite(userId, projectId, flagId == 1);

            if (project != null)
            {
                var result = _mapper.Map<ProjectDTO>(project);

                return Ok(result);
            }
            else
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Project Not Found.");
            }
        }

        // POST: api/Project
        [HttpPost("[action]/{id}/{projectId}")]
        public async Task<IActionResult> AddProjectFile([FromRoute] int userId, [FromRoute] int projectId, [FromBody] ProjectFileDTO projectFileDTO)
        {            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var existingProject = await _repository.GetProjectAsync( userId, projectId);

                if (existingProject != null)
                {
                    var projectFile = _mapper.Map<ProjectFile>(projectFileDTO);

                    projectFile.UserId = userId;

                    projectFile.ProjectId = projectId;

                    
                   
                    _repository.Add(projectFile);

                    await _repository.SaveChangesAsync();

                    projectFileDTO.ProjectFileId = projectFile.ProjectFileId;

                    return Ok(projectFileDTO);
                }
                else
                {
                    return this.StatusCode(StatusCodes.Status204NoContent, "Project does not exist.");
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Error.");
            }           
        }


        [HttpPost("[action]/{id}/{projectId}")]
        public async Task<IActionResult> AddProjectWriter([FromRoute] int userId, [FromRoute] int projectId, [FromBody] ProjectWriterDTO projectFileDTO)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var existingProject = await _repository.GetProjectAsync( userId, projectId);

            if (existingProject != null)
            {
                var projectFile = _mapper.Map<ProjectFile>(projectFileDTO);

                projectFile.UserId = userId;

                projectFile.ProjectId = projectId;

                _repository.Add(projectFile);

               

                await _repository.SaveChangesAsync();

                return Ok(projectFileDTO);
            }
            else
            {
                return this.StatusCode(StatusCodes.Status204NoContent, "Project does not exist.");
            }


        }


        [HttpPost("[action]/{id}/{projectId}")]
        public async Task<IActionResult> SetProjectWriter([FromRoute] int userId, [FromRoute] int projectId, [FromBody] ProjectWriterDTO projectFileDTO)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var existingProject = await _repository.GetProjectAsync( userId, projectId);

            if (existingProject != null)
            {
                var projectFile = _mapper.Map<ProjectFile>(projectFileDTO);

                projectFile.UserId = userId;

                projectFile.ProjectId = projectId;

                _repository.Add(projectFile);

                await _repository.SaveChangesAsync();

                return Ok(projectFileDTO);
            }
            else
            {
                return this.StatusCode(StatusCodes.Status204NoContent, "Project does not exist.");
            }


        }


        [HttpPost("[action]/{projectId}")]
        public async Task<IActionResult> UpdateProjectFile([FromRoute] int projectId, [FromBody] Dictionary<int,int> projectFileIdReaderIdDictionary)
        {
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _repository.SetReaderId(projectFileIdReaderIdDictionary);

            if (result)
            {                
                return Ok();
            }
            else
            {
                return this.StatusCode(StatusCodes.Status204NoContent, "Failed to update.");
            }
        }


        [HttpPost("[action]/{projectId}")]
        public async Task<ActionResult<SearchResult>> PreviewProjectFile([FromRoute] int projectId, [FromBody] int[] projectFileIds)
        {
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Console.WriteLine("preview file");
            var projectFiles = await _repository.GetProjectFiles(projectId, projectFileIds);
            List<string> files = null;
            Console.WriteLine("PreviewDone");
            if (projectFiles != null && projectFiles.Any())
            {
                Console.WriteLine("have some file ");
                files = (List<string>)projectFiles.Select(x => {Console.WriteLine(x.FilePath + " " + x.FileName);return Path.Combine(x.FilePath, x.FileName);}).ToList();
                
                foreach ( var f in files )
                   Console.WriteLine(f);
                var res =  Utils.GetFilesPreview(files.ToArray());
                Console.WriteLine("have preview data");
                return Ok(res);
            }
          
             return this.StatusCode(StatusCodes.Status204NoContent, "Failed to update.");
            
        }

        // GET: api/Projects/5
        [HttpGet("[action]/{projectId}")]
        public async Task<IActionResult> GetProjectFiles([FromRoute] int projectId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var projectFiles = await _repository.GetProjectFiles(projectId, true);

                if (projectFiles != null && projectFiles.Any())
                {
                    return Ok(_mapper.Map<ProjectFile[], ProjectFileDTO[]>(projectFiles));
                }
                else
                {
                    return this.StatusCode(StatusCodes.Status204NoContent, "Project files not found for selected project.");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        // GET: api/Projects/5
        [HttpGet("[action]/{projectId}/{isWorkflow:bool=false}")]
        public async Task<IActionResult> GetSearchHistories([FromRoute] int projectId, [FromRoute] bool isWorkflow)//add option for workflow projectid -> oprojectid
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                if (!isWorkflow)
                {
                    var searchHistories = await _repository.GetSearchHistories(projectId, userId);

                    if (searchHistories != null && searchHistories.Any())
                    {
                        return Ok(_mapper.Map<SearchHistoryDTO[]>(searchHistories));//map fro mworkflowsearchhistrydto ,,WorkflowProjectId->versionid
                    }
                    else
                    {
                        return this.StatusCode(StatusCodes.Status204NoContent, "Project history not found for selected project and user.");
                    }
                }
                else
                {
                    var searchHistories = await _repository.GetWorkflowSearchHistories(projectId, userId);
                    if (searchHistories != null && searchHistories.Any())
                    {
                        SearchHistoryDTO[] Searchdtpo = new SearchHistoryDTO[searchHistories.Length];
                        int index = 0;
                        foreach ( var history in searchHistories)
                        {
                            SearchHistoryDTO sdto = new SearchHistoryDTO
                            {
                                SchemaVersionId = history.WorkflowVersionId,
                                ProjectId = projectId,
                                SearchHistoryId = history.WorkflowSearchHistoryId,
                                SearchQuery = history.SearchQuery

                            };
                          
                            if (history.WorkflowSearchGraphs.Count > 0)
                            {
                                sdto.SearchGraphs = new SearchGraphDTO[history.WorkflowSearchGraphs.Count];
                                int count = 0;
                                foreach ( var graph in history.WorkflowSearchGraphs)
                                {
                                    SearchGraphDTO gDTo = new SearchGraphDTO
                                    {
                                        GraphDescription = graph.GraphDescription,
                                        SearchGraphId = graph.WorkflowSearchGraphId
                                    };
                                    sdto.SearchGraphs[count] = gDTo;
                                    count++;
                                }
                            }
                            Searchdtpo[index] = sdto;
                            index++;
                        }
                        if (Searchdtpo != null)
                        {
                            return Ok(Searchdtpo);
                        }
                        else
                        {
                            return this.StatusCode(StatusCodes.Status204NoContent, "Project history not found for selected project and user.");
                        }
                    }
                }
                return this.StatusCode(StatusCodes.Status204NoContent, "Project history not found for selected project and user.");
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        // GET: api/Projects/5
        [HttpGet("[action]")]
        public async Task<IActionResult> GetSavedQueries()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var searchHistories = await _repository.GetSavedQueriesPerProject(userId);

                if (searchHistories != null && searchHistories.Any())
                {
                    var Histories = _mapper.Map<SavedProjectQueryDTO[]>(searchHistories);
                    foreach (var savedQuery in Histories)
                    {
                        var project = await _repository.GetProjectAsync(userId, savedQuery.ProjectId);
                        savedQuery.ProjectName = project.ProjectName;
                    }
                    return Ok(Histories);
                }
                else
                {
                    return this.StatusCode(StatusCodes.Status204NoContent, "Project saved queries not found for the user.");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
        //add to searchhistorydto versionid...and map workflowversionid -> versionid and we can determine that it is for worklfow + workflowprojectid -> projectid
        [HttpPost("[action]/{projectId}/{isWorkflow:bool=false}")]
        public async Task<IActionResult> AddSearchQuery([FromRoute] int projectId, [FromBody] SearchHistoryDTO searchHistoryDTO , [FromHeader] string authorization, [FromRoute] bool isWorkflow = false)//add option workflow..mapping versionid-> projectid
        {
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var md5 = MD5Hash(searchHistoryDTO.SearchQuery.ToLower());

            if (!isWorkflow)
            {
                var existingProject = await _repository.GetProjectAsync(userId, projectId);

                if (existingProject != null)
                {                   
                    var searchHistory = await _repository.GetSearchHistoryByMd5(md5, userId, projectId);

                    if (searchHistory == null)
                    {
                        searchHistory = _mapper.Map<SearchHistory>(searchHistoryDTO);
                        searchHistory.Md5 = md5;
                        searchHistory.UserId = userId;
                        searchHistory.SearchHistoryName = Utils.GetShortUrl();
                        searchHistoryDTO.UserId = userId;
                        searchHistoryDTO.ProjectId = projectId;
                        //searchHistory.ResolvedSearchQuery = Utils.GetMappedQuery(searchHistory.SearchQuery, projectId, authorization).Result;
                        searchHistory.ResolvedSearchQuery = await Utils.GetMappedQuery(searchHistory.SearchQuery, projectId, authorization, -1,false );
                        Console.WriteLine($"searchHistory.ResolvedSearchQuery {searchHistory.ResolvedSearchQuery}");
                         // searchHistory.
                        //add cal to queryservic eget actual mappeed query
                        //TBD: add soem user fiendly name geneftaor for search histroy
                        _repository.Add(searchHistory);

                        await _repository.SaveChangesAsync();
                    }
                    searchHistoryDTO.SearchHistoryId = searchHistory.SearchHistoryId;
                    return Ok(searchHistoryDTO);
                }
                else
                {
                    return this.StatusCode(StatusCodes.Status204NoContent, "ProjectId does not exist.");
                }
            }
            else
            {
                var existingProject = await _repository.GetWorkflowProject(userId, projectId, true);

                if (existingProject != null)
                {
                    WorkflowSearchHistory searchHistory = await _repository.GetWorkflowSearchHistoryByMd5(md5, userId, searchHistoryDTO.ProjectId);

                    if (searchHistory == null)
                    {
                        searchHistory = new WorkflowSearchHistory
                        {
                            SearchQuery = searchHistoryDTO.SearchQuery,
                            WorkflowProjectId = searchHistoryDTO.ProjectId,
                            WorkflowVersionId = searchHistoryDTO.SchemaVersionId,
                            Md5 = md5
                        };
                        if (searchHistoryDTO.SearchGraphs != null)
                        {
                            foreach (var graph in searchHistoryDTO.SearchGraphs)
                            {
                                WorkflowSearchGraph wsg = new WorkflowSearchGraph
                                {
                                    GraphDescription = graph.GraphDescription
                                };
                                searchHistory.WorkflowSearchGraphs.Add(wsg);
                            }
                        }
                        searchHistory.UserId = userId;
                        searchHistoryDTO.UserId = userId;
                        searchHistoryDTO.ProjectId = projectId;
                        searchHistory.WorkflowSearchHistoryName = Utils.GetShortUrl();
                        //searchHistory.ResolvedSearchQuery = Utils.GetMappedQuery(searchHistory.SearchQuery, projectId, authorization,true).Result;
                         searchHistory.ResolvedSearchQuery = await Utils.GetMappedQuery(searchHistory.SearchQuery, projectId, authorization, searchHistoryDTO.SchemaVersionId, true);
                        Console.WriteLine($"searchHistory.ResolvedSearchQuery {searchHistory.ResolvedSearchQuery}");
                        searchHistoryDTO.SchemaVersionId = searchHistory.WorkflowVersionId;
                        _repository.Add(searchHistory);

                        await _repository.SaveChangesAsync();
                    }

                    searchHistoryDTO.SearchHistoryId = searchHistory.WorkflowSearchHistoryId;
                    return Ok(searchHistoryDTO);
                }
                else
                {
                    return this.StatusCode(StatusCodes.Status204NoContent, "ProjectId does not exist.");
                }
            }
            return this.StatusCode(StatusCodes.Status204NoContent, "ProjectId does not exist.");
        }

        [HttpPost("[action]/{searchHistoryId}/{isWorkflow:bool=false}")]
        public async Task<IActionResult> UpdateSearchQuery(int searchHistoryId, [FromBody] string friendlyName, [FromRoute] bool isWorkflow = false)//add option workflow..mapping versionid-> projectid
        {
            Console.WriteLine("update search query");

          int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!isWorkflow)
            {
                Console.WriteLine("update search query" + searchHistoryId.ToString() + " " + friendlyName);
                var ret = await _repository.UpdateSearchHistory(searchHistoryId, userId, friendlyName);
                Console.WriteLine("update search query done" + searchHistoryId.ToString() + " " + friendlyName);
                return Ok(ret);
            }
            return this.StatusCode(StatusCodes.Status204NoContent, "No searchHistoryId");
        }

        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }

        // GET: api/Projects/5
        [HttpGet("[action]/{projectId}/{sourceTypeId}")]
        public async Task<IActionResult> GetProjectFiles([FromRoute] int projectId, int sourceTypeId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var projectFiles = await _repository.GetProjectFiles(projectId, sourceTypeId);

                if (projectFiles != null && projectFiles.Any())
                {
                    return Ok(_mapper.Map<ProjectFileDTO[]>(projectFiles));
                }
                else
                {
                    return this.StatusCode(StatusCodes.Status204NoContent, "Project files not found for selected project.");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
        // map searchhistoryid from workflowsearchhistoryid...
        [HttpPost("[action]/{searchHistoryId}/{isWorkflow:bool=false}")]
        public async Task<IActionResult> AddSearchGraph([FromRoute] int searchHistoryId, [FromBody] SearchGraphDTO searchGraphDTO, bool isWorkflow)//add param saying workflow
        {
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (isWorkflow)
                {
                    if (searchGraphDTO.SearchGraphId > 0)
                    {
                        var searchGraphEntity = await _repository.UpdateWorkflowSearchGraph(searchGraphDTO.SearchGraphId, searchGraphDTO.GraphDescription);

                        if (searchGraphEntity != null)
                        {
                            return Ok(searchGraphDTO);
                        }
                        else
                        {
                            return this.StatusCode(StatusCodes.Status204NoContent, "Could not find search graph Id");
                        }
                    }
                    else
                    {
                        WorkflowSearchGraph searchGraph = new WorkflowSearchGraph
                        {
                            CreatedOn = DateTime.Now,
                            WorkflowSearchHistoryId = searchHistoryId,
                            GraphDescription = searchGraphDTO.GraphDescription
                        };

                        _repository.Add(searchGraph);
                       // searchGraph.WorkflowSearchGraphId = searchHistoryId;

                        _repository.Add(searchGraph);

                        await _repository.SaveChangesAsync();
                        searchGraphDTO.SearchGraphId = searchGraph.WorkflowSearchGraphId;
                        return Ok(searchGraphDTO);
                    }
                }
                else
                {
                    if (searchGraphDTO.SearchGraphId > 0)
                    {
                        var searchGraphEntity = await _repository.UpdateSearchGraph(searchGraphDTO.SearchGraphId, searchGraphDTO.GraphDescription);

                        if (searchGraphEntity != null)
                        {
                            return Ok(searchGraphDTO);
                        }
                        else
                        {
                            return this.StatusCode(StatusCodes.Status204NoContent, "Could not find search graph Id");
                        }
                    }
                    else
                    {
                        var searchGraphEntity = _mapper.Map<SearchGraph>(searchGraphDTO);

                        searchGraphEntity.SearchHistoryId = searchHistoryId;

                        _repository.Add(searchGraphEntity);

                        await _repository.SaveChangesAsync();
                        searchGraphDTO.SearchGraphId = searchGraphEntity.SearchGraphId;
                        return Ok(searchGraphDTO);
                    }
                }
            }
            catch (Exception ex)
            {

                return this.StatusCode(StatusCodes.Status204NoContent, "Database error");
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetTotalModelSize()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                long totalSize = await _repository.GetTotalModelSize(userId);
                Console.WriteLine("totalSize" + totalSize);
                return Ok(totalSize);
            }
            catch (Exception ex)
            {

                return this.StatusCode(StatusCodes.Status204NoContent, "Database error");
            }
        }

    }
}
