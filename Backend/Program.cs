using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Web;
using System.Net;
using LogLevel = NLog.LogLevel;

namespace Backend;

public static class Program
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static async Task Main(string[] args)
    {
        SetupLogging();

        Log.Info("Building WebHost ...");
        IWebHost host = new WebHostBuilder()
            .UseKestrel(options => options.Listen(IPAddress.Any, 3000))
            .UseContentRoot(Path.Combine(Directory.GetCurrentDirectory()))
            .UseStartup<Startup>()
            .SuppressStatusMessages(true)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            })
            .UseNLog()
            .Build();

        Log.Info("Running WebHost ...");
        await host.RunAsync();
    }

    private static void SetupLogging()
    {
        LoggingConfiguration loggingConfig = new LoggingConfiguration();

        LayoutWithHeaderAndFooter fullLayout = new LayoutWithHeaderAndFooter();
        fullLayout.Header = "#Software: FlixedIo.Etl.InternalDashboard${newline}#StartTime: ${date}";
        fullLayout.Layout = @"[${date} ${logger}] ${level:uppercase=true}: ${message}${onexception:${newline}${exception:format=tostring}}";

        LayoutWithHeaderAndFooter semiFullLayout = new LayoutWithHeaderAndFooter();
        semiFullLayout.Header = "#Software: FlixedIo.Etl.InternalDashboard${newline}#StartTime: ${date}";
        semiFullLayout.Layout = @"[${date} ${logger}] ${level:uppercase=true}: ${message}";

        LoggingRule ignoreMsRule = new LoggingRule() { LoggerNamePattern = "Microsoft.*", Final = true };
        ignoreMsRule.EnableLoggingForLevels(LogLevel.Trace, LogLevel.Info);
        loggingConfig.LoggingRules.Add(ignoreMsRule);

        FileTarget fileTarget = new FileTarget();
        fileTarget.Layout = fullLayout;
        fileTarget.FileName = "logs${dir-separator}FlixedIo.InternalDashboard.${date:universalTime=true:format=yyyy-MM-dd_HH-mm-ss_fff:cached=true}.log";
        fileTarget.FileNameKind = FilePathKind.Relative;
        fileTarget.KeepFileOpen = true;
        loggingConfig.AddTarget("file", fileTarget);
        loggingConfig.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));

        ConsoleWordHighlightingRule wordHighlightingRule = new ConsoleWordHighlightingRule();
        wordHighlightingRule.Regex = "\\[[^]]+\\] ";
        wordHighlightingRule.ForegroundColor = ConsoleOutputColor.Gray;

        ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
        consoleTarget.Layout = semiFullLayout;
        consoleTarget.WordHighlightingRules.Add(wordHighlightingRule);
        consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.White, ConsoleOutputColor.Red));
        consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
        consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
        consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.White, ConsoleOutputColor.NoChange));
        consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.DarkGray, ConsoleOutputColor.NoChange));
        consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Trace", ConsoleOutputColor.DarkCyan, ConsoleOutputColor.NoChange));
        loggingConfig.AddTarget("console", consoleTarget);
        loggingConfig.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));


        LogManager.Configuration = loggingConfig;
    }
}