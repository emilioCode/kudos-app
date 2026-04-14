using MediatR;
using Microsoft.AspNetCore.Mvc;
using KudosApp.Application.Features.Auth.Commands;
using KudosApp.Application.Features.Auth.DTOs;

namespace KudosApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterCommand command)
    {
        var response = await mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginCommand command)
    {
        var response = await mediator.Send(command);
        return Ok(response);
    }
}
