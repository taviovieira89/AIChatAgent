namespace AIChatAgent.Domain.Interfaces;

public interface IChatService
{
    Task<string> GetResponseAsync(string message, CancellationToken cancellationToken = default);
}