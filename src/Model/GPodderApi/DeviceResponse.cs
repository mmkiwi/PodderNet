namespace MMKiwi.PodderNet.Model.GPodderApi;

public record DeviceResponse(string Id, string Caption, DeviceType Type, int Subscriptions);