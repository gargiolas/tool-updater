using System.Diagnostics;

namespace ToolUpdater;

internal sealed class ProcessExecutor : IProcessExecutor
{
    private Process? _process;

    public string StandardOutput => _process?.StandardOutput.ReadToEnd() ?? string.Empty;
    public bool HasExited { get; }
    public Process Start(ProcessStartInfo startInfo)
    {
        _process = Process.Start(startInfo);
        return _process;
    }

    public void WaitForExit()
    {
        _process?.WaitForExit();
    }
}