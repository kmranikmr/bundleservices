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
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using FileUploadService.Filters;
using System.Globalization;
using System.Net;
using FileUploadService.Utils;

namespace FileUploadService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {

        private IHostingEnvironment _hostingEnvironment;
        private MetaDataStore _metaDataStore;
        private readonly ProjectFileContextDB _context;
        private Dictionary<int, int?> UserToProjects;
        private readonly IHubContext<ProgressHub> _progressHubContext;
        private readonly long _fileSizeLimit;
        private readonly string _targetFilePath;
        ////private readonly ILogger<StreamingController> _logger;
        private static readonly FormOptions _defaultFormOptions = new FormOptions();
        private readonly string[] _permittedExtensions = { ".csv", ".json", ".log", "txt" };
        public StreamController(ProjectFileContextDB context, IHubContext<ProgressHub> progressHubContext)
        {
            //  _hostingEnvironment = hostingEnvironment;
            //lets bring up current projects if not uploaded yet
            _metaDataStore = new MetaDataStore("");
            _context = context;
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
            int g = 0;
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
        [HttpPost("{projectid}"), DisableRequestSizeLimit]
        [DisableFormValueModelBinding]
       // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int projectid)//, [FromForm]CreatePostRequest request)
        {
           int id = 1;
            FormValueProvider formValueProvider = null;
            //var folderName = Path.Combine("StaticFiles", "UserData_" + id + "\\" + request.SourceId + "\\" + request.ProjectId);
            var folderName = Path.Combine("StaticFiles", "UserData_" + id + "\\" + id + "\\" + id);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }

            List<ProjectFileResponse> ProjResponse = new List<ProjectFileResponse>();

            string fileName = "test.out";
           /// foreach (IFormFile formfile in request.Files)
            {
                // var file = formfile;
                /// fileName = System.Net.Http.Headers.ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                //foreach (var file in Request.Form.Files)
                {
                    string fullPath = Path.Combine(pathToSave, fileName);
                    using (var stream = System.IO.File.Create(fullPath))
                    {
                        formValueProvider = await Request.StreamFile(stream);
                    }
                    var model = new StreamPostRequest();
                    var bindingSuccessful = await TryUpdateModelAsync(model, prefix: "",
                        valueProvider: formValueProvider);

                    
                    if (!bindingSuccessful)
                    {
                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }
                    }
                }
            }


            return Ok();////Json(uploadedData);
        }


   


    }

}
