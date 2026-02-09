namespace PetWorld.Infrastructure.Ai;

public sealed class PetWorldAiOptions
{
    public string? Endpoint { get; init; }     // np. https://xxx.openai.azure.com/
    public string? Key { get; init; }          // Azure OpenAI key
    public string? Deployment { get; init; }   // np. gpt-4o-mini
}
