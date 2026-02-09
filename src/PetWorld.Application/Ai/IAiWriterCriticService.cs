using PetWorld.Domain.Ai;

namespace PetWorld.Application.Ai;

public interface IAiWriterCriticService
{
    Task<WriterCriticResult> RunAsync(string question, CancellationToken ct = default);
}
