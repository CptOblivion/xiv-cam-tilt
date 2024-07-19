using Dalamud.Configuration;
using Dalamud.Plugin.Services;
using System;

namespace CamTilt;

[Serializable]
public class Configuration : IPluginConfiguration
{
  public int Version { get; set; } = 0;
  public bool GlobalEnable { get; set; } = true;
  public TiltValues ValuesOnFoot = new TiltValues();
  public bool SeparateValuesMounted = false;
  public TiltValues ValuesMounted = new TiltValues();
  public bool SeparateValuesFlying = true;
  public TiltValues ValuesFlying = new TiltValues
  {
    CurveExponent = 1,
    PitchLookingDown = 1,
    PitchLookingUp = 0,
    TiltMin = 0,
    TiltMax = 0
  };
  public void Save()
  {
    Plugin.PluginInterface.SavePluginConfig(this);
  }
}

[Serializable]
public class TiltValues
{
  public float CurveExponent { get; set; } = 2.2f;
  public float HeightOffset { get; set; } = 1.4f;
  public float PitchLookingDown { get; set; } = 0.75f;
  public float PitchLookingUp { get; set; } = 0.4f;
  public float TiltMin { get; set; }
  public float TiltMax { get; set; }
}
