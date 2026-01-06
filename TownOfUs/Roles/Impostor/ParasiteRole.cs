using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Patches.ControlSystem;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Roles.Impostor;

public sealed class ParasiteRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    [HideFromIl2Cpp] public PlayerControl? Controlled { get; set; }
    private float _overtakeKillLockoutUntil;
    private bool _killPendingFromTimer;

    private Camera? parasiteCam;
    private GameObject? parasiteBorderObj;
    private SpriteRenderer? parasiteBorderRenderer;
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
    public string LocaleKey => "Parasite";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = false,
        Icon = TouRoleIcons.Parasite,
        CanUseVent = OptionGroupSingleton<ParasiteOptions>.Instance.CanVent
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Overtake", "Overtake"),
            TouLocale.GetParsed($"TouRole{LocaleKey}OvertakeWikiDescription"),
            TouImpAssets.OvertakeSprite),
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Decay", "Kill"),
            TouLocale.GetParsed($"TouRole{LocaleKey}DecayWikiDescription"),
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
        ClearControlLocal();
        if (AdvancedMovementUtilities.MobileJoystickR.gameObject != null)
        {
            AdvancedMovementUtilities.MobileJoystickR.gameObject.Destroy();
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        var target = Controlled;
        if (Player.AmOwner && target != null)
        {
            if (!OptionGroupSingleton<ParasiteOptions>.Instance.SaveVictimIfMeetingCalled && !target.HasDied())
            {
                PlayerControl.LocalPlayer.RpcSpecialMurder(
                    target,
                    teleportMurderer: false,
                    showKillAnim: false,
                    causeOfDeath: "Parasite");
            }

            RpcParasiteEndControl(PlayerControl.LocalPlayer, target);
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

        if (!OptionGroupSingleton<ParasiteOptions>.Instance.SaveVictimIfParasiteDies && !target.HasDied())
        {
            PlayerControl.LocalPlayer.RpcSpecialMurder(
                target,
                teleportMurderer: false,
                showKillAnim: false,
                causeOfDeath: "Parasite");
        }

        RpcParasiteEndControl(PlayerControl.LocalPlayer, target);
    }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data == null || Player.HasDied() || !Player.AmOwner)
        {
            return;
        }

        var target = Controlled;
        if (target == null)
        {
            if (AdvancedMovementUtilities.MobileJoystickR != null)
            {
                AdvancedMovementUtilities.MobileJoystickR.ToggleVisuals(false);
            }
            _killPendingFromTimer = false;
            return;
        }

        if (target.Data == null || target.HasDied() || target.Data.Disconnected)
        {
            RpcParasiteEndControl(PlayerControl.LocalPlayer, target);
            _killPendingFromTimer = false;
            return;
        }

        if (Player.HasDied() && OptionGroupSingleton<ParasiteOptions>.Instance.SaveVictimIfParasiteDies)
        {
            RpcParasiteEndControl(PlayerControl.LocalPlayer, target);
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
            if (PlayerControl.LocalPlayer != null)
            {
                PlayerControl.LocalPlayer.RpcSpecialMurder(
                    target,
                    teleportMurderer: false,
                    showKillAnim: false,
                    causeOfDeath: "Parasite");
            }

            if (PlayerControl.LocalPlayer != null)
            {
                RpcParasiteEndControl(PlayerControl.LocalPlayer, target);
            }
        }
    }

    public void LateUpdate()
    {
        if (Player == null || !Player.AmOwner || Controlled == null || parasiteCam == null)
        {
            return;
        }

        var pos = Controlled.transform.position;
        parasiteCam.transform.position = new Vector3(pos.x, pos.y, parasiteCam.transform.position.z);

        // Border follows the camera rect; TickPiP() also calls UpdateCameraBorderLayout() every frame.
    }

    /// <summary>
    /// Called every frame while controlling someone to:
    /// - Apply local PiP settings (location/size)
    /// - Handle drag + smooth snap (camera + border together)
    /// - Keep the border perfectly aligned to "parasiteCam.rect"
    /// </summary>
    public void TickPiP()
    {
        if (Player == null || !Player.AmOwner || Controlled == null ||
            parasiteCam == null || parasiteBorderObj == null || parasiteBorderRenderer == null || Camera.main == null)
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
        if (parasiteCam == null || parasiteBorderObj == null || parasiteBorderRenderer == null || Camera.main == null)
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
        if (parasiteBorderObj == null || parasiteBorderRenderer == null)
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

        var locSetting = LocalSettingsTabSingleton<TownOfUsLocalRoleSettings>.Instance.ParasitePiPLocation.Value;
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
        if (parasiteCam == null || parasiteBorderObj == null || Camera.main == null)
        {
            return;
        }

        var col = parasiteBorderObj.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            return;
        }

        var down = false;
        var held = false;
        var up = false;
        Vector2 screenPos;

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
            RpcParasiteEndControl(local, target);
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

        local.RpcSpecialMurder(
            target,
            teleportMurderer: false,
            showKillAnim: false,
            causeOfDeath: "Parasite");

        RpcParasiteEndControl(local, target);
    }

    private void EnsureCamera()
    {
        if (parasiteCam != null)
        {
            return;
        }

        MarkPiPSettingsDirty(resetManualThisSession: true);

        parasiteCam = UnityEngine.Object.Instantiate(Camera.main);
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

        if (parasiteBorderObj != null)
        {
            parasiteBorderObj.Destroy();
            parasiteBorderObj = null;
            parasiteBorderRenderer = null;
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
        ClearNotifications();
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
        if (Controlled == null || PlayerControl.LocalPlayer == null || !Player.AmOwner)
        {
            return;
        }

        if (controllerNotification == null)
        {
            var controllerText = TouLocale.GetParsed("TouRoleParasiteControlNotif", $"You are controlling {Controlled.Data.PlayerName}!");
            controllerNotification = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Impostor.ToTextColor()}{controllerText.Replace("<player>", Controlled.Data.PlayerName)}</color></b>",
                Color.white, new Vector3(0f, 2f, -20f), spr: TouRoleIcons.Parasite.LoadAsset());
            controllerNotification?.AdjustNotification();
        }
    }

    private void ClearNotifications()
    {
        if (controllerNotification != null && controllerNotification.gameObject != null)
        {
            controllerNotification.gameObject.Destroy();
            controllerNotification = null;
        }
    }


    [MethodRpc((uint)TownOfUsRpc.ParasiteControl)]
    public static void RpcParasiteControl(PlayerControl parasite, PlayerControl target)
    {
        if (parasite.Data.Role is not ParasiteRole role)
        {
            Error("RpcParasiteControl - Invalid parasite");
            return;
        }

        if (target == null || target.Data == null || target.HasDied())
        {
            return;
        }

        if (target.IsInTargetingAnimState())
        {
            return;
        }

        role.Controlled = target;
        role._overtakeKillLockoutUntil = Time.time + OptionGroupSingleton<ParasiteOptions>.Instance.OvertakeKillCooldown;

        ParasiteControlState.SetControl(target.PlayerId, parasite.PlayerId);
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
            role.EnsureCamera();
            var btn = CustomButtonSingleton<TownOfUs.Buttons.Impostor.ParasiteOvertakeButton>.Instance;
            btn.SetActive(true, role);
            btn.StartControlEffectIfEnabled();
            role.CreateNotification();
            AdvancedMovementUtilities.ResizeMobileJoystick();
        }
    }

    [MethodRpc((uint)TownOfUsRpc.ParasiteEndControl)]
    public static void RpcParasiteEndControl(PlayerControl parasite, PlayerControl target)
    {
        if (parasite.Data.Role is not ParasiteRole role)
        {
            return;
        }

        if (target != null)
        {
            ParasiteControlState.ClearControl(target.PlayerId);
            if (target.TryGetModifier<ParasiteInfectedModifier>(out var mod))
            {
                target.RemoveModifier(mod);
            }

            if (target.MyPhysics != null)
            {
                if (target.MyPhysics.body != null)
                {
                    target.MyPhysics.body.velocity = Vector2.zero;
                }
                target.MyPhysics.SetNormalizedVelocity(Vector2.zero);
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
            else if (parasite != null && parasite.AmOwner)
            {
                NetTransformBacklogUtils.FlushAndSnap(target);
            }
            else
            {
                NetTransformBacklogUtils.FlushBacklog(target);
            }
        }

        role.ClearControlLocal();

        if (parasite != null)
        {
            if (parasite.AmOwner)
            {
                var pos = (Vector2)parasite.transform.position;
                if (parasite.NetTransform != null)
                {
                    try
                    {
                        parasite.NetTransform.SnapTo(pos);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            else
            {
                NetTransformBacklogUtils.FlushBacklog(parasite);
            }

            if (parasite.AmOwner)
            {
                parasite.walkingToVent = false;

                var btn = CustomButtonSingleton<TownOfUs.Buttons.Impostor.ParasiteOvertakeButton>.Instance;
                btn.SetActive(true, role);
                btn.StopControlEffectAndApplyCooldown();
            }
        }

        role.ClearNotifications();
    }

    [MethodRpc((uint)TownOfUsRpc.ParasiteTriggerInteraction)]
    public static void RpcParasiteTriggerInteraction(PlayerControl parasite, PlayerControl controlled, Vector2 interactablePosition)
    {
        if (parasite.Data.Role is not ParasiteRole role)
        {
            Error("RpcParasiteTriggerInteraction - Invalid parasite");
            return;
        }

        if (controlled == null || controlled.Data == null || controlled.HasDied())
        {
            return;
        }

        if (role.Controlled != controlled || !ParasiteControlState.IsControlled(controlled.PlayerId, out _))
        {
            return;
        }

        // Find the interactable at the position
        var interactable = FindInteractableAtPosition(interactablePosition, controlled);
        if (interactable == null)
        {
            return;
        }

        // Trigger the interaction as the controlled player
        TriggerInteractionAsPlayer(controlled, interactable);
    }

    private static IUsable? FindInteractableAtPosition(Vector2 position, PlayerControl player)
    {
        if (player == null)
        {
            return null;
        }

        var closestDistance = float.MaxValue;
        IUsable? closestInteractable = null;

        // Use cached interactables from ControlledPlayerInteractionPatches if available, otherwise scan
        var cached = ControlledPlayerInteractionPatches.GetCachedInteractables();
        var interactablesToCheck = cached != null && cached.Count > 0 
            ? cached 
            : GetInteractablesList();

        const float maxCheckDistance = 5f; // Most interactables have UsableDistance <= 3f

        foreach (var usable in interactablesToCheck)
        {
            if (usable == null)
            {
                continue;
            }

            // Get the MonoBehaviour to access transform
            var obj = usable.TryCast<MonoBehaviour>();
            if (obj == null)
            {
                continue;
            }

            // Quick distance check before expensive CanUse check
            var objPos = (Vector2)obj.transform.position;
            var distance = Vector2.Distance(position, objPos);
            if (distance > maxCheckDistance || distance > usable.UsableDistance)
            {
                continue;
            }

            // Check if player can use this
            bool canUse;
            usable.CanUse(player.Data, out canUse, out _);
            if (!canUse)
            {
                continue;
            }

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = usable;
            }
        }

        return closestInteractable;
    }

    private static List<IUsable> GetInteractablesList()
    {
        var result = new List<IUsable>();
        var allUsables = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
        foreach (var obj in allUsables)
        {
            if (obj.TryCast<IUsable>() is { } usable && usable.TryCast<Vent>() == null)
            {
                result.Add(usable);
            }
        }
        return result;
    }

    private static void TriggerInteractionAsPlayer(PlayerControl player, IUsable interactable)
    {
        if (player == null || interactable == null)
        {
            return;
        }

        // Handle different types of interactables
        if (interactable.TryCast<Ladder>() is { } ladder)
        {
            // Ladder climb MUST be initiated by the controlled player's owner, otherwise anticheat can kick.
            if (!player.AmOwner)
            {
                return;
            }
            player.MyPhysics.RpcClimbLadder(ladder);
            ladder.CoolDown = ladder.MaxCoolDown;
        }
        else if (interactable.TryCast<ZiplineConsole>() is { } ziplineConsole)
        {
            // Zipline use is host-authoritative in this codebase (see GhostRoleUsePatches.CheckUseZipline).
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                return;
            }
            if (ziplineConsole.zipline != null)
            {
                player.CheckUseZipline(player, ziplineConsole.zipline, ziplineConsole.atTop);
            }
        }
        else if (interactable.TryCast<OpenDoorConsole>() is { } openDoorConsole)
        {
            // Door state is host-authoritative.
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                return;
            }
            openDoorConsole.myDoor.SetDoorway(true);
        }
        else if (interactable.TryCast<DoorConsole>() is { } doorConsole)
        {
            // Door minigame - this needs to be handled on the controlled player's client
            if (player.AmOwner)
            {
                player.NetTransform.Halt();
                var minigame = Object.Instantiate(doorConsole.MinigamePrefab, Camera.main.transform);
                minigame.transform.localPosition = new Vector3(0f, 0f, -50f);

                try
                {
                    minigame.Cast<IDoorMinigame>().SetDoor(doorConsole.MyDoor);
                }
                catch (InvalidCastException)
                {
                    /* ignored */
                }

                minigame.Begin(null);
            }
        }
        else if (interactable.TryCast<PlatformConsole>() is { } platformConsole)
        {
            // Moving platform is host-authoritative (to avoid desync/cheat flags).
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                return;
            }
            var platform = platformConsole.Platform;
            if (platform != null)
            {
                var vector = platform.transform.position - player.transform.position;
                if (!platform.Target && vector.magnitude <= 3f)
                {
                    platform.IsDirty = true;
                    platform.StartCoroutine(platform.UsePlatform(player));
                }
            }
        }
        else if (interactable.TryCast<DeconControl>() is { } deconControl)
        {
            // Decon door trigger should be host-authoritative.
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                return;
            }
            deconControl.cooldown = 6f;
            if (Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(deconControl.UseSound, false);
            }
            deconControl.OnUse.Invoke();
        }
    }

    public void LobbyStart()
    {
        ParasiteControlState.ClearAll();

        foreach (var parasiteMod in ModifierUtils.GetActiveModifiers<ParasiteInfectedModifier>())
        {
            parasiteMod.Player.RemoveModifier(parasiteMod);
        }
    }
}