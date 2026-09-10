using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using CustomDataManager.UI;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Torch;
using Torch.API;
using Torch.API.ModAPI;
using Torch.API.Plugins;

namespace CustomDataManager;

public class Plugin : TorchPluginBase, IWpfPlugin
{
    private static readonly Logger Logger = LogManager.GetLogger("CustomDataManager");
    private const string ConfigFileName = "CustomDataManager.cfg";
    private Persistent<Settings>? _config;
    private Settings? Settings => _config?.Data;
    private MainViewModel? _mainViewModel;

    public override void Init(ITorchBase torch)
    {
        base.Init(torch);

        var configPath = Path.Combine(StoragePath, ConfigFileName);
        _config = Persistent<Settings>.Load(configPath);

        if (Settings == null)
        {
            Logger.Error("I have no idea what I am doing... Failed to load settings I guess?");
            return;
        }

        _mainViewModel = new(Settings, new AsyncCommand(ExportData, HandleErrors),
            new AsyncCommand(ImportData, HandleErrors));
    }

    private async Task ExportData()
    {
        if (Settings is null || _mainViewModel is null)
        {
            return;
        }

        Logger.Info("Exporting data...");

        Directory.CreateDirectory(Settings.TargetDir);

        var dataToExport = await Torch.InvokeAsync(()
            => MyEntities.GetEntities().OfType<MyCubeGrid>()
                .Where(e => e is { Closed: false, Physics: not null })
                .SelectMany(e => e.GetFatBlocks().OfType<IMyFunctionalBlock>().Where(MatchesFilter))
                .Select(b => new { Name = b.CustomName, b.CustomData })
                .ToList()
        ).ConfigureAwait(false);

        Regex? lineFilter = null;
        if (!string.IsNullOrWhiteSpace(Settings.LineFilter))
        {
            lineFilter = new(Settings.LineFilter, RegexOptions.Compiled);
        }

        foreach (var item in dataToExport)
        {
            if (!TryGetFileName(item.Name, out var fileName)) continue;

            var fullPath = Path.GetFullPath(Path.Combine(Settings!.TargetDir, fileName));
            var root = Path.GetFullPath(Settings.TargetDir);

            if (!fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                Logger.Error("Invalid block name for export (hacking attempt?): <{0}>", item.Name);
                continue;
            }

            var customData = string.Join("\n", item.CustomData.Split('\n').Where(r => !lineFilter?.IsMatch(r) ?? true));

            using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write,
                FileShare.None, bufferSize: 4096, useAsync: true);
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(customData).ConfigureAwait(false);

            Logger.Info("Created/Updated {0}", fullPath);
        }

        Logger.Info("Export complete!");
    }

    private async Task ImportData()
    {
        if (Settings is null || _mainViewModel is null)
        {
            return;
        }

        Logger.Info("Importing data...");

        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var f in Directory.EnumerateFiles(Settings.TargetDir))
        {
            using var stream = new FileStream(f, FileMode.Open, FileAccess.Read,
                FileShare.Read, bufferSize: 4096, useAsync: true);
            using var reader = new StreamReader(stream);
            files[Path.GetFileName(f)] = await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        await Torch.InvokeAsync(() =>
        {
            var blocks = MyEntities.GetEntities().OfType<MyCubeGrid>()
                .Where(e => e is { Closed: false, Physics: not null })
                .SelectMany(e => e.GetFatBlocks().OfType<IMyFunctionalBlock>().Where(MatchesFilter));

            foreach (var block in blocks)
            {
                if (!TryGetFileName(block.CustomName, out var name))
                {
                    continue;
                }

                if (!files.TryGetValue(name, out var content))
                {
                    continue;
                }

                if (block.CustomData != content)
                {
                    block.CustomData = content;
                    if (Settings.ForceRestartBlockAfter)
                    {
                        block.Enabled = false;
                        block.Enabled = true;
                    }
                    Logger.Info("Updated data for {0} : {1} ", block.CubeGrid.CustomName, block.CustomName);
                }
                else
                {
                    Logger.Info("Data unchanged for {0} : {1} ", block.CubeGrid.CustomName, block.CustomName);
                }
            }
        }).ConfigureAwait(false);

        Logger.Info("Import complete!");
    }

    private bool TryGetFileName(string blockName, out string fileName)
    {
        var safeName = string.Concat(blockName.Split(Path.GetInvalidFileNameChars()));
        if (string.IsNullOrWhiteSpace(safeName))
        {
            Logger.Error("Invalid block name for export: <{0}>", blockName);
            fileName = null!;
            return false;
        }

        fileName = safeName;
        return true;
    }

    private bool MatchesFilter(IMyFunctionalBlock b)
    {
        if (!string.IsNullOrEmpty(Settings?.SubtypeIdFilter))
        {
            if (b.BlockDefinition.SubtypeName != Settings!.SubtypeIdFilter)
            {
                return false;
            }
        }

        if (!string.IsNullOrEmpty(Settings?.TypeIdFilter))
        {
            if (b.BlockDefinition.TypeIdString != Settings!.TypeIdFilter)
            {
                return false;
            }
        }

        return true;
    }

    private void HandleErrors(Task t)
    {
        if (t.Exception != null)
        {
            Logger.Error(t.Exception, "Error during command execution!");
        }
    }

    public UserControl GetControl()
    {
        if (_mainViewModel is null)
        {
            Logger.Error("Failed to initialize _mainViewModel");
            return new() { Content = "Something went wrong, check the logs" };
        }

        return new MainView()
        {
            DataContext = _mainViewModel
        };
    }
}