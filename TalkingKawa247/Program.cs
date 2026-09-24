using System;
using Microsoft.AI.Foundry.Local;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;

await FoundryLocalManager.CreateAsync(new Configuration { AppName = "my-app" }, NullLogger.Instance);
var catalog = await FoundryLocalManager.Instance.GetCatalogAsync();
var models = catalog.ListModelsAsync().Result;
var model = await catalog.GetModelAsync("phi-3-mini-128k") ?? throw new Exception("Model not found.");
Console.WriteLine($"Downloading model... {model.Alias}");
await model.DownloadAsync();
Console.WriteLine("Loading model...");
await model.LoadAsync();
var chat = await model.GetChatClientAsync();

while (true)
{
    Console.Write("\r\nYou: ");
    var input = Console.ReadLine().ToLower();
    switch (input)
    {
        case "bye":
            return;
        case "list model":
        case "list models":
            models.ForEach(m => { Console.WriteLine($"Kawa: {m.Alias}"); });
            break;
        default:
            var response = await chat.CompleteChatAsync(new[] { new ChatMessage { Role = "user", Content = input } });
            Console.WriteLine($"Kawa: {response.Choices![0].Message.Content}");
            break;
    }
}