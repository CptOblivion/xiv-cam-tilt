using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using Dalamud.Game.Config;
using Dalamud.Logging.Internal;
using CamTilt.Windows;
using Dalamud.Game.ClientState.Conditions;



namespace CamTilt;

public class CamController : IDisposable
{
  private Configuration Configuration { get; init; }
  private ModuleLog Logger { get; init; }
  private float LastHeight { get; set; }
  private IFramework Framework { get; init; }
  private IClientState ClientState { get; init; }
  private IGameConfig GameConfig { get; init; }
  private ICondition Condition { get; init; }
  private ConfigWindow ConfigWindow { get; init; }

  private const float LIMIT_MIN = -.08f;
  private const float LIMIT_MAX = .21f;
  private const float LIMIT_RANGE = LIMIT_MAX - LIMIT_MIN;

  public CamController(Configuration configuration,
    IFramework framework,
    IClientState clientState,
    IGameConfig gameConfig,
    ICondition condition,
    ModuleLog logger,
    ConfigWindow configWindow)
  {
    Configuration = configuration;
    Framework = framework;
    ClientState = clientState;
    Condition = condition;
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

    TiltValues tiltValues = getTiltValues();
    playerPos.Y += tiltValues.HeightOffset; // TODO: replace this with the value that is surely stored somewhere in the game
    Vector3 vec = camPos - playerPos;
    vec = vec.Normalized;

    if (vec.Y == LastHeight)
    {
      return;
    }

    LastHeight = vec.Y;
    UpdateAngle(tiltValues);
  }

  private void UpdateAngle(TiltValues tiltValues)
  {
    ConfigWindow.SetRawAngle(LastHeight);

    float limitMin = tiltValues.TiltMin * LIMIT_RANGE + LIMIT_MIN;
    float limitMax = tiltValues.TiltMax * LIMIT_RANGE + LIMIT_MIN;

    float range = tiltValues.PitchLookingDown - tiltValues.PitchLookingUp;
    float tilt = 1 - (float)(Math.Acos(LastHeight) / Math.PI);
    ConfigWindow.SetCleanAngle(tilt);

    tilt = (tilt - tiltValues.PitchLookingUp) / range;
    tilt = 1 - (float)Math.Pow(Math.Max(1 - tilt, 0), tiltValues.CurveExponent);

    tilt = Math.Clamp(tilt, 0, 1);
    tilt = (1 - tilt) * (limitMax - limitMin) + limitMin;
    ConfigWindow.SetMappedTilt(tilt);

    GameConfig.Set(UiControlOption.TiltOffset, tilt);
  }

  private void UpdateAngleAction()
  {
    if (!CheckAllowCameraTilt()) return;
    UpdateAngle(getTiltValues());
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

  private TiltValues getTiltValues()
  {
    if (Configuration.SeparateValuesFlying && Condition[ConditionFlag.InFlight])
    {
      return Configuration.ValuesFlying;
    }
    if (Configuration.SeparateValuesMounted && (Condition[ConditionFlag.Mounted] || Condition[ConditionFlag.RidingPillion]))
    {
      return Configuration.ValuesMounted;
    }
    return Configuration.ValuesOnFoot;
  }
}