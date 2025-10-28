using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using AIChatAgent.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace AIChatAgent.Infrastructure.Services
{
    public class RedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private const int DEFAULT_EXPIRY_HOURS = 24;

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ConversationContext?> GetConversationContextAsync(string userId)
        {
            var db = _redis.GetDatabase();
            var key = $"chat:context:{userId}";
            
            var value = await db.StringGetAsync(key);
            if (!value.HasValue)
                return null;

            try
            {
                return JsonSerializer.Deserialize<ConversationContext>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing conversation context for user {UserId}", userId);
                return null;
            }
        }

        public async Task SaveConversationContextAsync(string userId, ConversationContext context)
        {
            var db = _redis.GetDatabase();
            var key = $"chat:context:{userId}";
            
            try
            {
                var json = JsonSerializer.Serialize(context, _jsonOptions);
                await db.StringSetAsync(
                    key, 
                    json,
                    expiry: TimeSpan.FromHours(DEFAULT_EXPIRY_HOURS)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving conversation context for user {UserId}", userId);
                throw;
            }
        }

        public async Task AddMessageToContextAsync(string userId, string role, string content)
        {
            var context = await GetConversationContextAsync(userId) ?? new ConversationContext { UserId = userId };
            
            context.Messages.Add(new ChatMessage
            {
                Role = role,
                Content = content,
                Timestamp = DateTime.UtcNow
            });
            
            context.LastUpdatedAt = DateTime.UtcNow;
            
            await SaveConversationContextAsync(userId, context);
        }

        public async Task ClearContextAsync(string userId)
        {
            var db = _redis.GetDatabase();
            var key = $"chat:context:{userId}";
            await db.KeyDeleteAsync(key);
        }
    }
}