using Fanex.Caching;
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
services.AddInternalService();
services.AddFanexLogSentry(configuration);

services.AddOneBookUmForService(configuration);
services.AddFanexSql(configuration);
services.AddMemoryCache(configuration.GetSection("fanex.caching"));
services.AddMassTransitForMail(configuration);
services.AddScheduler();
services.AddMessageBroker();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
await app.RunAsync();