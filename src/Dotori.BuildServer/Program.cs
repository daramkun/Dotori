using Dotori.BuildServer.Cache;
using Dotori.BuildServer.Services;
using Dotori.BuildServer.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddSingleton<BuildCache>();
builder.Services.AddSingleton<WorkerPool>();

var app = builder.Build();

app.MapGrpcService<CoordinatorService>();
app.MapGet("/", () => "dotori build server — use a gRPC client");

app.Run();
