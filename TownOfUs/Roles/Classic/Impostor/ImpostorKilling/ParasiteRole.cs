using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities.ControlSystem;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class ParasiteRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsLocalControlRole, IWikiDiscoverable, IDoomable
{
    public static ControlState ControlState { get; } = new(typeof(ParasiteRole));

    public ControlState State => ControlState;

    [HideFromIl2Cpp] public PlayerControl? Controlled { get; set; }
    private float _overtakeKillLockoutUntil;
    private bool _killPendingFromTimer;

    private Camera? parasiteCam;
    private GameObject parasiteBorderObj;
    private SpriteRenderer parasiteBorderRenderer;
    private bool _pipDragging;
    private bool _pipManualMovedThisSession;
    private Vector2 _pipDragOffsetViewport;
    private bool _pipSnapping;
    private Rect _pipSnapFrom;
    private Rect _pipSnapTo;
    private float _pipSnapStartTime;
    private bool _pipSettingsDirty = true;

    private const float PipBaseMarginY = 0.04f;
    private const float PipBaseHeight = 0.3f;
    private const float PipBaseMarginXAspectFactor = 0.04f;
    private const float PipBaseWidthAspectFactor = 0.3f;
    private const float PipSnapDurationSeconds = 0.12f;

    private LobbyNotificationMessage? controllerNotification;

    public DoomableType DoomHintType => DoomableType.Perception;
    public string IdPart => "Parasite";

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Parasite, "TouMira.Role.Impostor.Parasite", 1.45f),
        UseVanillaKillButton = false,
        IntroSound = TouAudio.ScreamIntro,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        Icon = TouRoleIcons.Parasite,
        CanUseVent = OptionGroupSingleton<ParasiteOptions>.Instance.CanVent
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Overtake", "Overtake"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Overtake.WikiDescription"),
            TouImpAssets.OvertakeSprite),
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Decay", "Kill"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Indirect"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Decay.WikiDescription"),
            TouAssets.KillSprite)
    ];

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ClearControlLocal();
        if (Player.AmOwner)
        {
            AdvancedMovementUtilities.CreateMobileJoystick();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
        ClearControlLocal();
        if (AdvancedMovementUtilities.MobileJoystickR && AdvancedMovementUtilities.MobileJoystickR.gameObject != null)
        {
            AdvancedMovementUtilities.MobileJoystickR.gameObject.DeepDestroy();
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        var target = Controlled;
        if (Player.AmOwner && target != null)
        {

            RpcParasiteEndControl(PlayerControl.LocalPlayer, target, !OptionGroupSingleton<ParasiteOptions>.Instance.SaveVictimIfMeetingCalled && !target.HasDied());
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        var target = Controlled;
        if (!Player.AmOwner || target == null)
        {
            ClearControlLocal();
            return;
        }

        RpcParasiteEndControl(PlayerControl.LocalPlayer, target, !OptionGroupSingleton<ParasiteOptions>.Instance.SaveVictimIfParasiteDies && !target.HasDied());
    }

    public void FixedUpdate()
    {
        if (!Player || Player.Data == null || Player.HasDied() || !Player.AmOwner)
        {
            return;
        }

        var target = Controlled;
        if (target == null)
        {
            AdvancedMovementUtilities.MobileJoystickR?.ToggleVisuals(false);
            _killPendingFromTimer = false;
            return;
        }

        if (target.Data == null || target.HasDied() || target.Data.Disconnected)
        {
            RpcParasiteEndControl(PlayerControl.LocalPlayer, target, false);
            _killPendingFromTimer = false;
            return;
        }

        if (Player.HasDied() && OptionGroupSingleton<ParasiteOptions>.Instance.SaveVictimIfParasiteDies)
        {
            RpcParasiteEndControl(PlayerControl.LocalPlayer, target, false);
            _killPendingFromTimer = false;
            return;
        }

        if ((_killPendingFromTimer && !target.HasDied()) &&
            !target.IsInTargetingAnimState() &&
            !target.inVent &&
            !target.inMovingPlat &&
            !target.onLadder &&
            !target.walkingToVent)
        {
            _killPendingFromTimer = false;

            if (PlayerControl.LocalPlayer)
            {
                RpcParasiteEndControl(PlayerControl.LocalPlayer, target, true);
            }
        }
    }

    public void LateUpdate()
    {
        if (!Player || !Player.AmOwner || Controlled == null || parasiteCam == null)
        {
            return;
        }

        // Camera follows controlled player position
        var pos = Controlled.transform.position;
        parasiteCam.transform.position = new Vector3(pos.x, pos.y, parasiteCam.transform.position.z);

        // Border follows the camera rect; TickPiP() also calls UpdateCameraBorderLayout() every frame.
    }

    public void UpdateOverlay()
    {
        TickPiP();
    }

    /// <summary>
    /// Called every frame while controlling someone to:
    /// - Apply local PiP settings (location/size)
    /// - Handle drag + smooth snap (camera + border together)
    /// - Keep the border perfectly aligned to "parasiteCam.rect"
    /// </summary>
    public void TickPiP()
    {
        if (!Player || !Player.AmOwner || Controlled == null ||
            parasiteCam == null || !parasiteBorderObj || !parasiteBorderRenderer || Camera.main == null)
        {
            return;
        }

        EnsureBorderCollider();

        if (_pipSettingsDirty)
        {
            ApplyPiPRectFromSettings(force: true);
            _pipSettingsDirty = false;
        }

        UpdateSnapAnimation();
        HandleDragInput();

        UpdateCameraBorderLayout();
    }

    /// <summary>
    /// Keeps the border perfectly aligned and scaled around "parasiteCam.rect"
    /// This mirrors the original working behavior ("Normal" size in Bottom Left).
    /// </summary>
    public void UpdateCameraBorderLayout()
    {
        if (parasiteCam == null || !parasiteBorderObj || !parasiteBorderRenderer || Camera.main == null)
        {
            return;
        }

        if (parasiteBorderRenderer.sprite == null)
        {
            return;
        }

        var rect = parasiteCam.rect;

        var screenWidth = Screen.width;
        var screenHeight = Screen.height;

        var viewportX = rect.x * screenWidth;
        var viewportY = rect.y * screenHeight;
        var viewportWidth = rect.width * screenWidth;
        var viewportHeight = rect.height * screenHeight;

        var hudCam = Camera.main;
        var worldBottomLeft = hudCam.ScreenToWorldPoint(new Vector3(viewportX, viewportY, hudCam.nearClipPlane));
        var worldTopRight =
            hudCam.ScreenToWorldPoint(new Vector3(viewportX + viewportWidth, viewportY + viewportHeight, hudCam.nearClipPlane));

        var worldCenter = new Vector3(
            (worldBottomLeft.x + worldTopRight.x) * 0.5f,
            (worldBottomLeft.y + worldTopRight.y) * 0.5f,
            parasiteBorderObj.transform.position.z
        );
        parasiteBorderObj.transform.position = worldCenter;

        var worldWidth = Mathf.Abs(worldTopRight.x - worldBottomLeft.x);
        var worldHeight = Mathf.Abs(worldTopRight.y - worldBottomLeft.y);

        var spriteSize = parasiteBorderRenderer.sprite.bounds.size;
        if (spriteSize.x > 0f && spriteSize.y > 0f)
        {
            const float scaleMultiplier = 1.42f;
            parasiteBorderObj.transform.localScale = new Vector3(
                (worldWidth * scaleMultiplier) / spriteSize.x,
                (worldHeight * scaleMultiplier) / spriteSize.y,
                1f
            );
        }

        parasiteBorderRenderer.color = new Color(1f, 1f, 1f, 0.95f);
    }

    public void MarkPiPSettingsDirty(bool resetManualThisSession = true)
    {
        _pipSettingsDirty = true;
        _pipSnapping = false;
        _pipDragging = false;
        if (resetManualThisSession)
        {
            _pipManualMovedThisSession = false;
        }
    }

    private void EnsureBorderCollider()
    {
        if (!parasiteBorderObj || !parasiteBorderRenderer)
        {
            return;
        }

        var col = parasiteBorderObj.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            return;
        }

        col = parasiteBorderObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        if (parasiteBorderRenderer.sprite != null)
        {
            col.size = parasiteBorderRenderer.sprite.bounds.size;
        }
    }

    private void ApplyPiPRectFromSettings(bool force)
    {
        if (parasiteCam == null)
        {
            return;
        }

        var locSetting = LocalSettingsTabSingleton<TouLocalTabGameplay>.Instance.ParasitePiPLocation.Value;
        var sizeMultiplier = ParasitePiPUtilities.GetScaleMultiplier();

        ParasitePiPLocation location;
        if (locSetting == ParasitePiPLocation.Dynamic)
        {
            if (!_pipManualMovedThisSession || force)
            {
                location = ParasitePiPUtilities.GetDynamicLocation();
            }
            else
            {
                return;
            }
        }
        else
        {
            location = locSetting;
        }

        parasiteCam.rect = ClampRectToViewport(GetAnchorRect(location, sizeMultiplier));
    }

    private static Rect GetAnchorRect(ParasitePiPLocation location, float sizeMultiplier)
    {
        var aspect = (float)Screen.height / Screen.width;
        var width = aspect * PipBaseWidthAspectFactor * sizeMultiplier;
        var height = PipBaseHeight * sizeMultiplier;
        var marginX = aspect * PipBaseMarginXAspectFactor;
        var marginY = PipBaseMarginY;

        var x = marginX;
        var y = marginY;

        switch (location)
        {
            case ParasitePiPLocation.TopLeft:
                x = marginX;
                y = 1f - height - marginY;
                break;
            case ParasitePiPLocation.MiddleLeft:
                x = marginX;
                y = (1f - height) * 0.5f;
                break;
            case ParasitePiPLocation.Dynamic:
            case ParasitePiPLocation.BottomLeft:
                x = marginX;
                y = marginY;
                break;
            case ParasitePiPLocation.TopRight:
                x = 1f - width - marginX;
                y = 1f - height - marginY;
                break;
            case ParasitePiPLocation.MiddleRight:
                x = 1f - width - marginX;
                y = (1f - height) * 0.5f;
                break;
            case ParasitePiPLocation.BottomRight:
                x = 1f - width - marginX;
                y = marginY;
                break;
        }

        return new Rect(x, y, width, height);
    }

    private static Rect ClampRectToViewport(Rect r)
    {
        var w = Mathf.Clamp01(r.width);
        var h = Mathf.Clamp01(r.height);
        var x = Mathf.Clamp(r.x, 0f, 1f - w);
        var y = Mathf.Clamp(r.y, 0f, 1f - h);
        return new Rect(x, y, w, h);
    }

    private void StartSnapToNearestAnchor()
    {
        if (parasiteCam == null)
        {
            return;
        }

        var sizeMultiplier = ParasitePiPUtilities.GetScaleMultiplier();
        var current = parasiteCam.rect;
        var currentCenter = new Vector2(current.x + current.width * 0.5f, current.y + current.height * 0.5f);

        var anchors = new[]
        {
            ParasitePiPLocation.TopLeft,
            ParasitePiPLocation.MiddleLeft,
            ParasitePiPLocation.BottomLeft,
            ParasitePiPLocation.TopRight,
            ParasitePiPLocation.MiddleRight,
            ParasitePiPLocation.BottomRight
        };

        var best = GetAnchorRect(anchors[0], sizeMultiplier);
        var bestCenter = new Vector2(best.x + best.width * 0.5f, best.y + best.height * 0.5f);
        var bestDist = Vector2.Distance(currentCenter, bestCenter);

        for (var i = 1; i < anchors.Length; i++)
        {
            var r = GetAnchorRect(anchors[i], sizeMultiplier);
            var c = new Vector2(r.x + r.width * 0.5f, r.y + r.height * 0.5f);
            var d = Vector2.Distance(currentCenter, c);
            if (d < bestDist)
            {
                bestDist = d;
                best = r;
            }
        }

        _pipSnapping = true;
        _pipSnapFrom = current;
        _pipSnapTo = ClampRectToViewport(best);
        _pipSnapStartTime = Time.time;
    }

    private void UpdateSnapAnimation()
    {
        if (!_pipSnapping || parasiteCam == null)
        {
            return;
        }

        var t = Mathf.Clamp01((Time.time - _pipSnapStartTime) / PipSnapDurationSeconds);
        t = 1f - Mathf.Pow(1f - t, 3f);

        parasiteCam.rect = LerpRect(_pipSnapFrom, _pipSnapTo, t);

        if (t >= 1f)
        {
            _pipSnapping = false;
            parasiteCam.rect = _pipSnapTo;
        }
    }

    private static Rect LerpRect(Rect a, Rect b, float t)
    {
        return new Rect(
            Mathf.Lerp(a.x, b.x, t),
            Mathf.Lerp(a.y, b.y, t),
            Mathf.Lerp(a.width, b.width, t),
            Mathf.Lerp(a.height, b.height, t)
        );
    }

    private void HandleDragInput()
    {
        if (parasiteCam == null || !parasiteBorderObj || Camera.main == null)
        {
            return;
        }

        var col = parasiteBorderObj.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            return;
        }

        Vector2 screenPos;


        bool down;
        bool held;
        bool up;
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            screenPos = touch.position;
            down = touch.phase == TouchPhase.Began;
            held = touch.phase is TouchPhase.Moved or TouchPhase.Stationary;
            up = touch.phase is TouchPhase.Ended or TouchPhase.Canceled;
        }
        else
        {
            down = Input.GetMouseButtonDown(0);
            held = Input.GetMouseButton(0);
            up = Input.GetMouseButtonUp(0);
            screenPos = Input.mousePosition;
        }

        var world = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane));
        var hit = col.OverlapPoint(new Vector2(world.x, world.y));

        if (down && hit)
        {
            _pipDragging = true;
            _pipSnapping = false;
            _pipManualMovedThisSession = true;

            var rect = parasiteCam.rect;
            var rectCenter = new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f);
            var pointerViewport = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
            _pipDragOffsetViewport = rectCenter - pointerViewport;
        }

        if (_pipDragging && held)
        {
            var rect = parasiteCam.rect;
            var pointerViewport = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
            var desiredCenter = pointerViewport + _pipDragOffsetViewport;

            var x = desiredCenter.x - rect.width * 0.5f;
            var y = desiredCenter.y - rect.height * 0.5f;
            parasiteCam.rect = ClampRectToViewport(new Rect(x, y, rect.width, rect.height));
        }

        if (_pipDragging && up)
        {
            _pipDragging = false;
            StartSnapToNearestAnchor();
        }
    }
    public void KillControlledFromTimer()
    {
        var local = PlayerControl.LocalPlayer;
        var target = Controlled;
        if (local == null || target == null)
        {
            return;
        }

        if (target.HasDied())
        {
            RpcParasiteEndControl(local, target, false);
            return;
        }

        if (target.IsInTargetingAnimState() ||
            target.inVent ||
            target.inMovingPlat ||
            target.onLadder ||
            target.walkingToVent)
        {
            _killPendingFromTimer = true;
            return;
        }

        RpcParasiteEndControl(local, target, true);
    }

    private void EnsureCamera()
    {
        if (parasiteCam != null)
        {
            return;
        }

        MarkPiPSettingsDirty(resetManualThisSession: true);

        parasiteCam = Instantiate(Camera.main);
        parasiteCam.name = "TOU-ParasiteCam";
        parasiteCam.orthographicSize = 1.5f;
        parasiteCam.transform.DestroyChildren();
        parasiteCam.GetComponent<FollowerCamera>()?.Destroy();
        parasiteCam.nearClipPlane = -1;
        parasiteCam.depth = Camera.main.depth + 1;
        parasiteCam.gameObject.SetActive(true);

        if (HudManager.InstanceExists && HudManager.Instance.FullScreen != null)
        {
            parasiteBorderObj = Instantiate(TouAssets.ParasiteOverlay.LoadAsset(), HudManager.Instance.FullScreen.transform.parent);
            parasiteBorderObj.layer = HudManager.Instance.FullScreen.gameObject.layer;

            parasiteBorderRenderer = parasiteBorderObj.GetComponent<SpriteRenderer>();
            parasiteBorderRenderer.sortingOrder = 1000;
            parasiteBorderRenderer.color = new Color(1f, 1f, 1f, 0.95f);

            EnsureBorderCollider();
            TickPiP();
        }
    }

    private void DestroyCamera()
    {
        if (parasiteCam?.gameObject != null)
        {
            parasiteCam.Destroy();
            parasiteCam = null;
        }

        if (parasiteBorderObj)
        {
            parasiteBorderObj.Destroy();
            parasiteBorderObj = null!;
            parasiteBorderRenderer = null!;
        }
    }

    public void ClearControlLocal()
    {
        Controlled = null;
        _overtakeKillLockoutUntil = 0f;
        _killPendingFromTimer = false;
        _pipDragging = false;
        _pipSnapping = false;
        _pipManualMovedThisSession = false;
        _pipSettingsDirty = true;

        DestroyCamera();
        ClearNotification();
    }

    public float GetOvertakeKillLockoutRemainingSeconds()
    {
        if (Controlled == null)
        {
            return 0f;
        }

        return Mathf.Max(0f, _overtakeKillLockoutUntil - Time.time);
    }

    private void CreateNotification()
    {
        if (Controlled == null || !PlayerControl.LocalPlayer || !Player.AmOwner)
        {
            return;
        }

        if (controllerNotification == null)
        {
            var controllerText = MiraLocaleManager.Get("TownOfUsMira.Role.ParasiteOvertakeNotifSelf");
            controllerNotification = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Impostor.ToTextColor()}{controllerText.Replace("<player>", Controlled.Data.PlayerName)}</color></b>",
                Color.white, new Vector3(0f, 2f, -20f), spr: TouRoleIcons.Parasite);
            controllerNotification.AdjustNotification();
            controllerNotification.alphaTimer = OptionGroupSingleton<PuppeteerOptions>.Instance.ControlDuration.Value;
        }
    }

    public void ClearNotification()
    {
        if (controllerNotification != null && controllerNotification.gameObject != null)
        {
            controllerNotification.gameObject.DeepDestroy();
            controllerNotification = null;
        }
    }


    [MethodRpc((uint)TownOfUsRpc.ParasiteControl)]
    public static void RpcParasiteControl(PlayerControl parasite, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(parasite);
            return;
        }
        if (parasite.Data.Role is not ParasiteRole role)
        {
            Error("RpcParasiteControl - Invalid parasite");
            return;
        }

        if (target?.Data == null || target.HasDied())
        {
            return;
        }

        Coroutines.Start(role.CoFinishControl(target));
    }

    public IEnumerator CoFinishControl(PlayerControl target)
    {
        var parasite = Player;
        while (target.IsInTargetingAnimState())
        {
            yield return null;
        }

        Controlled = target;
        _overtakeKillLockoutUntil = Time.time + OptionGroupSingleton<ParasiteOptions>.Instance.OvertakeKillCooldown;

        ControlState.SetControl(target.PlayerId, parasite.PlayerId);
        if (!target.HasModifier<ParasiteInfectedModifier>())
        {
            target.AddModifier<ParasiteInfectedModifier>(parasite);
        }

        if (target.inVent)
        {
            target.MyPhysics.ExitAllVents();
        }

        if (target.AmOwner)
        {
            var pos = (Vector2)target.transform.position;
            if (target.NetTransform != null)
            {
                try
                {
                    target.NetTransform.SnapTo(pos);
                }
                catch
                {
                    // ignored
                }
            }
        }
        else if (parasite.AmOwner)
        {
            NetTransformBacklogUtils.FlushAndSnap(target);
        }
        else
        {
            NetTransformBacklogUtils.FlushBacklog(target);
        }

        if (parasite.AmOwner)
        {
            EnsureCamera();
            var btn = CustomButtonSingleton<Buttons.Impostor.ParasiteOvertakeButton>.Instance;
            btn.SetActive(true, this);
            btn.StartControlEffectIfEnabled();
            CreateNotification();
            AdvancedMovementUtilities.ResizeMobileJoystick();
        }
    }

    [MethodRpc((uint)TownOfUsRpc.ParasiteEndControl)]
    public static void RpcParasiteEndControl(PlayerControl parasite, PlayerControl target, bool toKillPlayer = false)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(parasite);
            return;
        }

        RemoteControlUtilities.ControllerEndControl(parasite, target);

        if (toKillPlayer)
        {
            new TouAbilityEvent(AbilityType.ParasiteKill, parasite, target).InvokeEvent();
            CustomTouMurderRpcs.Indirect(parasite, target, MeetingCheck.Ignore, true, true, true,
                true, !MeetingHud.Instance, false, !MeetingHud.Instance, !MeetingHud.Instance, "Parasite");
        }
    }

    public void SetupLocalOnClear(PlayerControl controller)
    {
        controller.walkingToVent = false;

        var btn = CustomButtonSingleton<Buttons.Impostor.ParasiteOvertakeButton>.Instance;
        btn.SetActive(true, controller.Data.Role);
        btn.StopControlEffectAndApplyCooldown();
    }
}