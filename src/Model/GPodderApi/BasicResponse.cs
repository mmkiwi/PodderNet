using System.Net;

namespace MMKiwi.PodderNet.Model.GPodderApi;

public record BasicResponse
{
    public BasicResponse(int code, string message)
    {
        Code = code;
        Message = message;
    }

    public BasicResponse(HttpStatusCode code, string message): this((int)code, message)
    {
    }

    public int Code { get; }
    public string Message { get; }
}