using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.Extensions.Logging;
using AIChatAgent.Application.Features.Chat;

namespace AIChatAgent.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IMediator mediator, ILogger<ChatController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] string message)
    {
        _logger.LogInformation("Received chat message: {Message}", message);
        var response = await _mediator.Send(new SendMessage.Command(message));
        _logger.LogInformation("Sent message to MediatR. Response received: {Response}", response);
        return Ok(response);
    }
}