using MediatR;
using Microsoft.AspNetCore.Mvc;
using KudosApp.Application.Features.Leaderboard.Queries;
using KudosApp.Application.Features.Leaderboard.DTOs;

namespace KudosApp.Api.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LeaderboardResponse>>> GetLeaderboard()
    {
        var response = await mediator.Send(new GetLeaderboardQuery());
        return Ok(response);
    }
}
