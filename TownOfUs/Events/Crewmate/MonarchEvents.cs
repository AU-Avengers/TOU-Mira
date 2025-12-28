using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Crewmate;

public static class MonarchEvents
{
    [RegisterEvent]
    [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
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
            $"<b>Your knight, {deadPlayer.Data.PlayerName}, has fallen...</b>",
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

        if (target == null || button == null || !button.CanClick())
            return;

        CheckForMonarchImmunity(@event, target);
    }

    [RegisterEvent]
    public static void MiraButtonCancelledEventHandler(MiraButtonCancelledEvent @event)
    {
        var source = PlayerControl.LocalPlayer;
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target == null || button is not IKillButton)
            return;

        if (!CheckForMonarchImmunity(null, target))
            return;

        ResetButtonTimer(source, button);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (CheckForMonarchImmunity(@event, target))
        {
            ResetButtonTimer(source);

            if (target.Data?.Role is MonarchRole && source.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(new Color(0f, 0.5f, 0f, 1f)));
            }
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
                (p.IsCrewmate() || (allowEvilKnights && (p.IsImpostor() || p.IsNeutral())))
            );

        if (!knightedAlive)
            return false;

        @event?.Cancel();
        return true;
    }

    private static void ResetButtonTimer(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        var reset = OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset;

        button?.SetTimer(reset);

        if (!source.AmOwner || !source.IsImpostor())
            return;

        source.SetKillTimer(reset);
    }
}
