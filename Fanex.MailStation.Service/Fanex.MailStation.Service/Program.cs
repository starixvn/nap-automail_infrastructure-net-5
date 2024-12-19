using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.MailStation.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddConfiguration();
services.AddFanexLogSentry(configuration);
services.AddOneBookUm(configuration);
services.AddFanexSql(configuration);
services.InternalServiceRegister();
services.AddMessageBroker();
services.AddScheduler();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
await app.RunAsync();