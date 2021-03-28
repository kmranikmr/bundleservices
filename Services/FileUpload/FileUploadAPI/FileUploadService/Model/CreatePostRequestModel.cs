using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadService.Model
{
    public class TestRequest
    {
        public int ProjectId { get; set; }
       // public List<IFormFile> Files { get; set; }
    }
    public class CreatePostRequest
    {
      
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }

        public string ProjectDescription { get; set; }
        public int SourceId { get; set; }
      
        public List<IFormFile> Files { get; set; }  
    }

    public class StreamPostRequest
    {

        public string ProjectName { get; set; }
        public int ProjectId { get; set; }

        public string ProjectDescription { get; set; }
        public int SourceId { get; set; }
        public List<string> FileNames { get; set; }

    }

    public class RemoveRequest
    {
        public RemoveRequest()
        {
            FileId = new List<int>();
        }
        public int ProjectId { get; set; }
        public int SourceId { get; set; }
        public List<int>  FileId { get; set; }
    }
 }
