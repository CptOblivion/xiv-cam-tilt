using Dalamud.Configuration;
using Dalamud.Plugin.Services;
using System;

namespace CamTilt;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool GlobalEnable { get; set; } = true;
    public float PlayerHeightOffset { get; set; } = 1.4f;
    public float PitchTop { get; set; } = .97f;
    public float PitchBottom { get; set; } = .25f;
    public float TiltMax { get; set; } = 0;
    public float TiltMin { get; set; } = 1;
    public bool ShowDebug { get; set; } = false;
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
