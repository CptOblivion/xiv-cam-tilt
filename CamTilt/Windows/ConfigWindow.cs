using System;
using Dalamud.Interface.Windowing;

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
        Flags = Dalamud.Bindings.ImGui.ImGuiWindowFlags.AlwaysAutoResize |
          Dalamud.Bindings.ImGui.ImGuiWindowFlags.NoResize |
          Dalamud.Bindings.ImGui.ImGuiWindowFlags.NoCollapse |
          Dalamud.Bindings.ImGui.ImGuiWindowFlags.NoScrollbar |
          Dalamud.Bindings.ImGui.ImGuiWindowFlags.NoScrollWithMouse;
        SizeCondition = Dalamud.Bindings.ImGui.ImGuiCond.Always; // TODO: look up what this does, remove if unnecessary
        Configuration = plugin.Configuration;
    }

    public void Dispose() { } // TODO: might need to discard the window or something

    public override void Draw()
    {
        DrawCheckbox("Enable Globally", () => Configuration.GlobalEnable, x => Configuration.GlobalEnable = x);

        Dalamud.Bindings.ImGui.ImGuiP.Separator();
        DrawTiltValues(Configuration.ValuesOnFoot, "ValsOnFoot");

        if (Dalamud.Bindings.ImGui.ImGuiP.CollapsingHeader("Mounted"))
        {
            DrawCheckbox("Use separate values for mounted (on ground)", () => Configuration.SeparateValuesMounted, x => Configuration.SeparateValuesMounted = x);
            DrawTiltValues(Configuration.ValuesMounted, "ValsMounted");
        }
        if (Dalamud.Bindings.ImGui.ImGuiP.CollapsingHeader("Flying"))
        {
            DrawCheckbox("Use separate values for flying", () => Configuration.SeparateValuesFlying, x => Configuration.SeparateValuesFlying = x);
            DrawTiltValues(Configuration.ValuesFlying, "ValsFlying");
        }

        Dalamud.Bindings.ImGui.ImGuiP.Separator();
        DrawDebugValues();
    }

    private void DrawCheckbox(string label, Func<bool> getter, Action<bool> setter)
    {
        var val = getter();
        if (Dalamud.Bindings.ImGui.ImGuiP.Checkbox(label, ref val))
        {
            setter(val);
            Configuration.Save();
            OnConfigChanged?.Invoke();
        }
      ;
    }

    private void DrawSlider(string label, Func<float> getter, Action<float> setter, float min, float max, Dalamud.Bindings.ImGui.ImGuiSliderFlags flags = Dalamud.Bindings.ImGui.ImGuiSliderFlags.None, string Id = "")
    {
        var val = getter();
        if (Id != "") label += $"##{Id}";
        if (Dalamud.Bindings.ImGui.ImGuiP.SliderFloat(label, ref val, min, max, null, flags))
        {
            setter(val);
            Configuration.Save();
            OnConfigChanged?.Invoke();
        }
      ;
    }

    private void DrawTiltValues(TiltValues tiltValues, string Id)
    {
        DrawSlider("Height offset", () => tiltValues.HeightOffset, x => tiltValues.HeightOffset = x, 0, 2);

        Dalamud.Bindings.ImGui.ImGuiP.LabelText("", "");
        Dalamud.Bindings.ImGui.ImGuiP.SameLine(tiltValues.PitchLookingUp * BAR_SIZE + 8);
        Dalamud.Bindings.ImGui.ImGuiP.PushItemWidth((1 - tiltValues.PitchLookingUp) * BAR_SIZE);
        DrawSlider("Limit Looking Down", () => tiltValues.PitchLookingDown, x => tiltValues.PitchLookingDown = x, tiltValues.PitchLookingUp + .01f, 1, Id: Id);
        Dalamud.Bindings.ImGui.ImGuiP.PopItemWidth();
        Dalamud.Bindings.ImGui.ImGuiP.PushItemWidth(tiltValues.PitchLookingDown * BAR_SIZE);
        DrawSlider("Limit Looking Up", () => tiltValues.PitchLookingUp, x => tiltValues.PitchLookingUp = x, 0, tiltValues.PitchLookingDown - .01f, Id: Id);
        Dalamud.Bindings.ImGui.ImGuiP.PopItemWidth();

        Dalamud.Bindings.ImGui.ImGuiP.Spacing();
        DrawSlider("Tilt While Looking Down", () => tiltValues.TiltMin * 100, x => tiltValues.TiltMin = x / 100, 0, 100, Id: Id);
        DrawSlider("Tilt While Looking Up", () => tiltValues.TiltMax * 100, x => tiltValues.TiltMax = x / 100, 0, 100, Id: Id);
        DrawSlider("Interpolation Curve", () => tiltValues.CurveExponent, x => tiltValues.CurveExponent = x, .5f, 3f, flags: Dalamud.Bindings.ImGui.ImGuiSliderFlags.Logarithmic, Id: Id);
    }

    private void DrawDebugValues()
    {
        if (Dalamud.Bindings.ImGui.ImGuiP.CollapsingHeader("Draw debug stuff"))
        {
            Dalamud.Bindings.ImGui.ImGuiP.Separator();
            Dalamud.Bindings.ImGui.ImGuiP.SliderFloat("Y offset", ref rawAngle, -1, 1, null, Dalamud.Bindings.ImGui.ImGuiSliderFlags.NoInput);
            Dalamud.Bindings.ImGui.ImGuiP.SliderFloat("Pitch (0-1)", ref cleanAngle, 0, 1, null, Dalamud.Bindings.ImGui.ImGuiSliderFlags.NoInput);
            Dalamud.Bindings.ImGui.ImGuiP.SliderFloat("mapped tilt", ref mappedTilt, -.08f, .21f, null, Dalamud.Bindings.ImGui.ImGuiSliderFlags.NoInput);
            Dalamud.Bindings.ImGui.ImGuiP.Separator();
        }
    }
}
