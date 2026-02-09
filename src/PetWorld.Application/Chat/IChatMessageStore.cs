namespace PetWorld.Application.Chat;

public sealed record ChatMessageDto(
    long Id,
    DateTime CreatedAtUtc,
    string Question,
    string Answer,
    int Iterations
);

public interface IChatMessageStore
{
    Task<long> AddAsync(string question, string answer, int iterations, CancellationToken ct = default);

    Task<IReadOnlyList<ChatMessageDto>> GetLatestAsync(int take = 100, CancellationToken ct = default);
}
