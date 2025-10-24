# AI Chat Agent

A modern chat agent implementation using .NET 9, supporting multiple AI providers (OpenAI and Google Gemini) with Clean Architecture principles.

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