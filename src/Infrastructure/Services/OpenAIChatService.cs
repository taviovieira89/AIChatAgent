using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIChatAgent.Domain.Interfaces;
using AIChatAgent.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Azure.AI.OpenAI;

namespace AIChatAgent.Infrastructure.Services
{
    public class OpenAIChatService : IChatService
    {
        private readonly OpenAIClient _client;
        private readonly ILogger<OpenAIChatService> _logger;
        private readonly OpenAIServiceOptions _options;

        public OpenAIChatService(IOptions<OpenAIServiceOptions> options, ILogger<OpenAIChatService> logger)
        {
            _logger = logger;
            _options = options.Value;

            if (string.IsNullOrEmpty(_options.ApiKey))
            {
                _logger.LogError("OpenAI API Key is missing or empty.");
                throw new InvalidOperationException("OpenAI API Key is not configured.");
            }

            _client = new OpenAIClient(_options.ApiKey);
            _logger.LogInformation("OpenAIClient initialized successfully.");
        }

        public async Task<string> GetResponseAsync(string message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Received message for OpenAI: {Message}", message);
            var completionsOptions = new ChatCompletionsOptions
            {
                DeploymentName = _options.DeploymentName,
                Messages =
                {
                    new ChatMessage(ChatRole.System, "You are a helpful AI assistant."),
                    new ChatMessage(ChatRole.User, message)
                }
            };

            try
            {
                var response = await _client.GetChatCompletionsAsync(completionsOptions, cancellationToken);
                var content = response.Value.Choices[0].Message.Content;
                _logger.LogInformation("OpenAI API call successful. Response: {ResponseContent}", content);
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
