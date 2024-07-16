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
using System.Reflection;



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
    ConfigWindow.OnConfigChanged += UpdateAngleAction;
  }

  public void Dispose()
  {
    Framework.Update -= OnFrameworkTick;
    ConfigWindow.OnConfigChanged -= UpdateAngleAction;
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
    if (ClientState.LocalPlayer == null || !CheckAllowCameraTilt()) return;

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
    UpdateAngle();
  }

  private void UpdateAngle()
  {
    ConfigWindow.SetRawAngle(LastHeight);

    // TODO: set a proper eased curve (slerp instead of lerp?) for angle
    // TODO: save separate slider positions for each curve type

    float limitMin = Configuration.TiltMin * LIMIT_RANGE + LIMIT_MIN;
    float limitMax = Configuration.TiltMax * LIMIT_RANGE + LIMIT_MIN;

    float range = Configuration.PitchTop - Configuration.PitchBottom;
    float tilt = 1 - (float)(Math.Acos(LastHeight) / Math.PI);
    ConfigWindow.SetCleanAngle(tilt);

    tilt = (tilt - Configuration.PitchBottom) / range;
    if (Configuration.Curve == Configuration.CurveOptions.Squared)
    {
      tilt = Math.Max(1 - tilt, 0);
      tilt = 1 - tilt * tilt;
    }

    tilt = Math.Clamp(tilt, 0, 1);
    tilt = (1 - tilt) * (limitMax - limitMin) + limitMin;
    ConfigWindow.SetMappedTilt(tilt);

    GameConfig.Set(UiControlOption.TiltOffset, tilt);
  }

  private void UpdateAngleAction()
  {
    if (!CheckAllowCameraTilt()) return;
    UpdateAngle();
  }

  private bool CheckAllowCameraTilt()
  {
    // TODO: skip this during cutscenes, first person
    if (!Configuration.GlobalEnable || ClientState.IsGPosing)
    {
      return false;
    }
    return true;
  }
}