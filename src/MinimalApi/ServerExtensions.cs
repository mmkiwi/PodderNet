using System.Net;
using System.Text;

using Microsoft.AspNetCore.Http.HttpResults;

using MMKiwi.PodderNet.Model.GPodderApi;

namespace MMKiwi.PodderNet.MinimalApi;

public static class ServerExtensions
{
    extension(WebApplication app)
    {
        public void AddGroup<T>() where T : class, IApplicationGroup
        {
            app.AddGroup(app.Services.GetRequiredService<T>());
        }

        public void AddGroup(IApplicationGroup group)
        {
            group.Build(app);
        }

        public void RegisterPodderNet()
        {
            app.AddGroup<Auth>();
            app.AddGroup<Device>();
        }
    }

    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddPodderNet()
        {
            return serviceCollection
                .AddSingleton<Auth>()
                .AddSingleton<Device>();
        }
    }

    extension(TypedResults)
    {
        public static Ok<BasicResponse> BasicOk(string message)
        {
            return TypedResults.Ok(new BasicResponse(HttpStatusCode.OK, message));
        }
    }

    extension(PodderNetServerSettings settings)
    {
        internal string GetRoot(ReadOnlySpan<char> subdirectory)
        {
            StringBuilder root = new(settings.GPodderApiRoot);
            root.Replace('\\', '/');
            if (root[^1] != '/')
                root.Append('/');
            root.Append(subdirectory);
            return root.ToString();
        }
    }
}