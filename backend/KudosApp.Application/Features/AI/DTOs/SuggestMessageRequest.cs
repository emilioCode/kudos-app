namespace KudosApp.Application.Features.AI.DTOs;

public class SuggestMessageRequest
{
    public required string RecipientName { get; set; }
    public required string CategoryName { get; set; }
}
