using web_api;
using web_api.Controllers;
using web_api.Interfaces;
using web_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer(); // This generates the OpenAPI spec
builder.Services.AddSwaggerGen();           // This adds the Swagger/OpenAPI generator

builder.Services.AddScoped<IArtificalInteligence, AIServices>();
builder.Services.AddSingleton<WeatherForecastController, WeatherForecastController>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();              // Serves the OpenAPI spec
    app.UseSwaggerUI();

}

app.UseAuthorization();

app.MapControllers();

app.Run();
