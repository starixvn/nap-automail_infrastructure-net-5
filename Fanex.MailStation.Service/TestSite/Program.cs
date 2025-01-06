using Fanex.SimpleMailDelivery;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMailDelivery(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapControllers();

app.Run();
