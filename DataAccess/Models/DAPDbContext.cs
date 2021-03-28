using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataAccess.Models
{
    public partial class DAPDbContext : DbContext
    {
        public DAPDbContext()
        {
        }

        public DAPDbContext(DbContextOptions<DAPDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<JobStatus> JobStatus { get; set; }
        public virtual DbSet<ModelMetadata> ModelMetadatas { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectAutomation> ProjectAutomations { get; set; }
        public virtual DbSet<ProjectFile> ProjectFiles { get; set; }
        public virtual DbSet<ProjectReader> ProjectReaders { get; set; }
        public virtual DbSet<ProjectSchema> ProjectSchemas { get; set; }
        public virtual DbSet<ProjectUser> ProjectUsers { get; set; }
        public virtual DbSet<ProjectWriter> ProjectWriters { get; set; }
        public virtual DbSet<Reader> Readers { get; set; }
        public virtual DbSet<ReaderType> ReaderTypes { get; set; }
        public virtual DbSet<SchemaModel> SchemaModels { get; set; }
        public virtual DbSet<SearchGraph> SearchGraphs { get; set; }
        public virtual DbSet<SearchHistory> SearchHistories { get; set; }
        public virtual DbSet<SourceType> SourceTypes { get; set; }
        public virtual DbSet<UserApiKey> UserApiKeys { get; set; }
        public virtual DbSet<UserApiKeyLog> UserApiKeyLogs { get; set; }
        public virtual DbSet<UserSharedUrl> UserSharedUrls { get; set; }
        public virtual DbSet<WorkflowAutomation> WorkflowAutomations { get; set; }
        public virtual DbSet<WorkflowAutomationState> WorkflowAutomationStates { get; set; }
        public virtual DbSet<WorkflowElement> WorkflowElements { get; set; }
        public virtual DbSet<WorkflowModelMetadata> WorkflowModelMetadatas { get; set; }
        public virtual DbSet<WorkflowMonitor> WorkflowMonitors { get; set; }
        public virtual DbSet<WorkflowOutputModel> WorkflowOutputModels { get; set; }
        public virtual DbSet<WorkflowProject> WorkflowProjects { get; set; }
        public virtual DbSet<WorkflowSearchGraph> WorkflowSearchGraphs { get; set; }
        public virtual DbSet<WorkflowSearchHistory> WorkflowSearchHistories { get; set; }
        public virtual DbSet<WorkflowServerType> WorkflowServerTypes { get; set; }
        public virtual DbSet<WorkflowSessionAttempt> WorkflowSessionAttempts { get; set; }
        public virtual DbSet<WorkflowSessionLog> WorkflowSessionLogs { get; set; }
        public virtual DbSet<WorkflowStateModelMap> WorkflowStateModelMaps { get; set; }
        public virtual DbSet<WorkflowStatusType> WorkflowStatusTypes { get; set; }
        public virtual DbSet<WorkflowTest> WorkflowTests { get; set; }
        public virtual DbSet<WorkflowVersion> WorkflowVersions { get; set; }
        public virtual DbSet<Writer> Writers { get; set; }
        public virtual DbSet<WriterType> WriterTypes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=dap_master;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => new { e.JobId, e.ProjectFileId });

                entity.ToTable("job");

                entity.Property(e => e.JobId).HasColumnName("job_id");

                entity.Property(e => e.ProjectFileId).HasColumnName("project_file_id");

                entity.Property(e => e.CompletedOn).HasColumnName("completed_on");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.JobDescription)
                    .HasColumnName("job_description")
                    .IsUnicode(false);

                entity.Property(e => e.JobStatusId).HasColumnName("job_status_id");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.SchemaId).HasColumnName("schema_id");

                entity.Property(e => e.StartedOn).HasColumnName("started_on");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.JobStatus)
                    .WithMany(p => p.Jobs)
                    .HasForeignKey(d => d.JobStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__job__job_status___72C60C4A");

                entity.HasOne(d => d.ProjectFile)
                    .WithMany(p => p.Jobs)
                    .HasForeignKey(d => d.ProjectFileId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__job__project_fil__71D1E811");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Jobs)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__job__project_id__73BA3083");

                entity.HasOne(d => d.Schema)
                    .WithMany(p => p.Jobs)
                    .HasForeignKey(d => d.SchemaId)
                    .HasConstraintName("FK__job__schema_id__74AE54BC");
            });

            modelBuilder.Entity<JobStatus>(entity =>
            {
                entity.HasKey(e => e.JobStatusId);

                entity.ToTable("job_status");

                entity.Property(e => e.JobStatusId).HasColumnName("job_status_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.StatusName)
                    .HasColumnName("status_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ModelMetadata>(entity =>
            {
                entity.HasKey(e => e.MetadataId);

                entity.ToTable("model_metadata");

                entity.Property(e => e.MetadataId).HasColumnName("metadata_id");

                entity.Property(e => e.ColumnName)
                    .HasColumnName("column_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DataType)
                    .HasColumnName("data_type")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ModelId).HasColumnName("model_id");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.HasOne(d => d.Model)
                    .WithMany(p => p.ModelMetadatas)
                    .HasForeignKey(d => d.ModelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__model_met__model__66603565");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ModelMetadatas)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__model_met__proje__656C112C");
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("project");

                entity.HasIndex(e => e.ProjectName)
                    .HasName("UQ__project__4A0B0D699EB55863")
                    .IsUnique();

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.IsFavorite).HasColumnName("is_favorite");

                entity.Property(e => e.LastAccessedOn)
                    .HasColumnName("last_accessed_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ProjectDescription)
                    .IsRequired()
                    .HasColumnName("project_description")
                    .HasMaxLength(450);

                entity.Property(e => e.ProjectName)
                    .IsRequired()
                    .HasColumnName("project_name")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<ProjectAutomation>(entity =>
            {
                entity.ToTable("project_automation");

                entity.Property(e => e.ProjectAutomationId).HasColumnName("project_automation_id");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.FolderPath)
                    .IsRequired()
                    .HasColumnName("folder_path");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.ProjectSchemaId).HasColumnName("project_schema_id");

                entity.Property(e => e.ReaderId).HasColumnName("reader_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectAutomations)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_a__proje__367C1819");

                entity.HasOne(d => d.ProjectSchema)
                    .WithMany(p => p.ProjectAutomations)
                    .HasForeignKey(d => d.ProjectSchemaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_a__proje__3864608B");

                entity.HasOne(d => d.Reader)
                    .WithMany(p => p.ProjectAutomations)
                    .HasForeignKey(d => d.ReaderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_a__reade__37703C52");
            });

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

                entity.Property(e => e.ReaderId).HasColumnName("reader_id");

                entity.Property(e => e.SchemaId).HasColumnName("schema_id");

                entity.Property(e => e.SourceConfiguration).HasColumnName("source_configuration");

                entity.Property(e => e.SourceTypeId).HasColumnName("source_type_id");

                entity.Property(e => e.UploadDate)
                    .HasColumnName("upload_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectFiles)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_f__proje__5629CD9C");

                entity.HasOne(d => d.Reader)
                    .WithMany(p => p.ProjectFiles)
                    .HasForeignKey(d => d.ReaderId)
                    .HasConstraintName("FK__project_f__reade__59063A47");

                entity.HasOne(d => d.Schema)
                    .WithMany(p => p.ProjectFiles)
                    .HasForeignKey(d => d.SchemaId)
                    .HasConstraintName("FK__project_f__schem__59FA5E80");

                entity.HasOne(d => d.SourceType)
                    .WithMany(p => p.ProjectFiles)
                    .HasForeignKey(d => d.SourceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_f__sourc__571DF1D5");
            });

            modelBuilder.Entity<ProjectReader>(entity =>
            {
                entity.HasKey(e => new { e.ProjectId, e.ReaderId });

                entity.ToTable("project_reader");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.ReaderId).HasColumnName("reader_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectReaders)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_r__proje__4CA06362");

                entity.HasOne(d => d.Reader)
                    .WithMany(p => p.ProjectReaders)
                    .HasForeignKey(d => d.ReaderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_r__reade__4D94879B");
            });

            modelBuilder.Entity<ProjectSchema>(entity =>
            {
                entity.HasKey(e => e.SchemaId);

                entity.ToTable("project_schema");

                entity.Property(e => e.SchemaId).HasColumnName("schema_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.SchemaName)
                    .IsRequired()
                    .HasColumnName("schema_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TypeConfig)
                    .IsRequired()
                    .HasColumnName("type_config")
                    .IsUnicode(false);

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectSchemas)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_s__proje__5070F446");
            });

            modelBuilder.Entity<ProjectUser>(entity =>
            {
                entity.HasKey(e => new { e.ProjectId, e.UserId });

                entity.ToTable("project_user");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PermissionBit).HasColumnName("permission_bit");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectUsers)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_u__proje__2B3F6F97");
            });

            modelBuilder.Entity<ProjectWriter>(entity =>
            {
                entity.HasKey(e => new { e.ProjectId, e.WriterId });

                entity.ToTable("project_writer");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.WriterId).HasColumnName("writer_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectWriters)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_w__proje__48CFD27E");

                entity.HasOne(d => d.Writer)
                    .WithMany(p => p.ProjectWriters)
                    .HasForeignKey(d => d.WriterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__project_w__write__49C3F6B7");
            });

            modelBuilder.Entity<Reader>(entity =>
            {
                entity.ToTable("reader");

                entity.Property(e => e.ReaderId).HasColumnName("reader_id");

                entity.Property(e => e.ConfigurationName)
                    .HasColumnName("configuration_name")
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ReaderConfiguration).HasColumnName("reader_configuration");

                entity.Property(e => e.ReaderTypeId).HasColumnName("reader_type_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.ReaderType)
                    .WithMany(p => p.Readers)
                    .HasForeignKey(d => d.ReaderTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__reader__reader_t__4316F928");
            });

            modelBuilder.Entity<ReaderType>(entity =>
            {
                entity.ToTable("reader_type");

                entity.HasIndex(e => e.ReaderTypeName)
                    .HasName("UQ__reader_t__8BCDA5A9693B60D0")
                    .IsUnique();

                entity.Property(e => e.ReaderTypeId).HasColumnName("reader_type_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ReaderTypeName)
                    .IsRequired()
                    .HasColumnName("reader_type_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SchemaModel>(entity =>
            {
                entity.HasKey(e => e.ModelId);

                entity.ToTable("schema_model");

                entity.Property(e => e.ModelId).HasColumnName("model_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ModelConfig).HasColumnName("model_config");

                entity.Property(e => e.ModelName)
                    .HasColumnName("model_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.SchemaId).HasColumnName("schema_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.SchemaModels)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__schema_mo__proje__5FB337D6");

                entity.HasOne(d => d.Schema)
                    .WithMany(p => p.SchemaModels)
                    .HasForeignKey(d => d.SchemaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__schema_mo__schem__5EBF139D");
            });

            modelBuilder.Entity<SearchGraph>(entity =>
            {
                entity.ToTable("search_graph");

                entity.Property(e => e.SearchGraphId).HasColumnName("search_graph_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.GraphDescription).HasColumnName("graph_description");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.SearchHistoryId).HasColumnName("search_history_id");

                entity.HasOne(d => d.SearchHistory)
                    .WithMany(p => p.SearchGraphs)
                    .HasForeignKey(d => d.SearchHistoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__search_gr__searc__04E4BC85");
            });

            modelBuilder.Entity<SearchHistory>(entity =>
            {
                entity.ToTable("search_history");

                entity.HasIndex(e => e.Md5)
                    .HasName("UQ__search_h__DF502978CAD6D233")
                    .IsUnique();

                entity.HasIndex(e => e.SearchHistoryName)
                    .HasName("UQ__search_h__398B8157921017F4")
                    .IsUnique();

                entity.Property(e => e.SearchHistoryId).HasColumnName("search_history_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.LastExecutedOn)
                    .HasColumnName("last_executed_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Md5)
                    .HasColumnName("md5")
                    .HasMaxLength(50);

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.ResolvedSearchQuery).HasColumnName("resolved_search_query");

                entity.Property(e => e.SearchHistoryName)
                    .HasColumnName("search_history_name")
                    .HasMaxLength(50);

                entity.Property(e => e.SearchQuery).HasColumnName("search_query");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.WriterId).HasColumnName("writer_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.SearchHistories)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__search_hi__proje__7D439ABD");

                entity.HasOne(d => d.Writer)
                    .WithMany(p => p.SearchHistories)
                    .HasForeignKey(d => d.WriterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__search_hi__write__7E37BEF6");
            });

            modelBuilder.Entity<SourceType>(entity =>
            {
                entity.ToTable("source_type");

                entity.Property(e => e.SourceTypeId).HasColumnName("source_type_id");

                entity.Property(e => e.SourceTypeName)
                    .HasColumnName("source_type_name")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<UserApiKey>(entity =>
            {
                entity.ToTable("user_api_key");

                entity.Property(e => e.UserApiKeyId).HasColumnName("user_api_key_id");

                entity.Property(e => e.ApiKey)
                    .HasColumnName("api_key")
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Scope)
                    .HasColumnName("scope")
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedOn)
                    .HasColumnName("updated_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("user_id");
            });

            modelBuilder.Entity<UserApiKeyLog>(entity =>
            {
                entity.ToTable("user_api_key_log");

                entity.Property(e => e.UserApiKeyLogId).HasColumnName("user_api_key_log_id");

                entity.Property(e => e.AccessedOn)
                    .HasColumnName("accessed_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.AccessedUrl)
                    .HasColumnName("accessed_url")
                    .HasMaxLength(255);

                entity.Property(e => e.AccessedUrlBody)
                    .HasColumnName("accessed_url_body")
                    .HasMaxLength(1000);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Metadata)
                    .HasColumnName("metadata")
                    .HasMaxLength(1000);

                entity.Property(e => e.UserApiKeyId).HasColumnName("user_api_key_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");
            });

            modelBuilder.Entity<UserSharedUrl>(entity =>
            {
                entity.ToTable("user_shared_url");

                entity.Property(e => e.UserSharedUrlId).HasColumnName("user_shared_url_id");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.SearchHistoryId).HasColumnName("search_history_id");

                entity.Property(e => e.SharedUrl)
                    .HasColumnName("shared_url")
                    .HasMaxLength(255);

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.WorkflowSearchHistoryId).HasColumnName("workflow_search_history_id");
            });

            modelBuilder.Entity<WorkflowAutomation>(entity =>
            {
                entity.ToTable("workflow_automation");

                entity.Property(e => e.WorkflowAutomationId).HasColumnName("workflow_automation_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.WorkflowAutomationStateId).HasColumnName("workflow_automation_state_id");

                entity.Property(e => e.WorkflowProjectId).HasColumnName("workflow_project_id");

                entity.Property(e => e.WorkflowVersionId).HasColumnName("workflow_version_id");

                entity.HasOne(d => d.WorkflowAutomationState)
                    .WithMany(p => p.WorkflowAutomations)
                    .HasForeignKey(d => d.WorkflowAutomationStateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__69FBBC1F");

                entity.HasOne(d => d.WorkflowProject)
                    .WithMany(p => p.WorkflowAutomations)
                    .HasForeignKey(d => d.WorkflowProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__681373AD");

                entity.HasOne(d => d.WorkflowVersion)
                    .WithMany(p => p.WorkflowAutomations)
                    .HasForeignKey(d => d.WorkflowVersionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__690797E6");
            });

            modelBuilder.Entity<WorkflowAutomationState>(entity =>
            {
                entity.ToTable("workflow_automation_state");

                entity.Property(e => e.WorkflowAutomationStateId).HasColumnName("workflow_automation_state_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.StateStatus)
                    .IsRequired()
                    .HasColumnName("state_status")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.WorkflowVersionId).HasColumnName("workflow_version_id");

                entity.HasOne(d => d.WorkflowVersion)
                    .WithMany(p => p.WorkflowAutomationStates)
                    .HasForeignKey(d => d.WorkflowVersionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__58D1301D");
            });

            modelBuilder.Entity<WorkflowElement>(entity =>
            {
                entity.ToTable("workflow_element");

                entity.Property(e => e.WorkflowElementId).HasColumnName("workflow_element_id");

                entity.Property(e => e.ElementIconName)
                    .HasColumnName("element_icon_name")
                    .HasMaxLength(255);

                entity.Property(e => e.ElementName)
                    .IsRequired()
                    .HasColumnName("element_name")
                    .HasMaxLength(255);

                entity.Property(e => e.ElementProperties).HasColumnName("element_properties");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            });

            modelBuilder.Entity<WorkflowModelMetadata>(entity =>
            {
                entity.ToTable("workflow_model_metadata");

                entity.Property(e => e.WorkflowModelMetadataId).HasColumnName("workflow_model_metadata_id");

                entity.Property(e => e.ColumnName)
                    .HasColumnName("column_name")
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.WorkflowOutputModelId).HasColumnName("workflow_output_model_id");

                entity.Property(e => e.WorkflowVersionId).HasColumnName("workflow_version_id");

                entity.HasOne(d => d.WorkflowOutputModel)
                    .WithMany(p => p.WorkflowModelMetadatas)
                    .HasForeignKey(d => d.WorkflowOutputModelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__4A8310C6");

                entity.HasOne(d => d.WorkflowVersion)
                    .WithMany(p => p.WorkflowModelMetadatas)
                    .HasForeignKey(d => d.WorkflowVersionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__498EEC8D");
            });

            modelBuilder.Entity<WorkflowMonitor>(entity =>
            {
                entity.ToTable("workflow_monitor");

                entity.Property(e => e.WorkflowMonitorId).HasColumnName("workflow_monitor_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ModelId).HasColumnName("model_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.WorkflowOutputModelId).HasColumnName("workflow_output_model_id");

                entity.Property(e => e.WorkflowProjectId).HasColumnName("workflow_project_id");

                entity.Property(e => e.WorkflowVersionId).HasColumnName("workflow_version_id");

                entity.HasOne(d => d.Model)
                    .WithMany(p => p.WorkflowMonitors)
                    .HasForeignKey(d => d.ModelId)
                    .HasConstraintName("FK__workflow___model__5224328E");

                entity.HasOne(d => d.WorkflowOutputModel)
                    .WithMany(p => p.WorkflowMonitors)
                    .HasForeignKey(d => d.WorkflowOutputModelId)
                    .HasConstraintName("FK__workflow___workf__531856C7");

                entity.HasOne(d => d.WorkflowProject)
                    .WithMany(p => p.WorkflowMonitors)
                    .HasForeignKey(d => d.WorkflowProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__503BEA1C");

                entity.HasOne(d => d.WorkflowVersion)
                    .WithMany(p => p.WorkflowMonitors)
                    .HasForeignKey(d => d.WorkflowVersionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__51300E55");
            });

            modelBuilder.Entity<WorkflowOutputModel>(entity =>
            {
                entity.ToTable("workflow_output_model");

                entity.Property(e => e.WorkflowOutputModelId).HasColumnName("workflow_output_model_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DisplayName)
                    .HasColumnName("display_name")
                    .HasMaxLength(100);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.TableName)
                    .HasColumnName("table_name")
                    .HasMaxLength(100);

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.WorkflowProjectId).HasColumnName("workflow_project_id");

                entity.Property(e => e.WorkflowVersionId).HasColumnName("workflow_version_id");

                entity.HasOne(d => d.WorkflowProject)
                    .WithMany(p => p.WorkflowOutputModels)
                    .HasForeignKey(d => d.WorkflowProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__42E1EEFE");

                entity.HasOne(d => d.WorkflowVersion)
                    .WithMany(p => p.WorkflowOutputModels)
                    .HasForeignKey(d => d.WorkflowVersionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__43D61337");
            });

            modelBuilder.Entity<WorkflowProject>(entity =>
            {
                entity.ToTable("workflow_project");

                entity.Property(e => e.WorkflowProjectId).HasColumnName("workflow_project_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(255);

                entity.Property(e => e.ExternalProjectId).HasColumnName("external_project_id");

                entity.Property(e => e.ExternalProjectName)
                    .HasColumnName("external_project_name")
                    .HasMaxLength(255);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.RecentVersionNumber)
                    .HasColumnName("recent_version_number")
                    .HasMaxLength(100);

                entity.Property(e => e.UpdatedOn)
                    .HasColumnName("updated_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.WorkflowServerTypeId).HasColumnName("workflow_server_type_id");

                entity.HasOne(d => d.WorkflowServerType)
                    .WithMany(p => p.WorkflowProjects)
                    .HasForeignKey(d => d.WorkflowServerTypeId)
                    .HasConstraintName("FK__workflow___workf__0D7A0286");
            });

            modelBuilder.Entity<WorkflowSearchGraph>(entity =>
            {
                entity.ToTable("workflow_search_graph");

                entity.Property(e => e.WorkflowSearchGraphId).HasColumnName("workflow_search_graph_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.GraphDescription).HasColumnName("graph_description");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.WorkflowSearchHistoryId).HasColumnName("workflow_search_history_id");

                entity.HasOne(d => d.WorkflowSearchHistory)
                    .WithMany(p => p.WorkflowSearchGraphs)
                    .HasForeignKey(d => d.WorkflowSearchHistoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__3E1D39E1");
            });

            modelBuilder.Entity<WorkflowSearchHistory>(entity =>
            {
                entity.ToTable("workflow_search_history");

                entity.HasIndex(e => e.Md5)
                    .HasName("UQ__workflow__DF50297844FD06F6")
                    .IsUnique();

                entity.HasIndex(e => e.WorkflowSearchHistoryName)
                    .HasName("UQ__workflow__BED47C7F5C62B2B8")
                    .IsUnique();

                entity.Property(e => e.WorkflowSearchHistoryId).HasColumnName("workflow_search_history_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.LastExecutedOn)
                    .HasColumnName("last_executed_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Md5)
                    .HasColumnName("md5")
                    .HasMaxLength(50);

                entity.Property(e => e.ResolvedSearchQuery).HasColumnName("resolved_search_query");

                entity.Property(e => e.SearchQuery).HasColumnName("search_query");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.WorkflowProjectId).HasColumnName("workflow_project_id");

                entity.Property(e => e.WorkflowSearchHistoryName)
                    .HasColumnName("workflow_search_history_name")
                    .HasMaxLength(50);

                entity.Property(e => e.WorkflowVersionId).HasColumnName("workflow_version_id");

                entity.HasOne(d => d.WorkflowProject)
                    .WithMany(p => p.WorkflowSearchHistories)
                    .HasForeignKey(d => d.WorkflowProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__2FCF1A8A");

                entity.HasOne(d => d.WorkflowVersion)
                    .WithMany(p => p.WorkflowSearchHistories)
                    .HasForeignKey(d => d.WorkflowVersionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__30C33EC3");
            });

            modelBuilder.Entity<WorkflowServerType>(entity =>
            {
                entity.ToTable("workflow_server_type");

                entity.Property(e => e.WorkflowServerTypeId)
                    .HasColumnName("workflow_server_type_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.WorkflowServerTypeName)
                    .HasColumnName("workflow_server_type_name")
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<WorkflowSessionAttempt>(entity =>
            {
                entity.ToTable("workflow_session_attempt");

                entity.Property(e => e.WorkflowSessionAttemptId).HasColumnName("workflow_session_attempt_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EndDate).HasColumnName("end_date");

                entity.Property(e => e.ExternalAttemptId).HasColumnName("external_attempt_id");

                entity.Property(e => e.ExternalProjectId).HasColumnName("external_project_id");

                entity.Property(e => e.ExternalWorkflowId).HasColumnName("external_workflow_id");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Result)
                    .HasColumnName("result")
                    .HasMaxLength(255);

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.VersionNumber).HasColumnName("version_number");

                entity.Property(e => e.WorkflowAutomationId).HasColumnName("workflow_automation_id");

                entity.Property(e => e.WorkflowProjectId).HasColumnName("workflow_project_id");

                entity.Property(e => e.WorkflowStatusTypeId).HasColumnName("workflow_status_type_id");

                entity.Property(e => e.WorkflowVersionId).HasColumnName("workflow_version_id");

                entity.HasOne(d => d.WorkflowProject)
                    .WithMany(p => p.WorkflowSessionAttempts)
                    .HasForeignKey(d => d.WorkflowProjectId)
                    .HasConstraintName("FK__workflow___workf__1CBC4616");

                entity.HasOne(d => d.WorkflowStatusType)
                    .WithMany(p => p.WorkflowSessionAttempts)
                    .HasForeignKey(d => d.WorkflowStatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__1EA48E88");

                entity.HasOne(d => d.WorkflowVersion)
                    .WithMany(p => p.WorkflowSessionAttempts)
                    .HasForeignKey(d => d.WorkflowVersionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__1BC821DD");
            });

            modelBuilder.Entity<WorkflowSessionLog>(entity =>
            {
                entity.ToTable("workflow_session_log");

                entity.Property(e => e.WorkflowSessionLogId).HasColumnName("workflow_session_log_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ExternalProjectId).HasColumnName("external_project_id");

                entity.Property(e => e.LogData).HasColumnName("log_data");

                entity.Property(e => e.VersionNumber).HasColumnName("version_number");

                entity.Property(e => e.WorkflowProjectId).HasColumnName("workflow_project_id");

                entity.Property(e => e.WorkflowSessionAttemptId).HasColumnName("workflow_session_attempt_id");

                entity.HasOne(d => d.WorkflowProject)
                    .WithMany(p => p.WorkflowSessionLogs)
                    .HasForeignKey(d => d.WorkflowProjectId)
                    .HasConstraintName("FK__workflow___workf__25518C17");

                entity.HasOne(d => d.WorkflowSessionAttempt)
                    .WithMany(p => p.WorkflowSessionLogs)
                    .HasForeignKey(d => d.WorkflowSessionAttemptId)
                    .HasConstraintName("FK__workflow___workf__245D67DE");
            });

            modelBuilder.Entity<WorkflowStateModelMap>(entity =>
            {
                entity.ToTable("workflow_state_model_map");

                entity.Property(e => e.WorkflowStateModelMapId).HasColumnName("workflow_state_model_map_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ModelId).HasColumnName("model_id");

                entity.Property(e => e.SessionId).HasColumnName("session_id");

                entity.Property(e => e.WorkflowAutomationStateId).HasColumnName("workflow_automation_state_id");

                entity.Property(e => e.WorkflowOutputModelId).HasColumnName("workflow_output_model_id");

                entity.Property(e => e.WorkflowVersionId).HasColumnName("workflow_version_id");

                entity.HasOne(d => d.Model)
                    .WithMany(p => p.WorkflowStateModelMaps)
                    .HasForeignKey(d => d.ModelId)
                    .HasConstraintName("FK__workflow___model__6166761E");

                entity.HasOne(d => d.WorkflowAutomationState)
                    .WithMany(p => p.WorkflowStateModelMaps)
                    .HasForeignKey(d => d.WorkflowAutomationStateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__5F7E2DAC");

                entity.HasOne(d => d.WorkflowOutputModel)
                    .WithMany(p => p.WorkflowStateModelMaps)
                    .HasForeignKey(d => d.WorkflowOutputModelId)
                    .HasConstraintName("FK__workflow___workf__625A9A57");

                entity.HasOne(d => d.WorkflowVersion)
                    .WithMany(p => p.WorkflowStateModelMaps)
                    .HasForeignKey(d => d.WorkflowVersionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__607251E5");
            });

            modelBuilder.Entity<WorkflowStatusType>(entity =>
            {
                entity.ToTable("workflow_status_type");

                entity.Property(e => e.WorkflowStatusTypeId)
                    .HasColumnName("workflow_status_type_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.WorkflowStatusTypeName)
                    .IsRequired()
                    .HasColumnName("workflow_status_type_name")
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<WorkflowTest>(entity =>
            {
                entity.ToTable("workflow_test");

                entity.Property(e => e.WorkflowTestId).HasColumnName("workflow_test_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ExternalAttemptId).HasColumnName("external_attempt_id");

                entity.Property(e => e.ExternalProjectId).HasColumnName("external_project_id");

                entity.Property(e => e.ExternalWorkflowId).HasColumnName("external_workflow_id");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.LogData).HasColumnName("log_data");

                entity.Property(e => e.Result)
                    .HasColumnName("result")
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedOn)
                    .HasColumnName("updated_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.WorkflowJson).HasColumnName("workflow_json");

                entity.Property(e => e.WorkflowProjectId).HasColumnName("workflow_project_id");

                entity.Property(e => e.WorkflowPropertyJson).HasColumnName("workflow_property_json");

                entity.Property(e => e.WorkflowStatusTypeId).HasColumnName("workflow_status_type_id");

                entity.Property(e => e.WorkflowVersionId).HasColumnName("workflow_version_id");

                entity.HasOne(d => d.WorkflowProject)
                    .WithMany(p => p.WorkflowTests)
                    .HasForeignKey(d => d.WorkflowProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__6FB49575");

                entity.HasOne(d => d.WorkflowStatusType)
                    .WithMany(p => p.WorkflowTests)
                    .HasForeignKey(d => d.WorkflowStatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__73852659");

                entity.HasOne(d => d.WorkflowVersion)
                    .WithMany(p => p.WorkflowTests)
                    .HasForeignKey(d => d.WorkflowVersionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__70A8B9AE");
            });

            modelBuilder.Entity<WorkflowVersion>(entity =>
            {
                entity.ToTable("workflow_version");

                entity.Property(e => e.WorkflowVersionId).HasColumnName("workflow_version_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ExternalProjectId).HasColumnName("external_project_id");

                entity.Property(e => e.ExternalWorkflowId).HasColumnName("external_workflow_id");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.IsPublished)
                    .HasColumnName("is_published")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.LastWorkflowSessionAttemptId).HasColumnName("last_workflow_session_attempt_id");

                entity.Property(e => e.OutputModelName)
                    .HasColumnName("output_model_name")
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedOn)
                    .HasColumnName("updated_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UploadedPath)
                    .HasColumnName("uploaded_path")
                    .HasMaxLength(255);

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.VersionNumber).HasColumnName("version_number");

                entity.Property(e => e.WorkflowJson).HasColumnName("workflow_json");

                entity.Property(e => e.WorkflowProjectId).HasColumnName("workflow_project_id");

                entity.Property(e => e.WorkflowPropertyJson).HasColumnName("workflow_property_json");

                entity.HasOne(d => d.LastWorkflowSessionAttempt)
                    .WithMany(p => p.WorkflowVersions)
                    .HasForeignKey(d => d.LastWorkflowSessionAttemptId)
                    .HasConstraintName("FK__workflow___last___2180FB33");

                entity.HasOne(d => d.WorkflowProject)
                    .WithMany(p => p.WorkflowVersions)
                    .HasForeignKey(d => d.WorkflowProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__workflow___workf__14270015");
            });

            modelBuilder.Entity<Writer>(entity =>
            {
                entity.ToTable("writer");

                entity.Property(e => e.WriterId).HasColumnName("writer_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DestinationPath)
                    .HasColumnName("destination_path")
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.WriterTypeId).HasColumnName("writer_type_id");

                entity.HasOne(d => d.WriterType)
                    .WithMany(p => p.Writers)
                    .HasForeignKey(d => d.WriterTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__writer__writer_t__3D5E1FD2");
            });

            modelBuilder.Entity<WriterType>(entity =>
            {
                entity.ToTable("writer_type");

                entity.HasIndex(e => e.WriterTypeName)
                    .HasName("UQ__writer_t__5476CA987357A0E0")
                    .IsUnique();

                entity.Property(e => e.WriterTypeId).HasColumnName("writer_type_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.WriterTypeName)
                    .IsRequired()
                    .HasColumnName("writer_type_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.HasSequence<int>("job_sequence");
        }
    }
}
