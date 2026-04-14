namespace KudosApp.Application.Features.Kudos.DTOs;

public class KudosResponse
{
    public Guid Id { get; set; }
    public required string SenderName { get; set; }
    public required string RecipientName { get; set; }
    public required string CategoryName { get; set; }
    public required string Message { get; set; }
    public int Points { get; set; }
    public DateTime CreatedAt { get; set; }
}
