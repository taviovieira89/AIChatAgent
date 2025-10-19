namespace AIChatAgent.Infrastructure.Options
{
    public class GeminiServiceOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ModelName { get; set; } = "gemini-pro";
    }
}