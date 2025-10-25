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
                // Use a more recent API surface - adjust if your Google project exposes a different version
                var version = "v1beta";
                var requestUri = $"/{version}/models/{_options.ModelName}:generateContent?key={_options.ApiKey}";

                var request = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = message }
                            }
                        }
                    },
                    safetySettings = new[]
                    {
                        new
                        {
                            category = "HARM_CATEGORY_HARASSMENT",
                            threshold = "BLOCK_NONE"
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);

                // Read body first so we can log details on non-success responses
                var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API returned {StatusCode}. Body: {Body}", response.StatusCode, jsonResponse);
                    throw new HttpRequestException($"Gemini API returned {response.StatusCode}: {jsonResponse}");
                }

                var responseObj = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                // Defensive parsing - check properties existence
                string generatedText = "No response generated";
                if (responseObj.ValueKind == JsonValueKind.Object && responseObj.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var first = candidates[0];
                    if (first.TryGetProperty("content", out var contentProp) && contentProp.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var part = parts[0];
                        if (part.TryGetProperty("text", out var textProp))
                        {
                            generatedText = textProp.GetString() ?? generatedText;
                        }
                    }
                }

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