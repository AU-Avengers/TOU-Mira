using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class GuardianEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        CustomRoleUtils.GetActiveRolesOfType<GuardianRole>()
            .Do(role => role.Clear());
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        if (PlayerControl.LocalPlayer.Data.Role is GuardianRole guardian && guardian.ProtectedRole.HasValue)
        {
            if (!guardian.ProtectedRoleExists)
            {
                MiscUtils.AddFakeChat(
                    PlayerControl.LocalPlayer.Data,
                    $"{TownOfUsColors.Guardian.ToTextColor()}Guardian Report</color>",
                    $"Your latest Aegis did not protect anyone. There must be no players with {FormatedTextForRole(guardian.ProtectedRole.Value)} role.",
                    false,
                    true);
                return;
            }
            
            foreach (var roleType in guardian.AegisAttacked)
            {
                MiscUtils.AddFakeChat(
                    PlayerControl.LocalPlayer.Data,
                    $"{TownOfUsColors.Guardian.ToTextColor()}Guardian Report</color>",
                    $"Your {FormatedTextForRole(roleType)} Aegis has been attacked!",
                    false,
                    true);
            }
        }
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (CheckForAegis(@event, source, target))
        {
            ResetButtonTimer(source);
        }
    }

    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var source = PlayerControl.LocalPlayer;
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;
        if (target == null || button is not IKillButton)
        {
            return;
        }

        if (CheckForAegis(@event, source, target))
        {
            ResetButtonTimer(source, button);
        }
    }

    private static bool CheckForAegis(MiraCancelableEvent @event, PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return false;
        }

        if (!target.HasModifier<GuardianAegisModifier>() ||
            source == null ||
            target.PlayerId == source.PlayerId ||
            (source.TryGetModifier<IndirectAttackerModifier>(out var indirect) && indirect.IgnoreShield))
        {
            return false;
        }

        @event.Cancel();

        var guardian = target.GetModifier<GuardianAegisModifier>()?.Guardian;

        if (guardian != null && source.AmOwner)
        {
            GuardianRole.RpcGuardianAegisAttacked(guardian, source, target);
        }

        return true;
    }

    private static void ResetButtonTimer(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        var reset = OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset;

        button?.SetTimer(reset);
        
        if (!source.AmOwner || !source.IsImpostor())
        {
            return;
        }

        source.SetKillTimer(reset);
    }

    private static string FormatedTextForRole(RoleTypes roleType)
    {
        var role = RoleManager.Instance.GetRole(roleType);
        return
            $"<b>{(role is ICustomRole aegisCustom ? aegisCustom.RoleColor : role.TeamColor).ToTextColor()}{role.NiceName}</color></b>";
    }
}