using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging.Abstractions;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;

namespace ToolCallingExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await FoundryLocalManager.CreateAsync(new Configuration { AppName = "my-app" }, NullLogger.Instance);
            var catalog = await FoundryLocalManager.Instance.GetCatalogAsync();
            var models = catalog.ListModelsAsync().Result;
            var model = await catalog.GetModelAsync("phi-3-mini-128k") ?? throw new Exception("Model not found.");
            Console.WriteLine($"Downloading model... {model.Alias}");
            await model.DownloadAsync();
            Console.WriteLine("Loading model...");
            await model.LoadAsync();
            var chat = await model.GetChatClientAsync();

            // Simple loop that accepts user prompts. A minimal "system prompt" behavior
            // is implemented here by intercepting user input and invoking the GetFileContentTool
            // when the user asks to display a file's content. This demonstrates how a
            // tool call can be triggered from a user prompt.
            var fileTool = new GetFileContentTool();

            while (true)
            {
                Console.Write("\r\nYou: ");
                var rawInput = Console.ReadLine() ?? string.Empty;
                var input = rawInput.Trim();

                if (string.Equals(input, "bye", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (string.Equals(input, "list model", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(input, "list models", StringComparison.OrdinalIgnoreCase))
                {
                    models.ForEach(m => { Console.WriteLine($"Kawa: {m.Alias}"); });
                    continue;
                }

                // Otherwise forward the prompt to the model as usual.
                // Add a system message that instructs the model how to request the GetFileContentTool.
                var systemMessage = new ChatMessage
                {
                    Role = "system",
                    Content = "If the user asks to display or read a file, respond with a single-line token in the exact form: CALL_TOOL:GetFileContentTool:<absolute-or-relative-path> and no other text. For any other user input respond normally."
                };

                var response = await chat.CompleteChatAsync(new[] { systemMessage, new ChatMessage { Role = "user", Content = input } });
                var modelOutput = response.Choices![0].Message.Content?.Trim() ?? string.Empty;

                // If model requested a tool call using the agreed token, invoke the tool.
                const string toolPrefix = "CALL_TOOL:GetFileContentTool:";
                if (modelOutput.StartsWith(toolPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var requestedPath = modelOutput.Substring(toolPrefix.Length).Trim();
                    Console.WriteLine($"[System] Model requested tool for path: {requestedPath}");
                    var toolResult = await fileTool.RunAsync(requestedPath);
                    Console.WriteLine("--- File Content (from tool) ---");
                    Console.WriteLine(toolResult);
                    Console.WriteLine("--- End File Content ---");
                }
                else
                {
                    Console.WriteLine($"Kawa: {modelOutput}");
                }
            }
        }
    }
}
