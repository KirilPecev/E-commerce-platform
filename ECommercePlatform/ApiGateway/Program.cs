using ECommercePlatform.Identity;

using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

builder
    .Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddOcelot();

builder
    .Services
    .AddOcelot(builder.Configuration)
    .AddPolly();

builder
    .Services
    .AddTokenAuthentication(builder.Configuration);

var app = builder.Build();

app.Use(async (context, next) =>
{
    const string ClientIdHeader = "X-ClientId";

    if (!context.Request.Headers.ContainsKey(ClientIdHeader))
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        context.Request.Headers[ClientIdHeader] = clientIp;
    }

    await next();
});

await app.UseOcelot();
await app.RunAsync();