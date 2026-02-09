using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using PetWorld.Application.Ai;
using PetWorld.Domain.Ai;
using Microsoft.Extensions.Configuration;

namespace PetWorld.Infrastructure.Ai;

public sealed class AgentFrameworkWriterCriticService : IAiWriterCriticService
{
    private readonly PetWorldAiOptions _opt;

    public AgentFrameworkWriterCriticService(IConfiguration cfg)
    {
        _opt = new PetWorldAiOptions
        {
            Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? cfg["AzureOpenAI:Endpoint"],
            Key = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? cfg["AzureOpenAI:Key"],
            Deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? cfg["AzureOpenAI:Deployment"] ?? "gpt-4o-mini",
        };
    }

    public async Task<WriterCriticResult> RunAsync(string question, CancellationToken ct = default)
    {
        question ??= string.Empty;

        // Fallback: brak konfiguracji => nie wywalaj aplikacji
        if (string.IsNullOrWhiteSpace(_opt.Endpoint) || string.IsNullOrWhiteSpace(_opt.Key))
        {
            var fallback = "Stub answer: " + question;
            return new WriterCriticResult(fallback, 1, true, null);
        }

        var client = new AzureOpenAIClient(new Uri(_opt.Endpoint), new AzureKeyCredential(_opt.Key));
        var chat = client.GetChatClient(_opt.Deployment!);

        var catalog =
@"PRODUCT CATALOG:
- Royal Canin Adult Dog 15kg (Dog food) 289 PLN
- Whiskas Adult Chicken 7kg (Cat food) 129 PLN
- Tetra AquaSafe 500ml (Aquaristics) 39 PLN
- JBL ProFlora CO2 Set (Aquaristics) 349 PLN";

        string answer = string.Empty;
        string? feedback = null;
        bool approved = false;

        for (int i = 1; i <= WriterCriticDefaults.MaxIterations; i++)
        {
            ct.ThrowIfCancellationRequested();

            // WRITER
            var writerSystem =
                "You are a helpful assistant for PetWorld e-commerce. " +
                "Answer clearly and briefly. If relevant, recommend products from the catalog.";

            var writerUser =
$@"USER QUESTION:
{question}

{catalog}

CRITIC FEEDBACK (if any):
{(feedback ?? "<none>")}

Write the best possible answer now.";

            answer = await CompleteTextAsync(chat, writerSystem, writerUser, ct);

            // CRITIC (zwraca TYLKO JSON)
            var criticSystem =
                "You are a strict reviewer. Return ONLY valid JSON in this exact schema: " +
                "{\"approved\": true|false, \"feedback\": \"...\"}. " +
                "Set approved=true only if answer is correct, useful, and includes recommendations when relevant.";

            var criticUser =
$@"Evaluate this ANSWER and return JSON only:

{answer}";

            var criticRaw = await CompleteTextAsync(chat, criticSystem, criticUser, ct);

            if (TryParseCritic(criticRaw, out approved, out feedback))
            {
                if (approved)
                    return new WriterCriticResult(answer, i, true, feedback);
            }
            else
            {
                approved = false;
                feedback = "Critic returned invalid JSON. Return only JSON with approved/feedback.";
            }
        }

        return new WriterCriticResult(answer, WriterCriticDefaults.MaxIterations, approved, feedback);
    }

    private static async Task<string> CompleteTextAsync(ChatClient chat, string system, string user, CancellationToken ct)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(system),
            new UserChatMessage(user)
        };

        var resp = await chat.CompleteChatAsync(messages, cancellationToken: ct);

        // Bezpieczne sklejenie segmentów tekstu
        if (resp?.Value?.Content is null || resp.Value.Content.Count == 0)
            return string.Empty;

        var text = string.Concat(resp.Value.Content.Select(c => c.Text));
        return text?.Trim() ?? string.Empty;
    }

    private static bool TryParseCritic(string raw, out bool approved, out string? feedback)
    {
        approved = false;
        feedback = null;

        if (string.IsNullOrWhiteSpace(raw))
            return false;

        // Czasem model dorzuca tekst przed/po JSON – wytnij fragment od pierwszego { do ostatniego }
        int start = raw.IndexOf('{');
        int end = raw.LastIndexOf('}');
        if (start < 0 || end <= start)
            return false;

        var json = raw.Substring(start, end - start + 1);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                return false;

            if (!root.TryGetProperty("approved", out var a) ||
                (a.ValueKind != JsonValueKind.True && a.ValueKind != JsonValueKind.False))
                return false;

            approved = a.GetBoolean();

            if (root.TryGetProperty("feedback", out var f) && f.ValueKind == JsonValueKind.String)
                feedback = f.GetString();

            return true;
        }
        catch
        {
            return false;
        }
    }
}
