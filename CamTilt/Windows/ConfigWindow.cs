using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace CamTilt.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration { get; init; }

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Cam Tilt Config###CamTilt Config")
    {
        Flags = ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoScrollWithMouse;
        // Size = new Vector2(232, 90);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        var globalEnabled = Configuration.GlobalEnable;
        if (ImGui.Checkbox("Enable Globally", ref globalEnabled))
        {
            Configuration.GlobalEnable = globalEnabled;
            Configuration.Save();
        };

        ImGui.Separator();

        var playerHeightOffset = Configuration.PlayerHeightOffset;
        if (ImGui.SliderFloat("Player height offset", ref playerHeightOffset, 0, 2))
        {
            Configuration.PlayerHeightOffset = playerHeightOffset;
            Configuration.Save();
        }

        var pitchMin = Configuration.PitchMin;
        if (ImGui.SliderFloat("Pitch Min", ref pitchMin, 0, Configuration.PitchMax - .01f))
        {
            Configuration.PitchMin = pitchMin;
            Configuration.Save();
        }

        var pitchMax = Configuration.PitchMax;
        if (ImGui.SliderFloat("Pitch Max", ref pitchMax, Configuration.PitchMin + .01f, 1))
        {
            Configuration.PitchMax = pitchMax;
            Configuration.Save();
        }
    }
}
