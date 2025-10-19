using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AIChatAgent.Domain.Interfaces;
using AIChatAgent.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIChatAgent.Infrastructure.Services
{
    public class GeminiChatService : IChatService
    {
        private readonly ILogger<GeminiChatService> _logger;
        private readonly GeminiServiceOptions _options;
        private readonly HttpClient _httpClient;

        public GeminiChatService(IOptions<GeminiServiceOptions> options, ILogger<GeminiChatService> logger)
        {
            _logger = logger;
            _options = options.Value;

            if (string.IsNullOrEmpty(_options.ApiKey))
            {
                _logger.LogError("Gemini API Key is missing or empty.");
                throw new InvalidOperationException("Gemini API Key is not configured.");
            }

            _logger.LogInformation("GeminiChatService initialized with model: {ModelName}", _options.ModelName);

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://generativelanguage.googleapis.com")
            };
        }

        public async Task<string> GetResponseAsync(string message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Received message for Gemini: {Message}", message);
            try
            {
                var version = "v1beta";
                var requestUri = $"/{version}/models/{_options.ModelName}:generateContent?key={_options.ApiKey}";

                var request = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = message
                                }
                            }
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                var responseObj = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                var generatedText = responseObj
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "No response generated";

                _logger.LogInformation("Gemini API call successful. Response: {ResponseContent}", generatedText);
                return generatedText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}