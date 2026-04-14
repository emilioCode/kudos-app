using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KudosApp.Application.Interfaces;
using KudosApp.Application.Features.AI.DTOs;

namespace KudosApp.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AIController(IOpenAiService openAiService) : ControllerBase
{
    [HttpPost("suggest-message")]
    public async Task<ActionResult<AISuggestionResponse>> SuggestMessage([FromBody] SuggestMessageRequest request)
    {
        var suggestion = await openAiService.SuggestKudosMessageAsync(request.RecipientName, request.CategoryName);
        return Ok(new AISuggestionResponse { Suggestion = suggestion });
    }

    [HttpPost("suggest-category")]
    public async Task<ActionResult<AISuggestionResponse>> SuggestCategory([FromBody] SuggestCategoryRequest request)
    {
        var suggestion = await openAiService.SuggestCategoryAsync(request.Message, request.AvailableCategories);
        return Ok(new AISuggestionResponse { Suggestion = suggestion });
    }
}
