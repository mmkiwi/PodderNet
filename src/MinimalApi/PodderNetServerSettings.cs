namespace MMKiwi.PodderNet.MinimalApi;

public record PodderNetServerSettings
{
    public string GPodderApiRoot { get; init; } = "/api/2/";
}