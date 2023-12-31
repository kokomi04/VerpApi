﻿using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Infrastructure.ServiceCore.Service
{

    public delegate void OptionHttpRequestDelegate(HttpRequestMessage request);

    public interface IHttpClientFactoryService
    {
        Task<T> Deleted<T>(string relativeUrl, object postData, OptionHttpRequestDelegate optionRequest = null, object queries = null);
        Task<T> Get<T>(string relativeUrl, object queries = null, OptionHttpRequestDelegate optionRequest = null, JsonSerializerSettings settings = null);
        Task<T> Post<T>(string relativeUrl, object postData, OptionHttpRequestDelegate optionRequest = null, JsonSerializerSettings settings = null, Func<string, ApiErrorResponse> errorHandler = null, object queries = null);
        Task<T> Put<T>(string relativeUrl, object postData, OptionHttpRequestDelegate optionRequest = null, JsonSerializerSettings settings = null, object queries = null);
        Task<Stream> Download(string relativeUrl, object postData, OptionHttpRequestDelegate optionRequest = null);
    }

    public class HttpClientFactoryService : IHttpClientFactoryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public HttpClientFactoryService(HttpClient httpClient, ILogger<HttpClientFactoryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<T> Post<T>(string relativeUrl, object postData, OptionHttpRequestDelegate optionRequest = null, JsonSerializerSettings settings = null, Func<string, ApiErrorResponse> errorHandler = null, object queries = null)
        {
            try
            {
                var uri = AppendQueryData(relativeUrl, queries);

                var body = postData.JsonSerialize();


                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Post,
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                };

                if (optionRequest != null)
                    optionRequest(request);

                var data = await _httpClient.SendAsync(request);

                var response = await data.Content.ReadAsStringAsync();

                if (!data.IsSuccessStatusCode)
                {
                    ApiErrorResponse errorResult = null;
                    if (errorHandler != null)
                        errorResult = errorHandler(response);

                    ThrowErrorResponse("POST", uri, postData, data, response, errorResult);
                }

                return response.JsonDeserialize<T>(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpClientFactoryService:Post");
                throw;
            }
        }


        public async Task<T> Put<T>(string relativeUrl, object postData, OptionHttpRequestDelegate optionRequest = null, JsonSerializerSettings settings = null, object queries = null)
        {
            try
            {
                var uri = AppendQueryData(relativeUrl, queries);

                var body = postData.JsonSerialize();


                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Put,
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                };

                if (optionRequest != null)
                    optionRequest(request);

                var data = await _httpClient.SendAsync(request);

                var response = await data.Content.ReadAsStringAsync();

                if (!data.IsSuccessStatusCode)
                {
                    ThrowErrorResponse("PUT", uri, postData, data, response);
                }

                return response.JsonDeserialize<T>(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpClientFactoryService:Put");
                throw;
            }
        }


        public async Task<T> Deleted<T>(string relativeUrl, object postData, OptionHttpRequestDelegate optionRequest = null, object queries = null)
        {
            try
            {
                var uri = AppendQueryData(relativeUrl, queries);

                var body = postData.JsonSerialize();


                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Delete,
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                };

                if (optionRequest != null)
                    optionRequest(request);

                var data = await _httpClient.SendAsync(request);

                var response = await data.Content.ReadAsStringAsync();

                if (!data.IsSuccessStatusCode)
                {
                    ThrowErrorResponse("DELETE", uri, postData, data, response);
                }

                return response.JsonDeserialize<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpClientFactoryService:Put");
                throw;
            }
        }

        public async Task<T> Get<T>(string relativeUrl, object queries = null, OptionHttpRequestDelegate optionRequest = null, JsonSerializerSettings settings = null)
        {
            try
            {
                var uri = AppendQueryData(relativeUrl, queries);

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Get
                };

                if (optionRequest != null)
                    optionRequest(request);

                var data = await _httpClient.SendAsync(request);

                var response = await data.Content.ReadAsStringAsync();

                if (!data.IsSuccessStatusCode)
                {
                    ThrowErrorResponse("GET", uri, null, data, response);
                }

                return response.JsonDeserialize<T>(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpClientFactoryService:Get");
                throw;
            }
        }



        public async Task<Stream> Download(string relativeUrl, object postData, OptionHttpRequestDelegate optionRequest = null)
        {
            try
            {
                var uri = relativeUrl;
                var body = postData.JsonSerialize();


                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Post,
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                };

                if (optionRequest != null)
                    optionRequest(request);

                var data = await _httpClient.SendAsync(request);

                var response = await data.Content.ReadAsStringAsync();

                if (!data.IsSuccessStatusCode)
                {
                    ThrowErrorResponse("POST", uri, postData, data, response);
                }

                return await data.Content.ReadAsStreamAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HttpClientFactoryService:Post");
                throw;
            }
        }

        private void ThrowErrorResponse(string method, string uri, object body, HttpResponseMessage responseMessage, string response, ApiErrorResponse result = null)
        {
            try
            {
                if (result == null)
                    result = response.JsonDeserialize<ApiErrorResponse>();
            }
            catch (Exception)
            {

            }

            if (result != null)
            {
                _logger.LogError($"HttpClientFactoryService:{method} {uri} {{0}} Warning {responseMessage.StatusCode} {{1}}", body, response);

                if (responseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, result.Message);
                }
                else
                {
                    throw new Exception(result.Message);
                }
            }

            _logger.LogError($"HttpClientFactoryService:{method} {uri} {{0}} Error {(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase} {{1}}", body, response);

            throw new Exception($"{(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase} {response}");
        }

        private string AppendQueryData(string relativeUrl, object queries = null)
        {
            var uri = relativeUrl;
            var queryData = new Dictionary<string, string>();
            if (queries != null)
            {
                var props = queries.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var v = prop.GetValue(queries, null);

                    if (!v.IsNullOrEmptyObject())
                    {
                        if (v.GetType().IsEnum)
                        {
                            queryData.Add(prop.Name, ((int)v).ToString());
                        }
                        else
                        {
                            if (v != null)
                                queryData.Add(prop.Name, v.ToString());
                        }
                    }
                }
            }

            if (queryData.Count > 0)
            {
                uri = QueryHelpers.AddQueryString(uri, queryData);
            }

            return uri;
        }
    }
}
