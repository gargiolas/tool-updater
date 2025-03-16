using System.Diagnostics;

namespace ToolUpdater;

public class CommandExecutor : ICommandExecutor
{
    public string[] Execute(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        using var reader = process?.StandardOutput;
        
        string output = reader?.ReadToEnd() ?? string.Empty;

        return output?.Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("Tool"))
            .Select(line => line.Split(' ').First().Trim())
            .Skip(2)
            .ToArray() ?? [];
    }
}