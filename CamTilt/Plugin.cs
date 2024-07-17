using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using CamTilt.Windows;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Lumina;
using Dalamud.Logging.Internal;

namespace CamTilt;

public sealed class Plugin : IDalamudPlugin
{
  [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
  [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

  private const string ToggleCommand = "/CamTiltToggle";
  private const string ToggleSettings = "/CamTiltSettings";

  public Configuration Configuration { get; init; }

  public readonly WindowSystem WindowSystem = new("CamTilt");
  private ConfigWindow ConfigWindow { get; init; }
  private CamController camController { get; init; }

  public Plugin(
          IDalamudPluginInterface pi,
          IClientState clientState,
          IFramework framework,
          IGameConfig gameConfig,
          ICondition condition)
  {
    Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
    ConfigWindow = new ConfigWindow(this);

    ModuleLog logger = new ModuleLog("CamTilt");

    camController = new CamController(Configuration, framework, clientState, gameConfig, condition, logger, ConfigWindow);

    WindowSystem.AddWindow(ConfigWindow);

    CommandManager.AddHandler(ToggleCommand, new CommandInfo(OnToggleCommand)
    {
      HelpMessage = "Globally toggle Cam Tilt plugin"
    });

    CommandManager.AddHandler(ToggleSettings, new CommandInfo(OnSettingsCommand)
    {
      HelpMessage = "Toggles Cam Tilt settings"
    });

    PluginInterface.UiBuilder.Draw += DrawUI;

    PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
  }

  public void Dispose()
  {
    WindowSystem.RemoveAllWindows();
    camController.Dispose();
    ConfigWindow.Dispose();
    CommandManager.RemoveHandler(ToggleCommand);
  }

  private void OnToggleCommand(string command, string args)
  {
    // expose command to enable/disable, for macros and such
    Configuration.GlobalEnable = !Configuration.GlobalEnable;
    Configuration.Save();
  }

  private void OnSettingsCommand(string command, string args)
  {
    ConfigWindow.Toggle();
  }

  private void DrawUI() => WindowSystem.Draw();

  public void ToggleConfigUI() => ConfigWindow.Toggle();
}
