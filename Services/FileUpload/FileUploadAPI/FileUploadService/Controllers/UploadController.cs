using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FileUploadService.Database;
using FileUploadService.Model;
//using FileUploadService.Model;
using FileUploadService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using AutoMapper;
namespace FileUploadService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {

        private IHostingEnvironment _hostingEnvironment;
        private MetaDataStore _metaDataStore;
        private readonly ProjectFileContextDB _context;
        private Dictionary<int, int?> UserToProjects;
        private readonly IHubContext<ProgressHub> _progressHubContext;
        private readonly IMapper _mapper;
        public UploadController(ProjectFileContextDB context, IHubContext<ProgressHub> progressHubContext, IMapper mapper)
        {
            //  _hostingEnvironment = hostingEnvironment;
            //lets bring up current projects if not uploaded yet
            _metaDataStore = new MetaDataStore("");
            _context = context;
            _mapper = mapper;
            UserToProjects = new Dictionary<int, int?>();
            if (_context.ProjectFile != null && _context.ProjectFile.Count() > 0)
            {
                var projects = _context.ProjectFile.ToList();
                foreach (Models.ProjectFile projfile in projects)
                {
                    if (!UserToProjects.ContainsKey(projfile.UserId))
                    {
                        UserToProjects.Add(projfile.UserId, projfile.ProjectId);
                    }
                    else
                    {
                        if (UserToProjects[projfile.UserId] < projfile.ProjectId)
                        {
                            UserToProjects[projfile.UserId] = projfile.ProjectId;
                        }

                    }
                }
            }

        }

        [HttpPost("remove/{id}"), DisableRequestSizeLimit]
        [Authorize]
        public async Task<IActionResult> Remove(int id, RemoveRequest request)
        {
            try
            {
                var task = await Task.Run<IActionResult>(async () =>
                {
                    var folderName = Path.Combine("StaticFiles", "UserData_" + id + "\\" + request.SourceId + "\\" + request.ProjectId);
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                    if (!Directory.Exists(pathToSave))
                    {
                        return Ok("None Removed");
                    }
                    if (request.FileId != null && request.FileId.Count > 0)
                    {
                        var list = _context.ProjectFile.Where(x => request.FileId.Contains(x.ProjectFileId) && x.ProjectId == request.ProjectId && x.SourceTypeId == request.SourceId).ToList();
                        foreach (Models.ProjectFile projFile in list)
                        {
                            string fullPath = Path.Combine(pathToSave, projFile.FileName);
                            if (System.IO.File.Exists(fullPath))//_context.ProjectFile)
                            {
                                Models.ProjectFile record = _context.ProjectFile.Find(projFile.ProjectFileId);
                                if (record != null)
                                {
                                    record.IsDeleted = true;
                                    _context.Entry(record).State = EntityState.Modified;
                                    await _context.SaveChangesAsync();
                                }
                                // System.IO.File.Delete(fullPath);
                            }
                        }

                        return Ok("None Removed");
                    }
                    return Ok();
                    //else
                    //{
                    //    foreach (string file in Directory.GetFiles(pathToSave))
                    //    {
                    //        Models.ProjectFile record = _context.ProjectFile.Find(x => x.FileName == file);
                    //        _context.Remove(record);
                    //        _context.SaveChangesAsync();
                    //    }
                    //    //Array.ForEach(Directory.GetFiles(pathToSave), System.IO.File.Delete);
                    //    return Ok("Removed");
                    //}

                });
                return task;
            }
            catch (Exception ex)
            {
                return BadRequest("Exception while Removing");
            }
        }
        [HttpPost("{id}"), DisableRequestSizeLimit]
        //   [Authorize]
        public async Task<IActionResult> Index(int id, [FromForm]CreatePostRequest request)
        {
            try
            {
                Console.WriteLine("enter");
                var usr = User.Identity;
                int? projectid = 0;
                bool newProject = false;
                int file_id_start = 1;
                Models.ProjectFile Pfile = null;
                if (!UserToProjects.ContainsKey(id))
                {

                    UserToProjects.Add(id, 1);
                    projectid = 1;
                    newProject = true;
                }
                else
                {
                    if (request.ProjectId == 0)
                    {
                        UserToProjects[id] += 1;
                        projectid = UserToProjects[id];
                        newProject = true;
                    }
                    else
                    {
                        newProject = true;//for testing wil lhave to be removed
                        Pfile = await _context.ProjectFile.Where(x => x.ProjectId == request.ProjectId && x.UserId == id).FirstOrDefaultAsync();
                        UserToProjects[id] = request.ProjectId;
                        projectid = request.ProjectId;

                        var projects = _context.ProjectFile.Where(x => x.UserId == id).ToList();
                        file_id_start = projects.OrderByDescending(x => x.ProjectFileId).FirstOrDefault().ProjectFileId;
                    }
                }
                var folderName = Path.Combine("StaticFiles", "UserData_" + id, + request.SourceId.ToString() , + request.ProjectId.ToString());
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                List<ProjectFileResponse> ProjResponse = new List<ProjectFileResponse>();
                if (request.Files == null) return NotFound();
                string fullPath = "";
                foreach (IFormFile formfile in request.Files)
                {
                    ProjectFileResponse resp = new ProjectFileResponse();
                    var file = formfile;//request.Files[0];//Request.Form.Files[0];                    

                    if (file.Length > 0)
                    {
                        string fileName = System.Net.Http.Headers.ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        fullPath = Path.Combine(pathToSave, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        resp.FileName = fileName;
                        resp.UserId = id;
                        resp.FileId = file_id_start + 1;
                        resp.Status = "uploaded";
                        resp.UploadedTime = DateTime.Now;
                    }

                    //if new add else update
                    if (newProject == true)
                    {
                        Models.ProjectFile pf = new Models.ProjectFile
                        {
                            FilePath = pathToSave,//folderName,
                            FileName = formfile.FileName,
                            ProjectId = projectid,
                            UserId = id,
                            SourceTypeId = 1
                        };
                        var projectFile = _mapper.Map<Models.ProjectFile>(request);
                        _context.ProjectFile.Add(pf);
                        _context.SaveChanges();
                         resp.FileId = pf.ProjectFileId;

                    }
                    else
                    {
                        Models.ProjectFile pf = new Models.ProjectFile
                        {
                            FilePath = pathToSave,
                            FileName = file.FileName,
                            ProjectId = projectid,
                            //ProjectName = request.ProjectName,
                            SourceTypeId = request.SourceId,
                            //Status = "uploading",
                            UserId = id,
                            // Id = Pfile != null ? Pfile.Id : 0
                        };


                        _context.Entry(pf).State = EntityState.Modified;
                        try
                        {

                            _context.ProjectFile.Update(pf);
                            _context.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            int G = 0;                   
                        }
                    }
                    ProjResponse.Add(resp);
                    file_id_start++;
                }
                return Ok(ProjResponse);
            }
            catch (Exception ex)
            {
                return BadRequest("Upload Failed: " + ex.Message);
            }
        }

    }
}
