using Dalamud.Configuration;
using Dalamud.Plugin.Services;
using System;

namespace CamTilt;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool GlobalEnable { get; set; } = true;
    public float PlayerHeightOffset { get; set; } = 1;
    public float PitchMin { get; set; } = 0;
    public float PitchMax { get; set; } = .5f;
    // TODO: converted offset, converted scale into settings
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
