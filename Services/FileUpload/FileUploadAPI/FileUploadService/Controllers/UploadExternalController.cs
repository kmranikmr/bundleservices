using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FileUploadService.Database;
using FileUploadService.Model;
using FileUploadService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FileUploadService.Utils;
using Newtonsoft.Json;
using RestSharp;

namespace FileUploadService.Controllers
{

    public class ConvertorInfo
    {
        public string Path { get; set; }
        public string[] Files { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class UploadExternalController : ControllerBase
    {

        private IHostingEnvironment _hostingEnvironment;
        private MetaDataStore _metaDataStore;
        private readonly ProjectFileContextDB _context;
        private Dictionary<int, int?> UserToProjects;
        private string apiUrl = @"https://api.bridgedataoutput.com/";// api/v2/OData/abor_ref/Properties?access_token=ab9cdbf09a4eaeb492f6abee6f26473f&$top=200&$skip=";
        private readonly IHubContext<ProgressHub> _progressHubContext;
        public UploadExternalController(ProjectFileContextDB context, IHubContext<ProgressHub> progressHubContext)
        {
            //  _hostingEnvironment = hostingEnvironment;
            //lets bring up current projects if not uploaded yet
            _progressHubContext = progressHubContext;
            _metaDataStore =new MetaDataStore("");
            _context = context;
            UserToProjects = new Dictionary<int, int?>();
            var projects = _context.ProjectFile.ToList();
            foreach (Models.ProjectFile projfile in projects )
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
            int g = 0;
        }


        [HttpPost("remove/{id}"), DisableRequestSizeLimit]
        //  [Authorize]
        public async Task<IActionResult> Remove(int id, RemoveRequest request)
        {
           
                var task = await Task.Run<IActionResult>(async ()=>
                    {
                        try
                        {

                            
                            var folderName = Path.Combine("StaticFiles", "UserData_" + id + "\\" + request.SourceId + "\\" + request.ProjectId);
                            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                            if (!Directory.Exists(pathToSave))
                            {
                                return NotFound();
                            }
                            string[] files = Directory.GetFiles(pathToSave);
                            if ( files == null)
                            {
                                return NotFound();
                            }
                            foreach ( string file in files )
                            {
                                Models.ProjectFile record =  await _context.ProjectFile.Where(x => file.Contains(x.FileName)).FirstOrDefaultAsync();
                                if (record != null)
                                { 
                                    record.IsDeleted = true;
                                    _context.Entry(record).State = EntityState.Modified;
                                    await _context.SaveChangesAsync();
                                }

                               // _context.Remove(record);
                              //  _context.SaveChangesAsync();
                            }
                           // Array.ForEach(Directory.GetFiles(pathToSave), System.IO.File.Delete);
                           
                            return Ok("Removed");
                        }
                        catch (Exception ex)
                        {
                            return BadRequest("Exception while Removing");
                        }
                    });
            return task;
            
        }
        [HttpPost("{id}"), DisableRequestSizeLimit]
       [Authorize]
        public async Task<IActionResult> Index(int id, CreatePostRequest request)
        {
            try
            {
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
                        Pfile = _context.ProjectFile.Where(x => x.ProjectId == request.ProjectId && x.UserId == id).FirstOrDefault();
                        UserToProjects[id] = request.ProjectId;
                        projectid = request.ProjectId;

                        var projects = _context.ProjectFile.Where(x => x.UserId == id).ToList();
                        file_id_start = projects.OrderByDescending(x => x.ProjectFileId).FirstOrDefault().ProjectFileId;
                    }
                }
                var folderName = Path.Combine("StaticFiles", "UserData_" + id + "\\" + request.SourceId + "\\" + request.ProjectId);
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                List<ProjectFileResponse> ProjResponse = new List<ProjectFileResponse>();

                using (var client = new HttpClient())
                {
                    bool haveData = true;

                    int count = 200;
                    ProjectFileResponse resp = new ProjectFileResponse();
                    //Passing service base url  
                    client.BaseAddress = new Uri(apiUrl);

                    client.DefaultRequestHeaders.Clear();
                    //Define request data format  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    while (haveData)
                    {
                        string fileName = $"{request.ProjectName}_TempFile_{id}_{count-200}_{count}";
                        string fullPath = Path.Combine(pathToSave, fileName);
                        int max = 10000;
                        using (var writer = new StreamWriter(fullPath))
                        {
                          
                            //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                            HttpResponseMessage Res = await client.GetAsync($"api/v2/OData/abor_ref/Properties?access_token=ab9cdbf09a4eaeb492f6abee6f26473f&$top=200&$skip={count}");

                            //Checking the response is successful or not which is sent using HttpClient  
                            if (Res.IsSuccessStatusCode)
                            {
                                string json = Res.Content.ReadAsStringAsync().Result;
                                writer.Write(json);
                                count += 200;
                                resp.FileName = fileName;
                                resp.UserId = id;
                                resp.FileId = file_id_start;
                                resp.Status = "uploaded";
                                resp.UploadedTime = DateTime.Now;
                                await _progressHubContext.Clients.User(User.Identity?.Name)//(ProgressHub.GROUP_NAME)
                                        .SendAsync("progressChanged", (int)(count* 100/max));// resp);
                                //if new add else update
                                if (newProject == true)
                                {
                                    Models.ProjectFile pf = new Models.ProjectFile
                                    {
                                        FilePath = folderName,
                                        FileName = fileName,
                                        ProjectId = projectid,
                                       // ProjectName = request.ProjectName,
                                        SourceTypeId = request.SourceId,
                                       // Status = "uploaded",
                                        UserId = id,
                                      //  ProjectFileId = file_id_start
                                    };
                                    _context.ProjectFile.Add(pf);
                                    _context.SaveChanges();
                                }
                                else
                                {
                                    Models.ProjectFile pf = new Models.ProjectFile
                                    {
                                        FilePath = folderName,
                                        FileName = fileName,
                                        ProjectId = projectid,
                                       // ProjectName = request.ProjectName,
                                        SourceTypeId = request.SourceId,
                                       // Status = "uploading",
                                        UserId = id,
                                       // ProjectFileId = 
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
                            else
                            {
                                haveData = false;
                            }
                        }

                    }
                }
                
                return Ok( ProjResponse);
            }
            catch (Exception ex)
            {
                return BadRequest("Upload Failed: " + ex.Message);
            }
        }

          [HttpPost("S3Data")]
        [AllowAnonymous]
        public async Task<IActionResult> S3Data (S3info s3Info)
        {
            int indexPath = s3Info.key.LastIndexOf('/');
            string path = "";
            string filename = "";
            if( indexPath >= 0 )
            {
                path = s3Info.key.Substring(0, indexPath);
                filename = s3Info.key.Substring(indexPath + 1);
            }
            Console.WriteLine($"path {path} filename {filename}");
            //var s3folderName = Path.Combine("AutoIngestion", path);
            var folderName = Path.Combine("AutoIngestion", path,"tbp");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }
                       
            string fullPath = Path.Combine(pathToSave, filename);
            Console.WriteLine ($"foldername {folderName} pathToSave {pathToSave} fullPath {fullPath} ");
            var files = await S3Helper.GetFiles(pathToSave, s3Info.bucketname, s3Info.key, path);
            
            var url = "http://EC2BasedServiceALB-760561316.us-east-1.elb.amazonaws.com:6011/convert";
            var restClient = new RestClient(url);
            var requestTable = new RestRequest(Method.POST);
            requestTable.AddHeader("Accept", "application/json");
            ConvertorInfo info = new ConvertorInfo();
            if (files != null && files.Length > 0)
            {
           
                info.Path = Path.GetFullPath(files[0]);
                Console.WriteLine(info.Path);
                List<string> fileList = new List<string>();
                foreach (var file in files)
                {
                    fileList.Add(file);
                }
                info.Files = fileList.ToArray();
                Console.WriteLine(" all files "+ string.Join(",",info.Files));
            }
            
            var json = JsonConvert.SerializeObject(info);
            Console.WriteLine(json);
            requestTable.AddParameter("application/json", json, ParameterType.RequestBody);
            Console.WriteLine("Post|ExecuteAsync Start");
            IRestResponse response = await restClient.ExecuteAsync(requestTable);
             Console.WriteLine("Post|ExecuteAsync Done" + response?.Content);
            //S3Helper.RunConversion(folderName, filename);
            return Ok();
        }


        //[HttpPost("{id}/twitter"), DisableRequestSizeLimit]
      //  [Authorize]
        //public async Task<IActionResult> Index(int id, CreatePostRequest request)
        //{
        //    try
        //    {
               
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
    }
}
