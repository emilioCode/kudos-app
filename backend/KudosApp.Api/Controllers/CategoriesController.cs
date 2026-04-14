using MediatR;
using Microsoft.AspNetCore.Mvc;
using KudosApp.Application.Features.Categories.Queries;
using KudosApp.Application.Features.Categories.DTOs;

namespace KudosApp.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryResponse>>> GetCategories()
    {
        var response = await mediator.Send(new GetCategoriesQuery());
        return Ok(response);
    }
}
