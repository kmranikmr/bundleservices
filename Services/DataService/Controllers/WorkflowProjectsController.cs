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
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace DataService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkflowProjectsController : ControllerBase
    {
        private readonly IRepository _repository;

        private readonly IMapper _mapper;

        private readonly ILogger<WorkflowProjectsController> _logger;

        public WorkflowProjectsController(IRepository repo, IMapper mapper, ILogger<WorkflowProjectsController> logger)
        {
            _repository = repo;
            _mapper = mapper;
            _logger = logger;
        }
/// <summary>
/// Get all  workflow projects for the user
/// </summary>
/// <param name="userId"></param>
/// <returns></returns>
        [HttpGet()]//"[action]"
        public async Task<ActionResult<WorkflowProjectDTO[]>> GetWorkflowProjects()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
              //  var lis = NodeRepository.GetNodeRepo();
              //  string json = JsonConvert.SerializeObject(lis[0]);
              //   json = JsonConvert.SerializeObject(lis[1]);
              //   json = JsonConvert.SerializeObject(lis[2]);
                var result = await _repository.GetWorkflowProjects(userId);
                var workflowDTOs = _mapper.Map<WorkflowProjectDTO[]>(result);
                if (workflowDTOs == null)
                {
                    return this.StatusCode(StatusCodes.Status204NoContent, "");
                }
                else
                {
                    return workflowDTOs;
                }
            }
            catch(Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,"");
            }
        }
        ///
        /// 
        ///

        [HttpGet("[action]/{workflowProjectid}/{workflowVersionId}/{workflowModelId}")]//"[action]"
        public async Task<ActionResult<string>>WorkflowModelName(int workflowProjectid, int workflowVersionId, int workflowModelId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var result = await _repository.GetWorkflowOutputTable(workflowProjectid, workflowVersionId, workflowModelId, userId);
                return result.TableName;
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "");
            }
        }

        ///



        [HttpPost("[action]/{workflowProjectid}/{workflowVersionId}")]//"[action]"
        public async Task<ActionResult<string[]>> WorkflowModelNames(int workflowProjectid, int workflowVersionId, [FromBody] string[] DisplayNames)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var result = await _repository.GetWorkflowOutputTableNames(workflowProjectid, workflowVersionId, userId, DisplayNames);
                List<string> tables = new List<string>();
                foreach (var model in result)
                {
                    tables.Add(model.TableName);
                }
                return tables.ToArray();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "");
            }
        }

        /// <summary>
        /// Post Workflow Porject when creating a new workflow project
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        [HttpPost()]//"[action]"
        public async Task<IActionResult> PostWorkflowProject( [FromBody] WorkflowProjectDTO project)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var workflowProject = _mapper.Map<WorkflowProject>(project);
            if ( workflowProject != null)
            {
                workflowProject.UserId = userId;
                workflowProject.CreatedOn = DateTime.Now;
                _repository.Add(workflowProject);

                await _repository.SaveChangesAsync();

                project.WorkflowProjectId = workflowProject.WorkflowProjectId;

                return CreatedAtAction("GetWorkflowProjects", new { id = workflowProject.WorkflowProjectId }, project);

            }
            return StatusCode(StatusCodes.Status404NotFound);
        }

/// <summary>
/// Delte project entry
/// </summary>
/// <param name="projectWorkflowId"></param>
/// <returns></returns>
        [HttpDelete("{projectWorkflowId}")]
        public async Task<IActionResult> DeleteProject([FromRoute] int projectWorkflowId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            bool result = await _repository.DeleteWorkflowProjects(userId, projectWorkflowId);

            if (result == false)
            {
                return NotFound();
            }
            else
            {
                return Ok(projectWorkflowId);
            }
        }

        [HttpGet("[action]")]//"[action]"
        public async Task<ActionResult<WorkflowElement[]>> GetNodeRepository()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var result = await _repository.GetNodeRepository();
             
                if (result == null)
                {
                    return this.StatusCode(StatusCodes.Status204NoContent, "");
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "");
            }
        }
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

                var searchHistories = await _repository.GetWorkflowSearchHistories(userId);

                if (searchHistories != null && searchHistories.Any())
                {
                    var Histories = _mapper.Map<SavedWorkflowQueryDTO[]>(searchHistories);
                    foreach (var savedQuery in Histories)
                    {
                        var project = await _repository.GetWorkflowProject(userId, savedQuery.WorkflowProjectId);
                        savedQuery.WorkflowName = project.ExternalProjectName;
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