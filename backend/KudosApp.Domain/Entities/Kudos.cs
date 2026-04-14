namespace KudosApp.Domain.Entities;

public class Kudos
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public Guid CategoryId { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? Sender { get; set; }
    public User? Recipient { get; set; }
    public Category? Category { get; set; }
}
