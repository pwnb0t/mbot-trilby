# SidekickApi.Api.DefaultApi

All URIs are relative to *http://127.0.0.1:28765*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**AddTagClip**](DefaultApi.md#addtagclip) | **POST** /v1/tags/{tag_name}/clips | Add Tag Clip |
| [**GetCurrentIntro**](DefaultApi.md#getcurrentintro) | **GET** /v1/intros/current | Get Current Intro |
| [**GetHealth**](DefaultApi.md#gethealth) | **GET** /v1/health | Health |
| [**GetRecentClipStats**](DefaultApi.md#getrecentclipstats) | **GET** /v1/clips/stats/recent | Recent Clip Stats |
| [**GetTopClipStats**](DefaultApi.md#gettopclipstats) | **GET** /v1/clips/stats/top | Top Clip Stats |
| [**ListClips**](DefaultApi.md#listclips) | **GET** /v1/clips | List Clips |
| [**ListTagClips**](DefaultApi.md#listtagclips) | **GET** /v1/tags/{tag_name}/clips | List Tag Clips |
| [**ListTags**](DefaultApi.md#listtags) | **GET** /v1/tags | List Tags |
| [**PlayClip**](DefaultApi.md#playclip) | **POST** /v1/clips/play | Play Clip |
| [**PlayRandomClip**](DefaultApi.md#playrandomclip) | **POST** /v1/clips/play-random | Play Random Clip |
| [**RemoveTagClip**](DefaultApi.md#removetagclip) | **DELETE** /v1/tags/{tag_name}/clips/{clip_trigger} | Remove Tag Clip |
| [**SetCurrentIntro**](DefaultApi.md#setcurrentintro) | **PUT** /v1/intros/current | Set Current Intro |
| [**StopClip**](DefaultApi.md#stopclip) | **POST** /v1/clips/stop | Stop Clip |

<a id="addtagclip"></a>
# **AddTagClip**
> AddTagClipResponse AddTagClip (string tagName, AddTagClipBody addTagClipBody)

Add Tag Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **tagName** | **string** |  |  |
| **addTagClipBody** | [**AddTagClipBody**](AddTagClipBody.md) |  |  |

### Return type

[**AddTagClipResponse**](AddTagClipResponse.md)

### Authorization

[APIKeyHeader](../README.md#APIKeyHeader)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **404** | Not found error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getcurrentintro"></a>
# **GetCurrentIntro**
> GetCurrentIntroResponse GetCurrentIntro (long guildId, long requesterUserId)

Get Current Intro


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **requesterUserId** | **long** |  |  |

### Return type

[**GetCurrentIntroResponse**](GetCurrentIntroResponse.md)

### Authorization

[APIKeyHeader](../README.md#APIKeyHeader)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **400** | Bad request error response. |  -  |
| **404** | Not found error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="gethealth"></a>
# **GetHealth**
> HealthResponse GetHealth ()

Health


### Parameters
This endpoint does not need any parameter.
### Return type

[**HealthResponse**](HealthResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getrecentclipstats"></a>
# **GetRecentClipStats**
> RecentClipStatsResponse GetRecentClipStats (long guildId, long requesterUserId = null, int limit = null, bool includeRandom = null)

Recent Clip Stats


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **requesterUserId** | **long** |  | [optional]  |
| **limit** | **int** |  | [optional] [default to 10] |
| **includeRandom** | **bool** |  | [optional] [default to true] |

### Return type

[**RecentClipStatsResponse**](RecentClipStatsResponse.md)

### Authorization

[APIKeyHeader](../README.md#APIKeyHeader)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="gettopclipstats"></a>
# **GetTopClipStats**
> TopClipStatsResponse GetTopClipStats (long guildId, long requesterUserId = null, string days = null, int limit = null, bool includeRandom = null)

Top Clip Stats


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **requesterUserId** | **long** |  | [optional]  |
| **days** | **string** |  | [optional] [default to &quot;7&quot;] |
| **limit** | **int** |  | [optional] [default to 10] |
| **includeRandom** | **bool** |  | [optional] [default to false] |

### Return type

[**TopClipStatsResponse**](TopClipStatsResponse.md)

### Authorization

[APIKeyHeader](../README.md#APIKeyHeader)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **400** | Bad request error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="listclips"></a>
# **ListClips**
> ListClipsResponse ListClips (long guildId, string search = null)

List Clips


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **search** | **string** |  | [optional]  |

### Return type

[**ListClipsResponse**](ListClipsResponse.md)

### Authorization

[APIKeyHeader](../README.md#APIKeyHeader)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="listtagclips"></a>
# **ListTagClips**
> ListTagClipsResponse ListTagClips (string tagName, long guildId)

List Tag Clips


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **tagName** | **string** |  |  |
| **guildId** | **long** |  |  |

### Return type

[**ListTagClipsResponse**](ListTagClipsResponse.md)

### Authorization

[APIKeyHeader](../README.md#APIKeyHeader)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **404** | Not found error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="listtags"></a>
# **ListTags**
> ListTagsResponse ListTags (long guildId, string search = null)

List Tags


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **search** | **string** |  | [optional]  |

### Return type

[**ListTagsResponse**](ListTagsResponse.md)

### Authorization

[APIKeyHeader](../README.md#APIKeyHeader)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="playclip"></a>
# **PlayClip**
> PlayClipResponse PlayClip (PlayClipRequest playClipRequest)

Play Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **playClipRequest** | [**PlayClipRequest**](PlayClipRequest.md) |  |  |

### Return type

[**PlayClipResponse**](PlayClipResponse.md)

### Authorization

[APIKeyHeader](../README.md#APIKeyHeader)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **400** | Bad request error response. |  -  |
| **404** | Clip not found error response. |  -  |
| **409** | Voice not connected error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="playrandomclip"></a>
# **PlayRandomClip**
> PlayRandomClipResponse PlayRandomClip (PlayRandomClipRequest playRandomClipRequest)

Play Random Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **playRandomClipRequest** | [**PlayRandomClipRequest**](PlayRandomClipRequest.md) |  |  |

### Return type

[**PlayRandomClipResponse**](PlayRandomClipResponse.md)

### Authorization

[APIKeyHeader](../README.md#APIKeyHeader)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **404** | Clip not found error response. |  -  |
| **409** | Voice not connected error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="removetagclip"></a>
# **RemoveTagClip**
> RemoveTagClipResponse RemoveTagClip (string tagName, string clipTrigger, long guildId)

Remove Tag Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **tagName** | **string** |  |  |
| **clipTrigger** | **string** |  |  |
| **guildId** | **long** |  |  |

### Return type

[**RemoveTagClipResponse**](RemoveTagClipResponse.md)

### Authorization

[APIKeyHeader](../README.md#APIKeyHeader)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **404** | Not found error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="setcurrentintro"></a>
# **SetCurrentIntro**
> SetCurrentIntroResponse SetCurrentIntro (SetCurrentIntroRequest setCurrentIntroRequest)

Set Current Intro


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **setCurrentIntroRequest** | [**SetCurrentIntroRequest**](SetCurrentIntroRequest.md) |  |  |

### Return type

[**SetCurrentIntroResponse**](SetCurrentIntroResponse.md)

### Authorization

[APIKeyHeader](../README.md#APIKeyHeader)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **400** | Bad request error response. |  -  |
| **404** | Not found error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="stopclip"></a>
# **StopClip**
> StopClipResponse StopClip (StopClipRequest stopClipRequest)

Stop Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **stopClipRequest** | [**StopClipRequest**](StopClipRequest.md) |  |  |

### Return type

[**StopClipResponse**](StopClipResponse.md)

### Authorization

[APIKeyHeader](../README.md#APIKeyHeader)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **404** | Guild not found error response. |  -  |
| **409** | Voice not connected error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

