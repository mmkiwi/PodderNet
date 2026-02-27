using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;

using Microsoft.AspNetCore.Http.HttpResults;

namespace MMKiwi.PodderNet.MinimalApi;

public class Auth : IApplicationGroup
{
    private readonly ILogger<Auth> _logger;
    private readonly PodderNetServerSettings _settings;

    public Auth(ILogger<Auth> logger, PodderNetServerSettings? settings = null)
    {
        _logger = logger;
        _settings = settings ?? new PodderNetServerSettings();
    }
    
    private const string SessionCookieName = "sessionid";
    private static TimeSpan SessionDuration => TimeSpan.FromDays(20);
    public void Build(WebApplication app)
    {
        var group = app.MapGroup(_settings.GetRoot("auth"));
        group.MapPost("{loginUser}/login.json", Login).WithName("login");
        group.MapPost("{logoutUser}/logout.json", Logout).WithName("logout");
    }
    
    private Results<Ok, UnauthorizedHttpResult, BadRequest<string>> Logout(string logoutUser, HttpContext context)
    {
        if (!context.Request.Cookies.ContainsKey(SessionCookieName))
            return TypedResults.Ok();
        if (!TryAuthenticateSession(context, out string? username))
            return TypedResults.Unauthorized();

        if (username != logoutUser)
            return TypedResults.BadRequest("Provided cookie is for a different username");
        
        context.Response.Cookies.Delete(SessionCookieName);
        return TypedResults.Ok();
    }

    private Results<Ok, UnauthorizedHttpResult, BadRequest<string>> Login(string loginUser, HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"];
        if (authHeader.Count > 1) return TypedResults.BadRequest("Multiple Authoirzation Headers");

        if (authHeader is [{ } headerValue])
        {
            var authHeaderVal = AuthenticationHeaderValue.Parse(headerValue);

            // RFC 2617 sec 1.2, "scheme" name is case-insensitive
            if (authHeaderVal.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) &&
                authHeaderVal.Parameter != null)
            {
                if (TryAuthenticate(loginUser, authHeaderVal.Parameter, out string? cookie))
                {
                    context.Response.Cookies.Append(SessionCookieName, cookie, new CookieOptions()
                    {
                        MaxAge = SessionDuration
                    });
                    return TypedResults.Ok();
                }
                else
                {
                    _logger.FailedAttempt(loginUser);
                    return TypedResults.Unauthorized();
                }
            }

            return TypedResults.BadRequest("Invalid authentication method");
        }
        else
        {
            context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Dev\", charset=\"UTF-8\"");
            return TypedResults.Unauthorized();
        }
    }

    private static bool TryAuthenticate(string providedUsername, string credentials,
        [NotNullWhen(true)] out string? sessionCookie)
    {
        credentials = Encoding.UTF8.GetString(Convert.FromBase64String(credentials));

        int separator = credentials.IndexOf(':');

        if (separator < 0 || credentials.Length <= separator + 1)
        {
            sessionCookie = null;
            return false;
        }

        string username = credentials[..separator].Normalize(NormalizationForm.FormC);
        string password = credentials[(separator + 1)..].Normalize(NormalizationForm.FormKC);

        if (providedUsername == username && CheckPassword(username, password))
        {
            // TODO implement proper authentication
            sessionCookie = username;
            return true;
        }
        else
        {
            sessionCookie = null;
            return false;
        }
    }

    private static bool CheckPassword(string username, string password) => username == "username" && password == "password";

    public bool TryAuthenticateSession(HttpContext httpContext,[NotNullWhen(true)] out string? username) => httpContext.Request.Cookies.TryGetValue(SessionCookieName, out username);
}