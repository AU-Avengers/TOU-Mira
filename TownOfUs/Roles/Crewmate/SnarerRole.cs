using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SnarerRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    public DoomableType DoomHintType => DoomableType.Trickster;
    public string LocaleKey => "Snarer";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(
                    TouLocale.GetParsed($"TouRole{LocaleKey}Snare", "Snare"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}SnareWikiDescription"),
                    TouCrewAssets.TrapSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Snarer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Trapper,
        IntroSound = TouAudio.EngineerIntroSound
    };

    public void LobbyStart()
    {
        VentSnareSystem.ClearAll();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        VentSnareSystem.ClearOwnedBy(targetPlayer.PlayerId);
    }

    [MethodRpc((uint)TownOfUsRpc.SnarerPlaceSnare)]
    public static void RpcSnarerPlaceSnare(PlayerControl snarer, int ventId)
    {
        if (snarer == null || snarer.Data?.Role is not SnarerRole)
        {
            return;
        }

        VentSnareSystem.Place(ventId, snarer.PlayerId);

        if (snarer.AmOwner)
        {
            var vent = Helpers.GetVentById(ventId);
            var room = vent != null ? MiscUtils.GetRoomName(vent.transform.position) : TouLocale.Get("Unknown", "Unknown");
            var msg = TouLocale.GetParsed("TouRoleSnarerPlaced", "Snared a vent in <room>!", new()
            {
                ["<room>"] = room
            });

            var notif = Helpers.CreateAndShowNotification(
                msg,
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: TouRoleIcons.Trapper.LoadAsset());
            notif.AdjustNotification();
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SnarerTriggerSnare)]
    public static IEnumerator RpcSnarerTriggerSnare(PlayerControl snarer, int ventId, byte victimId)
    {
        if (snarer == null)
        {
            yield break;
        }

        VentSnareSystem.Remove(ventId);

        var victim = MiscUtils.PlayerById(victimId);
        if (victim == null)
        {
            yield break;
        }

        if (!VentSnareSystem.IsEligibleToBeSnared(victim))
        {
            yield break;
        }

        var vent = Helpers.GetVentById(ventId);
        var ventTopPos = vent != null ? VentSnareSystem.GetVentTopPosition(vent) : (Vector2)victim.transform.position;

        yield return new WaitForSeconds(0.3f);

        if (victim.AmOwner)
        {
            CoApplySnareToVictimAfterVentAnim(victim, ventId, ventTopPos, vent);
        }
        else if (snarer.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snarer));

            var arrowDur = OptionGroupSingleton<SnarerOptions>.Instance.ArrowDuration;
            snarer.GetModifierComponent()?.AddModifier(new VentArrowModifier(ventTopPos, TownOfUsColors.Snarer, arrowDur));

            var room = vent != null ? MiscUtils.GetRoomName(vent.transform.position) : TouLocale.Get("Unknown", "Unknown");
            var msg = TouLocale.GetParsed("TouRoleSnarerTriggered", "Your snare was triggered in <room>!", new()
            {
                ["<room>"] = room
            });

            var notif = Helpers.CreateAndShowNotification(
                msg,
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: TouRoleIcons.Trapper.LoadAsset());
            notif.AdjustNotification();
        }
    }

    private static void CoApplySnareToVictimAfterVentAnim(PlayerControl victim, int ventId, Vector2 ventTopPos, Vent? vent)
    {
        if (victim == null || victim.HasDied() || !victim.AmOwner)
        {
            return;
        }

        var dur = OptionGroupSingleton<SnarerOptions>.Instance.SnareDuration;
        victim.GetModifierComponent()?.AddModifier(new SnaredOnVentModifier(ventTopPos, dur, ventId));

        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snarer));
        TouAudio.PlaySound(TouAudio.DiscoveredSound);

        var room = vent != null ? MiscUtils.GetRoomName(vent.transform.position) : TouLocale.Get("Unknown", "Unknown");
        var msg = TouLocale.GetParsed("TouRoleSnarerCaught", "You were caught in a snare in <room>!", new()
        {
            ["<room>"] = room
        });

        var notif = Helpers.CreateAndShowNotification(
            msg,
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: TouRoleIcons.Trapper.LoadAsset());
        notif.AdjustNotification();
    }
}