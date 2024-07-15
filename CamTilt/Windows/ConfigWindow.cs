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

    private float rawAngle;
    public void SetRawAngle(float angle) => rawAngle = angle;
    private float cleanAngle;
    public void SetCleanAngle(float angle) => cleanAngle = angle;
    private float mappedTilt;
    public void SetMappedTilt(float angle) => mappedTilt = angle;

    public void Dispose() { }

    public override void Draw()
    {
        DrawCheckbox("Enable Globally", () => Configuration.GlobalEnable, x => Configuration.GlobalEnable = x);

        ImGui.Separator();
        DrawSlider("Player height offset", () => Configuration.PlayerHeightOffset, x => Configuration.PlayerHeightOffset = x, 0, 2);
        DrawSlider("Pitch Min", () => Configuration.PitchMin, x => Configuration.PitchMin = x, 0, Configuration.PitchMax - .01f);
        DrawSlider("Pitch Max", () => Configuration.PitchMax, x => Configuration.PitchMax = x, Configuration.PitchMin + .01f, 1);

        ImGui.Separator();
        DrawCheckbox("Show debug stuff", () => Configuration.ShowDebug, x => Configuration.ShowDebug = x);
        if (Configuration.ShowDebug) DrawDebug();
    }

    private void DrawDebug()
    {
        ImGui.Separator();
        ImGui.SliderFloat("Y offset", ref rawAngle, -1, 1, null, ImGuiSliderFlags.NoInput);
        ImGui.SliderFloat("Pitch (0-1)", ref cleanAngle, 0, 1, null, ImGuiSliderFlags.NoInput);
        ImGui.SliderFloat("mapped tilt", ref mappedTilt, -.08f, .21f, null, ImGuiSliderFlags.NoInput);
        ImGui.Separator();
    }

    private void DrawCheckbox(string label, Func<bool> getter, Action<bool> setter)
    {
        var val = getter();
        if (ImGui.Checkbox(label, ref val))
        {
            setter(val);
            Configuration.Save();
        };
    }
    private void DrawSlider(string label, Func<float> getter, Action<float> setter, float min, float max)
    {
        var val = getter();
        if (ImGui.SliderFloat(label, ref val, min, max))
        {
            setter(val);
            Configuration.Save();
        };
    }
}
