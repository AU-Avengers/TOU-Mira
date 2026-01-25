using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches.PrefabChanging;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Buttons.Crewmate;
[MiraIgnore]
public abstract class SentryPortableCameraButtonBase : TownOfUsRoleButton<SentryRole>
{
    private static float _availableCharge;
    private static bool _batteryInitialized;
    private static Minigame? _securityMinigame;
    private static bool _reportedInUse;
    private static int _lastUpdateFrame = -1;

    public override string Name => TouLocale.GetParsed("TouRoleSentryPortableCamera", "View");
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override Color TextOutlineColor => TownOfUsColors.Sentry;
    public override float Cooldown => 0.001f;
    public override LoadableAsset<Sprite> Sprite => TouAssets.CameraSprite;

    protected static bool AllCamerasPlaced()
    {
        try
        {
            var placeBtn = CustomButtonSingleton<SentryPlaceCameraButton>.Instance;
            if (placeBtn != null)
            {
                if (placeBtn.PlacementInProgress)
                {
                    return false;
                }

                if (!placeBtn.LimitedUses)
                {
                    return false;
                }

                return placeBtn.UsesLeft <= 0;
            }
        }
        catch
        {
            // ignored
        }

        var options = OptionGroupSingleton<SentryOptions>.Instance;
        var initial = (int)options.InitialCameras.Value;
        if (initial == 0) return false;
        return SentryRole.Cameras.Count >= initial;
    }

    protected abstract bool ShouldBeVisible(SentryRole role);

    public override bool Enabled(RoleBehaviour? role)
    {
        if (role is not SentryRole sentryRole || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data.IsDead)
        {
            return false;
        }

        var options = OptionGroupSingleton<SentryOptions>.Instance;
        var mapId = MiscUtils.GetCurrentMap;
        var mapWithoutCameras = SentryCameraUtilities.IsMapWithoutCameras(mapId);

        var unlocked = options.PortableCamerasMode switch
        {
            SentryPortableCamerasMode.Immediately => true,
            SentryPortableCamerasMode.AfterTasks => sentryRole.CompletedAllTasks,
            SentryPortableCamerasMode.OnMapsWithoutCameras => mapWithoutCameras || sentryRole.CompletedAllTasks,
            _ => sentryRole.CompletedAllTasks
        };

        if (!unlocked)
        {
            return false;
        }

        return ShouldBeVisible(sentryRole);
    }

    public override void ClickHandler()
    {
        if (!CanClick() || PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() ||
            PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities) || !MiscUtils.CanUseUtility(GameUtility.Cams, true))
        {
            return;
        }

        if (LimitedUses)
        {
            UsesLeft--;
            Button?.SetUsesRemaining(UsesLeft);
            TownOfUsColors.UseBasic = false;
            if (TextOutlineColor != Color.clear)
            {
                SetTextOutline(TextOutlineColor);
                if (Button != null)
                {
                    Button.usesRemainingSprite.color = TextOutlineColor;
                }
            }

            TownOfUsColors.UseBasic = LocalSettingsTabSingleton<TownOfUsLocalRoleSettings>.Instance
                .UseCrewmateTeamColorToggle.Value;
        }

        OnClick();

        if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = Cooldown;
        }
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        EnsureBatteryInitialized();

        Button!.transform.localPosition =
            new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y + 1.1f, -150f);
        if (KeybindIcon != null)
        {
            KeybindIcon.transform.localPosition = new Vector3(0.4f, 0.45f, -9f);
        }
    }

    private static void EnsureBatteryInitialized()
    {
        if (!_batteryInitialized)
        {
            var options = OptionGroupSingleton<SentryOptions>.Instance;
            _availableCharge = options.PortableCamsBattery;
            _batteryInitialized = true;
        }
    }

    public static void HandleMinigameClosedStatic(Minigame closing)
    {
        try
        {
            if (_securityMinigame == null) return;
            if (closing == null) return;
            if (_securityMinigame != closing) return;

            _securityMinigame = null;

            if (_reportedInUse && PlayerControl.LocalPlayer != null)
            {
                _reportedInUse = false;
                SentryRole.RpcSentryPortableCamsInUse(PlayerControl.LocalPlayer, false);
            }
        }
        catch
        {
            // ignored
        }
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner || MeetingHud.Instance)
        {
            return;
        }

        if (_lastUpdateFrame != Time.frameCount)
        {
            _lastUpdateFrame = Time.frameCount;
            SharedUpdate(playerControl);
        }

        RefreshAbilityButton();

        Button?.usesRemainingText.gameObject.SetActive(true);
        Button?.usesRemainingSprite.gameObject.SetActive(true);
        var maxBattery = OptionGroupSingleton<SentryOptions>.Instance.PortableCamsBattery;
        var percent = maxBattery > 0f ? Mathf.Clamp(Mathf.RoundToInt((_availableCharge / maxBattery) * 100f), 0, 100) : 0;
        Button!.usesRemainingText.text = percent + "%";

        if (_securityMinigame == null && EffectActive)
        {
            ResetCooldownAndOrEffect();
        }
    }

    private void RefreshAbilityButton()
    {
        if (_availableCharge > 0f && PlayerControl.LocalPlayer != null && !PlayerControl.LocalPlayer.AreCommsAffected())
        {
            Button?.SetEnabled();
            return;
        }

        Button?.SetDisabled();
    }

    private static void SharedUpdate(PlayerControl playerControl)
    {
        EnsureBatteryInitialized();

        if (_securityMinigame != null)
        {
            var closed =
                Minigame.Instance == null ||
                Minigame.Instance != _securityMinigame ||
                _securityMinigame.gameObject == null ||
                !_securityMinigame.gameObject.activeInHierarchy;

            if (closed)
            {
                if (_reportedInUse && PlayerControl.LocalPlayer != null)
                {
                    _reportedInUse = false;
                    SentryRole.RpcSentryPortableCamsInUse(PlayerControl.LocalPlayer, false);
                }

                _securityMinigame = null;
            }
        }

        if (_securityMinigame != null && playerControl.AreCommsAffected())
        {
            _securityMinigame.Close();
            _securityMinigame = null;
            if (_reportedInUse && PlayerControl.LocalPlayer != null)
            {
                _reportedInUse = false;
                SentryRole.RpcSentryPortableCamsInUse(PlayerControl.LocalPlayer, false);
            }
            return;
        }

        if (_securityMinigame != null)
        {
            _availableCharge -= Time.deltaTime;
            if (_availableCharge <= 0f)
            {
                _availableCharge = 0f;
                _securityMinigame.Close();
                _securityMinigame = null;
                if (_reportedInUse && PlayerControl.LocalPlayer != null)
                {
                    _reportedInUse = false;
                    SentryRole.RpcSentryPortableCamsInUse(PlayerControl.LocalPlayer, false);
                }
            }
        }
    }

    public override bool CanUse()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (Minigame.Instance != null)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.AreCommsAffected())
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() || PlayerControl.LocalPlayer
                .GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        if (PlayerControl.LocalPlayer?.Data?.Role is not SentryRole sentryRole)
        {
            return false;
        }

        var options = OptionGroupSingleton<SentryOptions>.Instance;
        var mapId = MiscUtils.GetCurrentMap;
        var mapWithoutCameras = SentryCameraUtilities.IsMapWithoutCameras(mapId);

        var unlocked = options.PortableCamerasMode switch
        {
            SentryPortableCamerasMode.Immediately => true,
            SentryPortableCamerasMode.AfterTasks => sentryRole.CompletedAllTasks,
            SentryPortableCamerasMode.OnMapsWithoutCameras => mapWithoutCameras || sentryRole.CompletedAllTasks,
            _ => sentryRole.CompletedAllTasks
        };

        if (!unlocked)
        {
            return false;
        }

        if (!ShouldBeVisible(sentryRole))
        {
            return false;
        }

        var allCams = ShipStatus.Instance != null ? ShipStatus.Instance.AllCameras : null;
        if (allCams == null || allCams.Length == 0)
        {
            return false;
        }

        return Timer <= 0 && !EffectActive && _availableCharge > 0f;
    }

    protected override void OnClick()
    {
        var options = OptionGroupSingleton<SentryOptions>.Instance;
        if (options.PortableCamsBattery <= 0)
        {
            return;
        }

        EnsureBatteryInitialized();

        PlayerControl.LocalPlayer.NetTransform.Halt();

        var mapId = MiscUtils.GetCurrentMap;
        
        var mapWithoutCameras = SentryCameraUtilities.IsMapWithoutCameras(mapId);
        SystemConsole? basicCams = null;
        
        if (!mapWithoutCameras)
        {
            var allConsoles = Object.FindObjectsOfType<SystemConsole>();
            
            if (mapId is ExpandedMapNames.Airship)
            {
                basicCams = allConsoles.FirstOrDefault(x => x != null && x.gameObject != null && x.gameObject.name.Contains("task_cams"));
            }
            else if (mapId is ExpandedMapNames.Skeld or ExpandedMapNames.Dleks)
            {
                basicCams = allConsoles.FirstOrDefault(x => x != null && x.gameObject != null && x.gameObject.name.Contains("SurvConsole"));
            }
            else if (mapId is ExpandedMapNames.Submerged)
            {
                basicCams = allConsoles.FirstOrDefault(x => x != null && x.gameObject != null && x.gameObject.name.Contains("SecurityConsole"));
            }
            else
            {
                basicCams = allConsoles.FirstOrDefault(x => x != null && x.gameObject != null &&
                    (x.gameObject.name.Contains("Surv_Panel") || x.gameObject.name.Contains("Cam") ||
                     x.gameObject.name.Contains("BinocularsSecurityConsole")));
            }
        }

        if (basicCams == null)
        {
            try
            {
                var polus = PrefabLoader.Polus;
                if (polus != null)
                {
                    basicCams = polus.GetComponentsInChildren<SystemConsole>(true)
                        .FirstOrDefault(x => x != null && x.gameObject != null && x.gameObject.name.Contains("Surv_Panel"));
                }
            }
            catch
            {
                basicCams = null;
            }
        }

        if (basicCams == null)
        {
            Error("No Camera System Found!");
            return;
        }

        _securityMinigame = Object.Instantiate(basicCams.MinigamePrefab, Camera.main.transform, false);
        _securityMinigame.transform.SetParent(Camera.main.transform, false);
        _securityMinigame.transform.localPosition = new Vector3(0f, 0f, -50f);

        try
        {
            _securityMinigame.Begin(null);

            SentryCameraMinigameUtilities.SwapAllCamerasForNonSentry(_securityMinigame);
            SentryCameraMinigameUtilities.AddSentryCameras(_securityMinigame);
            
            if (!_reportedInUse && PlayerControl.LocalPlayer != null)
            {
                _reportedInUse = true;
                SentryRole.RpcSentryPortableCamsInUse(PlayerControl.LocalPlayer, true);
            }
        }
        catch
        {
            _securityMinigame.Close();
            _securityMinigame = null;
            Error("Portable Cameras: failed to initialize camera minigame prefab.");
        }
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();

        if (_securityMinigame != null)
        {
            _securityMinigame.Close();
            _securityMinigame = null;
        }

        if (_reportedInUse && PlayerControl.LocalPlayer != null)
        {
            _reportedInUse = false;
            SentryRole.RpcSentryPortableCamsInUse(PlayerControl.LocalPlayer, false);
        }
    }

    public static void ResetBatteryToMax()
    {
        _availableCharge = OptionGroupSingleton<SentryOptions>.Instance.PortableCamsBattery;
        _batteryInitialized = true;
    }

    public static void ResetBatteryState()
    {
        _availableCharge = 0f;
        _batteryInitialized = false;
    }
}