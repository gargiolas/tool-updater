namespace ToolUpdater;

public interface ICommandExecutor
{
    string[] Execute(string fileName, string arguments);
}