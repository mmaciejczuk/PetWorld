using Microsoft.EntityFrameworkCore;
using PetWorld.Application.Chat;
using PetWorld.Infrastructure.Persistence;
using PetWorld.Infrastructure.Persistence.Entities;

namespace PetWorld.Infrastructure.Chat;

public sealed class EfChatMessageStore : IChatMessageStore
{
    private readonly PetWorldDbContext _db;

    public EfChatMessageStore(PetWorldDbContext db)
    {
        _db = db;
    }

    public async Task<long> AddAsync(string question, string answer, int iterations, CancellationToken ct = default)
    {
        var entity = new ChatMessage
        {
            CreatedAtUtc = DateTime.UtcNow,
            Question = question ?? string.Empty,
            Answer = answer ?? string.Empty,
            Iterations = iterations
        };

        _db.ChatMessages.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task<IReadOnlyList<ChatMessageDto>> GetLatestAsync(int take = 100, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);

        return await _db.ChatMessages
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Take(take)
            .Select(x => new ChatMessageDto(
                x.Id,
                x.CreatedAtUtc,
                x.Question,
                x.Answer,
                x.Iterations
            ))
            .ToListAsync(ct);
    }
}
