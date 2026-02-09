using PetWorld.Application.Ai;
using PetWorld.Domain.Ai;

namespace PetWorld.Infrastructure.Ai;

public sealed class StubAiWriterCriticService : IAiWriterCriticService
{
    public Task<WriterCriticResult> RunAsync(string question, CancellationToken ct = default)
    {
        if (question is null) question = string.Empty;

        int max = WriterCriticDefaults.MaxIterations;
        string answer = string.Empty;
        string? feedback = null;

        for (int i = 1; i <= max; i++)
        {
            ct.ThrowIfCancellationRequested();

            if (i == 1)
                answer = "Stub answer: " + question;
            else
                answer = "Stub answer (improved #" + i + "): " + question + " | applied feedback: " + feedback;

            // Critic (stub)
            if (!string.IsNullOrWhiteSpace(answer) && answer.Length >= 20)
            {
                return Task.FromResult(new WriterCriticResult(answer, i, true, null));
            }

            feedback = "Answer too short. Add more detail.";
        }

        return Task.FromResult(new WriterCriticResult(answer, max, false, feedback));
    }
}
