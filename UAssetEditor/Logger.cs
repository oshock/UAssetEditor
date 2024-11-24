using Serilog;

namespace UAssetEditor;

public static class Logger
{
    public static void StartLogger()
    {
        Log.Logger =   new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
    }
}