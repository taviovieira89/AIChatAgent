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

// Get provider from environment variable or fallback to configuration
var defaultAIProvider = Environment.GetEnvironmentVariable("CHATPROVIDER") 
    ?? builder.Configuration["AIProvider:Default"] 
    ?? "OpenAI";

// Configure OpenAI options
builder.Services.AddOptions<OpenAIServiceOptions>().Configure(options => {
    options.ApiKey = Environment.GetEnvironmentVariable("OPENAI__APIKEY") 
        ?? builder.Configuration["AIProvider:OpenAI:ApiKey"] 
        ?? "";
    options.DeploymentName = builder.Configuration["AIProvider:OpenAI:DeploymentName"] 
        ?? "gpt-3.5-turbo";
});

// Configure Gemini options
builder.Services.AddOptions<GeminiServiceOptions>().Configure(options => {
    options.ApiKey = Environment.GetEnvironmentVariable("GEMINI__APIKEY") 
        ?? builder.Configuration["AIProvider:Gemini:ApiKey"] 
        ?? "";
    options.ModelName = builder.Configuration["AIProvider:Gemini:ModelName"] 
        ?? "gemini-pro";
});

// Register the appropriate service
if (defaultAIProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IChatService, OpenAIChatService>();
}
else if (defaultAIProvider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IChatService, GeminiChatService>();
}
else
{
    throw new InvalidOperationException($"Invalid CHATPROVIDER specified: {defaultAIProvider}. Must be 'OpenAI' or 'Gemini'.");
}

var app = builder.Build();

// Check for API test command
if (args.Length > 0 && args[0] == "test-api")
{
    if (args.Length < 3)
    {
        Console.WriteLine("Usage: test-api <provider> <message>");
        return 1;
    }

    var provider = args[1].ToLower();
    var message = args[2];
    
    try
    {
        using var scope = app.Services.CreateScope();
        var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();
        
        Console.WriteLine($"Testing {provider} API with message: {message}");
        var response = await chatService.GetResponseAsync(message);
        Console.WriteLine($"Response: {response}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error testing API: {ex.Message}");
        return 1;
    }
}

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

await app.RunAsync();
return 0;
