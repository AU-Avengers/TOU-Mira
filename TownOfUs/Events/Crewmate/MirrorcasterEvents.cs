using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Buttons;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class MirrorcasterEvents
{
    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (CheckForMagicMirror(@event, source, target))
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
        if (target == null || button is not IKillButton || !button.CanClick())
        {
            return;
        }

        if (CheckForMagicMirror(@event, source, target))
        {
            ResetButtonTimer(source, button);
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;

        if (source.Data.Role is not MirrorcasterRole)
        {
            return;
        }

        if (source.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
        {
            return;
        }

        var target = @event.Target;

        if (GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            if (!target.IsCrewmate() ||
                (target.TryGetModifier<AllianceGameModifier>(out var allyMod2) && !allyMod2.GetsPunished))
            {
                stats.CorrectKills += 1;
            }
            else if (source != target)
            {
                stats.IncorrectKills += 1;
            }
        }
    }

    private static bool CheckForMagicMirror(MiraCancelableEvent @event, PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return false;
        }

        // Magic Mirrors can NOT protect from Arsonist, bombs, veterans, anything of that nature.
        if (!target.HasModifier<MagicMirrorModifier>() ||
            source == null ||
            target.PlayerId == source.PlayerId ||
            source.HasModifier<IndirectAttackerModifier>() ||
            source.HasModifier<InvulnerabilityModifier>() ||
            source.HasModifier<VeteranAlertModifier>())
        {
            return false;
        }

        @event.Cancel();

        var mirrorcaster = target.GetModifier<MagicMirrorModifier>()?.Mirrorcaster.GetRole<MirrorcasterRole>();

        if (mirrorcaster != null && source.AmOwner)
        {
            MirrorcasterRole.RpcMagicMirrorAttacked(mirrorcaster.Player, source, target);
        }

        return true;
    }

    private static void ResetButtonTimer(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        button?.ResetCooldownAndOrEffect();

        if (source.Data.Role is WerewolfRole)
        {
            CustomButtonSingleton<WerewolfRampageButton>.Instance.ResetCooldownAndOrEffect();
        }

        // Reset impostor kill cooldown if they attack a shielded player
        if (!source.AmOwner || !source.IsImpostor())
        {
            return;
        }

        source.SetKillTimer(source.GetKillCooldown());
    }
}