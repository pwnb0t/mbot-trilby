# TrilbyApi.Api.DefaultApi

All URIs are relative to *http://127.0.0.1:28765*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**AddTagClip**](DefaultApi.md#addtagclip) | **POST** /v1/guilds/{guild_id}/tags/{tag_name}/clips | Add Tag Clip |
| [**CompleteDiscordAuth**](DefaultApi.md#completediscordauth) | **GET** /v1/auth/discord/callback | Complete Discord Auth |
| [**CopyClip**](DefaultApi.md#copyclip) | **POST** /v1/guilds/{guild_id}/clips/{clip_trigger}/copy | Copy Clip |
| [**CreateBrowserLaunch**](DefaultApi.md#createbrowserlaunch) | **POST** /v1/browser-auth/launch | Create Browser Launch |
| [**GetAuthenticatedSession**](DefaultApi.md#getauthenticatedsession) | **GET** /v1/auth/me | Get Authenticated Session |
| [**GetClipAudio**](DefaultApi.md#getclipaudio) | **GET** /v1/guilds/{guild_id}/clips/{clip_trigger}/audio | Get Clip Audio |
| [**GetCurrentIntro**](DefaultApi.md#getcurrentintro) | **GET** /v1/guilds/{guild_id}/intros/current | Get Current Intro |
| [**GetHaberdasheryBundleJsHaberdasheryStaticDistHaberdasheryBundleJsGet**](DefaultApi.md#gethaberdasherybundlejshaberdasherystaticdisthaberdasherybundlejsget) | **GET** /haberdashery/static/dist/haberdashery.bundle.js | Get Haberdashery Bundle Js |
| [**GetHaberdasheryCssHaberdasheryStaticHaberdasheryCssGet**](DefaultApi.md#gethaberdasherycsshaberdasherystatichaberdasherycssget) | **GET** /haberdashery/static/haberdashery.css | Get Haberdashery Css |
| [**GetHaberdasheryJsHaberdasheryStaticHaberdasheryJsGet**](DefaultApi.md#gethaberdasheryjshaberdasherystatichaberdasheryjsget) | **GET** /haberdashery/static/haberdashery.js | Get Haberdashery Js |
| [**GetHaberdasheryPageHaberdasheryGet**](DefaultApi.md#gethaberdasherypagehaberdasheryget) | **GET** /haberdashery | Get Haberdashery Page |
| [**GetHealth**](DefaultApi.md#gethealth) | **GET** /v1/health | Health |
| [**GetRecentClipStats**](DefaultApi.md#getrecentclipstats) | **GET** /v1/guilds/{guild_id}/clips/stats/recent | Recent Clip Stats |
| [**GetSharedTag**](DefaultApi.md#getsharedtag) | **GET** /v1/guilds/{guild_id}/shared-tag | Get Shared Tag |
| [**GetTopClipStats**](DefaultApi.md#gettopclipstats) | **GET** /v1/guilds/{guild_id}/clips/stats/top | Top Clip Stats |
| [**ListClips**](DefaultApi.md#listclips) | **GET** /v1/guilds/{guild_id}/clips | List Clips |
| [**ListTagClips**](DefaultApi.md#listtagclips) | **GET** /v1/guilds/{guild_id}/tags/{tag_name}/clips | List Tag Clips |
| [**ListTags**](DefaultApi.md#listtags) | **GET** /v1/guilds/{guild_id}/tags | List Tags |
| [**PlayClip**](DefaultApi.md#playclip) | **POST** /v1/guilds/{guild_id}/clips/play | Play Clip |
| [**PlayRandomClip**](DefaultApi.md#playrandomclip) | **POST** /v1/guilds/{guild_id}/clips/play-random | Play Random Clip |
| [**RefreshTrilbySession**](DefaultApi.md#refreshtrilbysession) | **POST** /v1/auth/refresh | Refresh Trilby Session |
| [**RemoveTagClip**](DefaultApi.md#removetagclip) | **DELETE** /v1/guilds/{guild_id}/tags/{tag_name}/clips/{clip_trigger} | Remove Tag Clip |
| [**SetCurrentIntro**](DefaultApi.md#setcurrentintro) | **PUT** /v1/guilds/{guild_id}/intros/current | Set Current Intro |
| [**SetSharedTag**](DefaultApi.md#setsharedtag) | **PUT** /v1/guilds/{guild_id}/shared-tag | Set Shared Tag |
| [**StartDiscordAuth**](DefaultApi.md#startdiscordauth) | **GET** /v1/auth/discord/start | Start Discord Auth |
| [**StopClip**](DefaultApi.md#stopclip) | **POST** /v1/guilds/{guild_id}/clips/stop | Stop Clip |
| [**UploadLogBundle**](DefaultApi.md#uploadlogbundle) | **POST** /v1/support/log-bundles | Upload Log Bundle |

<a id="addtagclip"></a>
# **AddTagClip**
> AddTagClipResponse AddTagClip (string guildId, string tagName, AddTagClipBody addTagClipBody)

Add Tag Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
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

<a id="copyclip"></a>
# **CopyClip**
> CopyClipResponse CopyClip (string guildId, string clipTrigger, CopyClipBody copyClipBody)

Copy Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
| **clipTrigger** | **string** |  |  |
| **copyClipBody** | [**CopyClipBody**](CopyClipBody.md) |  |  |

### Return type

[**CopyClipResponse**](CopyClipResponse.md)

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
| **409** | Conflict error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="createbrowserlaunch"></a>
# **CreateBrowserLaunch**
> CreateBrowserLaunchResponse CreateBrowserLaunch ()

Create Browser Launch


### Parameters
This endpoint does not need any parameter.
### Return type

[**CreateBrowserLaunchResponse**](CreateBrowserLaunchResponse.md)

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
| **403** | Forbidden error response. |  -  |
| **500** | Internal error response. |  -  |

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

<a id="getclipaudio"></a>
# **GetClipAudio**
> Object GetClipAudio (string guildId, string clipTrigger)

Get Clip Audio


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
| **clipTrigger** | **string** |  |  |

### Return type

**Object**

### Authorization

[HTTPBearer](../README.md#HTTPBearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Clip audio stream. |  -  |
| **401** | Unauthorized error response. |  -  |
| **404** | Clip not found error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="getcurrentintro"></a>
# **GetCurrentIntro**
> GetCurrentIntroResponse GetCurrentIntro (string guildId)

Get Current Intro


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |

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

<a id="gethaberdasherybundlejshaberdasherystaticdisthaberdasherybundlejsget"></a>
# **GetHaberdasheryBundleJsHaberdasheryStaticDistHaberdasheryBundleJsGet**
> void GetHaberdasheryBundleJsHaberdasheryStaticDistHaberdasheryBundleJsGet ()

Get Haberdashery Bundle Js


### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="gethaberdasherycsshaberdasherystatichaberdasherycssget"></a>
# **GetHaberdasheryCssHaberdasheryStaticHaberdasheryCssGet**
> void GetHaberdasheryCssHaberdasheryStaticHaberdasheryCssGet ()

Get Haberdashery Css


### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="gethaberdasheryjshaberdasherystatichaberdasheryjsget"></a>
# **GetHaberdasheryJsHaberdasheryStaticHaberdasheryJsGet**
> void GetHaberdasheryJsHaberdasheryStaticHaberdasheryJsGet ()

Get Haberdashery Js


### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="gethaberdasherypagehaberdasheryget"></a>
# **GetHaberdasheryPageHaberdasheryGet**
> string GetHaberdasheryPageHaberdasheryGet ()

Get Haberdashery Page


### Parameters
This endpoint does not need any parameter.
### Return type

**string**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/html


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |

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
> RecentClipStatsResponse GetRecentClipStats (string guildId, string scope = null, int limit = null, bool includeRandom = null)

Recent Clip Stats


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
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

<a id="getsharedtag"></a>
# **GetSharedTag**
> GetSharedTagResponse GetSharedTag (string guildId)

Get Shared Tag


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |

### Return type

[**GetSharedTagResponse**](GetSharedTagResponse.md)

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
> TopClipStatsResponse GetTopClipStats (string guildId, string scope = null, string days = null, int limit = null, bool includeRandom = null)

Top Clip Stats


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
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
> ListClipsResponse ListClips (string guildId, string search = null)

List Clips


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
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
> ListTagClipsResponse ListTagClips (string tagName, string guildId)

List Tag Clips


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **tagName** | **string** |  |  |
| **guildId** | **string** |  |  |

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
> ListTagsResponse ListTags (string guildId, string search = null)

List Tags


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
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
> PlayClipResponse PlayClip (string guildId, PlayClipBody playClipBody)

Play Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
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
> PlayRandomClipResponse PlayRandomClip (string guildId, PlayRandomClipBody playRandomClipBody)

Play Random Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
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
> RemoveTagClipResponse RemoveTagClip (string guildId, string tagName, string clipTrigger)

Remove Tag Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
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
> SetCurrentIntroResponse SetCurrentIntro (string guildId, SetCurrentIntroBody setCurrentIntroBody)

Set Current Intro


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
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

<a id="setsharedtag"></a>
# **SetSharedTag**
> SetSharedTagResponse SetSharedTag (string guildId, SetSharedTagBody setSharedTagBody)

Set Shared Tag


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
| **setSharedTagBody** | [**SetSharedTagBody**](SetSharedTagBody.md) |  |  |

### Return type

[**SetSharedTagResponse**](SetSharedTagResponse.md)

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
> StopClipResponse StopClip (string guildId, StopClipBody stopClipBody)

Stop Clip


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **string** |  |  |
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

<a id="uploadlogbundle"></a>
# **UploadLogBundle**
> UploadLogBundleResponse UploadLogBundle (UploadLogBundleBody uploadLogBundleBody)

Upload Log Bundle


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **uploadLogBundleBody** | [**UploadLogBundleBody**](UploadLogBundleBody.md) |  |  |

### Return type

[**UploadLogBundleResponse**](UploadLogBundleResponse.md)

### Authorization

[HTTPBearer](../README.md#HTTPBearer)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **400** | Bad request error response. |  -  |
| **401** | Unauthorized error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

