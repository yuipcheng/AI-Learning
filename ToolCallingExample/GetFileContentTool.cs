using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ToolCallingExample
{
    /// <summary>
    /// A simple tool implementation that reads a file's content and returns it as a string.
    /// Designed to mimic a "GetFileContentTool" that an AI could call via IFoundryTool.
    /// </summary>
    public class GetFileContentTool : IFoundryTool
    {
        public string Name => "GetFileContentTool";

        public async Task<string> RunAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "Error: no file path provided.";
            }

            try
            {
                var path = input.Trim();

                if (!File.Exists(path))
                {
                    return $"Error: file not found - {path}";
                }

                // Limit the maximum amount read to avoid returning extremely large files.
                const int maxBytes = 200_000; // ~200 KB

                using var stream = File.OpenRead(path);
                var length = (int)Math.Min(stream.Length, maxBytes);
                var buffer = new byte[length];
                var read = await stream.ReadAsync(buffer.AsMemory(0, length));

                var content = Encoding.UTF8.GetString(buffer, 0, read);

                if (stream.Length > maxBytes)
                {
                    content += "\n\n--- Truncated file content (too large) ---";
                }

                return content;
            }
            catch (Exception ex)
            {
                return $"Error reading file: {ex.Message}";
            }
        }
    }
}