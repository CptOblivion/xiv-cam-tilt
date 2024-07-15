using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
// using FFXIVClientStructs.FFXIV.Client.Game;
// using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Common.Math;
using Dalamud.Game.Config;
using Dalamud.Logging.Internal;
using CamTilt.Windows;



namespace CamTilt;

public class CamController : IDisposable
{
  private Configuration Configuration { get; init; }
  private ModuleLog Logger { get; init; }
  private float LastHeight { get; set; }
  private IFramework Framework { get; init; }
  private IClientState ClientState { get; init; }
  private IGameConfig GameConfig { get; init; }
  private ConfigWindow ConfigWindow { get; init; }

  private const float LIMIT_MIN = -.08f;
  private const float LIMIT_MAX = .21f;
  private const float LIMIT_RANGE = LIMIT_MAX - LIMIT_MIN;

  public CamController(Configuration configuration,
    IFramework framework,
    IClientState clientState,
    IGameConfig gameConfig,
    ModuleLog logger,
    ConfigWindow configWindow)
  {
    Configuration = configuration;
    Framework = framework;
    ClientState = clientState;
    Logger = logger;
    ConfigWindow = configWindow;
    GameConfig = gameConfig;

    // TODO: should only add when in-game (maybe wait until current player is valid?)
    Framework.Update += OnFrameworkTick;
  }

  /*
    Intention: get the pitch of the player look direction (moving mouse or thumbstick to aim up and down),
    apply secondary rotation to camera along its own pitch axis accordingly

    hacky option (since I can't find a built-in way to get the vertical look):
    get normalized vector from player to camera
    set "3rd person camera angle" setting based on height
    this behaves quite poorly when the camera collides with the floor and changes distance
  */
  private void OnFrameworkTick(IFramework framework)
  {

    if (!Configuration.GlobalEnable || ClientState.IsGPosing || ClientState.LocalPlayer == null)
    {
      return;
    }

    Vector3 camPos;
    unsafe
    {
      CameraManager* manager = CameraManager.Instance();
      if (manager == null)
      {
        return;
      }

      Camera* cam = manager->CurrentCamera;
      if (cam == null)
      {
        return;
      }
      camPos = cam->Position;
    }

    // TODO: skip this during cutscenes, first person

    IPlayerCharacter localPlayer = ClientState.LocalPlayer;

    Vector3 playerPos = localPlayer.Position;
    playerPos.Y += Configuration.PlayerHeightOffset;
    Vector3 vec = camPos - playerPos;
    vec = vec.Normalized;

    if (vec.Y == LastHeight)
    {
      return;
    }

    LastHeight = vec.Y;
    ConfigWindow.SetRawAngle(vec.Y);

    // TODO: set a proper eased curve (slerp instead of lerp?) for angle

    float range = Configuration.PitchMax - Configuration.PitchMin;
    float converted = 1 - (float)(Math.Acos(vec.Y) / Math.PI);
    ConfigWindow.SetCleanAngle(converted);

    converted = Math.Clamp((converted - Configuration.PitchMin) / range, 0, 1);
    converted = (1 - converted) * LIMIT_RANGE + LIMIT_MIN; // scale and fit to final range
    ConfigWindow.SetMappedAngle(converted);
    // Logger.Verbose($"raw {rangeFit} | converted {converted}");

    GameConfig.Set(UiControlOption.TiltOffset, converted);
  }

  public void Dispose()
  {
    Framework.Update -= OnFrameworkTick;
  }
}