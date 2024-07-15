﻿using Dalamud.Game.Command;
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

    private const string CommandName = "/ToggleCamTilt";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("CamTilt");
    private ConfigWindow ConfigWindow { get; init; }
    private CamController camController { get; init; }

    public Plugin(
            IDalamudPluginInterface pi,
            IClientState clientState,
            IFramework framework,
            IGameConfig gameConfig)
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ConfigWindow = new ConfigWindow(this);

        ModuleLog logger = new ModuleLog("CamTilt");

        camController = new CamController(Configuration, framework, clientState, gameConfig, logger);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnToggleCommand)
        {
            HelpMessage = "Globally toggle Cam Tilt plugin"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        camController.Dispose();

        CommandManager.RemoveHandler(CommandName);

        PluginInterface.RemoveChatLinkHandler();
    }

    private void OnToggleCommand(string command, string args)
    {
        // expose command to enable/disable, for macros and such
        Configuration.GlobalEnable = !Configuration.GlobalEnable;
        Configuration.Save();

    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
