using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KudosApp.Application.Features.Kudos.Commands;
using KudosApp.Application.Features.Kudos.Queries;
using KudosApp.Application.Features.Kudos.DTOs;

namespace KudosApp.Api.Controllers;

[ApiController]
[Route("api/kudos")]
[Authorize]
public class KudosController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<KudosResponse>> GiveKudos([FromBody] GiveKudosCommand command)
    {
        var response = await mediator.Send(command);
        return Ok(response);
    }

    [HttpGet("feed")]
    [AllowAnonymous]
    public async Task<ActionResult<List<KudosResponse>>> GetFeed()
    {
        var response = await mediator.Send(new GetKudosFeedQuery());
        return Ok(response);
    }
}
