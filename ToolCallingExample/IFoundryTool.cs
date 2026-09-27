using System.Threading.Tasks;

namespace ToolCallingExample
{
    /// <summary>
    /// Simple interface representing a Foundry-style tool that can be invoked by the AI.
    /// Implementations should perform their action and return a string result suitable for
    /// display in the chat UI.
    /// </summary>
    public interface IFoundryTool
    {
        /// <summary>
        /// Human-friendly name of the tool.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Run the tool with the provided input (for GetFileContentTool the input is a file path).
        /// Returns a string that can be displayed back to the user or model.
        /// </summary>
        Task<string> RunAsync(string input);
    }
}