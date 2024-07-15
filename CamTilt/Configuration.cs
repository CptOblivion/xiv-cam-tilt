using Dalamud.Configuration;
using Dalamud.Plugin.Services;
using System;

namespace CamTilt;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool GlobalEnable { get; set; } = true;
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
