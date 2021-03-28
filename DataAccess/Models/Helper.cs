using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace DataAccess.Models
{
    public class PublishResponse
    {
        public string status { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string revision { get; set; }
        public string workflowId { get; set; }
        public string externalErrorMessage { get; set; }
    
    }

    public class ConnectionStringsConfig
    {
        public string DefaultConnection { get; set; }
        public string WorkflowConnection { get; set; }
        public string QueryServiceConnection { get; set; }

    }

    public class queryData
    {
        public string searchQuery { get; set; }
        public bool Checked { get; set; }
    }

    public class ProjectQueryTask : QueryTask
    {
        public string SearchHistoryId { get; set; }
        public string projectName { get; set; }
        public int projectId { get; set; }
        public List<string> queryList { get; set; }
    }

    public class WorkflowQueryTask: QueryTask
    {
        public int SearchHistoryId { get; set; }
        public string workflowProjectName { get; set; }
        public int workflowProjectid { get; set; }
        public int workflowVersionId { get; set; }
        public List<string> queryList { get; set; }
    }

    public class QueryTask: InputTask
    {

    }
    public class S3InputTask: FileInputTask
    {
        public String filePath;
    }
    public class FileInputTask : InputTask
    {

    }

    public class InputTask
    {

    }

    public class DagEdge
    {

        public string sourceTaskId { get; set; }
        public string targetTaskId { get; set; }
    }
    public class InputTaskData
    {
        public ProjectQueryTask projectQueryTask { get; set; }
        public WorkflowQueryTask workflowQueryTask { get; set; }
        public S3InputTask s3InputTask { get; set; }
    }


    public class Header
    {
        // This is optional. If it doesn't exist, all the kv pairs will directly go under '_export'
        public string headerType { get; set; }
      
        public List<string> keyValues { set; get; }//{ set { keyValuesDict = value.ToDictionary(x => x.Split(',')[0], y => y.Split(',')[1]); } get { return keyValues; } } //Dictionary<string, string>
       
        public Dictionary<string, string> keyValuesMap { get; set; }
        public List<string> exportedVariables { get; set; }
        public bool ShouldSerializekeyValues()
        {
            return false;
        }
    }

    public class Schedule
    {
        public string scheduleType { get; set; }
        public string scheduleValue { get; set; }
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Operator
    {
        [EnumMember(Value = "py")]
        py ,
       [EnumMember(Value = "sql")]
        sql ,
        [EnumMember(Value = "pg")]
        pg
    }

    public class NodeTask
    {
        public string taskName { get; set; }
        public string taskDescription { get; set; }
        public string taskId { get; set; }
        public string nodeType { get; set; }
        public InputTaskData inputData { get; set; }
        //public List<Header> headerList;
        public Header headerList { get; set; }
        public int repeatLoop { get; set; }
        //[JsonProperty(PropertyName = "operator")]
       // [JsonConverter(typeof(StringEnumConverter))]
        public Operator oper { get; set; }
        public string commandBody { get; set; }
        public string template { get; set; }
        public NodeTask()
        {

        }
    }

    public class Workflow
    {
        public string id { set; get; }
        public Schedule schedule { set; get; }
        public string workflowServerType { set; get; }
        public string name { set; get; }
        public string revision { set; get; }
        public string description { set; get; }
        public string content { set; get; }
        //public List<Header> headerList;
        public Header headerList { set; get; }
        //[JsonProperty("workflowTaskList")]
        public List<NodeTask> taskList{ set { workflowTaskList = value; } get { return workflowTaskList; } }
        //  [JsonIgnore]
         public List<NodeTask> workflowTaskList { set; get; }
     //   public List<NodeTask> taskList { set; get; }
        public List<DagEdge> edgeList { set; get; }
        public string secretKey { get; set; }
        public string secretValue { get; set; }
        public bool ShouldSerializetaskList()
        { 
            return false;
        }
    }

    public class ProjectQueryDetails
    {
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public int SchemaId { get; set; }
        public string SchemaName { get; set; }
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public bool UseProjectId { get; set; }
    }

    public static class NodeRepository
    {
        public static List<ProjectQueryDetails> GetProjectModels(string sql, int ProjectId, int userId, bool automationON , out string formattedQuery)
        {
            char[] sep = new char[] { ' ', '-', ':' };
            List<string> tableNames = new List<string>();
            string[] toks = sql.ToLower().Split(' ');
            bool getTable = false;
            List<ProjectQueryDetails> ModelDetails = new List<ProjectQueryDetails>();
            for (int i = 0; i < toks.Length; i++)
            {
                if (getTable)
                {
                    if (toks[i] != " ")
                    {
                        Console.WriteLine(toks[i]);
                        string[] modelinfo = toks[i].Split('.');
                        if (modelinfo.Length == 2)
                        {
                            ProjectQueryDetails details = new ProjectQueryDetails
                            {
                                ProjectId = ProjectId,
                                SchemaName = modelinfo[0],
                                ModelName = modelinfo[1],
                                UseProjectId = true
                            };
                            ModelDetails.Add(details);
                            tableNames.Add(toks[i]);
                        }
                        else if (modelinfo.Length == 3)
                        {
                            ProjectQueryDetails details = new ProjectQueryDetails
                            {
                                ProjectName = modelinfo[0],
                                SchemaName = modelinfo[1],
                                ModelName = modelinfo[2],
                                UseProjectId = false
                            };
                            ModelDetails.Add(details);
                            tableNames.Add(toks[i]);
                        }
                        else if (toks[i] == "(")
                        {

                            //query table
                        }
                        
                        getTable = false;
                    }
                    else
                        continue;
                }
                if (toks[i] == "from" || toks[i] == "join")
                {
                    if (i <= toks.Length - 1)
                    {
                        getTable = true;
                    }
                    if ( toks[i] == "from")
                    {
                        if (automationON == true)
                        {
                            toks[i - 1] = toks[i - 1] + ", sessionid as real_session_id ";
                        }
                    }
                }

            }

            formattedQuery = string.Join(" ", toks);
            return ModelDetails;
        }
        public static string GetQueryStatement(string sql, int ProjectId, int userId)
        {
            string sqlQuery = sql;
            if (ProjectId != 0)
            {

                int startIndex = sql.ToLower().IndexOf("schema");
                List<int> sb = new List<int>();
                string query_with_join = "";
                string join_other_tablename = "";
                bool have_join = false;
                string QueryString = "";
                int index = 0;
                do
                {
                    index = sql.ToLower().IndexOf("schema", index);

                    if (index != -1)
                    {
                        sb.Add(index);
                        int endIndex = sql.ToLower().IndexOf(".", index);
                        string replacement = sql.ToLower().Substring(index, endIndex - index);
                        if (!replacement.Contains("_"))
                        {
                            string replace = Regex.Replace(sql.ToLower(), replacement, replacement + "_" + ProjectId + "_" + userId);
                            sqlQuery = replace;

                        }
                        index++;
                    }
                } while (index != -1);

            }
            return sqlQuery;
        }

        public static string GetQueryWorkflowStatement(string sql, int ProjectId, int versionID, int userId)
        {
            string sqlQuery = sql;
            if (ProjectId != 0)
            {
                int startIndex = sql.ToLower().IndexOf("schema");
                List<int> sb = new List<int>();
                string query_with_join = "";
                string join_other_tablename = "";
                bool have_join = false;
                string QueryString = "";
                int index = 0;
                do
                {
                    index = sql.ToLower().IndexOf("schema", index);

                    if (index != -1)
                    {
                        sb.Add(index);
                        int endIndex = sql.ToLower().IndexOf(".", index);
                        string replacement = sql.ToLower().Substring(index, endIndex - index);
                        if (!replacement.Contains("_"))
                        {
                            string replace = Regex.Replace(sql.ToLower(), replacement, replacement + "_" + ProjectId + "_" + userId);
                            QueryString = replace;
                        }
                        index++;
                    }
                } while (index != -1);

            }
            return sqlQuery;
        }
        public static string PrepareWorkflowJson(int revision, string name, string json, int userId, 
                                                 int workflowProjectId, int workflowVersionId, IRepository repository, out List<ProjectQueryDetails> ModelList, bool isTest = false, bool LimitedRun = false)
        {

            Dictionary<string, List<string>> DestToSourceNodes = new Dictionary<string, List<string>>();
            char[] sep = new char[] { ' ', '-', ':' };
            Dictionary<string, string> UUidToNodeName = new Dictionary<string, string>();
            string changedjson = "";
            Dictionary<string, string> NodeNametoTempNode = new Dictionary<string, string>();
            var dec = JsonConvert.DeserializeObject<Workflow>(json);
            dec.revision = revision.ToString();
            int projectId = 0;
            int versionId = 0;
            dec.name = name;
            dec.secretKey = "pg.password";
            dec.secretValue = "dapdata123";
            bool cleared = false;
            bool automationON = false;
            List<ProjectQueryDetails> ListOfModels = null;
            //get dagegdlist destnation to source dictionary. 
            foreach (var edge in dec.edgeList)
            {
                int dIndex = edge.targetTaskId.IndexOf("_");

                int sIndex = edge.sourceTaskId.IndexOf("_");
                if (dIndex > 0 && sIndex > 0)
                {
                    string dUUid = edge.targetTaskId.Substring(0, dIndex);
                    string sUUid = edge.sourceTaskId.Substring(0, sIndex);
                    edge.targetTaskId = dUUid;
                    edge.sourceTaskId = sUUid;
                }
                if (!DestToSourceNodes.ContainsKey(edge.targetTaskId))
                {
                    DestToSourceNodes.Add(edge.targetTaskId, new List<string> { edge.sourceTaskId });
                }
                else
                {
                    DestToSourceNodes[edge.targetTaskId].Add(edge.sourceTaskId);
                }
            }
            //iterate tasks and replace [INPUTDATA], [NODENAME]_[NODEID], [INPUTNODENAME]_[INPUTNODEID]

            foreach (var task in dec.taskList)
            {
                if (!UUidToNodeName.ContainsKey(task.taskId))
                {
                    UUidToNodeName.Add(task.taskId, task.taskName.Replace(sep, "_"));
                }
                if ( !NodeNametoTempNode.ContainsKey(task.taskName))
                {
                    NodeNametoTempNode.Add(task.taskName, task.taskId);
                }
                if (task.headerList != null && task.headerList.keyValues != null && task.headerList.keyValues.Count > 0)
                {
                    task.headerList.keyValuesMap = task.headerList.keyValues.ToDictionary(x => x.Split(',')[0], y => y.Split(',')[1]);
                    Console.WriteLine("diction" + task.headerList.keyValuesMap.Count);
                    task.headerList.keyValues = null;
                }
            }

            //test let's delete all old ones upfront..let's start with that
            string tempTables = "";
           // if (isTest)
          //  {
                List<string> TempTableNames = new List<string>();
            Console.WriteLine(dec.taskList.Count);
                foreach (var task in dec.taskList)
                {
                //if (DestToSourceNodes.ContainsKey(task.taskId))
                //{
                //    var inputid = DestToSourceNodes[task.taskId];
                //    var inputidChanged = inputid[0].Replace(sep, "_");
                //        Console.WriteLine("inputid" + inputidChanged);
                //if (UUidToNodeName.TryGetValue(inputid[0], out string taskNameChanged))
                //    {
                //        string tempTableName = taskNameChanged + "_" + inputidChanged;
                //    TempTableNames.Add(tempTableName);

                //    Console.WriteLine("tempTableNameforming" + tempTableName);
                //   }
                //}
              //  var inputid = DestToSourceNodes[task.taskId];
                //if (task.nodeType.Contains("output"))
                {
                        var inputidChanged = task.taskId.Replace(sep, "_");
                        var taskNameChanged = task.taskName.Replace(sep, "_");
                        string tempTableName = taskNameChanged + "_" + inputidChanged;
                        TempTableNames.Add(tempTableName);
                 }
                }
                tempTables = string.Join(",", TempTableNames);
           // }
           // else
           // {

          //  }
            foreach (var task in dec.taskList)
            {

                task.taskName = task.taskName.Replace(sep, "_");
                //if (task.headerList != null && task.headerList.keyValues != null && task.headerList.keyValues.Count > 0)
                //{
                //    task.headerList.keyValuesMap = task.headerList.keyValues.ToDictionary(x => x.Split(',')[0], y => y.Split(',')[1]);
                //    task.headerList.keyValues = null;
                //}
                //task.headerList = null;
                if (task.nodeType.Contains("input"))
                {
                    string query = "";
                    if (task.inputData != null)
                    {
                        if (task.inputData.projectQueryTask != null)
                        {
                            projectId = task.inputData.projectQueryTask.projectId;
                            if(task.headerList != null && task.headerList.keyValuesMap != null )
                            {
                                if (task.headerList.keyValuesMap.ContainsKey("automation"))
                                {
                                    automationON = true;
                                }

                            }
                            if (task.inputData.projectQueryTask.queryList != null && task.inputData.projectQueryTask.queryList.Count > 0)
                            {
                                query = task.inputData.projectQueryTask.queryList[0];//let's start with just one input
                                ListOfModels = GetProjectModels(query, projectId, userId, automationON, out query);
                                query = GetQueryStatement(query, projectId, userId);
                                if (isTest && LimitedRun == true)
                                {
                                    query = query + " limit 100";
                                }
                                //query = $"select * from ({query}}a where a.real_session_id = {}";
                            }

                        }
                        if (task.inputData.workflowQueryTask != null && task.inputData.workflowQueryTask.queryList != null && task.inputData.workflowQueryTask.queryList.Count > 0)
                        {
                            projectId = task.inputData.workflowQueryTask.workflowProjectid;
                            versionId = task.inputData.workflowQueryTask.workflowVersionId;
                            query = task.inputData.workflowQueryTask.queryList[0];
                            query = GetQueryWorkflowStatement(query, projectId, versionId, userId);
                            if (isTest && LimitedRun == true)
                            {
                                query = query + " limit 100";
                            }
                        }
                    }
                    if (task.oper == Operator.py)
                    {
                        task.template = GetInputCode();
                        if (!cleared)
                        {
                            Console.WriteLine("temptables" + tempTables);
                            task.template = task.template.Replace("[REMOVEALL_TEMP]", "remove_temps(" + "\"" + tempTables + "\"" + ")");
                        }else
                        {
                            task.template = task.template.Replace("[REMOVEALL_TEMP]", "");
                        }
                           
                    }
                    else
                    {
                        task.template = "INSERT INTO [NODENAME]_[NODEID] [INPUTDATA];";
                    }
                    // replace inputdata
                    task.template = task.template.Replace("[INPUTDATA]", query);


                }

                if (task.nodeType.Contains("process"))
                {
                    if (task.oper == Operator.py)
                    {
                        task.template = GetProcessCode();
                        if (!isTest)
                        {
                            task.template = task.template.Replace("[DELETE_TEMP]", "remove_temp_db('drop table public.[INPUTNODENAME]_[INPUTNODEID]')");
                        }
                        else
                        {
                            task.template = task.template.Replace("[DELETE_TEMP]", "");
                        }
                    }
                    else
                    {
                        task.template = "CREATE TABLE [NODENAME]_[NODEID] AS [COMMANDBODY]";
                    }
                }
                if (task.nodeType.Contains("output"))
                {
                    if ( task.oper == Operator.py)
                    {
                        task.template = GetOutputCode();
                        if (!isTest)
                        {
                            task.template = task.template.Replace("[DELETE_TEMP]", "remove_temp_db('drop table public.[INPUTNODENAME]_[INPUTNODEID]')");
                        }
                        else
                        {
                            task.template = task.template.Replace("[DELETE_TEMP]", "");
                        }
                    }
                    else
                    {
                        task.template = "";//INSERT INTO [OUTPUTNAME]_[PROJECTID]_[VERSIONID]_[TABLEINDEX]
                    }
                    
                    //if ( task.headerList.exportedVariables.Contains("artifact") == true)
                    //{
                    //    task.template = task.template.Replace("[POSTGRES_ARTIFACT_READ]", "model_df = input_postgres_complete('[INPUTNODENAME]_[INPUTNODEID]')");
                    //}
                    //else
                    //{
                    //    task.template = task.template.Replace("[POSTGRES_ARTIFACT_READ]", "");
                    //}
                    string tableName = "";
                    if (task.headerList != null && task.headerList.keyValuesMap != null && task.headerList.keyValuesMap.ContainsKey("TableName"))
                    {
                        tableName = task.headerList.keyValuesMap["TableName"];
                    }
                    if (!isTest)
                    {
                        var outputTableModel = repository.AddWorkflowOutputTable(workflowProjectId, workflowVersionId, userId);

                        Console.WriteLine("AddWorkflowOutputTable" + workflowProjectId + " " + workflowVersionId);
                        task.template = task.template.Replace("[OUTPUTNAME]", "workflowoutput");
                        task.template = task.template.Replace("[PROJECTID]", workflowProjectId.ToString());
                        task.template = task.template.Replace("[VERSIONID]", workflowVersionId.ToString());

                        if (outputTableModel != null)
                        {
                            Console.WriteLine("UpdateWorkflowOutputTable" + outputTableModel.Result.WorkflowOutputModelId);
                            task.template = task.template.Replace("[TABLEINDEX]", outputTableModel.Result.WorkflowOutputModelId.ToString());
                            var UpdatedModel = repository.UpdateWorkflowOutputTable(outputTableModel.Result.WorkflowOutputModelId, workflowProjectId, workflowVersionId, tableName);
                        }
                        else
                        {
                            //bad
                            task.template = task.template.Replace("[TABLEINDEX]", "1");
                        }
                    }
                    else
                    {

                        var inputidChanged = task.taskId.Replace(sep, "_");
                        var taskNameChanged = task.taskName.Replace(sep, "_");
                        string tempTableName = taskNameChanged + "_" + inputidChanged;
                     
                        ///for test we wil dump in a temp 

                        task.template = task.template.Replace("[OUTPUTNAME]_[PROJECTID]_[VERSIONID]_[TABLEINDEX]", tempTableName);
                    }
                    //task.template = task.template.Replace("[WORKFLOWATTEMPTID]", work);
                }


                task.template = task.template.Replace("[METHODNAME]", task.taskName);
                task.template = task.template.Replace("[NODENAME]", task.taskName.Replace(sep, "_"));
                task.template = task.template.Replace("[NODEID]", task.taskId.Replace(sep, "_"));
                if (!task.commandBody.Contains("commandbody"))
                {
                    if (task.oper == Operator.pg)
                    {
                        string transformedSql = "";
                        var tables = TransformSqlQuery(task.commandBody, NodeNametoTempNode, out transformedSql);//GetTableNames(task.commandBody);
                        //foreach (string table in tables)
                        //{
                        //    if (NodeNametoTempNode.TryGetValue(table, out string tempName))
                        //    {
                        //        tempName = tempName.Replace(sep, "_");
                        //        string tableChanged = table.Replace(sep, "_");
                        //        string replacewith = $"(Select * from {tableChanged}_{tempName}){tableChanged}";
                        //        task.commandBody = task.commandBody.Replace(table, replacewith);
                        //    }
                        //}
                        task.commandBody = transformedSql;

                        foreach ( var table in tables)
                        {
                            string tableChanged = table.Replace(sep, "_");
                            task.commandBody = task.commandBody.Replace(table+".", tableChanged+".");
                        }

                    }
                    task.template = task.template.Replace("[COMMANDBODY]", task.commandBody);
                    

                }
                if (DestToSourceNodes.ContainsKey(task.taskId))
                {
                    var inputid = DestToSourceNodes[task.taskId];
                    string articateReplace = "";
                    foreach (var inputItem in inputid)
                    {
                        if (dec.taskList == null )
                        {
                            Console.WriteLine(" null tasklist");
                            continue;
                        }
                        var incomingSrcTask = dec.taskList.Where(x => x.taskId == inputItem).FirstOrDefault();
                        Console.WriteLine($" incomingSrcTask ontained{incomingSrcTask}");
                        if (incomingSrcTask != null)
                        {
                            
                            //Console.WriteLine($" incomingSrcTask 1{incomingSrcTask.headerList.keyValuesMap.Count}");
                            if (incomingSrcTask.headerList != null && incomingSrcTask.headerList.keyValuesMap != null && incomingSrcTask.headerList.keyValuesMap.Count > 0)
                            {
                                Console.WriteLine($" incomingSrcTask 2");
                                if (incomingSrcTask.headerList.keyValuesMap.ContainsKey("artifact"))
                                {//incomingSrcTask.headerList.exportedVariables.FindIndex(x => x.Contains("artificat"));
                                 //if (idxArtifact > 0)
                                 //{
                                    Console.WriteLine($" incomingSrcTask 3");
                                    string key = incomingSrcTask.headerList.keyValuesMap["artifact"];//incomingSrcTask.headerList.exportedVariables[idxArtifact].Split(':')[1];
                                    string changedTaskID = inputItem.Replace(sep, "_");
                                    Console.WriteLine($" incomingSrcTask 4");
                                    if (UUidToNodeName.TryGetValue(inputItem, out string taskNameArtifactChanged))
                                    {
                                        Console.WriteLine($" incomingSrcTask 5");
                                        articateReplace += $"model_df['{key}'] = input_postgres_complete('{taskNameArtifactChanged}_{changedTaskID}')";
                                        articateReplace += "\r\n     ";
                                    }
                                    
                                }
                                else
                                {
                                    Console.WriteLine($" incomingSr else");

                                   task.template = task.template.Replace("[INPUTNODEID]", inputItem.Replace(sep, "_"));
                                    if (UUidToNodeName.TryGetValue(inputItem, out string taskNameChanged))
                                    {
                                        task.template = task.template.Replace("[INPUTNODENAME]", taskNameChanged);
                                    }
                                }
                            }
                            else
                            {
                                task.template = task.template.Replace("[INPUTNODEID]", inputItem.Replace(sep, "_"));
                                if (UUidToNodeName.TryGetValue(inputItem, out string taskNameChanged))
                                {
                                    task.template = task.template.Replace("[INPUTNODENAME]", taskNameChanged);
                                }
                            }
                        }
                    }
                    //if ( articateReplace != "" )
                    {
                        task.template=  task.template.Replace("[POSTGRES_ARTIFACT_READ]", articateReplace);
                    }
                }

                task.commandBody = task.template;
                if (task.oper == Operator.py)
                {
                    if (task.nodeType.Contains("input"))
                    {
                        task.template = GetInputCode();
                    }
                    else if (task.nodeType.Contains("process"))
                    {
                        task.template = GetProcessCode();
                    }
                    else if (task.nodeType.Contains("output"))
                    {
                        task.template = GetOutputCode();
                    }
                }
            }
            dec.schedule = null;
            ModelList = ListOfModels;
            Console.WriteLine($" out");

           changedjson = JsonConvert.SerializeObject(dec);
            changedjson = changedjson.Replace("oper", "operator");
            // Workflow changedWorkflow = JsonConvert.DeserializeObject<Workflow>(changedjson);
            return changedjson;
        }
        public static string[] TransformSqlQuery(string query, Dictionary<string, string> nodeNametoTempNode, out string transfomedSql)
        {
            char[] sep = new char[] { ' ', '-', ':' };
            List<string> tableNames = new List<string>();
            query = query.ToLower().Replace("\n", " ");
            string[] toks = query.ToLower().Split(' ');
            bool getTable = false;
            for (int i = 0; i < toks.Length; i++)
            {
                if (getTable)
                {
                    if (toks[i] != " ")
                    {
                        Console.WriteLine(toks[i]);
                        tableNames.Add(toks[i]);
                        if (nodeNametoTempNode.TryGetValue(toks[i], out string tempName))
                        {
                            tempName = tempName.Replace(sep, "_");
                            string tableChanged = toks[i].Replace(sep, "_");
                            string replacewith = $"(Select * from {tableChanged}_{tempName}){tableChanged}";
                            toks[i] = replacewith;
                        }
                        
                        getTable = false;
                    }
                    else
                        continue;
                }
                if (toks[i] == "from" || toks[i] == "join")
                {
                    if (i <= toks.Length - 1)
                    {
                        getTable = true;
                    }
                }

            }
            transfomedSql = string.Join(" ", toks);
            Console.WriteLine(transfomedSql);
            return tableNames.ToArray();
        }

       
       
        public static Workflow Test(string json, int userId)
        {
            //var dec = JsonConvert.DeserializeObject<Workflow>(json);
            List<ProjectQueryDetails> li;
            var ss = PrepareWorkflowJson(1, "", json, userId, 1, 1, null, out li);
            return null;
        }

        public static string GetInputCode()
        {
            using (StreamReader reader = new StreamReader("input_py.txt"))
            {
                var code = reader.ReadToEnd();
                return code;
            }
        }
        public static string GetProcessCode()
        {
            using (StreamReader reader = new StreamReader("process_py.txt"))
            {
                var code = reader.ReadToEnd();
                return code;
            }
        }
        public static string GetOutputCode()
        {
            using (StreamReader reader = new StreamReader("output_py.txt"))
            {
                var code = reader.ReadToEnd();
                return code;
            }
        }
        public static List<NodeTask> GetNodeRepo()
        {
            Header header = new Header();

            NodeTask task1 = new NodeTask();
            task1.headerList = new Header();//new List<Header>();
            task1.headerList.headerType = "header1";
            task1.headerList.exportedVariables = new List<string>() { "x:2" };
            // header.keyValues = new Dictionary<string, string>() { { "key1", "val1"}, { "key2", "val2" } };
            //task1.headerList.Add(header);
            task1.taskName = "postgres reader-py";
            task1.taskDescription = "input data";
            task1.nodeType = "input";
            task1.oper = Operator.py;
            task1.commandBody = "#def process_custom_code(df):\r\n#    add code here and uncomment\r\n#    return df\r\n\r\n\r\n";
            task1.template = GetInputCode();
            NodeTask task2 = new NodeTask();


            task2.taskName = "process-py";
            task2.taskDescription = "process data";
            task2.nodeType = "process";
            task2.oper = Operator.py;
            task2.commandBody = "#def process_custom_code(df):\r\n#    add code here and uncomment\r\n#    return df\r\n\r\n\r\n";
            task2.template = GetProcessCode();//"import os\r\nfrom datetime import time, datetime\r\nimport digdag\r\nimport pandas as pd\r\nimport psycopg2\r\nimport numpy as np\r\nfrom objectpath.utils.timeutils import now\r\nfrom pandas import DataFrame\r\nfrom psycopg2.extras import RealDictCursor\r\nfrom sqlalchemy import MetaData\r\nfrom sqlalchemy import create_engine\r\nfrom sqlalchemy import event\r\nimport sklearn\r\nfrom sklearn import preprocessing\r\nfrom sklearn import feature_extraction\r\nfrom sklearn.preprocessing import MinMaxScaler\r\nfrom sklearn.feature_extraction.text import TfidfTransformer\r\n\r\n\r\ndef remove_temp_db(delete_query):\r\n    con = psycopg2.connect(database=\"digdagdb\", user=\"ubuntu\", password=\"password\", host=\"172.17.0.1\", port=\"5432\")\r\n    print(\"Database opened successfully for removal\")\r\n    cur = con.cursor()\r\n    cur.execute(delete_query)\r\n\r\ndef do_transformation(df):\r\n    print(\"do_tranf\")\r\n    process_df = process_custom_code(df)\r\n    insert_data_postgres(process_df, '[NODENAME]_[NODEID]', 'public')# process_block_{userid}_{digdagprojectid}_{digdagworkflowid}_{digdagattemptid}_{nodeid}\r\n    \r\ndef input_postgres(input_table):\r\n    print(\"input postgres\")\r\n    input_query = \"select * from \"+input_table\r\n    con = psycopg2.connect(database=\"digdagdb\", user=\"ubuntu\", password=\"password\", host=\"172.17.0.1\", port=\"5432\")\r\n    print(\"Database opened successfully\")\r\n    with con.cursor(name='custom_cursor', cursor_factory=RealDictCursor) as cursor:\r\n        cursor.execute(input_query)\r\n\r\n        while True:\r\n            col_names = []\r\n            records = cursor.fetchmany(size=10000)\r\n            col_set = False\r\n            if not col_set:\r\n                for elt in cursor.description:\r\n                    col_names.append(elt[0])\r\n            if not records:\r\n                break\r\n            col_set = True\r\n            df = DataFrame(records)\r\n            print(\"dataframes read\")\r\n            df.columns = col_names\r\n            do_transformation(df)\r\n        cursor.close()  # don't forget to cleanup\r\n    con.close()\r\n\r\n\r\ndef insert_data_postgres(df, table_name, schema):\r\n    dbschema = schema\r\n    engine = create_engine('postgresql+psycopg2:\/\/ubuntu:password@172.17.0.1:5432\/digdagdb',\r\n                connect_args={'options': '-csearch_path={}'.format(dbschema)})\r\n\r\n    df.to_sql(table_name, engine, if_exists='append', index=False)\r\n    engine.dispose()\r\n    return True\r\n\r\n\r\ndef process_block():\r\n    print(\"input\")\r\n    input_postgres([NODENAME]_[INPUTNODEID])\r\n    print(\" done with process block\")\r\n    remove_temp_db('drop table [NODENAME]_[INPUTNODEID]')\r\nif __name__ == \"__main__\":\r\n    process_block()";
            NodeTask task3 = new NodeTask();


            task3.taskName = "postgres output-py";
            task3.taskDescription = "output data";
            task3.nodeType = "output";
            task3.oper = Operator.py;
            task3.commandBody = "#def process_custom_code(df):\r\n#    add code here and uncomment\r\n#    return df\r\n\r\n\r\n";
            task3.template = GetOutputCode();//"import os\r\nimport psycopg2\r\nimport pandas as pd\r\nfrom pandas import DataFrame\r\nfrom numpy import repeat\r\nfrom psycopg2.extras import RealDictCursor\r\nfrom sqlalchemy import MetaData\r\nfrom sqlalchemy import create_engine\r\nfrom sqlalchemy import event\r\nimport pickle\r\nfrom sklearn.linear_model import LinearRegression\r\nfrom sklearn.compose import ColumnTransformer\r\nfrom sklearn.pipeline import Pipeline\r\nfrom sklearn.preprocessing import OneHotEncoder, StandardScaler, MinMaxScaler, PolynomialFeatures\r\n\r\n\r\n\r\ndef remove_temp_db(delete_query):\r\n    con = psycopg2.connect(database=\"digdagdb\", user=\"ubuntu\", password=\"password\", host=\"172.17.0.1\", port=\"5432\")\r\n    print(\"Database opened successfully\")\r\n    cur = con.cursor()\r\n    cur.execute(delete_query)\r\n\r\ndef do_transformation(df):\r\n    process_df = process_custom_code(df)\r\n    if isinstance(process_df, pd.DataFrame):\r\n       print(\"regular\")\r\n       insert_data_postgres(process_df, '[NODENAME]_[NODEID]', 'public')# process_block_{userid}_{digdagprojectid}_{digdagworkflowid}\r\n    else:\r\n       print(\"model\")\r\n       insert_model_data_postgres(process_df, '[NODENAME]_[NODEID]', 'public')\r\n       \r\n\r\ndef input_postgres(input_table):\r\n    input_query = \"select * from \"+input_table\r\n    con = psycopg2.connect(database=\"digdagdb\", user=\"ubuntu\", password=\"password\", host=\"172.17.0.1\", port=\"5432\")\r\n    print(\"Database opened successfully\")\r\n    with con.cursor(name='custom_cursor', cursor_factory=RealDictCursor) as cursor:\r\n        cursor.execute(input_query)\r\n        while True:\r\n            col_names = []\r\n            records = cursor.fetchmany(size=1000)\r\n            col_set = False\r\n            if not col_set:\r\n                for elt in cursor.description:\r\n                    col_names.append(elt[0])\r\n            if not records:\r\n                break\r\n            col_set = True\r\n            df = DataFrame(records)\r\n            df.columns = col_names\r\n            do_transformation(df)\r\n            #insert_data_postgres(df, 'output11', 'public')\r\n        cursor.close()  # don't forget to cleanup\r\n    con.close()\r\n\r\n\r\n\r\ndef insert_data_postgres(df, table_name, schema):\r\n    dbschema = schema\r\n    print(\" postgre start \")\r\n    engine = create_engine('postgresql+psycopg2:\/\/postgres:dapdata123@idapt.duckdns.org:5432\/postgres', connect_args={'options': '-csearch_path={}'.format(dbschema)})\r\n\r\n    df.to_sql(table_name, engine, if_exists= 'append',  method='multi', index=False)\r\n    engine.dispose()\r\n    return True\r\n\r\ndef insert_model_data_postgres(model, table_name, schema):\r\n    dbschema = schema\r\n    sql_stmt = \"\"\"create table if not exists {}(model_pickle bytea)\"\"\".format( table_name)\r\n    #params = config()\r\n    # connect to the PostgresQL database\r\n    print(\"connect\")\r\n    conn = psycopg2.connect(dbname=\"digdagdb\",user=\"ubuntu\",password=\"password\",host=\"172.17.0.1\")\r\n    # create a new cursor object\r\n    print(\"connected\")\r\n    cur = conn.cursor()\r\n    cur.execute(sql_stmt)\r\n    # execute the INSERT statement\r\n    cur.execute(\"INSERT INTO \" + table_name +\" (model_pickle) \" +\r\n                    \"VALUES(%s)\",\r\n                    (psycopg2.Binary(pickle.dumps(model)),))\r\n    # commit the changes to the database\r\n    conn.commit()\r\n   # close the communication with the PostgresQL database\r\n    cur.close()\r\ndef output():\r\n\r\n    input_df = input_postgres('[NODENAME]_[INPUTNODEID]')\r\n    remove_temp_db('drop table public.[NODENAME]_[INPUTNODEID]')\r\n    print(\"done all\")\r\n\r\nif __name__ == \"__main__\":\r\n    output()\r\n";


            return new List<NodeTask> { task1, task2, task3 };
        }

      
    }

        
}

