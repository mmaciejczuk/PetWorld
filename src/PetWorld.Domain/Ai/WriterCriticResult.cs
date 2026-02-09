namespace PetWorld.Domain.Ai;

public static class WriterCriticDefaults
{
    public const int MaxIterations = 3;
}

public sealed record WriterCriticResult(
    string FinalAnswer,
    int Iterations,
    bool Approved,
    string? Feedback
);
