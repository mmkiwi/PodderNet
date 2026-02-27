using System.Text.Json.Serialization;

using MMKiwi.PodderNet.Model.GPodderApi;

namespace MMKiwi.PodderNet.Model;

[JsonSerializable(typeof(IAsyncEnumerable<DeviceResponse>))]
[JsonSerializable(typeof(DeviceRequest))]
[JsonSerializable(typeof(BasicResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
public partial class ApiJsonSerializerContext : JsonSerializerContext;