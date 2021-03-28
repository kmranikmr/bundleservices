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
using Microsoft.Extensions.Logging;

namespace DataService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WritersController : ControllerBase
    {


        private readonly IRepository _repository;

        private readonly IMapper _mapper;

        private readonly ILogger<WritersController> _logger;

        public WritersController(IRepository repo, IMapper mapper, ILogger<WritersController> logger)
        {
            _repository = repo;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Writers
        [HttpGet("{projectId}")]
        public async Task<ActionResult<WriterDTO[]>> GetWriters(int projectId)
        {
            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var userProject = await _repository.GetProjectAsync(userId, projectId);

                if (userProject != null)
                {
                    var writers = await _repository.GetWritersInProject(userId, projectId);

                    if (!writers.Any()) return this.StatusCode(StatusCodes.Status204NoContent, "Writers not available.");

                    return _mapper.Map<WriterDTO[]>(writers);
                }
                else
                {
                    return this.StatusCode(StatusCodes.Status401Unauthorized, "User does not have permission on project.");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        // GET: api/Readers
        [HttpGet]
        public async Task<ActionResult<WriterDTO[]>> GetWriters()
        {
            try
            {
                var writers = await _repository.GetWriters();

                if (!writers.Any()) return this.StatusCode(StatusCodes.Status204NoContent, "Writers not available.");

                return _mapper.Map<WriterDTO[]>(writers);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }


        // GET: api/Writers/5
        [HttpGet("GetWriter/{id}")]
        public async Task<IActionResult> GetWriter([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var writer = await _repository.GetWriterAsync(id);

            if (writer == null)
            {
                return this.StatusCode(StatusCodes.Status204NoContent, "Writer not found.");
            }

            return Ok(_mapper.Map<WriterDTO>(writer));
        }

        // DELETE: api/Writers/5
        [HttpDelete("{projectId}/{writerId}")]
        public async Task<IActionResult> DeleteWriter([FromRoute] int projectId, [FromRoute] int writerId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var projectWriter = await _repository.GetProjectWriterAsync(projectId, writerId);

            if (projectWriter == null)
            {
                return this.StatusCode(StatusCodes.Status204NoContent, "Project Writer not available.");
            }

            _repository.Delete<ProjectWriter>(projectWriter);
            if (await _repository.SaveChangesAsync())
            {
                return Ok(projectWriter);
            }
            else
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Project Writer could not be deleted.");
            }

        }

        [HttpPost("{projectId}/{writerId}")]
        public async Task<IActionResult> PostWriter([FromRoute] int projectId, [FromRoute] int writerId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var projectWriter = await _repository.GetProjectWriterAsync(projectId, writerId);

            if (projectWriter == null)
            {
                projectWriter = new ProjectWriter();
                projectWriter.WriterId = writerId;
                projectWriter.ProjectId = projectId;

                _repository.Add<ProjectWriter>(projectWriter);

                if (await _repository.SaveChangesAsync())
                {
                    return Ok(projectWriter);
                }
                else
                {
                    return this.StatusCode(StatusCodes.Status500InternalServerError, "Project Writer could not be associated.");
                }
            }
            else
            {
                return Ok(projectWriter);
            }
        }


    }
}