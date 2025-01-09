using Fanex.MailDeliver.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.LoadConfiguration();
builder.Services.SentryRegister();
builder.Services.FanexDataRegister(builder.Configuration);
builder.Services.InternalServiceRegister();
builder.Services.SubcriberMailService();
builder.Services.StartScheduler();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
await app.RunAsync();