using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.MailStation.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddConfiguration();
builder.Services.AddFanexLogSentry(configuration);
builder.Services.AddFanexSql(configuration);
builder.Services.InternalServiceRegister();
builder.Services.AddMessageBroker();
builder.Services.AddScheduler();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
await app.RunAsync();