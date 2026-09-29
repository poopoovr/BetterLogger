using System;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;

namespace BetterLogger;

[BepInPlugin("com.poopoovr.betterlogger", "BetterLogger", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static string[] Whitelist = { "com.poopoovr.betterlogger" };
    public static ILogListener OriginalConsole;

    private void Awake()
    {
        var consoleListeners = BepInEx.Logging.Logger.Listeners.Where(l => l.GetType().Name.Contains("Console")).ToList();
        foreach (var listener in consoleListeners)
        {
            OriginalConsole = listener;
            BepInEx.Logging.Logger.Listeners.Remove(listener);
        }

        BepInEx.Logging.Logger.Listeners.Add(new Filter());
        Console.Clear();

        var logger = BepInEx.Logging.Logger.CreateLogSource("BetterLogger");
        var pluginNames = Chainloader.PluginInfos.Values.Select(p => $"Loading [{p.Metadata.Name} {p.Metadata.Version}]").ToList();
        int count = pluginNames.Count;

        new System.Threading.Thread(() =>
        {
            System.Threading.Thread.Sleep(2000);
            logger.LogMessage("Chainloader started");
            logger.LogMessage($"{count} plugins to load");
            foreach (var name in pluginNames)
            {
                logger.LogMessage(name);
            }
        }).Start();
    }
}

public class Filter : ILogListener
{
    public void LogEvent(object sender, LogEventArgs args)
    {
        string source = args.Source.SourceName;

        if (source == "Unity" || source == "Unity Log") return;
        if (source == "BetterLogger") { Write(args); return; }
        if (Plugin.Whitelist.Contains(source)) { Write(args); return; }
        if (args.Level == LogLevel.Info) return;

        Write(args);
    }

    private void Write(LogEventArgs args)
    {
        Plugin.OriginalConsole?.LogEvent(this, args);
    }

    public void Dispose() {}
}
