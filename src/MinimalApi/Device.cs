using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using MMKiwi.PodderNet.Model;
using MMKiwi.PodderNet.Model.Database;
using MMKiwi.PodderNet.Model.GPodderApi;

namespace MMKiwi.PodderNet.MinimalApi;

public class Device : IApplicationGroup
{
    private readonly ILogger<Device> _logger;
    private readonly PodderNetServerSettings _settings;

    public Device(ILogger<Device> logger, PodderNetServerSettings? settings = null)
    {
        _logger = logger;
        _settings = settings ?? new PodderNetServerSettings();
    }

    public void Build(WebApplication app)
    {
        var group = app.MapGroup(_settings.GetRoot("devices"));
        group.MapPost("{username}/{deviceid}.json", PostDevice).WithName("Update Device Data");
        group.MapGet("{username}.json", GetDevices).WithName("List Devices");
    }

    private async Task<Results<Ok<BasicResponse>, UnauthorizedHttpResult, BadRequest<string>>> PostDevice(
        HttpContext context, IDatabaseManager connection, string username, string deviceId, [FromBody] DeviceRequest device)
    {
        int userId = 1;
        await connection.UpdateOrInsertDevice(userId, deviceId, device);
        return TypedResults.BasicOk("Device updated");
    }

    private Results<Ok<IAsyncEnumerable<DeviceResponse>>, UnauthorizedHttpResult, BadRequest<string>> GetDevices(
        HttpContext context, IDatabaseManager connection, string username)
    {
        return TypedResults.Ok(connection.GetDevices(username).Select(ModelMapper.ToDevice));
    }
}