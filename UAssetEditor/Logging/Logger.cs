global using static UAssetEditor.Logging.Logger;
using Serilog;

namespace UAssetEditor.Logging;

public static class Logger
{
    public static string OutputLocation;

    public static void StartLogger(string file)
    {
        OutputLocation = file;

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(file,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateLogger();
    }

    public static void LogInfo(string text) => Log.Information(text);
    
    public static void LogWarn(string text) => Log.Warning(text);
    
    public static void LogError(string text) => Log.Error(text);
    
    public static void LogVerbose(string text) => Log.Verbose(text);
    
    public static void LogFatal(string text) => Log.Fatal(text);
}