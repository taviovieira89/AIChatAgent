using MediatR;
using AIChatAgent.Domain.Interfaces;

namespace AIChatAgent.Application.Features.Chat;

public class SendMessage
{
    public record Command(string Message) : IRequest<string>;

    public class Handler : IRequestHandler<Command, string>
    {
        private readonly IChatService _chatService;

        public Handler(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task<string> Handle(Command request, CancellationToken cancellationToken)
        {
            return await _chatService.GetResponseAsync(request.Message, cancellationToken);
        }
    }
}