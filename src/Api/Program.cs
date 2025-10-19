using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AIChatAgent.Domain.Interfaces;
using AIChatAgent.Infrastructure.Services;
using AIChatAgent.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add logging
builder.Services.AddLogging(configure => configure.AddConsole());

// Register MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(AIChatAgent.Application.Features.Chat.SendMessage).Assembly);
});

// Conditional registration of IChatService based on configuration
var defaultAIProvider = builder.Configuration["AIProvider:Default"] ?? "OpenAI";

if (defaultAIProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IChatService, OpenAIChatService>();
    builder.Services.AddOptions<OpenAIServiceOptions>().Configure(options =>
        builder.Configuration.GetSection("AIProvider:OpenAI").Bind(options));
}
else if (defaultAIProvider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IChatService, GeminiChatService>();
    builder.Services.AddOptions<GeminiServiceOptions>().Configure(options =>
        builder.Configuration.GetSection("AIProvider:Gemini").Bind(options));
}
else
{
    throw new InvalidOperationException($"Invalid AIProvider:Default specified in configuration: {defaultAIProvider}");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting(); // Explicitly add routing middleware
app.UseAuthorization(); // Add authorization middleware

app.MapControllers(); // Map controller routes

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
