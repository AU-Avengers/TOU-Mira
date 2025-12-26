using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class ParasiteRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    [HideFromIl2Cpp] public PlayerControl? Controlled { get; set; }
    public float ControlTimer { get; set; }

    private Camera? parasiteCam;
    private GameObject? parasiteBorderObj;
    private SpriteRenderer? parasiteBorderRenderer;
    
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
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        ClearControlLocal();
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        if (Player.AmOwner && Controlled != null)
        {
            if (!OptionGroupSingleton<ParasiteOptions>.Instance.SaveVictimIfMeetingCalled && !Controlled.HasDied())
            {
                PlayerControl.LocalPlayer.RpcSpecialMurder(
                    Controlled,
                    teleportMurderer: false,
                    showKillAnim: false,
                    causeOfDeath: "Parasite");
            }

            RpcParasiteEndControl(PlayerControl.LocalPlayer, Controlled);
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        if (!Player.AmOwner || Controlled == null)
        {
            ClearControlLocal();
            return;
        }

        if (!OptionGroupSingleton<ParasiteOptions>.Instance.SaveVictimIfParasiteDies && !Controlled.HasDied())
        {
            PlayerControl.LocalPlayer.RpcSpecialMurder(
                Controlled,
                teleportMurderer: false,
                showKillAnim: false,
                causeOfDeath: "Parasite");
        }

        RpcParasiteEndControl(PlayerControl.LocalPlayer, Controlled);
    }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data == null || Player.HasDied() || !Player.AmOwner)
        {
            return;
        }

        if (Controlled == null)
        {
            return;
        }

        if (Controlled.Data == null || Controlled.HasDied() || Controlled.Data.Disconnected)
        {
            RpcParasiteEndControl(PlayerControl.LocalPlayer, Controlled);
            return;
        }

        if (Player.HasDied() && OptionGroupSingleton<ParasiteOptions>.Instance.SaveVictimIfParasiteDies)
        {
            RpcParasiteEndControl(PlayerControl.LocalPlayer, Controlled);
            return;
        }

        var duration = OptionGroupSingleton<ParasiteOptions>.Instance.ControlDuration;
        if (duration > 0f)
        {
            if (ControlTimer > duration)
            {
                ControlTimer = duration;
            }

            ControlTimer -= Time.fixedDeltaTime;
            if (ControlTimer <= 0f)
            {
                PlayerControl.LocalPlayer.GetRole<ParasiteRole>()?.KillControlledFromTimer();
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

        UpdateCameraBorderLayout();
    }

    private void UpdateCameraBorderLayout()
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
        var worldTopRight = hudCam.ScreenToWorldPoint(new Vector3(viewportX + viewportWidth, viewportY + viewportHeight, hudCam.nearClipPlane));

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
            var scaleMultiplier = 1.42f;
            parasiteBorderObj.transform.localScale = new Vector3(
                (worldWidth * scaleMultiplier) / spriteSize.x,
                (worldHeight * scaleMultiplier) / spriteSize.y,
                1f
            );
        }

        parasiteBorderRenderer.color = new Color(1f, 1f, 1f, 0.95f);
    }

    private void KillControlledFromTimer()
    {
        if (Controlled == null)
        {
            return;
        }

        if (!Controlled.HasDied())
        {
            PlayerControl.LocalPlayer.RpcSpecialMurder(
                Controlled,
                teleportMurderer: false,
                showKillAnim: false,
                causeOfDeath: "Parasite");
        }

        RpcParasiteEndControl(PlayerControl.LocalPlayer, Controlled);
    }

    private void EnsureCamera()
    {
        if (parasiteCam != null)
        {
            return;
        }

        parasiteCam = UnityEngine.Object.Instantiate(Camera.main);
        parasiteCam.name = "TOU-ParasiteCam";
        parasiteCam.orthographicSize = 1.5f;
        var aspect = (float)Screen.height / Screen.width;
        var width = aspect * 0.3f;
        var posw = aspect * 0.04f;
        parasiteCam.rect = new Rect(posw, 0.04f, width, 0.3f);
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

            UpdateCameraBorderLayout();
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
        ControlTimer = 0f;
        DestroyCamera();
        ClearNotifications();
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

        role.Controlled = target;
        role.ControlTimer = OptionGroupSingleton<ParasiteOptions>.Instance.ControlDuration;

        ParasiteControlState.SetControl(target.PlayerId, parasite.PlayerId);
        if (!target.HasModifier<ParasiteInfectedModifier>())
        {
            target.AddModifier<ParasiteInfectedModifier>(parasite);
        }

        if (target.inVent)
        {
            target.MyPhysics.ExitAllVents();
        }

        if (parasite.AmOwner)
        {
            role.EnsureCamera();
            CustomButtonSingleton<TownOfUs.Buttons.Impostor.ParasiteOvertakeButton>.Instance.SetActive(true, role);
            role.CreateNotification();
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
        }

        role.ClearControlLocal();

        if (parasite.AmOwner)
        {
            var btn = CustomButtonSingleton<TownOfUs.Buttons.Impostor.ParasiteOvertakeButton>.Instance;
            btn.SetActive(true, role);
            btn.SetTimer(Mathf.Max(btn.Timer, btn.Cooldown));
        }

        role.ClearNotifications();
    }

    [MethodRpc((uint)TownOfUsRpc.ParasiteMoveControlled, LocalHandling = Reactor.Networking.Rpc.RpcLocalHandling.Before)]
    public static void RpcParasiteMoveControlled(PlayerControl sender, byte controlledId, float x, float y)
    {
        ParasiteControlState.SetDirection(controlledId, new Vector2(x, y));
    }
}