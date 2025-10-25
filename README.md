# AI Chat Agent

A modern chat agent implementation using .NET 9, supporting multiple AI providers (OpenAI and Google Gemini) with Clean Architecture principles.

## 📦 Quick Start

Install the package:
```shell
dotnet add package AIChatAgent
```

Use in your code:
```csharp
using AIChatAgent.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

// Setup in your Startup.cs or Program.cs
services.Configure<GeminiServiceOptions>(configuration.GetSection("Gemini"));
// OR
services.Configure<OpenAIServiceOptions>(configuration.GetSection("OpenAI"));

// Configure the chat provider
services.AddScoped<IChatService, GeminiChatService>();
// OR
services.AddScoped<IChatService, OpenAIChatService>();

// Use in your code
public class ChatExample
{
    private readonly IChatService _chatService;

    public ChatExample(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<string> Chat(string message)
    {
        return await _chatService.GetResponseAsync(message);
    }
}
```

Configure in your appsettings.json:
```json
{
  "Gemini": {
    "ApiKey": "your-api-key-here",
    "ModelName": "gemini-pro"
  }
}
```

## 🆕 Recent Improvements

- Enhanced Gemini API Integration:
  - Improved error handling with detailed API response logging
  - Updated request payload format for better compatibility
  - Added defensive JSON parsing for API responses
  - Implemented proper model name configuration
- CI/CD Enhancements:
  - Added automated API testing workflow
  - Configured GitHub Actions for Gemini API validation
  - Improved environment secret management

## 🗺 Future Roadmap

### Phase 1: Core Enhancements
- [ ] Implement conversation history persistence
- [ ] Add Claude/Anthropic AI provider support
- [ ] Enable streaming responses for better UX
- [ ] Add domain-specific prompt templates

### Phase 2: Technical Improvements
- [ ] Implement response caching system
- [ ] Add rate limiting and usage controls
- [ ] Configure circuit breaker pattern
- [ ] Set up monitoring (Prometheus/Grafana)
- [ ] Expand test coverage

### Phase 3: User Interface & APIs
- [ ] Develop React/Angular frontend
- [ ] Create CLI interface
- [ ] Implement WebSocket support
- [ ] Add file processing capabilities

### Phase 4: Infrastructure
- [ ] Configure staging environment
- [ ] Implement automatic deployments
- [ ] Add cost monitoring for AI APIs
- [ ] Set up Docker/K8s deployment
- [ ] Configure auto-scaling

### Phase 5: Security & Performance
- [ ] Implement authentication/authorization
- [ ] Add per-user rate limiting
- [ ] Set up audit logging
- [ ] Add content validation
- [ ] Implement response caching

## 🚀 Features

- Clean Architecture implementation
- Multiple AI provider support (OpenAI/Gemini)
- Secure configuration management
- Swagger documentation
- Integration and Unit tests

## 🛠 Tech Stack

- .NET 9.0
- OpenAI SDK
- Google Cloud AI Platform
- Clean Architecture

## 📋 Prerequisites

- .NET 9 SDK
- OpenAI API Key
- Google Gemini API Key

## 🔧 Configuration

### Desenvolvimento Local

1. Crie um arquivo `appsettings.Development.json` na pasta `src/AIChatAgent/`:

```json
{
  "OpenAI": {
    "ApiKey": "seu-openai-api-key-aqui",
    "ModelName": "gpt-3.5-turbo"
  },
  "Gemini": {
    "ApiKey": "seu-gemini-api-key-aqui",
    "ModelName": "gemini-2.5-flash"
  }
}
```

2. **Importante**: O arquivo `appsettings.Development.json` está no `.gitignore` e não será commitado.

### Ambiente de Produção

Em produção, use variáveis de ambiente:

```bash
# OpenAI
OPENAI__APIKEY=sua-chave-aqui
OPENAI__MODELNAME=gpt-3.5-turbo

# Gemini
GEMINI__APIKEY=sua-chave-aqui
GEMINI__MODELNAME=gemini-2.5-flash

# Provider
CHATPROVIDER=Gemini  # ou OpenAI
```

### GitHub Secrets (CI/CD)

Para deploy seguro usando GitHub Actions:

1. Vá para `Settings > Secrets and variables > Actions` no seu repositório
2. Adicione dois secrets:
   - `OPENAI_API_KEY`: Sua chave da OpenAI
   - `GEMINI_API_KEY`: Sua chave do Gemini

O workflow em `.github/workflows/deploy.yml` já está configurado para usar estes secrets.

### Docker

Se estiver usando Docker, defina as variáveis no seu docker-compose.yml:

```yaml
environment:
  - OPENAI__APIKEY=${OPENAI_API_KEY}
  - GEMINI__APIKEY=${GEMINI_API_KEY}
  - CHATPROVIDER=Gemini
```

E crie um arquivo `.env` (que está no .gitignore):

```bash
OPENAI_API_KEY=sua-chave-aqui
GEMINI_API_KEY=sua-chave-aqui
```