#nullable enable
namespace Mochineko.LLMAgent.Summarization
{
    internal static class PromptTemplate
    {
        internal static string Summarize(string currentSummary, string conversations) => $@"
Progressively summarize the lines of conversation provided in JSON format with keys ""role"" and ""content"",
 adding onto the previous summary and returning a new summary.

EXAMPLE

Current summary:
The human asks what the AI thinks of artificial intelligence.
The AI thinks artificial intelligence is a force for good.

New lines of conversation in JSON format:
{{
  ""conversations"":
    [
      {{ ""role"": ""user"", ""content"": ""Why do you think artificial intelligence is a force for good?"" }},
      {{ ""role"": ""assistant"", ""content"": ""Because artificial intelligence will help humans reach their full potential."" }}
    ]
}}

New summary:
The human asks what the AI thinks of artificial intelligence. The AI thinks artificial intelligence is a force for good because it will help humans reach their full potential.

END OF EXAMPLE

Current summary:
{currentSummary}

New lines of conversation in JSON format:
{conversations}

New summary:
";
    }
}