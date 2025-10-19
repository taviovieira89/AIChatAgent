using AIChatAgent.Domain.Interfaces;
using AIChatAgent.Infrastructure.Options;
using AIChatAgent.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Options
builder.Services.Configure<OpenAIServiceOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<GeminiServiceOptions>(builder.Configuration.GetSection("Gemini"));

// Register Services based on configuration
var chatProvider = builder.Configuration["ChatProvider"]?.ToLower() ?? "openai";

builder.Services.AddScoped<IChatService>(serviceProvider =>
{
    return chatProvider switch
    {
        "openai" => serviceProvider.GetRequiredService<OpenAIChatService>(),
        "gemini" => serviceProvider.GetRequiredService<GeminiChatService>(),
        _ => throw new InvalidOperationException($"Unsupported chat provider: {chatProvider}")
    };
});

builder.Services.AddScoped<OpenAIChatService>();
builder.Services.AddScoped<GeminiChatService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint para chat
app.MapPost("/chat", async (ChatRequest request, IChatService chatService, CancellationToken cancellationToken) =>
{
    try
    {
        var response = await chatService.GetResponseAsync(request.Message, cancellationToken);
        return Results.Ok(new ChatResponse { Message = response });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error processing chat request",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

app.Run();

public record ChatRequest
{
    public required string Message { get; init; }
}

public record ChatResponse
{
    public required string Message { get; init; }
}