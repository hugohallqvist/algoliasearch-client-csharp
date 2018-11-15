﻿/*
* Copyright (c) 2018 Algolia
* http://www.algolia.com/
* Based on the first version developed by Christopher Maneu under the same license:
*  https://github.com/cmaneu/algoliasearch-client-csharp
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using Algolia.Search.Http;
using Algolia.Search.Models.Batch;
using Algolia.Search.Models.Enums;
using Algolia.Search.Models.Requests;
using Algolia.Search.Models.Responses;
using Algolia.Search.Transport;
using Algolia.Search.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Algolia.Search.Clients
{
    public class SearchClient : ISearchClient
    {
        private readonly IRequesterWrapper _requesterWrapper;

        /// <summary>
        /// Initialize a client with default settings
        /// </summary>
        public SearchClient() : this(new AlgoliaConfig(), new AlgoliaHttpRequester())
        {
        }

        /// <summary>
        /// Create a new search client for the given appID
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="apiKey"></param>
        public SearchClient(string applicationId, string apiKey) : this(new AlgoliaConfig { ApiKey = apiKey, AppId = applicationId }, new AlgoliaHttpRequester())
        {
        }

        /// <summary>
        /// Initialize a client with custom config
        /// </summary>
        /// <param name="config"></param>
        public SearchClient(AlgoliaConfig config) : this(config, new AlgoliaHttpRequester())
        {
        }

        /// <summary>
        /// Initialize the client with custom config and custom Requester
        /// </summary>
        /// <param name="config"></param>
        /// <param name="httpRequester"></param>
        public SearchClient(AlgoliaConfig config, IHttpRequester httpRequester)
        {
            if (httpRequester == null)
            {
                throw new ArgumentNullException(nameof(httpRequester), "An httpRequester is required");
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "A config is required");
            }

            if (string.IsNullOrWhiteSpace(config.AppId))
            {
                throw new ArgumentNullException(nameof(config.AppId), "Application ID is required");
            }

            if (string.IsNullOrWhiteSpace(config.ApiKey))
            {
                throw new ArgumentNullException(nameof(config.ApiKey), "An API key is required");
            }

            _requesterWrapper = new RequesterWrapper(config, httpRequester);
        }

        /// <summary>
        /// Initialize an index for the given client
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public SearchIndex InitIndex(string indexName)
        {
            return string.IsNullOrWhiteSpace(indexName)
                ? throw new ArgumentNullException(nameof(indexName), "The Index name is required")
                : new SearchIndex(_requesterWrapper, indexName);
        }

        /// <summary>
        /// Retrieve one or more objects, potentially from different indices, in a single API call.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public MultipleGetObjectsResponse<T> MultipleGetObjects<T>(MultipleGetObjectsRequest queries, RequestOption requestOptions = null) where T : class =>
                    AsyncHelper.RunSync(() => MultipleGetObjectsAsync<T>(queries, requestOptions));

        /// <summary>
        /// Retrieve one or more objects, potentially from different indices, in a single API call.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async Task<MultipleGetObjectsResponse<T>> MultipleGetObjectsAsync<T>(MultipleGetObjectsRequest queries, RequestOption requestOptions = null,
                            CancellationToken ct = default(CancellationToken)) where T : class
        {
            if (queries == null)
            {
                throw new ArgumentNullException(nameof(queries));
            }

            return await _requesterWrapper.ExecuteRequestAsync<MultipleGetObjectsResponse<T>>(HttpMethod.Post,
                "/1/indexes/*/objects", CallType.Read, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// This method allows to send multiple search queries, potentially targeting multiple indices, in a single API call.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public MultipleQueriesResponse<T> MultipleQueries<T>(MultipleQueriesRequest queries, RequestOption requestOptions = null) where T : class =>
                    AsyncHelper.RunSync(() => MultipleQueriesAsync<T>(queries, requestOptions));

        /// <summary>
        /// This method allows to send multiple search queries, potentially targeting multiple indices, in a single API call.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async Task<MultipleQueriesResponse<T>> MultipleQueriesAsync<T>(MultipleQueriesRequest queries, RequestOption requestOptions = null,
                            CancellationToken ct = default(CancellationToken)) where T : class
        {
            if (queries == null)
            {
                throw new ArgumentNullException(nameof(queries));
            }

            return await _requesterWrapper.ExecuteRequestAsync<MultipleQueriesResponse<T>>(HttpMethod.Post,
                "/1/indexes/*/queries", CallType.Read, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform multiple write operations, potentially targeting multiple indices, in a single API call.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public MultipleBatchResponse MultipleBatch<T>(IEnumerable<BatchOperation<T>> operations, RequestOption requestOptions = null) where T : class =>
                    AsyncHelper.RunSync(() => MultipleBatchAsync(operations, requestOptions));

        /// <inheritdoc />
        /// <summary>
        /// Perform multiple write operations, potentially targeting multiple indices, in a single API call.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public async Task<MultipleBatchResponse> MultipleBatchAsync<T>(IEnumerable<BatchOperation<T>> operations, RequestOption requestOptions = null,
                            CancellationToken ct = default(CancellationToken)) where T : class
        {
            if (operations == null)
            {
                throw new ArgumentNullException(nameof(operations));
            }

            var batch = new BatchRequest<T>(operations);

            return await _requesterWrapper.ExecuteRequestAsync<MultipleBatchResponse, BatchRequest<T>>(HttpMethod.Post,
                "/1/indexes/*/batch", CallType.Write, batch, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of indexes/indices with their associated metadata.
        /// </summary>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public ListIndexesResponse ListIndexes(RequestOption requestOptions = null) =>
                    AsyncHelper.RunSync(() => ListIndexesAsync(requestOptions));

        /// <summary>
        /// Get a list of indexes/indices with their associated metadata.
        /// </summary>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ListIndexesResponse> ListIndexesAsync(RequestOption requestOptions = null,
                    CancellationToken ct = default(CancellationToken))
        {
            return await _requesterWrapper.ExecuteRequestAsync<ListIndexesResponse>(HttpMethod.Get,
                "/1/indexes", CallType.Read, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete an index by name
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public DeleteResponse DeleteIndex(string indexName, RequestOption requestOptions = null) =>
                    AsyncHelper.RunSync(() => DeleteIndexAsync(indexName, requestOptions));

        /// <summary>
        /// Delete an index by name
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<DeleteResponse> DeleteIndexAsync(string indexName, RequestOption requestOptions = null,
                    CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                throw new ArgumentNullException(indexName);
            }

            return await _requesterWrapper.ExecuteRequestAsync<DeleteResponse>(HttpMethod.Delete,
                $"/1/indexes/{indexName}", CallType.Write, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the full list of API Keys.
        /// </summary>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public ListApiKeysResponse ListApiKeys(RequestOption requestOptions = null) =>
                    AsyncHelper.RunSync(() => ListApiKeysAsync(requestOptions));

        /// <summary>
        /// Get the full list of API Keys.
        /// </summary>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ListApiKeysResponse> ListApiKeysAsync(RequestOption requestOptions = null,
                    CancellationToken ct = default(CancellationToken))
        {
            return await _requesterWrapper.ExecuteRequestAsync<ListApiKeysResponse>(HttpMethod.Get,
                "/1/keys", CallType.Read, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the full list of API Keys.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public ApiKeysResponse GetApiKey(string apiKey, RequestOption requestOptions = null) =>
                    AsyncHelper.RunSync(() => GetApiKeyAsync(apiKey, requestOptions));

        /// <summary>
        /// Get the full list of API Keys.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ApiKeysResponse> GetApiKeyAsync(string apiKey, RequestOption requestOptions = null,
                    CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentNullException(apiKey);
            }

            return await _requesterWrapper.ExecuteRequestAsync<ApiKeysResponse>(HttpMethod.Get,
                $"/1/keys/{apiKey}", CallType.Read, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a new API Key with specific permissions/restrictions.
        /// </summary>
        /// <param name="acl"></param>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public AddApiKeyResponse AddApiKey(ApiKeyRequest acl, RequestOption requestOptions = null) =>
                    AsyncHelper.RunSync(() => AddApiKeyAsync(acl, requestOptions));

        /// <summary>
        /// Add a new API Key with specific permissions/restrictions.
        /// </summary>
        /// <param name="acl"></param>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<AddApiKeyResponse> AddApiKeyAsync(ApiKeyRequest acl, RequestOption requestOptions = null,
                    CancellationToken ct = default(CancellationToken))
        {
            if (acl == null)
            {
                throw new ArgumentNullException(nameof(acl));
            }

            return await _requesterWrapper.ExecuteRequestAsync<AddApiKeyResponse, ApiKeyRequest>(HttpMethod.Post,
                "/1/keys", CallType.Write, acl, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the permissions of an existing API Key.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="acl"></param>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public UpdateApiKeyResponse UpdateApiKey(string apiKey, ApiKeyRequest acl, RequestOption requestOptions = null) =>
                    AsyncHelper.RunSync(() => UpdateApiKeyAsync(apiKey, acl, requestOptions));

        /// <summary>
        /// Update the permissions of an existing API Key.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="acl"></param>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<UpdateApiKeyResponse> UpdateApiKeyAsync(string apiKey, ApiKeyRequest acl, RequestOption requestOptions = null,
                    CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentNullException(apiKey);
            }

            if (acl == null)
            {
                throw new ArgumentNullException(nameof(acl));
            }

            return await _requesterWrapper.ExecuteRequestAsync<UpdateApiKeyResponse, ApiKeyRequest>(HttpMethod.Put,
                $"/1/keys/{apiKey}", CallType.Write, acl, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete an existing API Key
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public DeleteResponse DeleteApiKey(string apiKey, RequestOption requestOptions = null) =>
            AsyncHelper.RunSync(() => DeleteApiKeyAsync(apiKey, requestOptions));

        /// <summary>
        /// Delete an existing API Key
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<DeleteResponse> DeleteApiKeyAsync(string apiKey, RequestOption requestOptions = null,
            CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentNullException(apiKey);
            }

            return await _requesterWrapper.ExecuteRequestAsync<DeleteResponse>(HttpMethod.Delete,
                $"/1/keys/{apiKey}", CallType.Write, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// List the clusters available in a multi-clusters setup for a single appID
        /// </summary>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public ListClustersResponse ListClusters(RequestOption requestOptions = null) =>
            AsyncHelper.RunSync(() => ListClustersAsync(requestOptions));

        /// <summary>
        /// List the clusters available in a multi-clusters setup for a single appID
        /// </summary>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ListClustersResponse> ListClustersAsync(RequestOption requestOptions = null,
            CancellationToken ct = default(CancellationToken))
        {
            return await _requesterWrapper.ExecuteRequestAsync<ListClustersResponse>(HttpMethod.Get,
                "/1/clusters", CallType.Read, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// List the userIDs assigned to a multi-clusters appID.
        /// </summary>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public SearchResponse<UserIdResponse> ListUserIds(RequestOption requestOptions = null) =>
            AsyncHelper.RunSync(() => ListUserIdsAsync(requestOptions));


        /// <summary>
        /// List the userIDs assigned to a multi-clusters appID.
        /// </summary>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<SearchResponse<UserIdResponse>> ListUserIdsAsync(RequestOption requestOptions = null,
            CancellationToken ct = default(CancellationToken))
        {
            return await _requesterWrapper.ExecuteRequestAsync<SearchResponse<UserIdResponse>>(HttpMethod.Get,
                "/1/clusters/mapping", CallType.Read, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the userID data stored in the mapping.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public UserIdResponse GetUserId(string userId, RequestOption requestOptions = null) =>
            AsyncHelper.RunSync(() => GetUserIdAsync(userId, requestOptions));

        /// <summary>
        /// Returns the userID data stored in the mapping.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<UserIdResponse> GetUserIdAsync(string userId, RequestOption requestOptions = null,
            CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(userId);
            }

            return await _requesterWrapper.ExecuteRequestAsync<UserIdResponse>(HttpMethod.Get,
                $"/1/clusters/mapping/{userId}", CallType.Read, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the top 10 userIDs with the highest number of records per cluster.
        /// The data returned will usually be a few seconds behind real-time, because userID usage may take up to a few seconds to propagate to the different clusters.
        /// </summary>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public TopUserIdResponse GetTopUserId(RequestOption requestOptions = null) =>
            AsyncHelper.RunSync(() => GetTopUserIdAsync(requestOptions));

        /// <summary>
        /// Get the top 10 userIDs with the highest number of records per cluster.
        /// The data returned will usually be a few seconds behind real-time, because userID usage may take up to a few seconds to propagate to the different clusters.
        /// </summary>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<TopUserIdResponse> GetTopUserIdAsync(RequestOption requestOptions = null,
            CancellationToken ct = default(CancellationToken))
        {
            return await _requesterWrapper.ExecuteRequestAsync<TopUserIdResponse>(HttpMethod.Get,
                "/1/clusters/mapping/top", CallType.Read, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Assign or Move a userID to a cluster.
        /// The time it takes to migrate (move) a user is proportional to the amount of data linked to the userID.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="clusterName"></param>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public AddObjectResponse AssignUserId(string userId, string clusterName, RequestOption requestOptions = null) =>
            AsyncHelper.RunSync(() => AssignUserIdAsync(userId, clusterName, requestOptions));

        /// <summary>
        /// Assign or Move a userID to a cluster.
        /// The time it takes to migrate (move) a user is proportional to the amount of data linked to the userID.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="clusterName"></param>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<AddObjectResponse> AssignUserIdAsync(string userId, string clusterName, RequestOption requestOptions = null,
            CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(userId);
            }

            if (string.IsNullOrWhiteSpace(clusterName))
            {
                throw new ArgumentNullException(clusterName);
            }

            var data = new AssignUserIdRequest { Cluster = clusterName };

            var removeUserId = new Dictionary<string, string>() { { "X-Algolia-USER-ID", userId } };

            if (requestOptions?.Headers != null && requestOptions.Headers.Any())
            {
                requestOptions.Headers = requestOptions.Headers.Concat(removeUserId).ToDictionary(x => x.Key, x => x.Value);
            }
            else if (requestOptions != null && requestOptions.Headers == null)
            {
                requestOptions.Headers = removeUserId;
            }
            else
            {
                requestOptions = new RequestOption { Headers = removeUserId };
            }

            return await _requesterWrapper.ExecuteRequestAsync<AddObjectResponse, AssignUserIdRequest>(HttpMethod.Post,
                "/1/clusters/mapping", CallType.Write, data, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove a userID and its associated data from the multi-clusters.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="requestOptions"></param>
        /// <returns></returns>
        public DeleteResponse RemoveUserId(string userId, RequestOption requestOptions = null) =>
            AsyncHelper.RunSync(() => RemoveUserIdAsync(userId, requestOptions));

        /// <summary>
        /// Remove a userID and its associated data from the multi-clusters.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<DeleteResponse> RemoveUserIdAsync(string userId, RequestOption requestOptions = null,
            CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(userId);
            }

            var removeUserId = new Dictionary<string, string>() { { "X-Algolia-USER-ID", userId } };

            if (requestOptions?.Headers != null && requestOptions.Headers.Any())
            {
                requestOptions.Headers = requestOptions.Headers.Concat(removeUserId).ToDictionary(x => x.Key, x => x.Value);
            }
            else if (requestOptions != null && requestOptions.Headers == null)
            {
                requestOptions.Headers = removeUserId;
            }
            else
            {
                requestOptions = new RequestOption { Headers = removeUserId };
            }

            return await _requesterWrapper.ExecuteRequestAsync<DeleteResponse>(HttpMethod.Delete,
                "/1/clusters/mapping'", CallType.Write, requestOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Get logs for the given index
        /// </summary>
        /// <returns></returns>
        public LogResponse GetLogs(RequestOption requestOptions = null) =>
            AsyncHelper.RunSync(() => GetLogsAsync(requestOptions));

        /// <summary>
        /// Get logs for the given index
        /// </summary>
        /// <param name="requestOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<LogResponse> GetLogsAsync(RequestOption requestOptions = null,
            CancellationToken ct = default(CancellationToken))
        {
            return await _requesterWrapper.ExecuteRequestAsync<LogResponse>(HttpMethod.Get, "/1/logs", CallType.Read,
                requestOptions, ct).ConfigureAwait(false);
        }
    }
}