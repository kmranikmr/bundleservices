using FileUploadService.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadService.Database
{
    public class ProjectFileDbContext : DbContext
    {
        public DbSet<ProjectFile> ProjectFile { get; set; }

        public ProjectFileDbContext(DbContextOptions<ProjectFileDbContext> options)
            : base(options)
        {

        }
    }
}
