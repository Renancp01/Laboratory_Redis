using Infra.Redis.Extensions;
using WebApi.Boundaries.UseCases;
using WebApi.UseCases;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddScoped<IGetWeatherForecastUseCase, GetWeatherForecastUseCase>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();