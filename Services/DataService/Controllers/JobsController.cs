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
    public class JobsController : ControllerBase
    {      
        private readonly IRepository _repository;

        private readonly IMapper _mapper;

        private readonly ILogger<JobsController> _logger;

        public JobsController(IRepository repo, IMapper mapper, ILogger<JobsController> logger)
        {
            _repository = repo;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Jobs
        [HttpGet("{id}/{projectId}")]
        public async Task<ActionResult<JobDTO[]>> GetJobs(int id, int projectId)
        {
            try
            {
                var jobs = await _repository.GetJobsInProject(id, projectId);

                if (!jobs.Any()) return NotFound();

                return _mapper.Map<JobDTO[]>(jobs);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        // GET: api/Jobs
        [HttpGet("[action]/{jobId}")]
        public async Task<ActionResult<JobSummaryDTO>> GetJobSummary(int jobId)
        {
            try
            {
                //int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var jobs = await _repository.GetJobSummary(jobId);

                if (jobs.Any())
                {
                    JobSummaryDTO jobSummary = new JobSummaryDTO();

                    jobSummary.ProjectName = jobs.ElementAt(0).Project.ProjectName;
                    jobSummary.JobId = jobId;
                    jobSummary.CompletedFile = jobs.Where(j => j.JobStatusId == 3).Count();
                    jobSummary.TotalFile = jobs.Count();
                    jobSummary.FileList = new List<FileDTO>();
                    foreach (var j in jobs)
                    {
                        FileDTO fl = new FileDTO();
                        fl.JobId = j.JobId;
                        fl.ProjectFileId = j.ProjectFileId;
                        fl.InputFileName = j.ProjectFile.FileName;
                        fl.Source = j.ProjectFile.Reader?.ReaderType?.ReaderTypeName;
                        fl.Status = j.JobStatus.StatusName;
                        if (j.StartedOn.HasValue) fl.StartTime = j.StartedOn.Value;
                        if (j.CompletedOn.HasValue) fl.EndTime = j.CompletedOn.Value;
                        jobSummary.FileList.Add(fl);
                    }
                    var job_project = await _repository.GetJobSummary(jobs.ElementAt(0).ProjectId, true);
                    foreach(var j in job_project)
                    {
                        if (j.JobId == jobId) continue;
                        FileDTO fl = new FileDTO();
                        fl.JobId = j.JobId;
                        fl.ProjectFileId = j.ProjectFileId;
                        fl.InputFileName = j.ProjectFile.FileName;
                        fl.Source = j.ProjectFile.Reader?.ReaderType?.ReaderTypeName;
                        fl.Status = j.JobStatus.StatusName;
                        if (j.StartedOn.HasValue) fl.StartTime = j.StartedOn.Value;
                        if (j.CompletedOn.HasValue) fl.EndTime = j.CompletedOn.Value;
                        jobSummary.FileList.Add(fl);
                    }
                    jobSummary.FileList = jobSummary.FileList.OrderBy(x => x.StartTime).ToList();
                    return jobSummary;
                }

                return this.StatusCode(StatusCodes.Status204NoContent, "No job summary found.");
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("[action]/{projectId}")]
        public async Task<ActionResult<JobSummaryDTO>> GetAllJobs(int projectId)
        {
            try
            {
                //int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var jobs = await _repository.GetJobSummary(projectId, true);

                if (jobs.Any())
                {
                    JobSummaryDTO jobSummary = new JobSummaryDTO();

                    jobSummary.ProjectName = jobs.ElementAt(0).Project.ProjectName;
                  
                    jobSummary.CompletedFile = jobs.Where(j => j.JobStatusId == 3).Count();
                    jobSummary.TotalFile = jobs.Count();
                    jobSummary.FileList = new List<FileDTO>();
                    foreach (var j in jobs)
                    {
                        FileDTO fl = new FileDTO();
                        fl.JobId = j.JobId;
                        jobSummary.JobId = j.JobId;
                        fl.ProjectFileId = j.ProjectFileId;
                        fl.InputFileName = j.ProjectFile.FileName;
                        fl.Source = j.ProjectFile.Reader?.ReaderType?.ReaderTypeName;
                        fl.Status = j.JobStatus.StatusName;
                        if (j.StartedOn.HasValue) fl.StartTime = j.StartedOn.Value;
                        if (j.CompletedOn.HasValue) fl.EndTime = j.CompletedOn.Value;
                        jobSummary.FileList.Add(fl);
                    }
                   
                    jobSummary.FileList = jobSummary.FileList.OrderBy(x => x.StartTime).ToList();
                    return jobSummary;
                }

                return this.StatusCode(StatusCodes.Status204NoContent, "No job summary found.");
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
        // GET: api/Jobs/5
        [HttpGet("{id}/{jobId}")]
        public async Task<IActionResult> GetJob([FromRoute] int id, int jobId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var job = await _repository.GetJobAsync(id, jobId);

            if (job == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<JobDTO>(job));
        }
        
        // POST: api/Jobs
        [HttpPost("{id}")]
        public async Task<IActionResult> PostJob(int id, [FromBody] JobDTO jobDTO)
        {
            int userId = id;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var job = _mapper.Map<Job>(jobDTO);

            job.UserId = id;

            _repository.Add(job);
           
            bool saved = await _repository.SaveChangesAsync();

            if (saved)
            {
                return CreatedAtAction("GetJob", new { id = userId, projectId = job.ProjectId }, job);
            }
            else
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }            
        }

       

    }
}