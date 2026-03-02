# SidekickApi.Api.DefaultApi

All URIs are relative to *http://127.0.0.1:8765*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetHealth**](DefaultApi.md#gethealth) | **GET** /v1/health | Health |
| [**ListClips**](DefaultApi.md#listclips) | **GET** /v1/clips | List Clips |
| [**PlayClip**](DefaultApi.md#playclip) | **POST** /v1/clips/play | Play Clip |

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

<a id="listclips"></a>
# **ListClips**
> ListClipsResponse ListClips (int guildId, string search = null)

List Clips


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **guildId** | **int** |  |  |
| **search** | **string** |  | [optional]  |

### Return type

[**ListClipsResponse**](ListClipsResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
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

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successful Response |  -  |
| **400** | Bad request error response. |  -  |
| **404** | Clip not found error response. |  -  |
| **409** | Voice not connected error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

