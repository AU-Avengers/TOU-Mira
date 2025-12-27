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
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class PuppeteerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    [HideFromIl2Cpp] public PlayerControl? Controlled { get; set; }
    public float ControlTimer { get; set; }

    private LobbyNotificationMessage? controllerNotification;

    public DoomableType DoomHintType => DoomableType.Trickster;
    public string LocaleKey => "Puppeteer";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Puppeteer,
        MaxRoleCount = 1,
        CanUseVent = OptionGroupSingleton<PuppeteerOptions>.Instance.CanVent
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Control", "Control"),
            TouLocale.GetParsed($"TouRole{LocaleKey}ControlWikiDescription"),
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
        if (Player == null || Player.Data == null || Player.HasDied() || !Player.AmOwner)
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
            var controllerText = TouLocale.GetParsed("TouRolePuppeteerControlNotif", $"You are controlling {Controlled.Data.PlayerName}!");
            controllerNotification = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Impostor.ToTextColor()}{controllerText.Replace("<player>", Controlled.Data.PlayerName)}</color></b>",
                Color.white, new Vector3(0f, 2f, -20f), spr: TouRoleIcons.Puppeteer.LoadAsset());
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

    [MethodRpc((uint)TownOfUsRpc.PuppeteerControl)]
    public static void RpcPuppeteerControl(PlayerControl puppeteer, PlayerControl target)
    {
        if (puppeteer.Data.Role is not PuppeteerRole role)
        {
            Error("RpcPuppeteerControl - Invalid puppeteer");
            return;
        }

        if (target == null || target.Data == null || target.HasDied())
        {
            return;
        }

        role.Controlled = target;
        role.ControlTimer = OptionGroupSingleton<PuppeteerOptions>.Instance.ControlDuration.Value;

        PuppeteerControlState.SetControl(target.PlayerId, puppeteer.PlayerId);
        if (!target.HasModifier<PuppeteerControlModifier>())
        {
            target.AddModifier<PuppeteerControlModifier>(puppeteer);
        }

        if (target.inVent)
        {
            target.MyPhysics.ExitAllVents();
        }

        if (puppeteer.AmOwner)
        {
            CustomButtonSingleton<TownOfUs.Buttons.Impostor.PuppeteerControlButton>.Instance.SetActive(true, role);
            role.CreateNotification();
        }
    }

    [MethodRpc((uint)TownOfUsRpc.PuppeteerEndControl)]
    public static void RpcPuppeteerEndControl(PlayerControl puppeteer, PlayerControl target)
    {
        if (puppeteer.Data.Role is not PuppeteerRole role)
        {
            return;
        }

        if (target != null)
        {
            PuppeteerControlState.ClearControl(target.PlayerId);
            if (target.TryGetModifier<PuppeteerControlModifier>(out var mod))
            {
                target.RemoveModifier(mod);
            }
        }

        role.ClearControlLocal();

        if (puppeteer.AmOwner)
        {
            var btn = CustomButtonSingleton<TownOfUs.Buttons.Impostor.PuppeteerControlButton>.Instance;
            btn.ResetCooldownAndOrEffect();
        }

        role.ClearNotifications();
    }

    [MethodRpc((uint)TownOfUsRpc.PuppeteerMoveControlled, LocalHandling = Reactor.Networking.Rpc.RpcLocalHandling.Before)]
    public static void RpcPuppeteerMoveControlled(PlayerControl sender, byte controlledId, float x, float y)
    {
        PuppeteerControlState.SetDirection(controlledId, new Vector2(x, y));
    }

    public void LobbyStart()
    {
        PuppeteerControlState.ClearAll();

        foreach (var puppetMod in ModifierUtils.GetActiveModifiers<PuppeteerControlModifier>())
        {
            puppetMod.Player.RemoveModifier(puppetMod);
        }
    }
}