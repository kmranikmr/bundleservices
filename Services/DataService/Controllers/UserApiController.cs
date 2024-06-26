﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using Common.Utils;
using DataAccess.DTO;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataService.Controllers
{
    public class PredictDTO
    {
        public string urlModel;
        public int searchHistoryId;
    }
    public class SimilarDTO
    {
        public string urlModel;
        public int searchHistoryId;
        public string collection;
    }
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    
    public class UserApiController : ControllerBase
    {
        private readonly IRepository _repository;

        private readonly IMapper _mapper;

        private readonly ILogger<UserApiController> _logger;

        public UserApiController(IRepository repo, IMapper mapper, ILogger<UserApiController> logger)
        {
            _repository = repo;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Jobs
        [HttpGet("[action]")]
        public async Task<ActionResult<UserKeyDTO>> AddUserKey()
        {
            try
            {
                Console.WriteLine("adduserkey");
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                //call add user key
                //return the key
                string APIKey = "";
                using (var cryptoProvider = new RNGCryptoServiceProvider())
                {
                    byte[] secretKeyByteArray = new byte[32]; //256 bit
                    cryptoProvider.GetBytes(secretKeyByteArray);
                    APIKey = Convert.ToBase64String(secretKeyByteArray);
                }
                Console.WriteLine($"apikey {APIKey}");
                var userKey = await _repository.AddUserKey(userId, Api.GenerateHashedKey(APIKey));
                Console.WriteLine($"userKey {userKey}");
                if (userKey != null)
                {
                    return new UserKeyDTO(APIKey);
                }
                else
                {
                    return new UserKeyDTO(APIKey);
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

        }
        ////[HttpPost("[action]/{isWorkflow:bool=false}")]
        ////public async Task<ActionResult<UserSharedUrlDTO[]>> AddSharedUrl(string type, [FromBody] int searchHistoryId, [FromBody] string queryBody,  [FromHeader] string authorization , bool isWorkflow)
        ////{
        ////    try
        ////    {
        ////        int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        ////        var SharedUrl = await _repository.GetSharedUrl(userId, searchHistoryId, isWorkflow);
        ////        if (!isWorkflow)
        ////        {
        ////            var history = await _repository.GetSearchHistory(searchHistoryId, userId);
        ////            if (history != null)
        ////            {
        ////            }
        ////        }
        ////        else
        ////        {
        ////            var history = await _repository.GetWorkflowSearchHistory(searchHistoryId, userId);
        ////            if (history != null)
        ////            {
        ////            }
        ////        }
        ////    }
        ////    catch (Exception)
        ////    {
        ////        return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
        ////    }
        ////}

        //--
        [HttpPost("[action]/{isWorkflow:bool=false}")]
        public async Task<ActionResult<UserSharedUrlDTO[]>> AddSharedPredictUrl([FromBody] PredictDTO pDTO, [FromHeader] string authorization, bool isWorkflow)
        {
            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var allSharedUrl = await _repository.GetSharedUrl(userId, isWorkflow);
                var allSharedPredict = allSharedUrl.Where(x => x.SharedUrl.Contains("predict"));
                if(allSharedPredict != null)
                {
                    if (!isWorkflow)
                    {
                        var history = await _repository.GetSearchHistory(pDTO.searchHistoryId, userId);
                        if (history != null)
                        {

                            string Name = history.SearchHistoryName;

                            Console.WriteLine($"Name {Name}");
                            var updated = await _repository.AddSharedUrl(userId, pDTO.searchHistoryId, $"/endpoint/predict/{Name}{{\"urlmodel\":urlph/{pDTO.urlModel}, \"data\":dataph}}", false);
                            
                            //Console.WriteLine($"resolvedquery {history.ResolvedSearchQuery}");
                            //var ret = Utils.CallCreateView(history.ResolvedSearchQuery, Name, authorization);

                            if (updated == null)
                            {
                                return null;
                            }
                            allSharedPredict.Append(updated);
                        }
                    }
                    else
                    {
                        var history = await _repository.GetWorkflowSearchHistory(pDTO.searchHistoryId, userId);
                        if (history != null)
                        {
                            string Name = history.WorkflowSearchHistoryName;
                            Console.WriteLine($"Workflow Name {Name}");

                            //var updated = await _repository.AddSharedUrl(userId, id, $"/workflow/{Name}", true);
                            var updated = await _repository.AddSharedUrl(userId, pDTO.searchHistoryId, $"/endpoint/predict/{Name}{{\"urlmodel\":urlph/{pDTO.urlModel}, \"data\":dataph}}", false);

                            //var ret = Utils.CallCreateView(history.ResolvedSearchQuery, Name, authorization);

                            if (updated == null)
                            {
                                return null;
                            }
                            allSharedPredict.Append(updated);
                        }
                    }
                }
                var reDTO = _mapper.Map<UserSharedUrlDTO[]>(allSharedPredict.ToArray());
                return reDTO;

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
        [HttpPost("[action]/{isWorkflow:bool=false}")]
        public async Task<ActionResult<UserSharedUrlDTO[]>> AddSharedSimilarUrl([FromBody] SimilarDTO sDTO, [FromHeader] string authorization, bool isWorkflow)
        {
            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var allSharedUrl = await _repository.GetSharedUrl(userId, isWorkflow);
                var allSharedSimilar = allSharedUrl.Where(x => x.SharedUrl.Contains("similar"));
                if (allSharedSimilar != null)
                {
                    if (!isWorkflow)
                    {
                        var history = await _repository.GetSearchHistory(sDTO.searchHistoryId, userId);
                        if (history != null)
                        {

                            string Name = history.SearchHistoryName;

                            Console.WriteLine($"Name {Name}");
                            var updated = await _repository.AddSharedUrl(userId, sDTO.searchHistoryId, $"/endpoint/similarity/{Name}{{\"type\":\"simple\",\"lookup_api\":urlph/{sDTO.urlModel}, \"values\":valuesph, \"headers\":headersph, \"field_name\": field_nameph }}", false);

                            //Console.WriteLine($"resolvedquery {history.ResolvedSearchQuery}");
                            //var ret = Utils.CallCreateView(history.ResolvedSearchQuery, Name, authorization);

                            if (updated == null)
                            {
                                return null;
                            }
                            allSharedSimilar.Append(updated);
                        }
                    }
                    else
                    {
                        var history = await _repository.GetWorkflowSearchHistory(sDTO.searchHistoryId, userId);
                        if (history != null)
                        {
                            string Name = history.WorkflowSearchHistoryName;
                            Console.WriteLine($"Workflow Name {Name}");

                            //var updated = await _repository.AddSharedUrl(userId, id, $"/workflow/{Name}", true);
                            var updated = await _repository.AddSharedUrl(userId, sDTO.searchHistoryId, $"/endpoint/similarity/{Name}_{sDTO.collection}{{\"type\":\"simple\",\"lookup_api\":urlph/{sDTO.urlModel}, \"values\":valuesph, \"headers\":headersph, \"field_name\": field_nameph }}", true);

                            //var ret = Utils.CallCreateView(history.ResolvedSearchQuery, Name, authorization);

                            if (updated == null)
                            {
                                return null;
                            }
                            allSharedSimilar.Append(updated);
                        }
                    }
                }
                var reDTO = _mapper.Map<UserSharedUrlDTO[]>(allSharedSimilar.ToArray());
                return reDTO;

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
        //--
        [HttpPost("[action]/{isWorkflow:bool=false}")]
        public async Task<ActionResult<UserSharedUrlDTO[]>> AddSharedUrl([FromBody] int[] searchHistoryIds, [FromHeader] string authorization , bool isWorkflow)
        {
            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var allSharedUrl = await _repository.GetSharedUrl(userId, isWorkflow);
                if (allSharedUrl != null)
                {
                    IEnumerable<int> notSetIds = null;
                    if (!isWorkflow)
                    {
                        notSetIds = searchHistoryIds.Where(p => allSharedUrl.All(p2 => p2.SearchHistoryId != p));
                    }
                    else
                    {
                        notSetIds = searchHistoryIds.Where(p => allSharedUrl.All(p2 => p2.WorkflowSearchHistoryId != p));
                    }

                    if (notSetIds != null)
                    {
                        foreach (var item in notSetIds)
                        {
                            var ret = await _repository.RemoveSharedUrl(userId, item, isWorkflow);
                        }
                    }
                }
                
                foreach (int id in searchHistoryIds)
                {
                    if (!isWorkflow)
                    {
                        var history = await _repository.GetSearchHistory(id, userId);
                        if (history != null)
                        {
                         
                            string Name = history.SearchHistoryName;
                           
                            Console.WriteLine($"Name {Name}");
                            var updated = await _repository.AddSharedUrl(userId, id, $"/project/{Name}", false);
                            
                            Console.WriteLine($"resolvedquery {history.ResolvedSearchQuery}");
                            var ret = Utils.CallCreateView(history.ResolvedSearchQuery, Name, authorization);

                            if ( ret.Result == false)
                            {
                                return null;
                            }
                        }
                    }
                    else
                    {
                        var history = await _repository.GetWorkflowSearchHistory(id, userId);
                        if (history != null)
                        {
                            string Name = history.WorkflowSearchHistoryName;
                            Console.WriteLine($"Workflow Name {Name}");
                             
                            var updated = await _repository.AddSharedUrl(userId, id, $"/workflow/{Name}", true);
                            var ret = Utils.CallCreateView(history.ResolvedSearchQuery, Name, authorization);

                            if (ret.Result == false)
                            {
                                Console.WriteLine("return null");
                                return null;
                            }
                        }
                    }
                }
                var allSharedUrlNew = await _repository.GetSharedUrl(userId, isWorkflow);
                var reDTO = _mapper.Map<UserSharedUrlDTO[]>(allSharedUrlNew);
                foreach ( var dto in reDTO)
                {
                    if ( dto.SearchHistoryId != null )
                    {
                        var sh = await _repository.GetSearchHistory((int)dto.SearchHistoryId, userId);
                        if (sh.FriendlyName == null)
                        {
                            dto.FriendlyName = sh.SearchQuery;
                        }
                        else
                        {
                            dto.FriendlyName = sh.FriendlyName;
                        }
                    }
                    else if (dto.WorkflowSearchHistoryId != null )
                    {
                        var sh = await _repository.GetWorkflowSearchHistory((int)dto.WorkflowSearchHistoryId, userId);
                        if (sh.FriendlyName == null)
                        {
                            dto.FriendlyName = sh.SearchQuery;
                        }
                        else
                        {

                            dto.FriendlyName = sh.FriendlyName;
                        }
                    }
                }
                return reDTO;
                ///call getsearchistory with each id then get searchhistoryname.. construct urlll../api/project/{made upname}
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("[action]/{isWorkflow:bool=false}")]
        public async Task<ActionResult<UserSharedUrlDTO[]>> GetSharedUrlList( bool isWorkflow)
        {
            try
            {
                int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var allSharedUrlNew = await _repository.GetSharedUrl(userId, isWorkflow);
                var reDTO = _mapper.Map<UserSharedUrlDTO[]>(allSharedUrlNew);
                foreach (var dto in reDTO)
                {
                    if (dto.SearchHistoryId != null)
                    {
                        var sh = await _repository.GetSearchHistory((int)dto.SearchHistoryId, userId);
                        if (sh.FriendlyName == null)
                        {
                            dto.FriendlyName = sh.SearchQuery;
                        }
                        else
                        {
                            dto.FriendlyName = sh.FriendlyName;
                        }
                    }
                    else if (dto.WorkflowSearchHistoryId != null)
                    {
                        var sh = await _repository.GetWorkflowSearchHistory((int)dto.WorkflowSearchHistoryId, userId);
                        if (sh.FriendlyName == null)
                        {
                            dto.FriendlyName = sh.SearchQuery;
                        }
                        else
                        {

                            dto.FriendlyName = sh.FriendlyName;
                        }
                    }
                }
                return reDTO;
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
                //--

                /*
                // POST make shared url
                [HttpPost("[action]/{isWorkflow:bool=false}")]
                public async Task<ActionResult<UserSharedUrlDTO[]>> AddSharedUrl([FromBody] int[] searchHistoryIds, [FromHeader] string authorization , bool isWorkflow)
                {
                    try
                    {
                        int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                        var allSharedUrl = _repository.GetSharedUrl(userId, isWorkflow);
                        if (allSharedUrl != null)
                        {
                            IEnumerable<int> notSetIds = null;
                            if (!isWorkflow)
                            {
                                notSetIds = searchHistoryIds.Where(p => allSharedUrl.Result.All(p2 => p2.SearchHistoryId != p));
                            }
                            else
                            {
                                notSetIds = searchHistoryIds.Where(p => allSharedUrl.Result.All(p2 => p2.WorkflowSearchHistoryId != p));
                            }

                            if (notSetIds != null)
                            {
                                foreach (var item in notSetIds)
                                {
                                    var ret = await _repository.RemoveSharedUrl(userId, item, isWorkflow);
                                }
                            }
                        }

                        foreach (int id in searchHistoryIds)
                        {
                            if (!isWorkflow)
                            {
                                var history =  _repository.GetSearchHistory(id, userId);
                                if (history != null)
                                {

                                    string Name = history.Result.SearchHistoryName;

                                    Console.WriteLine($"Name {Name}");
                                    var updated = _repository.AddSharedUrl(userId, id, $"/project/{Name}", false);

                                    Console.WriteLine($"resolvedquery {history.Result.ResolvedSearchQuery}");
                                    var ret = Utils.CallCreateView(history.Result.ResolvedSearchQuery, Name, authorization);

                                    if ( ret.Result == false)
                                    {
                                        return null;
                                    }
                                }
                            }
                            else
                            {
                                var history =  _repository.GetWorkflowSearchHistory(id, userId);

                                if (history != null)
                                {
                                    string Name = history.Result.WorkflowSearchHistoryName;
                                     Console.WriteLine($"Workflow Name {Name}");
                                    var updated = _repository.AddSharedUrl(userId, id, $"/workflow/{Name}", true);
                                    var ret = Utils.CallCreateView(history.Result.ResolvedSearchQuery, Name, authorization);

                                    if (ret.Result == false)
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                        var allSharedUrlNew = await _repository.GetSharedUrl(userId, isWorkflow);
                        var reDTO = _mapper.Map<UserSharedUrlDTO[]>(allSharedUrlNew);
                        return reDTO;
                        ///call getsearchistory with each id then get searchhistoryname.. construct urlll../api/project/{made upname}
                    }
                    catch (Exception)
                    {
                        return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
                    }
                }

                */

                [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<ActionResult<bool>> Validate()//, [FromBody] string url)
        {
            try
            {
                var re = Request;
                var headers = re.Headers;
                StringValues apiKey = "";
              //  if (headers.Contains("api-key"))
                {
                    headers.TryGetValue("api-key" , out apiKey);
                }
                
                string apiKeyAvailable = apiKey.FirstOrDefault();
                if (apiKeyAvailable != null)
                {
                    var userApi = await _repository.GetKeyDetailsfromApiKey(apiKeyAvailable.ToString());
                    if (userApi == null)
                    {
                        return new UnauthorizedResult();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [AllowAnonymous]
        [HttpGet("[action]/{queryName}")]
        public async Task<ActionResult<bool>> CheckSharedData([FromRoute] string queryName)//, [FromBody] string url)
        {
            try
            {
                
                var re = Request;
                var headers = re.Headers;
                StringValues apiKey = "";             
                headers.TryGetValue("api-key", out apiKey);
                
                string apiKeyAvailable = apiKey.FirstOrDefault();

                if (apiKeyAvailable != null)
                {
                    var userApi = await _repository.GetKeyDetailsfromApiKey(apiKeyAvailable);

                    if (userApi != null)
                    {
                        var searchHistory = await _repository.GetSearchHistory(queryName, userApi.UserId);
                        bool ret = await _repository.CheckSharedUrl(userApi.UserId, queryName);
                        if ( ret == true)
                        {
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }


        // on ApiGatewayService
        /// 1. Shared url comes with api key: first we check the api key is good
        /// 2. if authorized then use the api key find the actual user id - 
        /// 3. use that user is ..find by tat searchname..based on workflow /project///
        /// 4..if we find the sharedname -> get the id also from 3. 
        /// 5. for taht searchhistroy id...construct actual urll...using projectid..find the schema name and model name...from dataseervce
        /// 6. we convert a[i/project/sharedname -> /api/{databasename}/{schemaname}_{projectid}_{userid}/{modelname}=search?={query}
        /// 7. now call the prest ..we get some dynamic json...return the result as-is..

    }
}
