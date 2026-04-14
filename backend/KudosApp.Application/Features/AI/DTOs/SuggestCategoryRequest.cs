using System.Collections.Generic;

namespace KudosApp.Application.Features.AI.DTOs;

public class SuggestCategoryRequest
{
    public required string Message { get; set; }
    public required List<string> AvailableCategories { get; set; }
}
