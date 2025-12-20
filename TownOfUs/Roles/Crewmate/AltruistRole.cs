using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Modules.Anims;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class AltruistRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Death;
    public string LocaleKey => "Altruist";
    
    public static bool IsReviveInProgress { get; private set; }
    public static string ReviveString()
    {
        switch ((ReviveType)OptionGroupSingleton<AltruistOptions>.Instance.ReviveMode.Value)
        {
            case ReviveType.Sacrifice:
                return "Sacrifice";
            case ReviveType.GroupSacrifice:
                return "GroupSacrifice";
        }
        return string.Empty;
    }
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription{ReviveString()}");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Revive", "Revive"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}Revive{ReviveString()}WikiDescription"),
                    TouCrewAssets.ReviveSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Altruist;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.AltruistReviveSound,
        Icon = TouRoleIcons.Altruist
    };



    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (CustomButtonSingleton<AltruistReviveButton>.Instance != null)
        {
            CustomButtonSingleton<AltruistReviveButton>.Instance.RevivedInRound = false;
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        Error($"AltruistRole.OnMeetingStart");

        ClearArrows();
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (CustomButtonSingleton<AltruistReviveButton>.Instance != null)
        {
            CustomButtonSingleton<AltruistReviveButton>.Instance.RevivedInRound = false;
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        ClearArrows();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        ClearArrows();
    }

    [HideFromIl2Cpp]
    public static void ClearArrows()
    {
        Error($"AltruistRole.ClearArrows");

        if (PlayerControl.LocalPlayer.IsImpostorAligned() || PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling))
        {
            Error($"AltruistRole.ClearArrows BadGuys Only");

            foreach (var playerWithArrow in ModifierUtils.GetPlayersWithModifier<AltruistArrowModifier>())
            {
                playerWithArrow.RemoveModifier<AltruistArrowModifier>();
            }
        }
    }

    [HideFromIl2Cpp]
    public IEnumerator CoRevivePlayer(PlayerControl dead)
    {
        IsReviveInProgress = true;
        try
        {
            var roleWhenAlive = dead.GetRoleWhenAlive();
            var freezeAltruist = OptionGroupSingleton<AltruistOptions>.Instance.FreezeDuringRevive.Value;

            //if (roleWhenAlive == null)
            //{
            //    Error($"CoRevivePlayer - Dead player {dead.PlayerId} does not have a role when alive, cannot revive");
            //    yield break; // cannot revive if no role when alive
            //}

            if (freezeAltruist)
            {
                Player.moveable = false;
                Player.NetTransform.Halt();
            }

            var body = FindObjectsOfType<DeadBody>()
                .FirstOrDefault(b => b.ParentId == dead.PlayerId);
            var position = new Vector2(Player.transform.localPosition.x, Player.transform.localPosition.y);

            if (body != null)
            {
                position = new Vector2(body.transform.localPosition.x, body.transform.localPosition.y + 0.3636f);
                if (OptionGroupSingleton<AltruistOptions>.Instance.HideAtBeginningOfRevive)
                {
                    Destroy(body.gameObject);
                }
            }

            yield return new WaitForSeconds(OptionGroupSingleton<AltruistOptions>.Instance.ReviveDuration.Value);

            var killOnStart = OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value;
            if (!MeetingHud.Instance && (!Player.HasDied() || killOnStart))
            {
                GameHistory.ClearMurder(dead);

                dead.Revive();

                var reviveFlashColor = new Color(0f, 0.5f, 0f, 1f);
                if (dead.AmOwner)
                {
                    TouAudio.PlaySound(TouAudio.AltruistReviveSound);
                    Coroutines.Start(MiscUtils.CoFlash(reviveFlashColor));
                    // Notify the revived player
                    var revivedText = TouLocale.GetParsed("TouRoleAltruistRevivedNotif");
                    var notif = Helpers.CreateAndShowNotification(
                        $"<b>{TownOfUsColors.Altruist.ToTextColor()}{revivedText}</color></b>",
                        Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Altruist.LoadAsset());
                    notif.AdjustNotification();
                }
                if (Player.AmOwner && Player != dead)
                {
                    TouAudio.PlaySound(TouAudio.AltruistReviveSound);
                    Coroutines.Start(MiscUtils.CoFlash(reviveFlashColor));
                    // Notify the Altruist
                    var successText = TouLocale.GetParsed("TouRoleAltruistReviveSuccessNotif")
                        .Replace("<player>", dead.Data.PlayerName);
                    var notif = Helpers.CreateAndShowNotification(
                        $"<b>{TownOfUsColors.Altruist.ToTextColor()}{successText}</color></b>",
                        Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Altruist.LoadAsset());
                    notif.AdjustNotification();
                }

                dead.transform.position = new Vector2(position.x, position.y);
                if (dead.AmOwner)
                {
                    PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(position.x, position.y));
                }

                if (ModCompatibility.IsSubmerged() && PlayerControl.LocalPlayer.PlayerId == dead.PlayerId)
                {
                    ModCompatibility.ChangeFloor(dead.transform.position.y > -7);
                }

                if (dead.AmOwner && !dead.HasModifier<LoverModifier>())
                {
                    HudManager.Instance.Chat.gameObject.SetActive(false);
                }

                dead.ChangeRole((ushort)roleWhenAlive!.Role, false);

                if (dead.Data.Role is IAnimated animated)
                {
                    animated.IsVisible = true;
                    animated.SetVisible();
                }

                foreach (var button in CustomButtonManager.Buttons.Where(x => x.Enabled(dead.Data.Role))
                             .OfType<IAnimated>())
                {
                    button.IsVisible = true;
                    button.SetVisible();
                }

                foreach (var modifier in dead.GetModifiers<GameModifier>().Where(x => x is IAnimated))
                {
                    var animatedMod = modifier as IAnimated;
                    if (animatedMod != null)
                    {
                        animatedMod.IsVisible = true;
                        animatedMod.SetVisible();
                    }
                }

                dead.RemainingEmergencies = 0;

                Player.RemainingEmergencies = 0;

                body = FindObjectsOfType<DeadBody>()
                    .FirstOrDefault(b => b.ParentId == dead.PlayerId);
                if (body != null)
                {
                    Destroy(body.gameObject);
                }

                var altruistBody = FindObjectsOfType<DeadBody>()
                    .FirstOrDefault(b => b.ParentId == Player.PlayerId);
                if (altruistBody != null)
                {
                    Destroy(altruistBody.gameObject);
                }

                var opts = (InformedKillers)OptionGroupSingleton<AltruistOptions>.Instance.KillersAlertedAtEnd.Value;
                if (opts.ToDisplayString().Contains("Impostors") && PlayerControl.LocalPlayer.IsImpostorAligned() || opts.ToDisplayString().Contains("Neutrals") && PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling))
                {
                    if (Player.HasModifier<AltruistArrowModifier>())
                    {
                        Player.RemoveModifier<AltruistArrowModifier>();
                    }

                    if (!dead.HasModifier<AltruistArrowModifier>() && dead != PlayerControl.LocalPlayer)
                    {
                        dead.AddModifier<AltruistArrowModifier>(PlayerControl.LocalPlayer, Color.white);
                    }
                }
            }

            if (freezeAltruist && !Player.HasDied())
            {
                Player.moveable = true;
            }
        }
        finally
        {
            IsReviveInProgress = false;
        }
    }

    [MethodRpc((uint)TownOfUsRpc.AltruistRevive)]
    public static void RpcRevive(PlayerControl alt, PlayerControl target)
    {
        if (alt.Data.Role is not AltruistRole role)
        {
            Error("RpcRevive - Invalid altruist");
            return;
        }

        var opts = (InformedKillers)OptionGroupSingleton<AltruistOptions>.Instance.KillersAlertedAtStart.Value;
        if (opts.ToDisplayString().Contains("Impostors") && PlayerControl.LocalPlayer.IsImpostorAligned() || opts.ToDisplayString().Contains("Neutrals") && PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling))
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Altruist));

            if (!alt.HasModifier<AltruistArrowModifier>())
            {
                alt.AddModifier<AltruistArrowModifier>(PlayerControl.LocalPlayer, TownOfUsColors.Impostor);
            }
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.AltruistRevive, alt, target);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value && !alt.HasDied())
        {
            alt.RpcCustomMurder(alt, showKillAnim: false, createDeadBody: true);
        }

        Coroutines.Start(role.CoRevivePlayer(target));
    }
}