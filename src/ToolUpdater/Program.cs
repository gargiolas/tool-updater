using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ToolUpdater;

Console.WriteLine("Verifica aggiornamenti per i tool .NET globali...\n");

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<ICommandExecutor, CommandExecutor>();
builder.Services.AddTransient<IProcessExecutor, ProcessExecutor>();

using IHost host = builder.Build();

var tools = GetInstalledGlobalTools(host.Services);

if (tools.Length == 0)
{
    Console.WriteLine("Nessun tool globale installato.");
    return;
}

foreach (var tool in tools)
{
    var updateAvailable = CheckForUpdate(host.Services, tool);

    Console.WriteLine(updateAvailable
        ? $"Aggiornamento disponibile per {tool}!"
        : $"Nessun aggiornamento disponibile per {tool}.");

    if (!updateAvailable) continue;

    Console.Write($"Vuoi aggiornare {tool}? (y/n): ");

    var userInput = Console.ReadLine();

    if (userInput?.Equals("Y", StringComparison.InvariantCultureIgnoreCase) == false)
    {
        Console.WriteLine($"Aggiornamento di {tool} annullato.");
        continue;
    }

    Console.WriteLine($"Aggiornamento in corso per {tool}...");
    var success = UpdateTool(tool);
    Console.WriteLine(success
        ? $"Tool {tool} aggiornato con successo!"
        : $"Si è verificato un errore durante l'aggiornamento di {tool}.");
}

await host.RunAsync();

static string[] GetInstalledGlobalTools(IServiceProvider hostProvider)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;
    var commandExecutor = provider.GetRequiredService<ICommandExecutor>();

    var output =
        commandExecutor.Execute("dotnet", "tool list --global");

    return output;
}


static bool CheckForUpdate(IServiceProvider hostProvider, string tool)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;
    var processExecutor = provider.GetRequiredService<IProcessExecutor>();

    var startInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments =
            $"tool update {tool} --dry-run", // Il flag --dry-run non esegue l'aggiornamento, ma fornisce informazioni
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = processExecutor.Start(startInfo);
    var output = processExecutor?.StandardOutput;

    return output?.Contains("update available") == true;
}

static bool UpdateTool(string tool)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = $"tool update {tool}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = Process.Start(startInfo);
    using (var reader = process?.StandardOutput)
    {
        var output = reader?.ReadToEnd();
        Console.WriteLine(output);
    }

    using (var reader = process?.StandardError)
    {
        var error = reader?.ReadToEnd();
        if (string.IsNullOrWhiteSpace(error)) return true;

        Console.WriteLine($"Errore: {error}");
        return false;
    }
}