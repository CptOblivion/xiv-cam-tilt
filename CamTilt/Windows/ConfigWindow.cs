using System;
using System.Numerics;
using System.Reflection.Metadata;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace CamTilt.Windows;

public class ConfigWindow : Window, IDisposable
{
    private const float BAR_SIZE = 256;
    private Configuration Configuration { get; init; }
    private float rawAngle;
    public void SetRawAngle(float angle) => rawAngle = angle;
    private float cleanAngle;
    public void SetCleanAngle(float angle) => cleanAngle = angle;
    private float mappedTilt;
    public void SetMappedTilt(float angle) => mappedTilt = angle;
    public delegate void ConfigChangedHandler();
    public event ConfigChangedHandler OnConfigChanged = delegate { };
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
        DrawCheckbox("Enable Globally", () => Configuration.GlobalEnable, x => Configuration.GlobalEnable = x);

        ImGui.Separator();
        ImGui.BeginGroup();
        ImGui.LabelText("Mapping Curve", "");
        DrawRadioButton("linear", () => (int)Configuration.Curve, x => Configuration.Curve = (Configuration.CurveOptions)x, 0);
        // DrawRadioButton("divided", () => (int)Configuration.Curve, x => Configuration.Curve = (Configuration.CurveOptions)x, 1);
        DrawRadioButton("squared", () => (int)Configuration.Curve, x => Configuration.Curve = (Configuration.CurveOptions)x, 2);
        ImGui.EndGroup();

        ImGui.Spacing();
        DrawSlider("Player height offset", () => Configuration.PlayerHeightOffset, x => Configuration.PlayerHeightOffset = x, 0, 2);

        ImGui.LabelText("", "");
        ImGui.SameLine(Configuration.PitchBottom * BAR_SIZE + 8);
        ImGui.PushItemWidth((1 - Configuration.PitchBottom) * BAR_SIZE);
        DrawSlider("Pitch Top", () => Configuration.PitchTop, x => Configuration.PitchTop = x, Configuration.PitchBottom + .01f, 1);
        ImGui.PopItemWidth();
        ImGui.PushItemWidth(Configuration.PitchTop * BAR_SIZE);
        DrawSlider("Pitch Bottom", () => Configuration.PitchBottom, x => Configuration.PitchBottom = x, 0, Configuration.PitchTop - .01f);
        ImGui.PopItemWidth();

        ImGui.Spacing();

        ImGui.LabelText("", "");
        ImGui.SameLine(Configuration.TiltMin * BAR_SIZE + 8);
        ImGui.PushItemWidth((1 - Configuration.TiltMin) * BAR_SIZE);
        DrawSlider("Tilt Max", () => Configuration.TiltMax * 100, x => Configuration.TiltMax = x / 100, Configuration.TiltMin * 100, 100);
        ImGui.PopItemWidth();
        ImGui.PushItemWidth(Configuration.TiltMax * BAR_SIZE);
        DrawSlider("Tilt Min", () => Configuration.TiltMin * 100, x => Configuration.TiltMin = x / 100, 0, Configuration.TiltMax * 100);
        ImGui.PopItemWidth();


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
            OnConfigChanged?.Invoke();
        };
    }
    private void DrawSlider(string label, Func<float> getter, Action<float> setter, float min, float max)
    {
        var val = getter();
        if (ImGui.SliderFloat(label, ref val, min, max))
        {
            setter(val);
            Configuration.Save();
            OnConfigChanged?.Invoke();
        };
    }

    private void DrawRadioButton(string label, Func<int> getter, Action<int> setter, int index)
    {
        var val = getter();
        if (ImGui.RadioButton(label, ref val, index))
        {
            setter(val);
            Configuration.Save();
            OnConfigChanged?.Invoke();
        };
    }
}
