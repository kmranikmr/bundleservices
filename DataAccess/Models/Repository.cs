using Common.Utils;
using DataAccess.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Repository : IRepository
    {

        private readonly DAPDbContext _context;

        private readonly ILogger<Repository> _logger;


        public Repository(DAPDbContext context, ILogger<Repository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Add<T>(T entity) where T : class
        {
            //_logger.LogInformation($"Adding an object of type {entity.GetType()} to the context.");
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
           // _logger.LogInformation($"Removing an object of type {entity.GetType()} to the context.");
            _context.Remove(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save the changes in the context");

            // Only return success if at least one row was changed
            return (await _context.SaveChangesAsync()) > 0;
        }

        #region Project

        public async Task<Project> GetProjectByName(string projectName)
        {
            IQueryable<Project> query = _context.Projects.Where(x => string.Compare(x.ProjectName, projectName, true) == 0);

            return await query.FirstOrDefaultAsync();
        }
        
        public async Task<Project[]> GetAllProjectsByUserId(int userId, bool includeSummary = false, bool includeModelSchema = false)
        {
            IQueryable<Project> query = _context.Projects;

            if (includeSummary)
            {
                query = query.Include(c => c.ProjectSchemas).Include(p => p.Jobs);
            }
            if (includeModelSchema)
            {
                query = query.Include(x => x.ProjectSchemas).ThenInclude(ps => ps.SchemaModels).ThenInclude(sm => sm.ModelMetadatas);
            }

            query = query.Where(x => x.CreatedBy == userId && x.IsActive == true && x.IsDeleted == false);

            return await query.ToArrayAsync();
        }

        public async Task<Project[]> GetAllProjectsAsync(bool includeSummary = false)
        {
            IQueryable<Project> query = _context.Projects;

            if (includeSummary)
            {
                query = query.Include(c => c.ProjectSchemas).Include(p => p.Jobs);
            }

            query = query.Where(x => x.IsActive == true && x.IsDeleted == false);

            return await query.ToArrayAsync();
        }

        public async Task<Project> GetProjectAsync(int userId, int projectId, bool includeSummary = false)
        {
            IQueryable<Project> query = _context.Projects;

            if (includeSummary)
            {
                query = query.Include(c => c.ProjectSchemas).Include(p => p.Jobs);
            }

            query = query.Where(x => x.CreatedBy == userId && x.ProjectId == projectId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Project> SetFavorite(int userId, int projectId, bool flag)
        {
            var project = await _context.Projects.FindAsync(projectId);

            if (project != null && project.CreatedBy == userId)
            {
                project.IsFavorite = flag;

                _context.Entry(project).Property(x => x.IsFavorite).IsModified = true;

                await _context.SaveChangesAsync();

                return project;
            }

            return null;
        }
        #endregion

        #region Models
        public async Task<SchemaModel[]> GetModelsAsync(int userId, int schemaId)
        {
            IQueryable<SchemaModel> query = _context.SchemaModels.Include(p=>p.ModelMetadatas);

            query = query.Where(x => x.UserId == userId && x.SchemaId == schemaId && x.IsActive == true && x.IsDeleted == false);

            return await query.ToArrayAsync();
        }



        #endregion

        #region Schema
        public async Task<long> GetTotalModelSize(int userId)
        {
            IQueryable<SchemaModel> query = _context.SchemaModels;
            var sm = query.Where(x => x.UserId == userId); 
            if (sm != null)
            {
                long modelSize =  await sm.SumAsync(x => x.ModelSize.HasValue ? x.ModelSize.Value : 0);

                IQueryable<ProjectFile> pf = _context.ProjectFiles.Where(x => x.UserId == userId);

                long fileSize = 0;
                foreach ( var f in pf)
                {
                    try
                    {
                        if (f.FilePath == null || f.FileName == null)
                            continue;
                        int p = f.FilePath.IndexOf("/UserData");
                        string file = "";
                        if (p > 0)
                        {
                            string path = f.FilePath.Substring(p);
                            file = Path.Combine("/mnt/efs", path, f.FileName);
                            Console.WriteLine($"Checking Path Model File Name {file}");
                            if (!File.Exists(file))
                                continue;
                        }
                        else
                            continue;
                     Console.WriteLine($"Path Model File Name {file}");
                    fileSize += new System.IO.FileInfo(file).Length;
                    Console.WriteLine($"Path Model Size {fileSize}");
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }

                modelSize += fileSize;
                return modelSize;
            }
            return 0;
        }

        public async Task<ProjectSchema[]> GetSchemasAsync(int userId, int projectId, bool includeModels = false)
        {
            IQueryable<ProjectSchema> query = _context.ProjectSchemas;

            if (includeModels)
            {
                query = query.Include(ps => ps.SchemaModels)
                    .ThenInclude(m => m.ModelMetadatas);
            }

            query = query.Where(x => x.ProjectId == projectId && x.IsActive == true && !x.IsDeleted);

            return await query.ToArrayAsync();
        }
        public async Task<bool> AddSchemaAsync(ProjectSchema projectSchema)
        {

            //projectSchema.SchemaModels = new List<SchemaModel>() { };

            // _context.Entry(projectSchema.SchemaModels).State = EntityState.Unchanged;
            // foreach ( ModelMetadata mdata in projectSchema.SchemaModels.Select(x=>x.ModelMetadatas))
            // {
            //     _context.Entry(mdata).State = EntityState.Unchanged;
            // }

            _context.ProjectSchemas.Add(projectSchema);
            _context.Entry(projectSchema).State = EntityState.Added;
            return await _context.SaveChangesAsync() > 0;


        }
        public async Task<ProjectSchema> SetSchemaAsync(int schemaId, ProjectSchema projectSchema)
        {
            try
            {

                if (schemaId != 0)
                {
                    var pschema = await _context.ProjectSchemas.FindAsync(schemaId);
                    // _context.Entry(projectSchema).State = EntityState.Modified;
                    _context.Entry(pschema).CurrentValues.SetValues(projectSchema);

                    foreach (var model in projectSchema.SchemaModels)
                    {

                        model.UserId = projectSchema.UserId;
                        model.SchemaId = projectSchema.SchemaId;
                        model.IsActive = projectSchema.IsActive;
                        var existingChild = pschema.SchemaModels
                            .Where(c => c.ModelId == model.ModelId)
                            .SingleOrDefault();

                        if (existingChild != null)
                        {

                            _context.Entry(existingChild).CurrentValues.SetValues(model);

                        }
                        else
                        {
                            var newModel = new SchemaModel { ModelConfig = model.ModelConfig, ModelName = model.ModelName, UserId = model.UserId, ProjectId = model.ProjectId, ModelMetadatas = model.ModelMetadatas };
                            pschema.SchemaModels.Add(newModel);
                        }

                    }

                    //    pschema.SchemaName = projectSchema.SchemaName;
                    //pschema.SchemaModels = projectSchema.SchemaModels;
                    //pschema.TypeConfig = projectSchema.TypeConfig;

                    // pschema = projectSchema;

                    await _context.SaveChangesAsync();
                    return pschema;
                }
                return null;
            }
            catch (Exception ex)
            {
                int G = 0;
                return null;
            }
        }

        public async Task<bool> DeleteSchema(int userId, int projectId, int schemaId)
        {
            var pSchema = await _context.ProjectSchemas.FindAsync(schemaId);

            if (pSchema != null && pSchema.UserId == userId)
            {
                pSchema.IsDeleted = true;

                _context.Entry(pSchema).Property(x => x.IsDeleted).IsModified = true;

                return await _context.SaveChangesAsync() > 0;

            }

            return false;
        }
        public async Task<ProjectSchema> GetSchemaAsync(int schemaId, bool includeModels = false)
        {
            IQueryable<ProjectSchema> query = _context.ProjectSchemas;

            if (includeModels)
            {
                query = query.Include(ps => ps.SchemaModels)
                    .ThenInclude(m => m.ModelMetadatas);
            }

            query = query.Where(x => x.SchemaId == schemaId && x.IsDeleted == false && x.IsActive == true);

            return await query.FirstOrDefaultAsync();
        }
        public async Task<SchemaModel> GetModel(int userId, int projectId, string schemaName, string ModelName)
        {
            try{
            // IQueryable<ProjectSchema> query = _context.ProjectSchemas;
            // query = query.Include(ps => ps.SchemaModels);
            var query = await GetSchemasAsync(userId, projectId, true);
            var schemaModels = query.Where(x => x.ProjectId == projectId && x.SchemaName.ToLower().Replace(" ","") == schemaName && x.UserId == userId);

            var Model = schemaModels.Select(x => x.SchemaModels.Where(y => y.ModelName.ToLower() == ModelName).SingleOrDefault()).SingleOrDefault(); //. ..Where(x => x.SchemaModels.Where(y => y.ModelName == ModelName).Where(z=>z.).SingleOrDefault()).SingleOrDefault();
            
            if ( Model != null )
            {
                return Model;
            }
            return null; ;
          }catch(Exception ex)
	  {
              Console.WriteLine("GetModel Error " + ex);
              return null;
	  }
        }
        public async Task<SchemaModel> GetModel(int userId, string projectName, string schemaName, string ModelName)
        {
            var project = await GetProjectByName(projectName);
            if ( project != null )
            {
                return await GetModel(userId, project.ProjectId, schemaName, ModelName);
            }
            return null;
        }
            #endregion

            #region Jobs

            public async Task<Job[]> GetJobsInProject(int userId, int projectId)
        {
            IQueryable<Job> query = _context.Jobs;

            query = query.Include(j => j.JobStatus);

            query = query.Where(x => x.UserId == userId && x.ProjectId == projectId && x.IsActive == true && !x.IsDeleted);

            return await query.ToArrayAsync();
        }

        public int GetNewJobId()
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT NEXT VALUE FOR job_sequence";
                _context.Database.OpenConnection();

                var jobId = command.ExecuteScalar();

                return Convert.ToInt32(jobId);
            }
        }

        public async Task<Job[]> GetJobSummary(int jobId, bool All = false)
        {
            IQueryable<Job> query = _context.Jobs;

            query = query.Include(j => j.JobStatus);

            query = query.Include(j => j.Project).Include(j => j.ProjectFile).ThenInclude(pf => pf.Reader).ThenInclude(r => r.ReaderType);

            query = query.Include(j => j.ProjectFile).ThenInclude(pf => pf.SourceType);

            if (!All)
            {
                query = query.Where(x => x.JobId == jobId && x.IsActive == true && !x.IsDeleted);
            }
            else
            {
                query = query.Where(x => x.ProjectId == jobId && x.IsActive == true && !x.IsDeleted);

            }
            return await query.ToArrayAsync();
        }

        public async Task<Job> GetJobAsync(int userId, int jobId)
        {
            IQueryable<Job> query = _context.Jobs;

            query = query.Include(j => j.JobStatus);

            query = query.Where(x => x.UserId == userId && x.JobId == jobId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Job> UpdateJobStatus(int jobId, int statusId, int projectFileId)
        {
            var job = await _context.Jobs.FindAsync(jobId, projectFileId);

            if (job != null)
            {
                job.JobStatusId = statusId;
                _context.Entry(job).Property(x => x.JobStatusId).IsModified = true;
                await _context.SaveChangesAsync();
                return job;
            }
            return null;
        }
        public async Task<bool> AddJob(int userId, int projectId, int jobId, int schemaId, List<int> FileId)
        {
            IQueryable<Job> query = _context.Jobs;

            query = query.Include(j => j.JobStatus);

            query = query.Where(x => x.UserId == userId && x.ProjectId == projectId && x.IsActive == true && !x.IsDeleted);

            var jobs = query.Where(x => x.JobStatus.StatusName == "Created" && FileId.Any(y => y == x.ProjectFileId)).ToList();

            //if (jobs != null && jobs.Count > 0 )
            //{
            //    foreach (Job job in jobs)
            //    {
            //        // job.IsActive = false;
            //        job.IsDeleted = true;
            //        _context.Entry(job).Property(x => x.IsDeleted).IsModified = true;
            //        await _context.SaveChangesAsync();
            //    }
            //}

            var projFiles = _context.ProjectFiles.Where(x => FileId.Any(y => y == x.ProjectFileId)).ToList();

            foreach (ProjectFile projFile in projFiles)
            {

                var schema = await GetSchemaAsync((int)projFile.SchemaId, true);
                var foundJob = jobs.Find(x => x.ProjectFileId == projFile.ProjectFileId);
                //if (foundJob != null)
                //{
                //    if ( foundJob.JobStatusId == 1)
                //    {
                //        foundJob.SchemaId = (int)projFile.SchemaId;
                //        _context.Entry(foundJob).Property(x => x.SchemaId).IsModified = true;

                //    }
                //}
                //else
                //{
                Job job1 = new Job()
                {
                    JobId = jobId,
                    ProjectFileId = projFile.ProjectFileId,
                    SchemaId = projFile.SchemaId,
                    ProjectId = projectId,
                    UserId = userId,
                    JobStatusId = 1,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = DateTime.Now,
                    JobDescription = schema.TypeConfig

                };
                _context.Jobs.Add(job1);
                //_context.Entry(job1).State = EntityState.Added;

                //}
            }
            await _context.SaveChangesAsync();



            return true;
        }
        public async Task<bool> UpdateJob(int userId, int projectId, int schemaId, List<int> FileId)
        {
            return false;
        }
        public async Task<bool> UpdateJobStart(int jobId, int projectFileId)
        {
            var job = await _context.Jobs.FindAsync(jobId, projectFileId);

            if (job != null)
            {
                job.StartedOn = DateTime.Now;
                _context.Entry(job).Property(x => x.StartedOn).IsModified = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<bool> UpdateJobEnd(int jobId, int projectFileId)
        {
            var job = await _context.Jobs.FindAsync(jobId, projectFileId);

            if (job != null)
            {
                job.CompletedOn = DateTime.Now;
                _context.Entry(job).Property(x => x.CompletedOn).IsModified = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        #endregion

        #region Writers

        public async Task<Writer[]> GetWritersInProject(int userId, int projectId)
        {
            IQueryable<ProjectWriter> query = _context.ProjectWriters.Include(pr => pr.Project).Include(pr => pr.Writer).ThenInclude(w => w.WriterType);

            query = query.Where(x => x.ProjectId == projectId && x.Project.CreatedBy == userId && x.Writer.IsDeleted == false && x.Writer.IsActive == true);

            return await query.Select(x => x.Writer).Include(x => x.WriterType).ToArrayAsync();
        }

        public async Task<Writer[]> GetWritersViaUser(int userId)
        {
            IQueryable<Writer> query = _context.Writers;

            query = query.Include(x => x.WriterType);

            query = query.Where(x => x.UserId == userId && x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        public async Task<Writer[]> GetWriters()
        {
            IQueryable<Writer> query = _context.Writers;

            query = query.Include(x => x.WriterType);

            query = query.Where(x => x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        public async Task<Writer> GetWriterAsync(int writerId)
        {
            IQueryable<Writer> query = _context.Writers;

            query = query.Include(x => x.WriterType);

            query = query.Where(x => x.WriterId == writerId && x.IsDeleted == false && x.IsActive == true);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ProjectWriter> GetProjectWriterAsync(int projectId, int writerId)
        {
            IQueryable<ProjectWriter> query = _context.ProjectWriters;

            query = query.Where(x => x.WriterId == writerId && x.ProjectId == projectId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<WriterType[]> GetWriterTypes()
        {
            IQueryable<WriterType> query = _context.WriterTypes;

            query = query.Where(x => x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        #endregion

        #region Readers

        public async Task<Reader[]> GetReadersInProject(int userId, int projectId)
        {
            IQueryable<ProjectReader> query = _context.ProjectReaders.Include(pr => pr.Project).Include(pr => pr.Reader).ThenInclude(r =>r.ReaderType);

            query = query.Where(x => x.ProjectId == projectId && x.Project.CreatedBy == userId && x.Reader.IsDeleted == false && x.Reader.IsActive == true);

            return await query.Select(x => x.Reader).Include(r =>r.ReaderType).ToArrayAsync();
        }

        public async Task<Reader[]> GetReadersViaUser(int userId)
        {
            IQueryable<Reader> query = _context.Readers;

            query = query.Where(x => x.UserId == userId && x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        public async Task<Reader[]> GetReadersInProjectByTypes(int projectId, int readerTypeId)
        {
            IQueryable<ProjectReader> query = _context.ProjectReaders.Include(pr => pr.Reader);

            query = query.Where(x => x.ProjectId == projectId && x.Reader.ReaderTypeId == readerTypeId && x.Reader.IsDeleted == false && x.Reader.IsActive == true);

            return await query.Select(x => x.Reader).ToArrayAsync();
        }

        public async Task<Reader> GetReaderAsync(int readerId)
        {
            IQueryable<Reader> query = _context.Readers;

            query = query.Where(x => x.ReaderId == readerId && x.IsDeleted == false && x.IsActive == true);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ReaderType[]> GetReaderTypes()
        {
            IQueryable<ReaderType> query = _context.ReaderTypes;

            query = query.Where(x => x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        public async Task<bool> DeleteReader(int userId, int readerId)
        {
            var reader = await _context.Readers.FindAsync(readerId);

            if (reader != null && reader.UserId == userId)
            {
                reader.IsDeleted = true;

                _context.Entry(reader).Property(x => x.IsDeleted).IsModified = true;

                return await _context.SaveChangesAsync() > 0;

            }

            return false;
        }



        #endregion

        #region ProjectFiles

        public async Task<ProjectFile[]> GetProjectFiles(int projectId, int sourceTypeId)
        {
            IQueryable<ProjectFile> query = _context.ProjectFiles;

            query = query.Where(x => x.ProjectId == projectId && x.SourceTypeId == sourceTypeId && x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        public async Task<ProjectFile[]> GetProjectFiles(int projectId, int[] projectFileIds)
        {
            IQueryable<ProjectFile> query = _context.ProjectFiles;

            query = query.Where(x => x.ProjectId == projectId && projectFileIds.Any( y => y == x.ProjectFileId)  && x.IsDeleted == false );

            return await query.ToArrayAsync();
        }

        public async Task<ProjectFile[]> GetProjectFiles(int projectId, bool includeReader = false)
        {
            IQueryable<ProjectFile> query = _context.ProjectFiles;
            if (includeReader)
            {
                query = query.Include(x => x.Reader);
            }
            query = query.Where(x => x.ProjectId == projectId && x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        public async Task<bool> SetReaderId(Dictionary<int, int> projectFileIdReaderIdDict)
        {
            IQueryable<ProjectFile> query = _context.ProjectFiles.Where(pf => projectFileIdReaderIdDict.ContainsKey(pf.ProjectFileId));

            var projectFiles = await query.ToArrayAsync();

            if (projectFiles.Any())
            {
                foreach (var item in projectFiles)
                {
                    item.ReaderId = projectFileIdReaderIdDict[item.ProjectFileId];
                    _context.Entry(item).Property(x => x.ReaderId).IsModified = true;
                }

                return await _context.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<bool> SetSchemaId(int projectFIleId, int schemaId)
        {
            ProjectFile projectFile = await _context.ProjectFiles.FindAsync(projectFIleId);


            if (projectFile != null)
            {
                projectFile.SchemaId = schemaId;
                _context.Entry(projectFile).Property(x => x.SchemaId).IsModified = true;
                return await _context.SaveChangesAsync() > 0;
            }

            return false;
        }
        #endregion

        #region SearchHistory

        public async Task<SearchHistory[]> GetSearchHistories(int projectId, int userId)
        {
            IQueryable<SearchHistory> query = _context.SearchHistories.Include(x => x.SearchGraphs);

            query = query.Where(x => x.ProjectId == projectId && x.UserId == userId && x.IsActive == true && !x.IsDeleted);

            return await query.ToArrayAsync();
        }
        public async Task<SearchHistory> GetSearchHistory(int searchHistoryId, int userId)
        {
            SearchHistory query = _context.SearchHistories.FindAsync(searchHistoryId).Result;
            if ( query != null)
            {
                return query;
            }
            return null;
        }

        public async Task<SearchHistory> GetSearchHistory(string searchName, int userId)
        {
            SearchHistory query = _context.SearchHistories.Where ( x=>x.SearchHistoryName == searchName && x.UserId == userId).FirstOrDefault();
            if (query != null)
            {
                return query;
            }
            return null;
        }

        public async Task<SearchHistory> UpdateSearchHistory(int searchHistoryId, int userId, string friendlyName)
        {
            SearchHistory query = _context.SearchHistories.FindAsync(searchHistoryId).Result;
            if (query != null)
            {
                query.FriendlyName = friendlyName;

                _context.Entry(query).Property(x => x.FriendlyName).IsModified = true;

                await _context.SaveChangesAsync();

                return query;
            }
            return null;
        }

        public async Task<SearchHistory> GetSearchHistoryByMd5(string md5, int userId, int projectId)
        {
            SearchHistory query = _context.SearchHistories.Where(x => x.UserId ==userId && x.Md5 == md5 && x.ProjectId == projectId).FirstOrDefault();
            if (query != null)
            {
                return query;
            }
            return null;
        }

        public async Task<WorkflowSearchHistory> GetWorkflowSearchHistory(int searchHistoryId, int userId)
        {
            WorkflowSearchHistory query = _context.WorkflowSearchHistories.FindAsync(searchHistoryId).Result;
            if (query != null)
            {
                return query;
            }
            return null;
           
        }

        public async Task<WorkflowSearchHistory> GetWorkflowSearchHistory(string searchHistoryName, int userId)
        {
            WorkflowSearchHistory query = _context.WorkflowSearchHistories.Where(x => x.WorkflowSearchHistoryName == searchHistoryName && x.UserId == userId).FirstOrDefault();
            if (query != null)
            {
                return query;
            }
            return null;            
        }

        public async Task<WorkflowSearchHistory> GetWorkflowSearchHistoryByMd5(string md5, int userId, int projectId)
        {
            WorkflowSearchHistory query = _context.WorkflowSearchHistories.Where(x => x.UserId == userId && x.Md5 == md5 && x.WorkflowProjectId == projectId).FirstOrDefault();
            if (query != null)
            {
                return query;
            }
            return null;
        }

        public async Task<WorkflowSearchHistory[]> GetWorkflowSearchHistories(int projectId, int userId)
        {
            IQueryable<WorkflowSearchHistory> query = _context.WorkflowSearchHistories.Include(x => x.WorkflowSearchGraphs);

            query = query.Where(x => x.WorkflowProjectId == projectId && x.UserId == userId && x.IsActive == true && !x.IsDeleted);

            return await query.ToArrayAsync();
        }

        public async Task<SearchGraph> UpdateSearchGraph(int searchGraphId, string graphDescription)
        {
            var searchGraph = await _context.SearchGraphs.FindAsync(searchGraphId);

            if (searchGraph != null)
            {
                searchGraph.GraphDescription = graphDescription;

                _context.Entry(searchGraph).Property(x => x.GraphDescription).IsModified = true;

                await _context.SaveChangesAsync();

                return searchGraph;
            }

            return null;
        }
        public async Task<WorkflowSearchGraph> UpdateWorkflowSearchGraph(int WorkflowSearchGraphId, string graphDescription)
        {
            var searchGraph = await _context.WorkflowSearchGraphs.FindAsync(WorkflowSearchGraphId);

            if (searchGraph != null)
            {
                searchGraph.GraphDescription = graphDescription;

                _context.Entry(searchGraph).Property(x => x.GraphDescription).IsModified = true;

                await _context.SaveChangesAsync();

                return searchGraph;
            }

            return null;
        }

        public async Task<SearchHistory[]> GetSavedQueriesPerProject(int userId)
        {
            IQueryable<SearchHistory> query = _context.SearchHistories;

            query = query.Where( x=> x.UserId == userId && x.IsActive == true && !x.IsDeleted);


            return await query.ToArrayAsync();
        }

        public async Task<WorkflowSearchHistory[]> GetWorkflowSearchHistories(int userId)
        {
            IQueryable<WorkflowSearchHistory> query = _context.WorkflowSearchHistories;

            query = query.Where(x => x.UserId == userId && x.IsActive == true && !x.IsDeleted);

            return await query.ToArrayAsync();
        }

        #endregion

        #region Automation
        public async Task<ProjectAutomation[]> GetProjectAutomations()
        {
            IQueryable<ProjectAutomation> query = _context.ProjectAutomations;
            
            query = query.Where(x => x.IsActive == true && x.IsDeleted == false);

            return await query.ToArrayAsync();
        }

        
        #endregion

        #region WorkflowProject
        //get all workflow projects
        public async Task<WorkflowProject[]> GetWorkflowProjects(int userId, bool includeVersions = false)
        {
            IQueryable<WorkflowProject> query = _context.WorkflowProjects;
            if (includeVersions)
            {
                query = query.Include(x => x.WorkflowVersions);
            }
            query = query.Where(x => x.UserId == userId && x.IsDeleted == false);

            return await query.ToArrayAsync();
        }

        //get all workflow projects
        public async Task<WorkflowProject> GetWorkflowProject(int userId, int workflowProjectId, bool includeVersions = false)
        {
            var query = _context.WorkflowProjects.Where( x=>x.WorkflowProjectId == workflowProjectId  && x.UserId == userId);
            if (includeVersions)
            {
                query = query.Include(x => x.WorkflowVersions);
            }
            return await query.FirstOrDefaultAsync();
        }
        #endregion
        //dleete woriflow project given workflowprojectid
        public async Task<bool> DeleteWorkflowProjects(int userId, int workflowProjectId)
        {
            var workflowProject = await _context.WorkflowProjects.FindAsync(workflowProjectId);

            if (workflowProject != null && workflowProject.UserId == userId)
            {
                workflowProject.IsDeleted = true;

                _context.Entry(workflowProject).Property(x => x.IsDeleted).IsModified = true;

                return await _context.SaveChangesAsync() > 0;

            }

            return false;
        }

        public async Task<WorkflowSessionAttempt[]> GetAllWorkflowVersionAttempts(int userId)
        {
            IQueryable<WorkflowSessionAttempt> query = _context.WorkflowSessionAttempts.Where(x => x.UserId == userId);
            ///query = query.Where(x => x.IsDeleted == false);
            if (query == null)
            {
                return null;
            }

            return await query.ToArrayAsync();
        }

        // delete workflow versions given workflowprojectid/workflowversionid
        public async Task<bool> DeleteWorkflowVersion(int userId, int workflowProjectId, int workflowVersionId)
        {
            var workflowversion = _context.WorkflowVersions.Where( x=>  x.WorkflowProjectId == workflowProjectId &&  x.WorkflowVersionId == workflowVersionId).FirstOrDefault();

            if (workflowversion != null)
            {
                workflowversion.IsDeleted = true;

                _context.Entry(workflowversion).Property(x => x.IsDeleted).IsModified = true;

                return await _context.SaveChangesAsync() > 0;

            }

            return false;
        }

       
        #region WorkflowVersion

        //get workflow version given projectid
        public async Task<WorkflowVersion[]> GetWorkflowVersions(int userId, int workflowProjectId, bool includeAttempts = false, bool inlcudeLastAttempt = false)
        {
            IQueryable<WorkflowVersion> query = _context.WorkflowVersions.Where(x => x.WorkflowProjectId == workflowProjectId);
            query = query.Where(x => x.UserId == userId && x.IsDeleted == false);
            if (includeAttempts)
            {
                query = query.Include(x => x.WorkflowSessionAttempts);
            }
            if ( inlcudeLastAttempt)
            {
                query = query.Include(x => x.LastWorkflowSessionAttempt);
            }
            // query = query.Where(x => x.IsDeleted == false);

            return await query.ToArrayAsync();
        }
        //get workflow attempts given workflowprojectid, workflowversionid
        public async Task<WorkflowSessionAttempt[]> GetWorkflowVersionAttempts(int userId, int workflowProjectId, int workflowVersionId, bool includeLog = false)
        {
            IQueryable<WorkflowSessionAttempt> query = _context.WorkflowSessionAttempts.Where(x => x.UserId == userId && x.WorkflowProjectId == workflowProjectId && x.WorkflowVersionId == workflowVersionId);
            ///query = query.Where(x => x.IsDeleted == false);
            if(query != null)
            {
                if (includeLog)
                {
                    query = query.Include(c => c.WorkflowSessionLogs);
                }
            }

            return await query.ToArrayAsync();
        }

        //workflow versions logs 
        public async Task<WorkflowSessionLog[]> GetWorkflowVersionLogs(int userId, int workflowProjectId, int workflowAttemptId)
        {
            IQueryable<WorkflowSessionLog> query = _context.WorkflowSessionLogs.Where(x => x.WorkflowProjectId == workflowProjectId && x.WorkflowSessionAttemptId == workflowAttemptId);
            ///query = query.Where(x => x.IsDeleted == false);

            return await query.ToArrayAsync();
        }

        public async Task<WorkflowVersion> GetWorkflowVersion(int userId, int workflowVersionId)
        {
            var query = await _context.WorkflowVersions.FindAsync(workflowVersionId);
            //query = query.Where(x => x.UserId == userId && x.IsDeleted == false);
            if (query != null && query.UserId == userId)
            {
                return query;
            }
            // query = query.Where(x => x.IsDeleted == false);

            return null;
        }
        //update workflow project with eternalrpojectid
        public async Task<bool> UpdateWorkflowProject(int workflowProjectId, int externalWorkflowProjectId)
        {
            var workflowProject = await _context.WorkflowProjects.FindAsync(workflowProjectId);
            if(workflowProject != null)
            {
                workflowProject.ExternalProjectId = externalWorkflowProjectId;
                _context.Entry(workflowProject).Property(x => x.ExternalProjectId).IsModified = true;

                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        //set published
        public async Task<bool> SetWorkflowPublished(int WorkflowVersionId, int WorkflowProjectId, int publish)
        {

            var versionFlow = _context.WorkflowVersions.Where(x=>x.WorkflowProjectId == WorkflowProjectId && x.WorkflowVersionId == WorkflowVersionId ).FirstOrDefault();
            if (versionFlow != null)
            {
               
                versionFlow.IsPublished = publish == 1 ? true : false;
                _context.Entry(versionFlow).Property(x => x.IsPublished).IsModified = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
            //update workflow version with externalprojectid, external workflowid

        public async Task<bool> UpdateWorkflowVersion(int workflowVersionId, int workflowProjectId, int externalWorkflowProjectId, int externalWorkflowId)
        {

            var versionFlow = await _context.WorkflowVersions.FindAsync(workflowVersionId);
            if (versionFlow != null)
            {
                versionFlow.ExternalProjectId = externalWorkflowProjectId;
                versionFlow.ExternalWorkflowId = externalWorkflowId;
                versionFlow.IsPublished = true;
                _context.Entry(versionFlow).Property(x => x.ExternalProjectId).IsModified = true;
                _context.Entry(versionFlow).Property(x => x.ExternalWorkflowId).IsModified = true;
                _context.Entry(versionFlow).Property(x => x.IsPublished).IsModified = true;

                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }


        public async Task<bool> DisableWorkflowVersion(int workflowVersionId, int workflowProjectId )
        {

            var versionFlow = await _context.WorkflowVersions.FindAsync(workflowVersionId);
            if (versionFlow != null)
            {
                
                _context.Entry(versionFlow).Property(x => x.IsActive).IsModified = false;
              

                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        //update workflow version to disable
        //public async Task<bool> UpdateWorkflowVersionOutputTable(int workflowVersionId, int workflowProjectId, string outputTable)
        //{
        //    var versionFlow = await _context.WorkflowVersions.FindAsync(workflowVersionId, workflowProjectId);
        //    if (versionFlow != null)
        //    {
        //        versionFlow.OutputModelName = outputTable;
        //        _context.Entry(versionFlow).Property(x => x.OutputModelName).IsModified = true;
        //        await _context.SaveChangesAsync();
        //        return true;
        //    }
        //    return false;

        //}
        //update  withexternal attempt id

        //public async Task<bool> UpdateWorkflowAttemptId(int workflowVersionId, int workflowProjectId, int workflowAttemptId, int externalAttemptId)
        //{

        //    var attempt = _context.WorkflowSessionAttempts.Where(x => x.WorkflowProjectId == workflowProjectId && x.WorkflowVersionId == workflowVersionId && x.WorkflowSessionAttemptId == workflowAttemptId).SingleOrDefault();
        //    if (attempt != null)
        //    {
        //        attempt.e
        //    }
        //}
        //update workflow version's json
        public async Task<bool> UpdateWorkflowVersionJson(int workflowVersionId, int workflowProjectId, string workflowJson, string workflowPropertyJson)
        {
            var versionFlow = await _context.WorkflowVersions.FindAsync(workflowVersionId, workflowProjectId);
            if (versionFlow != null)
            {
                versionFlow.WorkflowJson = workflowJson;
                versionFlow.WorkflowPropertyJson = workflowPropertyJson;
                _context.Entry(versionFlow).Property(x => x.WorkflowJson).IsModified = true;
                _context.Entry(versionFlow).Property(x => x.WorkflowPropertyJson).IsModified = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;

        }
        //update last session attempt id

        public async Task<bool> UpdateWorkflowVersionLastAttemptId(int workflowVersionId, int workflowProjectId, int attemptId)
        {
            var versionFlow = await _context.WorkflowVersions.FindAsync(workflowVersionId);
            if (versionFlow != null)
            {
                versionFlow.LastWorkflowSessionAttemptId = attemptId;
                _context.Entry(versionFlow).Property(x => x.LastWorkflowSessionAttemptId).IsModified = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;

        }

        //update workflow version's json
        public async Task<bool> UpdateWorkflowVersionOutputTable(int workflowVersionId, int workflowProjectId, string outputTable)
        {
            var versionFlow = await _context.WorkflowVersions.FindAsync(workflowVersionId, workflowProjectId);
            if (versionFlow != null)
            {
                versionFlow.OutputModelName = outputTable;
                _context.Entry(versionFlow).Property(x => x.OutputModelName).IsModified = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;

        }
        //add output_table
        public async Task<WorkflowOutputModel> AddWorkflowOutputTable(int workflowProjectId, int workflowVersionId, int userId )
        {
            var workflowModels = _context.WorkflowOutputModels;

            var modelData = workflowModels.Where(x => x.WorkflowVersionId == workflowVersionId && workflowProjectId == x.WorkflowProjectId).FirstOrDefault();
            if (modelData == null)
            {
                WorkflowOutputModel outputModel = new WorkflowOutputModel
                {
                    UserId = userId,
                    WorkflowProjectId = workflowProjectId,
                    WorkflowVersionId = workflowVersionId,

                    CreatedOn = DateTime.Now
                };
                _context.WorkflowOutputModels.Add(outputModel);
                _context.Entry(outputModel).State = EntityState.Added;
                await _context.SaveChangesAsync();
                return outputModel;
            }
            else
            {
                return modelData;
            }
           

        }
        public async Task<WorkflowOutputModel> UpdateWorkflowOutputTable(int pkindex, int workflowProjectId, int workflowVersionId,  string displayName)
        {
            var query = _context.WorkflowOutputModels;
            var workflowOutputModel = query.Where(x => x.WorkflowProjectId == workflowProjectId && x.WorkflowVersionId == workflowVersionId).SingleOrDefault();
            if (workflowOutputModel != null)
            {
                if (displayName == "")
                {
                    workflowOutputModel.DisplayName = "workflowoutput_" + pkindex;
                }
                else
                {
                    workflowOutputModel.DisplayName = displayName.ToLower()+"_" + pkindex;
                }
                _context.Entry(workflowOutputModel).Property(x => x.DisplayName).IsModified = true;
                workflowOutputModel.TableName = "workflowoutput_" + workflowProjectId + "_"+ workflowVersionId +"_"+ pkindex;
                _context.Entry(workflowOutputModel).Property(x => x.TableName).IsModified = true;
                await _context.SaveChangesAsync();
                return workflowOutputModel;
            }
            return null;
        }

        //getmodeltables
        public async Task<WorkflowOutputModel[]> GetWorkflowOutputTable(int workflowProjectId, int workflowVersionId, int userId, bool includeMetadata = false)
        {
            IQueryable<WorkflowOutputModel> query = _context.WorkflowOutputModels.Where(x=>x.WorkflowProjectId == workflowProjectId && x.WorkflowVersionId == workflowVersionId && x.UserId == userId);
            if (includeMetadata)
            {
                query = query.Include(x => x.WorkflowModelMetadatas);
            }
            return await query.ToArrayAsync();
        }

        public async Task<WorkflowOutputModel> GetWorkflowOutputTable(int workflowProjectId, int workflowVersionId, int workflowModelId, int userId)
        {
            var query = await _context.WorkflowOutputModels.Where(x => x.WorkflowProjectId == workflowProjectId && x.WorkflowVersionId == workflowVersionId && x.WorkflowOutputModelId == workflowModelId && x.UserId == userId).SingleOrDefaultAsync();
            return query;
        }

       

        public async Task<WorkflowOutputModel[]> GetWorkflowOutputTableNames(int workflowProjectId, int workflowVersionId, int userId, string[] DisplayNames)
        {
            IQueryable<WorkflowOutputModel> query = _context.WorkflowOutputModels.Where(x => x.WorkflowProjectId == workflowProjectId && x.WorkflowVersionId == workflowVersionId && x.UserId == userId && DisplayNames.Contains(x.DisplayName));
          
            return await query.ToArrayAsync();
        }
        //add workflow version
        public async Task<WorkflowVersion> AddWorkFlowVersion(int userId, int workflowProjectId, WorkflowVersion workflowVersion )//, int workflowVersionId = -1)
        {

            IQueryable<WorkflowVersion> query = _context.WorkflowVersions;
            var workflowTotal = query.Where(x => x.UserId == userId && x.WorkflowProjectId == workflowProjectId);
            var workflow = query.Where(x => x.UserId == userId && x.WorkflowProjectId == workflowProjectId && x.IsPublished == false && !x.IsDeleted);//.SingleOrDefault();
            if ( workflowVersion.VersionNumber <= 0 && workflowTotal != null)
            {
                workflowVersion.VersionNumber = (int)workflowTotal.Count() + 1;
            }
            workflowVersion.CreatedOn = DateTime.Now;
            _context.WorkflowVersions.Add(workflowVersion);
            _context.Entry(workflowVersion).State = EntityState.Added;
                await _context.SaveChangesAsync();
                
            return workflowVersion;
            
           
        }//update workflow version - when saving
        public async Task<WorkflowVersion> UpdateWorkFlowVersion(int userId, int workflowProjectId, WorkflowVersion workflowVersion, int workflowVersionId )
        {
            IQueryable<WorkflowVersion> query = _context.WorkflowVersions;

            var workflow = query.Where(x => x.UserId == userId && x.WorkflowProjectId == workflowProjectId && x.IsPublished == false && !x.IsDeleted);//.SingleOrDefault();
            bool workflowCreated = false;
            if (workflow != null)
            {
                if (workflowVersionId != -1)
                {
                    workflowCreated = true;
                    var workflowSelected = workflow.Where(x => x.WorkflowVersionId == workflowVersionId).FirstOrDefault();

                    //update 
                    workflowSelected.UpdatedOn = DateTime.Now;
                    // workflow.UserId = userId;
                  //  workflowSelected.VersionNumber = workflowVersion.VersionNumber + 1;
                    workflowSelected.WorkflowJson = workflowVersion.WorkflowJson;
                    workflowSelected.WorkflowPropertyJson = workflowVersion.WorkflowPropertyJson;

                    _context.Entry(workflowSelected).Property(x => x.UpdatedOn).IsModified = true;
                    _context.Entry(workflowSelected).Property(x => x.VersionNumber).IsModified = true;
                    _context.Entry(workflowSelected).Property(x => x.WorkflowJson).IsModified = true;
                    _context.Entry(workflowSelected).Property(x => x.WorkflowPropertyJson).IsModified = true;

                    await _context.SaveChangesAsync();
                    return workflowSelected;
                }
                
            }

            if (!workflowCreated) //whatever reason not updated then create new
            {
                await AddWorkFlowVersion(userId, workflowProjectId, workflowVersion);
            }
            
            return null;
        }
        //add new owrkflow attempt while running
        public async Task<WorkflowSessionAttempt> AddWorkflowAttempt(int workflowProjectId, int userId, int workflowVersionId)
        {
            IQueryable<WorkflowVersion> query = _context.WorkflowVersions;

            var workflow = query.Where(x => x.WorkflowVersionId == workflowVersionId && x.UserId == userId && x.WorkflowProjectId == workflowProjectId && x.IsPublished == true && x.IsDeleted == false).FirstOrDefault();

            if ( workflow != null)
            {
                WorkflowSessionAttempt attempt = new WorkflowSessionAttempt
                {
                    CreatedOn = DateTime.Now,
                    ExternalProjectId = workflow.ExternalProjectId,                    
                    VersionNumber = workflow.VersionNumber,
                    WorkflowVersionId = workflow.WorkflowVersionId,
                    WorkflowProjectId = workflow.WorkflowProjectId,
                    WorkflowStatusTypeId = 1,
                    UserId = userId,
                    Result = "Created"
                };

                //if ( workflow.WorkflowSessionAttempts != null)
                //{
                //    workflow.WorkflowSessionAttempts.Add(attempt);
                //}
                //else
                //{
                //    workflow.WorkflowSessionAttempts = new List<WorkflowSessionAttempt>();
                //    workflow.WorkflowSessionAttempts.Add(attempt);
                //}
                _context.WorkflowSessionAttempts.Add(attempt);
                _context.Entry(attempt).State = EntityState.Added;
                await _context.SaveChangesAsync();
                return attempt;
            }
            return null;
        }

        //update workflow attempt  with result
        public async Task<WorkflowSessionAttempt> UpdateWorkflowAttempt(int attemptId, int workflowProjectId, int userId, int workflowVersionId, string Result)
        {
            IQueryable<WorkflowSessionAttempt> query = _context.WorkflowSessionAttempts;

            var workflowAttempt = query.Where(x =>x.WorkflowSessionAttemptId == attemptId && x.WorkflowVersionId == workflowVersionId && x.UserId == userId && x.WorkflowProjectId == workflowProjectId).SingleOrDefault();

            if ( workflowAttempt != null)
            {
                workflowAttempt.Result = Result;
                _context.Entry(workflowAttempt).Property(x => x.Result).IsModified = true;
                if (Result.ToLower() == "success" || Result.ToLower() == "failure")
                {
                    workflowAttempt.EndDate = DateTime.Now;
                    _context.Entry(workflowAttempt).Property(x => x.EndDate).IsModified = true;
                }
                await _context.SaveChangesAsync();
                return workflowAttempt;
            }
            return null;
        }
        public async Task<bool> DisableWorkflowVersionAttempt(int externalWorkflowVersionId, int externalProjectId)
        {

            var versionFlow =  _context.WorkflowSessionAttempts.Where(x=>x.ExternalWorkflowId == externalProjectId && x.ExternalProjectId == externalProjectId).SingleOrDefault();
            if (versionFlow != null)
            {

                _context.Entry(versionFlow).Property(x => x.IsActive).IsModified = false;


                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }


        //Update workflow Attempt Log
        public async Task<WorkflowSessionLog> UpdateWorkflowAttemptLog(int attemptId, int workflowProjectId, int userId, int workflowId, string Log)
        {
            IQueryable<WorkflowSessionAttempt> query = _context.WorkflowSessionAttempts;

            var workflowAttempt = query.Where(x => x.WorkflowSessionAttemptId == attemptId && x.WorkflowVersionId == workflowId && x.UserId == userId && x.WorkflowProjectId == workflowProjectId).SingleOrDefault();

            if (workflowAttempt != null)
            {
                WorkflowSessionLog logData = new WorkflowSessionLog
                {
                    LogData = Log,
                    CreatedOn = DateTime.Now,
                    ExternalProjectId = workflowAttempt.ExternalProjectId,
                    VersionNumber = workflowAttempt.VersionNumber,
                    WorkflowProjectId = workflowAttempt .WorkflowProjectId,
                    WorkflowSessionAttemptId = workflowAttempt.WorkflowSessionAttemptId                
                };
                _context.WorkflowSessionLogs.Add(logData);
                _context.Entry(logData).State = EntityState.Added;
                await _context.SaveChangesAsync();
                return logData;
            }
            return null;
        }
        //update workflow attempt  with result and log given external attempt id and external workflowid
        public async Task<WorkflowSessionAttempt> UpdateWorkflowAttempt(int externalAttemptId, int externalWorkflowId, int userId, string Result, string Log)
        {
            IQueryable<WorkflowSessionAttempt> query = _context.WorkflowSessionAttempts;

            var workflowAttempt = query.Where(x => x.ExternalAttemptId == externalAttemptId && x.ExternalWorkflowId == externalWorkflowId).SingleOrDefault();

            if (workflowAttempt != null)
            {
               
                if (Result.ToLower() == "processing"
                    || Result.ToLower() == "true" 
                    || Result.ToLower() == "false"
                    || Result.ToLower() == "success" 
                    || Result.ToLower() == "failure")
                {
                    if (Result.ToLower().Contains( "true"))
                    {
                        workflowAttempt.Result = "Success";
                    }
                    else if (Result.ToLower().Contains("false"))
                    {
                        workflowAttempt.Result = "Failure";
                    }
                    else
                    {
                        workflowAttempt.Result = Result;
                    }
                   _context.Entry(workflowAttempt).Property(x => x.Result).IsModified = true;
                    workflowAttempt.EndDate = DateTime.Now;
                    _context.Entry(workflowAttempt).Property(x => x.EndDate).IsModified = true;
                    if (Log != "")
                    {
                        WorkflowSessionLog logData = new WorkflowSessionLog
                        {
                            LogData = Log,
                            CreatedOn = DateTime.Now,
                            ExternalProjectId = workflowAttempt.ExternalProjectId,
                            VersionNumber = workflowAttempt.VersionNumber,
                            WorkflowProjectId = workflowAttempt.WorkflowProjectId,
                            WorkflowSessionAttemptId = workflowAttempt.WorkflowSessionAttemptId
                        };
                        _context.WorkflowSessionLogs.Add(logData);
                        _context.Entry(logData).State = EntityState.Added;
                    }
                }
                await _context.SaveChangesAsync();
                return workflowAttempt;
            }
            return null;
        }
        public async Task<WorkflowElement[]>GetNodeRepository()
        {
            IQueryable<WorkflowElement> query = _context.WorkflowElements;
            return await query.ToArrayAsync();

        }


        #endregion
        #region workflowgettreee
        public async Task<WorkflowProject[]> GetAllWorkflowProjectsByUserId(int userId, bool includeModelSchema = false)
        {
            IQueryable<WorkflowProject> query = _context.WorkflowProjects;

           
            if (includeModelSchema)
            {
                query = query.Include(x => x.WorkflowVersions);//.ThenInclude(y => y.WorkflowSessionAttempts);
               
            }

            query = query.Where(x => x.UserId == userId && x.IsActive == true && x.IsDeleted == false);

            return await query.ToArrayAsync();
        }
        #endregion

        #region workflowmetadata
        public async Task<bool> AddWorkflowModelMetaData(int userId, List<TableInfo> tableInfos, int workflowVersionId, int ModelId)
        {
           foreach(TableInfo tableInfo in tableInfos)
           {
                var TableModelData = new WorkflowModelMetadata
                {
                    ColumnName = tableInfo.ColumnName,
                    CreatedOn = DateTime.Now,
                    WorkflowOutputModelId = ModelId,
                    WorkflowVersionId = workflowVersionId, 
                };
                
                _context.WorkflowModelMetadatas.Add(TableModelData);
           }
             await _context.SaveChangesAsync();
           return true;
        }
        #endregion

        #region workflow automation

        public async Task<bool> AddWorkflowMonitor(int userId, int workflowProjectId, int workflowVersionId, List<int> modelIds, bool isWorkflow = false)
        {
            var workflowModels = _context.WorkflowMonitors;
            foreach (int modelId in modelIds)
            {
                WorkflowMonitor monitor = new WorkflowMonitor
                {
                    UserId = userId,
                    CreatedOn = DateTime.Now,
                    WorkflowProjectId = workflowProjectId,
                    WorkflowVersionId = workflowVersionId,
                    ModelId = isWorkflow == true ? null : (int?)modelId,
                    WorkflowOutputModelId = isWorkflow == false ? null : (int?)modelId
                    
                };

                _context.WorkflowMonitors.Add(monitor);
                _context.Entry(monitor).State = EntityState.Added;
            }
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<WorkflowMonitor> GetWorkflowMonitor(int userId, int modelId)
        {
            var workflowMonitors= _context.WorkflowMonitors;
            var monitor = await workflowMonitors.Where(x => x.ModelId == modelId).FirstOrDefaultAsync();
            return monitor;
        }

        public async Task<WorkflowMonitor[]> GetWorkflowMonitor(int workflowVersionId)
        {
            var workflowMonitors = _context.WorkflowMonitors;
            var monitor = await workflowMonitors.Where(x => x.WorkflowVersionId == workflowVersionId).ToArrayAsync();
            return monitor;
        }
        public async Task<WorkflowAutomationState> GetWorkflowAutomationState( int workflowVersionId)
        {
            var workflowState = await _context.WorkflowAutomationStates.Where(x => x.WorkflowVersionId == workflowVersionId).SingleOrDefaultAsync();
            return workflowState;
        }
        public async Task<WorkflowAutomationState> AddWorkflowAutomationState(int workflowVersionId)
        {
            WorkflowAutomationState state = new WorkflowAutomationState
            {
                CreatedOn = DateTime.Now,
                WorkflowVersionId = workflowVersionId
            };

            _context.WorkflowAutomationStates.Add(state);
            _context.Entry(state).State = EntityState.Added;
            await _context.SaveChangesAsync();
            return state;
        }

        public async Task<bool> ResetWorkflowAutomationState(int workflowVersionId)
        {
            var workflowState = await _context.WorkflowAutomationStates.Where(x => x.WorkflowVersionId == workflowVersionId).SingleOrDefaultAsync();

            if(workflowState != null)
            {
                workflowState.StateStatus = false;
                _context.Entry(workflowState).Property(x => x.StateStatus).IsModified = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<WorkflowStateModelMap> AddWorkflowStateModelMap(int automationStateId, int workflowVersionId, int jobId, int modelId, bool isWorkflow = false)
        {
            WorkflowStateModelMap stateMap = new WorkflowStateModelMap
            {
                CreatedOn = DateTime.Now,
                WorkflowAutomationStateId = automationStateId,
                WorkflowVersionId = workflowVersionId,
                SessionId = jobId,
                ModelId = isWorkflow == false ? (int?)modelId : null,
                WorkflowOutputModelId = isWorkflow == true ? (int?)modelId : null
            };

            _context.WorkflowStateModelMaps.Add(stateMap);
            _context.Entry(stateMap).State = EntityState.Added;
            await _context.SaveChangesAsync();
            return stateMap;
        }
        public async Task<bool> CheckWorkflowStateModelMap(int automationStateId, int workflowVersionId, int jobId, int?[] modelIds)
        {

            // check satte map
            var checkAll =  await _context.WorkflowStateModelMaps.Where(x => modelIds.ToList().All(y => y == x.ModelId) == true).FirstOrDefaultAsync();
            if (  checkAll != null)//== true)
            {
                //all models are in state of running.. we can trigger an attempt
                return true;
            }
            return false;
        }
        //get all jobid assosiated with state id
        public async Task<int[]> GetJobIdfromWorkflowStateModelMap(int automationStateId)
        {
            var query = _context.WorkflowStateModelMaps.Where(x => x.WorkflowAutomationStateId == automationStateId);

            var jobId = query.Select(x => x.SessionId);

            return await jobId.ToArrayAsync();

        }
        public async Task<WorkflowAutomation> AddWorkflowAutomation(int automationStateId, int workflowProjectId, int workflowVersionId)
        {
            WorkflowAutomation auto = new WorkflowAutomation
            {
                CreatedOn = DateTime.Now,
                WorkflowAutomationStateId = automationStateId,
                WorkflowVersionId = workflowVersionId,
                WorkflowProjectId = workflowProjectId
            };

            _context.WorkflowAutomations.Add(auto);
            _context.Entry(auto).State = EntityState.Added;
            await _context.SaveChangesAsync();
            return auto;
        }
        #endregion
        #region workflow test

        public async Task<WorkflowTest[]> GetWorkflowTest(int userId, int workflowProjectId, int workflowVersionId)
        {
            IQueryable<WorkflowTest> query = _context.WorkflowTests.Where(x => x.WorkflowProjectId == workflowProjectId && x.WorkflowVersionId == workflowVersionId && userId == x.UserId);
            return await query.ToArrayAsync();
        }
        public WorkflowTest AddWorkflowTest(int userId, int workflowProjectId, int workflowVersionId, string WorkflowJson, string WorkflowPropertyJson)
        {
            WorkflowTest wt = new WorkflowTest
            {
                CreatedOn = DateTime.Now,
                WorkflowProjectId = workflowProjectId,
                WorkflowVersionId = workflowVersionId,
                WorkflowJson = WorkflowJson,
                WorkflowStatusTypeId = 1,
                WorkflowPropertyJson = WorkflowPropertyJson,
                UserId = userId
            };

            _context.WorkflowTests.Add(wt);
            _context.Entry(wt).State = EntityState.Added;
            _context.SaveChanges();
            return wt;
        }

        public async Task<WorkflowTest> UpdateWorkflowTest(int userId, int WorkflowTestId, int workflowProjectId, int workflowVersionId, int externalAttemptId)
        {

            var Test = await _context.WorkflowTests.FindAsync(WorkflowTestId);
            if (Test != null)
            {
                Test.ExternalAttemptId = externalAttemptId;
                Test.WorkflowStatusTypeId = 1;
                Test.Result = "Test Created";
                _context.Entry(Test).Property(x => x.ExternalAttemptId).IsModified = true;
                _context.Entry(Test).Property(x => x.WorkflowStatusTypeId).IsModified = true;
                _context.Entry(Test).Property(x => x.Result).IsModified = true;

                await _context.SaveChangesAsync();
                return Test;

            }
            return null;

        }

        public async Task<WorkflowTest> UpdateWorkflowTestPublish(int workflowtestId, int workflowProjectId, int externalWorkflowProjectId, int externalWorkflowId, int externalAttemptId = -1)
        {

            var versionFlow = await _context.WorkflowTests.FindAsync(workflowtestId);
            if (versionFlow != null)
            {
                versionFlow.ExternalProjectId = externalWorkflowProjectId;
                versionFlow.ExternalWorkflowId = externalWorkflowId;
                versionFlow.Result = "Created";
                versionFlow.WorkflowStatusTypeId = 1;
                _context.Entry(versionFlow).Property(x => x.ExternalProjectId).IsModified = true;
                _context.Entry(versionFlow).Property(x => x.ExternalWorkflowId).IsModified = true;
                _context.Entry(versionFlow).Property(x => x.Result).IsModified = true;
                _context.Entry(versionFlow).Property(x => x.WorkflowStatusTypeId).IsModified = true;
                if ( externalAttemptId != -1)
                {
                    versionFlow.ExternalAttemptId = externalAttemptId;
                    _context.Entry(versionFlow).Property(x => x.ExternalAttemptId).IsModified = true;
                }
                await _context.SaveChangesAsync();
                return versionFlow;
            }
            return null;
        }

        public async Task<bool> UpdateWorkflowTestRun(int workflowtestId, int workflowProjectId, int externalAttemptId)
        {

            var versionFlow = await _context.WorkflowTests.FindAsync(workflowtestId);
            if (versionFlow != null)
            {
                versionFlow.ExternalAttemptId = externalAttemptId;
                _context.Entry(versionFlow).Property(x => x.ExternalAttemptId).IsModified = true;

                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<WorkflowTest> UpdateWorkflowTestResult(int externalAttemptId, int externalWorkflowId, int userId, string Result, string Log)
        {
            IQueryable<WorkflowTest> query = _context.WorkflowTests;

            var workflowAttempt = query.Where(x => x.ExternalAttemptId == externalAttemptId && x.ExternalWorkflowId == externalWorkflowId).SingleOrDefault();

            if (workflowAttempt != null)
            {
                if (Result.ToLower() == "processing"
                   || Result.ToLower() == "true"
                   || Result.ToLower() == "false"
                   || Result.ToLower() == "success"
                   || Result.ToLower() == "failure")
                {
                    if (Result.ToLower().Contains("true"))
                    {
                        workflowAttempt.Result = "Success";
                        workflowAttempt.WorkflowStatusTypeId = 4;
                    }
                    else if (Result.ToLower().Contains("false"))
                    {
                        workflowAttempt.Result = "Failure";
                        workflowAttempt.WorkflowStatusTypeId = 3;
                    }
                    else
                    {
                        workflowAttempt.Result = Result;
                    }
                   
                    _context.Entry(workflowAttempt).Property(x => x.Result).IsModified = true;
                    _context.Entry(workflowAttempt).Property(x => x.WorkflowStatusTypeId).IsModified = true;
                    if (Log != "")
                    {
                        workflowAttempt.LogData = Log;
                        _context.Entry(workflowAttempt).Property(x => x.LogData).IsModified = true;
                    }
                    workflowAttempt.UpdatedOn = DateTime.Now;
                    _context.Entry(workflowAttempt).Property(x => x.UpdatedOn).IsModified = true;
                }
                await _context.SaveChangesAsync();
                return workflowAttempt;
            }
            return null;
        }
        #endregion

        #region user api

        public async Task<UserApiKey> GetUserKey(int userId)
        {
            IQueryable<UserApiKey> userdata = _context.UserApiKeys.Where(x => x.UserId == userId);

            if (userdata != null)
            {
                return await userdata.FirstOrDefaultAsync();
            }
            return null;
        }

        public async Task<UserApiKey> GetKeyDetailsfromApiKey(string incomingKey)
        {
            UserApiKey userdata = _context.UserApiKeys.Where(x=> Api.MatchHashedKey(incomingKey, x.ApiKey) == true).FirstOrDefault();

            if (userdata != null)
            {
                return  userdata;
            }
            return null;
        }
        public async Task<UserApiKey> AddUserKey(int userId, string apiKey)
        {
           try{
            Console.WriteLine($"adduserkey repository {apiKey}");
            UserApiKey userdata = _context.UserApiKeys.Where(x => x.UserId == userId && x.IsDeleted == false).FirstOrDefault();

            if (userdata == null)
            {
                Console.WriteLine("userdata is null");
                UserApiKey userApiKey = new UserApiKey();
               
                userApiKey.ApiKey = apiKey;//hash and add
                userApiKey.CreatedOn = DateTime.Now;
                userApiKey.Scope = "data";
                userApiKey.UserId = userId;
                _context.UserApiKeys.Add(userApiKey);
                _context.Entry(userApiKey).State = EntityState.Added;
                _context.SaveChanges();
                return userApiKey;
            }
            else
            {
               Console.WriteLine("userapikey exists");
                userdata.IsDeleted = true;
                _context.Entry(userdata).Property(x => x.IsDeleted).IsModified = true;
                await _context.SaveChangesAsync();
               Console.WriteLine("saved isdeleted to true");
                UserApiKey userApiKey = new UserApiKey();

                userApiKey.ApiKey = apiKey;//hash and add
                userApiKey.CreatedOn = DateTime.Now;
                userApiKey.Scope = "data";
                userApiKey.UserId = userId;
                _context.UserApiKeys.Add(userApiKey);
                _context.Entry(userApiKey).State = EntityState.Added;
                _context.SaveChanges();
               Console.WriteLine("savedCHanges");
                return userApiKey;

            }
          }catch(Exception ex)
          {
              Console.WriteLine(ex);
              return null;
          } 
        }
        public async Task<UserSharedUrl[]> GetSharedUrl(int userId, bool isWorkflow = false)
        {
            IQueryable<UserSharedUrl> query = null;
            if (!isWorkflow)
            {
                query = _context.UserSharedUrls.Where(x => x.UserId == userId && x.WorkflowSearchHistoryId == null && x.IsDeleted == false);
            }
            else
            {
                query = _context.UserSharedUrls.Where(x => x.UserId == userId && x.WorkflowSearchHistoryId != null && x.IsDeleted == false);
            }
            return await query.ToArrayAsync();
        }
        public async Task<bool> CheckSharedUrl(int userId, string queryName)
        {
            UserSharedUrl query = null;
            
            query = await _context.UserSharedUrls.Where(x => x.UserId == userId && x.SharedUrl.Contains(queryName)).FirstOrDefaultAsync();

            if (query == null)
            {
                return false;
            }
            return true;
        }
        public async Task<UserSharedUrl> AddSharedUrl(int userId, int searchHistoryIds, string Url, bool isWorkflow = false)
        {
            UserSharedUrl query =  null;//
            UserSharedUrl sharedUrl = null;
            if (!isWorkflow)
            {
                query = _context.UserSharedUrls.Where(x => x.SearchHistoryId == searchHistoryIds && x.IsDeleted == false).FirstOrDefault();
            }
            else
            {
                query = _context.UserSharedUrls.Where(x => x.WorkflowSearchHistoryId== searchHistoryIds && x.IsDeleted == false).FirstOrDefault();
            }
            if (!isWorkflow)
            {
                if (query == null)
                {
                    sharedUrl = new UserSharedUrl()
                    {
                        UserId = userId,
                        SearchHistoryId = searchHistoryIds,
                        SharedUrl = Url
                      

                    };

                }
            }
            else
            {
                if (query == null)
                {
                    sharedUrl = new UserSharedUrl()
                    {
                        UserId = userId,
                        WorkflowSearchHistoryId = searchHistoryIds,
                        SharedUrl = Url

                    };

                }
            }
            if ( sharedUrl != null)
            {
                _context.UserSharedUrls.Add(sharedUrl);
                _context.Entry(sharedUrl).State = EntityState.Added;
                _context.SaveChanges();
                return sharedUrl;
            }
               //var sharedUrl = new UserSharedUrl();
            //IQueryable<UserApiKey> userdata = _context.UserSharedUrls;

            return null;
        }

        public async Task<bool> RemoveSharedUrl(int userId, int searchHistoryIds, bool isWorkflow = false)
        {
             UserSharedUrl query = null;//
                                                      //set is deleted
            if (isWorkflow)
            {
                query = _context.UserSharedUrls.Where(x => x.SearchHistoryId == searchHistoryIds).FirstOrDefault();
            }
            else
            {
                query = _context.UserSharedUrls.Where(x => x.WorkflowSearchHistoryId == searchHistoryIds).FirstOrDefault();
            }
            if ( query != null)
            {
                query.IsDeleted = true;
                _context.Entry(query).Property(x => x.IsDeleted).IsModified = true;
                await _context.SaveChangesAsync();
                return true;
            }
            //var sharedUrl = new UserSharedUrl();
            //IQueryable<UserApiKey> userdata = _context.UserSharedUrls;

            return false;
        }
        #endregion

    }


}
