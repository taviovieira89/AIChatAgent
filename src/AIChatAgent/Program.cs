using AIChatAgent.Domain.Interfaces;
using AIChatAgent.Infrastructure.Options;
using AIChatAgent.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Options
builder.Services.Configure<OpenAIServiceOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<GeminiServiceOptions>(builder.Configuration.GetSection("Gemini"));

// Configure Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));
builder.Services.AddScoped<RedisCacheService>();

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
app.MapPost("/chat", async (ChatRequest request, IChatService chatService, RedisCacheService redisCache, CancellationToken cancellationToken) =>
{
    try
    {
        // Adiciona a mensagem do usuário ao contexto
        await redisCache.AddMessageToContextAsync(request.UserId, "user", request.Message);
        
        // Obtém o contexto completo da conversa
        var context = await redisCache.GetConversationContextAsync(request.UserId);
        
        // Concatena o histórico recente para criar contexto
        var conversationHistory = string.Join("\n", context?.Messages.TakeLast(5).Select(m => $"{m.Role}: {m.Content}") ?? Array.Empty<string>());
        var messageWithContext = $"{conversationHistory}\nuser: {request.Message}";
        
        // Obtém resposta do serviço de chat
        var response = await chatService.GetResponseAsync(messageWithContext, cancellationToken);
        
        // Salva a resposta no contexto
        await redisCache.AddMessageToContextAsync(request.UserId, "assistant", response);
        
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
    public required string UserId { get; init; }  // Identificador do usuário para manter contexto
}

public record ChatResponse
{
    public required string Message { get; init; }
}