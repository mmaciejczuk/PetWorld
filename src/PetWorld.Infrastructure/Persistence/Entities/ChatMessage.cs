namespace PetWorld.Infrastructure.Persistence.Entities;

public sealed class ChatMessage
{
    public long Id { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public int Iterations { get; set; }
}
