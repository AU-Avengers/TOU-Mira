using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Events.Crewmate;

public static class MonarchEvents
{
    [RegisterEvent]
    public static void OnKnightKilled(AfterMurderEvent @event)
    {
        var deadPlayer = @event.Target;

        if (!OptionGroupSingleton<MonarchOptions>.Instance.InformWhenKnightDies)
            return;

        if (!deadPlayer.HasModifier<KnightedModifier>())
            return;

        var monarch = PlayerControl.AllPlayerControls
            .ToArray()
            .FirstOrDefault(p => !p.HasDied() && p.Data.Role is MonarchRole);

        if (monarch == null || !monarch.AmOwner)
            return;

        var notif = Helpers.CreateAndShowNotification(
            $"<b>{TouLocale.GetParsed("TouRoleMonarchKnightFallenFeedback").Replace("<player>", deadPlayer.Data.PlayerName)}</b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: TouRoleIcons.Monarch.LoadAsset());

        notif.Text.SetOutlineThickness(0.4f);
    }

    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target == null || button == null || button is not IKillButton || !button.CanClick())
            return;

        if (CheckForMonarchImmunity(@event, target))
        {
            ResetButtonTimer(PlayerControl.LocalPlayer, button);
        }
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (CheckForMonarchImmunity(@event, target))
        {
            ResetButtonTimer(source);
        }
    }

    private static bool CheckForMonarchImmunity(MiraCancelableEvent? @event, PlayerControl target)
    {
        if (!OptionGroupSingleton<MonarchOptions>.Instance.CrewKnightsGrantKillImmunity)
            return false;

        if (MeetingHud.Instance || ExileController.Instance)
            return false;

        if (target.Data?.Role is not MonarchRole)
            return false;

        var allowEvilKnights = target.HasModifier<EgotistModifier>();

        var knightedAlive = Helpers.GetAlivePlayers()
            .Any(p =>
                p.HasModifier<KnightedModifier>() &&
                (allowEvilKnights || p.IsCrewmate())
            );

        if (!knightedAlive)
            return false;

        @event?.Cancel();
        return true;
    }

    private static void ResetButtonTimer(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        if (!source.AmOwner)
        {
            return;
        }

        var reset = OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset;

        button?.SetTimer(reset);
        source.SetKillTimer(reset);
    }
}
