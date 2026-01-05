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

public sealed class TrapperRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    public DoomableType DoomHintType => DoomableType.Trickster;
    public string LocaleKey => "Trapper";
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
                    TouLocale.GetParsed($"TouRole{LocaleKey}Trap", "Trap"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}TrapWikiDescription"),
                    TouCrewAssets.TrapSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Trapper;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Trapper,
        IntroSound = TouAudio.EngineerIntroSound
    };

    public void LobbyStart()
    {
        VentTrapSystem.ClearAll();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        VentTrapSystem.ClearOwnedBy(targetPlayer.PlayerId);
    }

    [MethodRpc((uint)TownOfUsRpc.TrapperPlaceTrap)]
    public static void RpcTrapperPlaceTrap(PlayerControl trapper, int ventId)
    {
        if (trapper == null || trapper.Data?.Role is not TrapperRole)
        {
            return;
        }

        VentTrapSystem.Place(ventId, trapper.PlayerId);

        if (trapper.AmOwner)
        {
            var vent = Helpers.GetVentById(ventId);
            var room = vent != null ? MiscUtils.GetRoomName(vent.transform.position) : TouLocale.Get("Unknown", "Unknown");
            var msg = TouLocale.GetParsed("TouRoleTrapperPlaced", "Trapped a vent in <room>!", new()
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

    [MethodRpc((uint)TownOfUsRpc.TrapperTriggerTrap)]
    public static IEnumerator RpcTrapperTriggerTrap(PlayerControl trapper, int ventId, byte victimId)
    {
        if (trapper == null)
        {
            yield break;
        }

        VentTrapSystem.Remove(ventId);

        var victim = MiscUtils.PlayerById(victimId);
        if (victim == null)
        {
            yield break;
        }

        if (!VentTrapSystem.IsEligibleToBeTrapped(victim))
        {
            yield break;
        }

        var vent = Helpers.GetVentById(ventId);
        var ventTopPos = vent != null ? VentTrapSystem.GetVentTopPosition(vent) : (Vector2)victim.transform.position;

        yield return new WaitForSeconds(0.3f);

        if (victim.AmOwner)
        {
            CoApplyTrapToVictimAfterVentAnim(victim, ventId, ventTopPos, vent);
        }
        else if (trapper.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Trapper));

            var arrowDur = OptionGroupSingleton<TrapperOptions>.Instance.ArrowDuration;
            trapper.GetModifierComponent()?.AddModifier(new VentArrowModifier(ventTopPos, TownOfUsColors.Trapper, arrowDur));

            var room = vent != null ? MiscUtils.GetRoomName(vent.transform.position) : TouLocale.Get("Unknown", "Unknown");
            var msg = TouLocale.GetParsed("TouRoleTrapperTriggered", "Your trap was triggered in <room>!", new()
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

    private static void CoApplyTrapToVictimAfterVentAnim(PlayerControl victim, int ventId, Vector2 ventTopPos, Vent? vent)
    {
        if (victim == null || victim.HasDied() || !victim.AmOwner)
        {
            return;
        }

        var dur = OptionGroupSingleton<TrapperOptions>.Instance.Trappeduration;
        victim.GetModifierComponent()?.AddModifier(new TrappedOnVentModifier(ventTopPos, dur, ventId));

        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Trapper));
        TouAudio.PlaySound(TouAudio.DiscoveredSound);

        var room = vent != null ? MiscUtils.GetRoomName(vent.transform.position) : TouLocale.Get("Unknown", "Unknown");
        var msg = TouLocale.GetParsed("TouRoleTrapperCaught", "You were caught in a trap in <room>!", new()
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