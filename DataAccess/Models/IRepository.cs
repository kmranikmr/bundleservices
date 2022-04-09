using DataAccess.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public interface IRepository
    {
        // General 
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveChangesAsync();

        #region Projects
        Task<Project[]> GetAllProjectsAsync(bool includeSummary = false);
        Task<Project> GetProjectAsync(int userId, int projectId, bool includeSummary = false);
        Task<Project[]> GetAllProjectsByUserId(int userId, bool includeummary = false, bool includeModelSchema = false);
        Task<Project> SetFavorite(int userId, int projectId, bool flag);
        #endregion

        #region ProjectFile
        Task<ProjectFile[]> GetProjectFiles(int projectId, int sourceTypeId);
        Task<ProjectFile[]> GetProjectFiles(int projectId, bool includeReader = false);
        Task<bool> SetReaderId(Dictionary<int, int> projectFileIdReaderIdDict);
        Task<bool> SetSchemaId(int projectFIleId, int schemaId);
        // Schema - returns schemas along with models
        Task<ProjectSchema[]> GetSchemasAsync(int userId, int projectId, bool includeModels = false);
        Task<ProjectSchema> GetSchemaAsync(int schemaId, bool includeModels = false);
        Task<ProjectSchema> SetSchemaAsync(int schemaId, ProjectSchema projectSchema);
        Task<bool> AddSchemaAsync(ProjectSchema projectSchema);
        Task<bool> DeleteSchema(int userId, int projectId, int schemaId);
          Task<ProjectFile[]> GetProjectFiles(int projectId, int[] projectFileIds);       
        #endregion

        #region Jobs
        Task<Job[]> GetJobsInProject(int userId, int projectId);
        Task<Job[]> GetJobSummary(int projectId, bool All = false);
        Task<Job> GetJobAsync(int userId, int projectId);
        int GetNewJobId();
        Task<bool> AddJob(int userId, int projectId, int jobId, int schemaId, List<int> FileId);
        Task<bool> UpdateJob(int userId, int projectId, int schemaId, List<int> FileId);
       
        #endregion

        #region Writers

        Task<Job> UpdateJobStatus(int jobId, int statusId, int projectFileId);
        Task<bool> UpdateJobStart(int jobId , int projectFileId);
        Task<bool> UpdateJobEnd(int jobId, int projectFileId);
        //Readers
        Task<Writer[]> GetWritersInProject(int userId, int projectId);
        Task<Writer[]> GetWriters();
        Task<Writer> GetWriterAsync(int writerId);
        Task<ProjectWriter> GetProjectWriterAsync(int projectId, int writerId);

        #endregion

        #region Readers

        Task<Reader[]> GetReadersInProject(int userId, int projectId);
        Task<Reader[]> GetReadersInProjectByTypes(int projectId, int readerTypeId);
        Task<Reader[]> GetReadersViaUser(int userId);
        Task<Reader> GetReaderAsync(int readerId);
        Task<ReaderType[]> GetReaderTypes();
        Task<bool> DeleteReader(int userId, int readerId);

        #endregion


        #region Models
        Task<SchemaModel[]> GetModelsAsync(int userId, int schemaId);
        Task<Project> GetProjectByName(string projectName);
        Task<SchemaModel> GetModel(int userId, int projectId, string schemaName, string ModelName);
        Task<SchemaModel> GetModel(int userId, string projectName, string schemaName, string ModelName);
        Task<double> GetTotalModelSize(int userId);
        #endregion

        #region Search History

        Task<SearchHistory[]> GetSearchHistories(int projectId, int userId);
        Task<SearchGraph> UpdateSearchGraph(int searchGraphId, string graphDescription);
        Task<SearchHistory[]> GetSavedQueriesPerProject(int userId);
        Task<WorkflowSearchGraph> UpdateWorkflowSearchGraph(int WorkflowSearchGraphId, string graphDescription);
        Task<SearchHistory> GetSearchHistory(int searchHistoryId, int userId);
        Task<SearchHistory> GetSearchHistoryByMd5(string md5, int userId, int projectId);
        Task<WorkflowSearchHistory> GetWorkflowSearchHistory(int searchHistoryId, int userId);
        Task<WorkflowSearchHistory> GetWorkflowSearchHistory(string searchHistoryName, int userId);
        Task<WorkflowSearchHistory> GetWorkflowSearchHistoryByMd5(string md5, int userId, int projectId);
        Task<SearchHistory> GetSearchHistory(string searchName, int userId);
        Task<SearchHistory> UpdateSearchHistory(int searchHistoryId, int userId, string friendlyName);
        Task<WorkflowSearchHistory> UpdateWorkflowSearchHistory(int searchHistoryId, int userId, string friendlyName);
        #endregion

        #region automation

        Task<ProjectAutomation[]> GetProjectAutomations();

        #endregion

        #region workflow
        Task<WorkflowProject[]> GetWorkflowProjects(int userId, bool includeVersions = false);
        Task<WorkflowProject> GetWorkflowProject(int userId, int workflowProjectId, bool includeVersions = false);
        Task<WorkflowVersion[]> GetWorkflowVersions(int userId, int workflowProjectId, bool includeAttempts = false, bool inlcudeLastAttempt = false);
        Task<WorkflowVersion> GetWorkflowVersion(int userId, int workflowVersionId);
        Task<WorkflowSessionAttempt[]> GetWorkflowVersionAttempts(int userId, int workflowProjectId, int workflowVersionId, bool includeLog = false);
        Task<WorkflowSessionLog[]> GetWorkflowVersionLogs(int userId, int workflowProjectId, int workflowAttemptId);
        Task<bool> UpdateWorkflowProject(int workflowProjectId, int externalWorkflowProjectId);
        Task<bool> UpdateWorkflowVersion(int workflowVersionId, int workflowProjectId, int externalWorkflowProjectId, int externalWorkflowId);
        Task<bool> UpdateWorkflowVersionJson(int workflowVersionId, int workflowProjectId, string workflowJson, string workflowPropertyJson);
        Task<bool> UpdateWorkflowVersionLastAttemptId(int workflowVersionId, int workflowProjectId, int attemptId);
        Task<bool> SetWorkflowPublished(int workflowVersionId, int workflowProjectId, int publish);
        Task<WorkflowVersion> AddWorkFlowVersion(int userId, int workflowProjectId, WorkflowVersion workflowVersion);
        Task<WorkflowVersion> UpdateWorkFlowVersion(int userId, int workflowProjectId, WorkflowVersion workflowVersion, int workflowVersionId);
        Task<WorkflowOutputModel> AddWorkflowOutputTable(int workflowProjectId, int workflowVersionId, int userId);
        Task<WorkflowOutputModel> UpdateWorkflowOutputTable(int pkindex, int workflowProjectId, int workflowVersionId, string displayName);
        Task<WorkflowOutputModel[]> GetWorkflowOutputTable(int workflowProjectId, int workflowVersionId, int userId, bool inlcude = false);
        Task<bool> DisableWorkflowVersion(int workflowVersionId, int workflowProjectId);
        Task<bool> DisableWorkflowVersionAttempt(int externalWorkflowVersionId, int externalProjectId);
        Task<WorkflowTest> GetWorkflowTest(int WorkflowTestId);


        Task<WorkflowSessionAttempt> AddWorkflowAttempt(int workflowProjectId, int userId, int workflowId);
        Task<WorkflowSessionAttempt> UpdateWorkflowAttempt(int attemptId, int workflowProjectId, int userId, int workflowId, string Result);
        Task<WorkflowSessionAttempt> UpdateWorkflowAttempt(int externalAttemptId, int externalWorkflowId, int userId, string Result, string Log);
        Task<WorkflowSessionLog> UpdateWorkflowAttemptLog(int attemptId, int workflowProjectId, int userId, int workflowId, string Log);
        Task<WorkflowOutputModel> GetWorkflowOutputTable(int workflowProjectId, int workflowVersionId, int workflowModelId, int userId);
        Task<WorkflowOutputModel[]> GetWorkflowOutputTableNames(int workflowProjectId, int workflowVersionId, int userId, string[] DisplayNames);
        Task<WorkflowOutputModel> GetWorkflowOutputTableName(int workflowProjectId, int workflowVersionId);
        Task<WorkflowSearchHistory[]> GetWorkflowSearchHistories(int userId);
        Task<bool> DeleteWorkflowProjects(int userId, int workflowProjectId);
        Task<bool> DeleteWorkflowVersion(int userId, int workflowProjectId, int workflowVersionId);
        Task<WorkflowSessionAttempt[]> GetAllWorkflowVersionAttempts(int userId);
        Task<WorkflowElement[]> GetNodeRepository();
        #endregion
        Task<WorkflowProject[]> GetAllWorkflowProjectsByUserId(int userId, bool includeModelSchema = false);
        Task<WorkflowSearchHistory[]> GetWorkflowSearchHistories(int projectId, int userId);
        Task<bool> AddWorkflowModelMetaData(int userId, List<TableInfo> tableInfos, int workflowVersionId, int ModelId);
        Task<WorkflowMonitor[]> GetWorkflowMonitor( int workflowVersionId);
        #region workflowautomation
        Task<bool> AddWorkflowMonitor(int userId, int workflowProjectId, int workflowVersionId, List<int> modelIds, bool isWorkflow = false);
        Task<WorkflowMonitor> GetWorkflowMonitor(int userId, int modelId);
        Task<WorkflowAutomationState> GetWorkflowAutomationState(int workflowVersionId);
        Task<WorkflowAutomationState> AddWorkflowAutomationState(int workflowVersionId);
        Task<bool> ResetWorkflowAutomationState(int workflowVersionId);
        Task<WorkflowStateModelMap> AddWorkflowStateModelMap(int automationStateId, int workflowVersionId, int jobId, int modelId, bool isWorkflow = false);
        Task<bool> CheckWorkflowStateModelMap(int automationStateId, int workflowVersionId, int jobId, int?[] modelIds);
        Task<int[]> GetJobIdfromWorkflowStateModelMap(int automationStateId);
        Task<WorkflowAutomation> AddWorkflowAutomation(int automationStateId, int workflowProjectId, int workflowVersionId);
        WorkflowTest AddWorkflowTest(int userId, int workflowProjectId, int workflowVersionId, string WorkflowJson, string WorkflowPropertyJson);
        Task<WorkflowTest> UpdateWorkflowTest(int userId, int WorkflowTestId, int workflowProjectId, int workflowVersionId, int externalAttemptId);
        Task<WorkflowTest> UpdateWorkflowTestPublish(int workflowtestId, int workflowProjectId, int externalWorkflowProjectId, int externalWorkflowId, int externalAttemptId = -1);
        Task<bool> UpdateWorkflowTestRun(int workflowtestId, int workflowProjectId, int externalAttemptId);
        Task<WorkflowTest> UpdateWorkflowTestResult(int externalAttemptId, int externalWorkflowId, int userId, string Result, string Log);
        Task<WorkflowTest[]> GetWorkflowTest(int userId, int workflowProjectId, int workflowVersionId);
        #endregion
        Task<UserApiKey> GetUserKey(int userId);
        Task<UserApiKey> AddUserKey(int userId, string apiKey);
        Task<UserSharedUrl> AddSharedUrl(int userId, int searchHistoryIds, string Url, bool isWorkflow = false);
        Task<bool> RemoveSharedUrl(int userId, int searchHistoryIds, bool isWorkflow = false);
        Task<UserSharedUrl[]> GetSharedUrl(int userId, bool isWorkflow = false);
        Task<UserApiKey> GetKeyDetailsfromApiKey(string incomingKey);
        Task<bool> CheckSharedUrl(int userId, string queryName);
    }
}
