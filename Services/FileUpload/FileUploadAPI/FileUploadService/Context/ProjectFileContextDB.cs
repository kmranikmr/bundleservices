using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FileUploadService.Models
{
    public partial class ProjectFileContextDB : DbContext
    {
        public ProjectFileContextDB()
        {
        }

        public ProjectFileContextDB(DbContextOptions<ProjectFileContextDB> options)
            : base(options)
        {
        }

        public virtual DbSet<ProjectFile> ProjectFile { get; set; }
        public virtual DbSet<SourceType> SourceType { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=(localdb)\\v11.0;Database=dap_master;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<ProjectFile>(entity =>
            {
                entity.ToTable("project_file");

                entity.Property(e => e.ProjectFileId).HasColumnName("project_file_id");

                entity.Property(e => e.FileName)
                    .HasColumnName("file_name")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FilePath)
                    .HasColumnName("file_path")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.SourceConfiguration).HasColumnName("source_configuration");

                entity.Property(e => e.SourceTypeId).HasColumnName("source_type_id");

                entity.Property(e => e.UploadDate)
                    .HasColumnName("upload_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.SourceType)
                    .WithMany(p => p.ProjectFile)
                    .HasForeignKey(d => d.SourceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_f__sourc__1ED998B2");
            });

            modelBuilder.Entity<SourceType>(entity =>
            {
                entity.ToTable("source_type");

                entity.Property(e => e.SourceTypeId).HasColumnName("source_type_id");

                entity.Property(e => e.SourceTypeName)
                    .HasColumnName("source_type_name")
                    .HasMaxLength(100);
            });
        }
    }
}
