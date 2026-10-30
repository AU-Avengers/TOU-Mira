using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class PuppeteerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsLocalControlRole, IWikiDiscoverable, IDoomable, ITransportTrigger
{
    public static ControlState ControlState { get; } = new(typeof(PuppeteerRole));

    public ControlState State => ControlState;

    [HideFromIl2Cpp]
    public MonoBehaviour? OnTransport()
    {
        return Controlled;
    }
    [HideFromIl2Cpp] public PlayerControl? Controlled { get; set; }
    public float ControlTimer { get; set; }

    private LobbyNotificationMessage? controllerNotification;

    public DoomableType DoomHintType => DoomableType.Trickster;
    public string IdPart => "Puppeteer";

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Puppeteer, "TouMira.Role.Impostor.Puppeteer", 1.45f),
        UseVanillaKillButton = false,
        Icon = TouRoleIcons.Puppeteer,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        MaxRoleCount = 1,
        CanUseVent = OptionGroupSingleton<PuppeteerOptions>.Instance.CanVent
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Control", "Control"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Menu"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Control.WikiDescription"),
            TouImpAssets.ControlSprite),
    ];

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ClearControlLocal();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
        ClearControlLocal();
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        if (Player.AmOwner && Controlled != null)
        {
            RpcPuppeteerEndControl(PlayerControl.LocalPlayer, Controlled);
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

        RpcPuppeteerEndControl(PlayerControl.LocalPlayer, Controlled);
    }

    public void FixedUpdate()
    {
        if (!Player || Player.Data == null || Player.HasDied() || !Player.AmOwner)
        {
            return;
        }

        if (Controlled == null)
        {
            return;
        }

        if (Controlled.Data == null || Controlled.HasDied() || Controlled.Data.Disconnected || Player.HasDied())
        {
            RpcPuppeteerEndControl(PlayerControl.LocalPlayer, Controlled);
            return;
        }

        var duration = OptionGroupSingleton<PuppeteerOptions>.Instance.ControlDuration.Value;
        if (duration > 0f)
        {
            if (ControlTimer > duration)
            {
                ControlTimer = duration;
            }

            ControlTimer -= Time.fixedDeltaTime;

            if (ControlTimer <= 0f && Controlled != null)
            {
                RpcPuppeteerEndControl(PlayerControl.LocalPlayer, Controlled);
            }
        }
    }


    public void ClearControlLocal()
    {
        Controlled = null;
        ControlTimer = 0f;
        ClearNotification();
    }

    private void CreateNotification()
    {
        if (Controlled == null || !PlayerControl.LocalPlayer || !Player.AmOwner)
        {
            return;
        }

        if (controllerNotification == null)
        {
            var controllerText = MiraLocaleManager.Get("TownOfUsMira.Role.PuppeteerControlNotifSelf");
            controllerNotification = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Impostor.ToTextColor()}{controllerText.Replace("<player>", Controlled.Data.PlayerName)}</color></b>",
                Color.white, new Vector3(0f, 2f, -20f), spr: TouRoleIcons.Puppeteer);
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

    [MethodRpc((uint)TownOfUsRpc.PuppeteerControl)]
    public static void RpcPuppeteerControl(PlayerControl puppeteer, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(puppeteer);
            return;
        }
        if (puppeteer.Data.Role is not PuppeteerRole role)
        {
            Error("RpcPuppeteerControl - Invalid puppeteer");
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
        var puppeteer = Player;
        while (target.IsInTargetingAnimState())
        {
            yield return null;
        }

        Controlled = target;
        ControlTimer = OptionGroupSingleton<PuppeteerOptions>.Instance.ControlDuration.Value;

        new TouAbilityEvent(AbilityType.PuppeteerControl, puppeteer, target).InvokeEvent();

        ControlState.SetControl(target.PlayerId, puppeteer.PlayerId);
        if (!target.HasModifier<PuppeteerControlModifier>())
        {
            target.AddModifier<PuppeteerControlModifier>(puppeteer);
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
        else if (puppeteer.AmOwner)
        {
            NetTransformBacklogUtils.FlushAndSnap(target);
        }
        else
        {
            NetTransformBacklogUtils.FlushBacklog(target);
        }

        if (puppeteer.AmOwner)
        {
            CustomButtonSingleton<Buttons.Impostor.PuppeteerControlButton>.Instance.SetActive(true, this);
            CreateNotification();
        }
        else if (target.AmOwner && OptionGroupSingleton<PuppeteerOptions>.Instance.VictimSeesControlDirection > 0)
        {
            puppeteer.AddModifier<PuppeteerHintArrowModifier>(target);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.PuppeteerEndControl)]
    public static void RpcPuppeteerEndControl(PlayerControl puppeteer, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(puppeteer);
            return;
        }
        if (puppeteer.Data.Role is not PuppeteerRole role)
        {
            return;
        }

        if (target != null)
        {
            ControlState.ClearControl(target.PlayerId);
            target.TryRemoveModifier<PuppeteerControlModifier>();

            if (target.MyPhysics != null)
            {
                target.MyPhysics.body?.velocity = Vector2.zero;
                target.MyPhysics.SetNormalizedVelocity(Vector2.zero);
            }

            if (target.NetTransform != null)
            {
                try
                {
                    var finalPos = (Vector2)target.transform.position;

                    NetTransformBacklogUtils.FlushBacklog(target);

                    if (target.AmOwner)
                    {
                        target.NetTransform.SnapTo(finalPos);
                    }
                    else if (puppeteer != null && puppeteer.AmOwner)
                    {
                        NetTransformBacklogUtils.FlushAndSnap(target);
                    }
                    else
                    {
                        NetTransformBacklogUtils.FlushBacklog(target);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        role.ClearControlLocal();

        if (puppeteer != null)
        {
            if (puppeteer.AmOwner)
            {
                var pos = (Vector2)puppeteer.transform.position;
                if (puppeteer.NetTransform != null)
                {
                    try
                    {
                        puppeteer.NetTransform.SnapTo(pos);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            else
            {
                NetTransformBacklogUtils.FlushBacklog(puppeteer);
            }

            if (puppeteer.AmOwner)
            {
                var btn = CustomButtonSingleton<Buttons.Impostor.PuppeteerControlButton>.Instance;
                btn.ResetCooldownAndOrEffect();
            }
        }

        role.ClearNotification();
    }
}