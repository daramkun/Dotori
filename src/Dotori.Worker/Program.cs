using Dotori.Worker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<WorkerService>();
app.MapGet("/", () => "dotori worker — use a gRPC client");

app.Run();
