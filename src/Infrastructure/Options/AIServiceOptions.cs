namespace AIChatAgent.Infrastructure.Options
{
    public class OpenAIServiceOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string DeploymentName { get; set; } = string.Empty;
    }
}