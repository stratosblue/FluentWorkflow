using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json")
                     .AddUserSecrets<Program>();

var sampleAddress = builder.Configuration.GetValue<string>("SampleAddress")!;
var sampleCookie = builder.Configuration.GetValue<string>("SampleCookie")!;

var services = builder.Services;

services.AddOpenApi();

services.AddFluentWorkflowManagementManager(IPEndPoint.Parse(sampleAddress), sampleCookie);
services.AddFluentWorkflowManagementApi();

services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

app.MapOpenApi();
app.MapSwaggerUI();

app.UseCors();

app.MapFluentWorkflowManagementApi();
app.MapFluentWorkflowManagementUI();

app.Run();
