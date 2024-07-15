using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using Dalamud.Game.Config;
using Dalamud.Logging.Internal;



namespace CamTilt;

public unsafe class CamController : IDisposable
{
  private Configuration Configuration { get; init; }
  private ModuleLog Logger { get; init; }
  private CameraManager* CurrentCameraManager { get; set; }
  private Camera* CurrentCamera { get; set; }
  // private IPlayerCharacter PlayerCharacter { get; set; }
  private float LastHeight { get; set; }
  private IFramework Framework { get; init; }
  private IClientState ClientState { get; init; }
  private IGameConfig GameConfig { get; init; }

  private const float LIMIT_MIN = -.08f;
  private const float LIMIT_MAX = .21f;
  private const float LIMIT_RANGE = LIMIT_MAX - LIMIT_MIN;

  public unsafe CamController(Configuration configuration, IFramework framework, IClientState clientState, IGameConfig gameConfig, ModuleLog logger)
  {
    Configuration = configuration;
    Framework = framework;
    ClientState = clientState;
    Logger = logger;
    GameConfig = gameConfig;

    CurrentCameraManager = CameraManager.Instance();
    if (CurrentCameraManager == null)
    {
      logger.Error("no camera manager found!");
      return;
    }

    // TODO: should only add when in-game (maybe wait until current player is valid?)
    Framework.Update += OnFrameworkTick;
  }

  private void OnFrameworkTick(IFramework framework)
  {
    // Intention: get the pitch of the player look direction (moving mouse or thumbstick to aim up and down),
    // apply secondary rotation to camera along its own pitch axis accordingly

    // hacky option (since I can't find a built-in way to get the vertical look):
    // get normalized vector from player to camera
    // set "3rd person camera angle" setting based on height
    // ref Tilted plugin for assigning values to that setting

    if (!Configuration.GlobalEnable || ClientState.IsGPosing || ClientState.LocalPlayer == null)
    {
      return;
    }
    // TODO: skip this during cutscenes, first person

    IPlayerCharacter localPlayer = ClientState.LocalPlayer;

    if (CameraManager.Instance() != CurrentCameraManager)
    {
      CurrentCameraManager = CameraManager.Instance();
      Logger.Warning("Camera manager changed??");
    }

    if (CurrentCameraManager->CurrentCamera != CurrentCamera)
    {
      CurrentCamera = CurrentCameraManager->CurrentCamera;
      Logger.Warning("Camera changed");
    }

    Vector3 playerPos = localPlayer.Position;
    playerPos.Y += Configuration.PlayerHeightOffset;
    Vector3 vec = CurrentCamera->Position - playerPos;
    vec = vec.Normalized;

    if (vec.Y == LastHeight)
    {
      return;
    }

    LastHeight = vec.Y;

    // TODO: set a proper curve for converted instead of just a clamp-scale-and-shift

    float range = Configuration.PitchMax - Configuration.PitchMin;
    float rangeFit = (vec.Y + 1) * 0.5f;
    float converted = Math.Clamp((rangeFit - Configuration.PitchMin) / range, 0, 1);
    converted = (1 - converted) * LIMIT_RANGE + LIMIT_MIN; // scale and fit to final range
    Logger.Verbose($"raw {rangeFit} | converted {converted}");

    GameConfig.Set(UiControlOption.TiltOffset, converted);
  }

  public void Dispose()
  {
    Framework.Update -= OnFrameworkTick;
  }
}