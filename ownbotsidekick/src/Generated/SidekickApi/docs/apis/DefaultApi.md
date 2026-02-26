# SidekickApi.Api.DefaultApi

All URIs are relative to *http://127.0.0.1:8765*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**GetHealth**](DefaultApi.md#gethealth) | **GET** /v1/health | Health |
| [**PlayTrigger**](DefaultApi.md#playtrigger) | **POST** /v1/triggerables/play | Play Trigger |

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

<a id="playtrigger"></a>
# **PlayTrigger**
> PlayTriggerResponse PlayTrigger (PlayTriggerRequest playTriggerRequest)

Play Trigger


### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **playTriggerRequest** | [**PlayTriggerRequest**](PlayTriggerRequest.md) |  |  |

### Return type

[**PlayTriggerResponse**](PlayTriggerResponse.md)

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
| **404** | Trigger not found error response. |  -  |
| **409** | Voice not connected error response. |  -  |
| **500** | Internal error response. |  -  |
| **422** | Validation Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

