using Fanex.Logging.Sentry.RavenClient;
using Fanex.MailDeliver.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddConfig();
services.AddFanexLogSentry(configuration);
services.AddFanexSql(configuration);
services.AddInternalServices();
services.SubcriberMailService();
services.StartScheduler();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
await app.RunAsync();