# TrilbyApi.Api.DefaultApi

All URIs are relative to *http://127.0.0.1:28765*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**AddTagClip**](DefaultApi.md#addtagclip) | **POST** /v1/guilds/{guild_id}/tags/{tag_name}/clips | Add Tag Clip |
| [**CompleteDiscordAuth**](DefaultApi.md#completediscordauth) | **GET** /v1/auth/discord/callback | Complete Discord Auth |
| [**GetAuthenticatedSession**](DefaultApi.md#getauthenticatedsession) | **GET** /v1/auth/me | Get Authenticated Session |
| [**GetCurrentIntro**](DefaultApi.md#getcurrentintro) | **GET** /v1/guilds/{guild_id}/intros/current | Get Current Intro |
| [**GetHealth**](DefaultApi.md#gethealth) | **GET** /v1/health | Health |
| [**GetRecentClipStats**](DefaultApi.md#getrecentclipstats) | **GET** /v1/guilds/{guild_id}/clips/stats/recent | Recent Clip Stats |
| [**GetTopClipStats**](DefaultApi.md#gettopclipstats) | **GET** /v1/guilds/{guild_id}/clips/stats/top | Top Clip Stats |
| [**ListClips**](DefaultApi.md#listclips) | **GET** /v1/guilds/{guild_id}/clips | List Clips |
| [**ListTagClips**](DefaultApi.md#listtagclips) | **GET** /v1/guilds/{guild_id}/tags/{tag_name}/clips | List Tag Clips |
| [**ListTags**](DefaultApi.md#listtags) | **GET** /v1/guilds/{guild_id}/tags | List Tags |
| [**PlayClip**](DefaultApi.md#playclip) | **POST** /v1/guilds/{guild_id}/clips/play | Play Clip |
| [**PlayRandomClip**](DefaultApi.md#playrandomclip) | **POST** /v1/guilds/{guild_id}/clips/play-random | Play Random Clip |
| [**RefreshTrilbySession**](DefaultApi.md#refreshtrilbysession) | **POST** /v1/auth/refresh | Refresh Trilby Session |
| [**RemoveTagClip**](DefaultApi.md#removetagclip) | **DELETE** /v1/guilds/{guild_id}/tags/{tag_name}/clips/{clip_trigger} | Remove Tag Clip |
| [**SetCurrentIntro**](DefaultApi.md#setcurrentintro) | **PUT** /v1/guilds/{guild_id}/intros/current | Set Current Intro |
| [**StartDiscordAuth**](DefaultApi.md#startdiscordauth) | **GET** /v1/auth/discord/start | Start Discord Auth |
| [**StopClip**](DefaultApi.md#stopclip) | **POST** /v1/guilds/{guild_id}/clips/stop | Stop Clip |

<a id="addtagclip"></a>
# **AddTagClip**
> AddTagClipResponse AddTagClip (long guildId, string tagName, AddTagClipBody addTagClipBody)

Add Tag Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **tagName** | **string** |  |  |
| **addTagClipBody** | [**AddTagClipBody**](AddTagClipBody.md) |  |  |

### Return type

[**AddTagClipResponse**](AddTagClipResponse.md)

### Authorization

[HTTPBearer](../README.md#HTTPBearer)

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

<a id="completediscordauth"></a>
# **CompleteDiscordAuth**
> Object CompleteDiscordAuth (string code = null, string state = null, string error = null, string errorDescription = null)

Complete Discord Auth


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **code** | **string** |  | [optional]  |
| **state** | **string** |  | [optional]  |
| **error** | **string** |  | [optional]  |
| **errorDescription** | **string** |  | [optional]  |

### Return type

**Object**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getauthenticatedsession"></a>
# **GetAuthenticatedSession**
> SessionSummaryResponse GetAuthenticatedSession ()

Get Authenticated Session


### Parameters
This endpoint does not need any parameter.
### Return type

[**SessionSummaryResponse**](SessionSummaryResponse.md)

### Authorization

[HTTPBearer](../README.md#HTTPBearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getcurrentintro"></a>
# **GetCurrentIntro**
> GetCurrentIntroResponse GetCurrentIntro (long guildId)

Get Current Intro


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |

### Return type

[**GetCurrentIntroResponse**](GetCurrentIntroResponse.md)

### Authorization

[HTTPBearer](../README.md#HTTPBearer)

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
> RecentClipStatsResponse GetRecentClipStats (long guildId, string scope = null, int limit = null, bool includeRandom = null)

Recent Clip Stats


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **scope** | **string** |  | [optional] [default to me] |
| **limit** | **int** |  | [optional] [default to 10] |
| **includeRandom** | **bool** |  | [optional] [default to true] |

### Return type

[**RecentClipStatsResponse**](RecentClipStatsResponse.md)

### Authorization

[HTTPBearer](../README.md#HTTPBearer)

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
> TopClipStatsResponse GetTopClipStats (long guildId, string scope = null, string days = null, int limit = null, bool includeRandom = null)

Top Clip Stats


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **scope** | **string** |  | [optional] [default to me] |
| **days** | **string** |  | [optional] [default to &quot;7&quot;] |
| **limit** | **int** |  | [optional] [default to 10] |
| **includeRandom** | **bool** |  | [optional] [default to false] |

### Return type

[**TopClipStatsResponse**](TopClipStatsResponse.md)

### Authorization

[HTTPBearer](../README.md#HTTPBearer)

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

[HTTPBearer](../README.md#HTTPBearer)

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

[HTTPBearer](../README.md#HTTPBearer)

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

[HTTPBearer](../README.md#HTTPBearer)

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
> PlayClipResponse PlayClip (long guildId, PlayClipBody playClipBody)

Play Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **playClipBody** | [**PlayClipBody**](PlayClipBody.md) |  |  |

### Return type

[**PlayClipResponse**](PlayClipResponse.md)

### Authorization

[HTTPBearer](../README.md#HTTPBearer)

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
> PlayRandomClipResponse PlayRandomClip (long guildId, PlayRandomClipBody playRandomClipBody)

Play Random Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **playRandomClipBody** | [**PlayRandomClipBody**](PlayRandomClipBody.md) |  |  |

### Return type

[**PlayRandomClipResponse**](PlayRandomClipResponse.md)

### Authorization

[HTTPBearer](../README.md#HTTPBearer)

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

<a id="refreshtrilbysession"></a>
# **RefreshTrilbySession**
> SessionResponse RefreshTrilbySession (RefreshSessionRequest refreshSessionRequest)

Refresh Trilby Session


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **refreshSessionRequest** | [**RefreshSessionRequest**](RefreshSessionRequest.md) |  |  |

### Return type

[**SessionResponse**](SessionResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **401** | Unauthorized error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="removetagclip"></a>
# **RemoveTagClip**
> RemoveTagClipResponse RemoveTagClip (long guildId, string tagName, string clipTrigger)

Remove Tag Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **tagName** | **string** |  |  |
| **clipTrigger** | **string** |  |  |

### Return type

[**RemoveTagClipResponse**](RemoveTagClipResponse.md)

### Authorization

[HTTPBearer](../README.md#HTTPBearer)

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
> SetCurrentIntroResponse SetCurrentIntro (long guildId, SetCurrentIntroBody setCurrentIntroBody)

Set Current Intro


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **setCurrentIntroBody** | [**SetCurrentIntroBody**](SetCurrentIntroBody.md) |  |  |

### Return type

[**SetCurrentIntroResponse**](SetCurrentIntroResponse.md)

### Authorization

[HTTPBearer](../README.md#HTTPBearer)

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

<a id="startdiscordauth"></a>
# **StartDiscordAuth**
> Object StartDiscordAuth (string clientCallbackUrl)

Start Discord Auth


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **clientCallbackUrl** | **string** |  |  |

### Return type

**Object**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="stopclip"></a>
# **StopClip**
> StopClipResponse StopClip (long guildId, StopClipBody stopClipBody)

Stop Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **long** |  |  |
| **stopClipBody** | [**StopClipBody**](StopClipBody.md) |  |  |

### Return type

[**StopClipResponse**](StopClipResponse.md)

### Authorization

[HTTPBearer](../README.md#HTTPBearer)

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

