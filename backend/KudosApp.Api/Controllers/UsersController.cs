using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KudosApp.Application.Features.Users.Queries;
using KudosApp.Application.Features.Users.DTOs;

namespace KudosApp.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        var response = await mediator.Send(new GetUsersQuery());
        return Ok(response);
    }
}
