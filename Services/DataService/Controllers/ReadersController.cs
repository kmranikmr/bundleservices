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
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace DataService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReadersController : ControllerBase
    {
        private readonly IRepository _repository;

        private readonly IMapper _mapper;

        private readonly ILogger<ReadersController> _logger;

        public ReadersController(IRepository repo, IMapper mapper, ILogger<ReadersController> logger)
        {
            _repository = repo;
            _mapper = mapper;
        }

        // GET: api/Readers
        [HttpGet("[action]/{projectId}")]
        public async Task<ActionResult<ReaderDTO[]>> GetReaders(int projectId)
        {
            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var userProject = await _repository.GetProjectAsync( userId, projectId);

                if (userProject != null)
                {
                    var readers = await _repository.GetReadersInProject(userId, projectId);

                    if (!readers.Any()) return NotFound();

                    return _mapper.Map<ReaderDTO[]>(readers);
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
        public async Task<ActionResult<ReaderDTO[]>> GetReaders()
        {
            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var readers = await _repository.GetReadersViaUser(userId);

                if (!readers.Any()) return NotFound();

                return _mapper.Map<ReaderDTO[]>(readers);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        // GET: api/Readers
        [HttpGet("[action]")]
        public async Task<ActionResult<ReaderTypeDTO[]>> GetReaderTypes()
        {
            try
            {
                var readerTypes = await _repository.GetReaderTypes();

                if (!readerTypes.Any()) return NotFound();

                return _mapper.Map<ReaderTypeDTO[]>(readerTypes);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        // GET: api/Readers/5
        [HttpGet("[action]/{readerId}")]
        public async Task<IActionResult> GetReader(int readerId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reader = await _repository.GetReaderAsync(readerId);

            if (reader == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ReaderDTO>(reader));
        }

        // GET: api/Readers/5
        [HttpGet("[action]/{projectId}/{readerTypeId}")]
        public async Task<IActionResult> GetReaders(int projectId, int readerTypeId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var readers = await _repository.GetReadersInProjectByTypes(projectId, readerTypeId);

            if (readers != null && readers.Any())
            {
                return Ok(_mapper.Map<ReaderDTO[]>(readers));
            }
            else
            {
                return this.StatusCode(StatusCodes.Status204NoContent, "No Readers available");
            }
        }

        // GET: api/Readers/5
        [HttpGet("[action]/{projectId}/{readerTypeId}")]
        public async Task<IActionResult> GetReaderConfigurationList(int projectId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reader = await _repository.GetReadersInProjectByTypes(projectId, 0);

            if (reader !=null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ReaderDTO[]>(reader));
        }

        // POST: api/Readers
        [HttpPost("{projectId}")]
        public async Task<IActionResult> PostReader([FromRoute] int projectId, [FromBody] ReaderDTO readerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var reader = _mapper.Map<Reader>(readerDTO);

            reader.UserId = userId;

            reader.ProjectReaders.Add(new ProjectReader()
            {
                ProjectId = projectId
            });

            _repository.Add(reader);

            await _repository.SaveChangesAsync();

            readerDTO.ReaderId = reader.ReaderId;

            return CreatedAtAction("GetReader", new { id = reader.ReaderId }, readerDTO);
        }

        // DELETE: api/Readers/5
        [HttpDelete("{readerId}")]
        public async Task<IActionResult> DeleteReader([FromRoute] int readerId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            bool result = await _repository.DeleteReader(userId, readerId);

            if (result == false)
            {
                return NotFound();
            }
            else
            {
                return Ok(readerId);
            }            
        }       
    }
}