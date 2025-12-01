using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Neutral;

public static class GuardianAngelEvents
{
    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick() || button is not IKillButton)
        {
            return;
        }

        CheckForGaProtection(@event, target, PlayerControl.LocalPlayer);
    }

    [RegisterEvent]
    public static void MiraButtonCancelledEventHandler(MiraButtonCancelledEvent @event)
    {
        var source = PlayerControl.LocalPlayer;
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;
        if (target == null || button is not IKillButton)
        {
            return;
        }

        if (target && !target!.HasModifier<GuardianAngelProtectModifier>())
        {
            return;
        }

        ResetButtonTimer(source, button);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (!CheckForGaProtection(@event, target, source))
        {
            return;
        }

        ResetButtonTimer(source);
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        foreach (var ga in CustomRoleUtils.GetActiveRolesOfType<FairyRole>())
        {
            ga.CheckTargetDeath(@event.Player);
        }
    }

    private static bool CheckForGaProtection(MiraCancelableEvent @event, PlayerControl target,
        PlayerControl source)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return false;
        }

        if (!target.HasModifier<GuardianAngelProtectModifier>() ||
            source.PlayerId == target.PlayerId ||
            (source.TryGetModifier<IndirectAttackerModifier>(out var indirect) && indirect.IgnoreShield))
        {
            return false;
        }

        @event.Cancel();
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, $"{target.Data.PlayerName} has a ga shield, stopping an attack from {source.Data.PlayerName}!");

        return true;
    }

    private static void ResetButtonTimer(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        var reset = OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset;

        button?.SetTimer(reset);

        // Reset impostor kill cooldown if they attack a shielded player
        if (!source.AmOwner || !source.IsImpostor())
        {
            return;
        }

        source.SetKillTimer(reset);
    }
}