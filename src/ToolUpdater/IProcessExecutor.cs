using System.Diagnostics;

namespace ToolUpdater;

public interface IProcessExecutor
{
    string StandardOutput { get; }
    bool HasExited { get; }
    Process Start(ProcessStartInfo startInfo);
    void WaitForExit();
}