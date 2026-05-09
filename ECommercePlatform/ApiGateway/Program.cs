using ECommercePlatform.Identity;

using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;

var builder = WebApplication.CreateBuilder(args);

builder
    .Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddOcelot();

builder
    .Services
    .AddOcelot(builder.Configuration)
    .AddPolly();

builder
    .Services
    .AddTokenAuthentication(builder.Configuration);

var app = builder.Build();

await app.UseOcelot();
await app.RunAsync();