using Microsoft.Data.Sqlite;

using MMKiwi.PodderNet.Database.Sqlite;
using MMKiwi.PodderNet.MinimalApi;
using MMKiwi.PodderNet.Model.Database;

using Device = MMKiwi.PodderNet.MinimalApi.Device;

namespace MMKiwi.PodderNet.Server;

public partial class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, Model.ApiJsonSerializerContext.Default);
        });

        builder.Services.AddPodderNet();
        
        builder.Services.AddSingleton<Auth>();
        builder.Services.AddSingleton<Device>();
        
        var conString = builder.Configuration.GetConnectionString("PodderNet") ??
                        throw new InvalidOperationException("Connection string 'PodderNet' not found.");
        
        builder.Services.AddScoped<SqliteConnection>(_ =>  new SqliteConnection(conString));
        builder.Services.AddScoped<IDatabaseManager, SqliteDatabaseManager>();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "v1"); });
        }

        app.RegisterPodderNet();

        using (var scope = app.Services.CreateScope())
        {
            await using var connection = scope.ServiceProvider.GetRequiredService<IDatabaseManager>();
            await connection.EnsureCreated();
        }
        
        
        await app.RunAsync();
        return 0;
    }
}