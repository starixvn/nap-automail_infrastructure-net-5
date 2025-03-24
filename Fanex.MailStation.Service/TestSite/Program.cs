using Fanex.SimpleMailDelivery;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Add services to the container.

services.AddControllers();
services.AddMailDelivery(configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapControllers();

await app.RunAsync();
