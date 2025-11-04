using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Neutral;

public static class MercenaryEvents
{
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
    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var source = PlayerControl.LocalPlayer;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick())
        {
            return;
        }

        // only check if this interaction was via a custom button
        CheckForMercenaryGuard(@event, source, target);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        // only check if this interaction was via the standard kill button
        if (source.Data.Role is ICustomRole { Configuration.UseVanillaKillButton: true } ||
            (source.Data.Role is not ICustomRole && source.IsImpostor()))
        {
            CheckForMercenaryGuard(@event, source, target);
        }
    }

    private static void CheckForMercenaryGuard(MiraCancelableEvent @event, PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        if (!target.TryGetModifier<MercenaryGuardModifier>(out var guardMod))
        {
            return;
        }

        var mercOpts = OptionGroupSingleton<MercenaryOptions>.Instance;

        var noAttack = (target.PlayerId == source.PlayerId ||
                        source.HasModifier<IndirectAttackerModifier>() ||
                        source.HasModifier<InvulnerabilityModifier>() ||
                        source.HasModifier<VeteranAlertModifier>() ||
                        @event is not BeforeMurderEvent);

        var isAttack = false;
        CustomActionButton<PlayerControl>? button = null;
        if (@event is MiraButtonClickEvent clickEvent)
        {
            var button2 = clickEvent.Button as CustomActionButton<PlayerControl>;
            var target2 = button?.Target;

            if (target2 != null && button != null && button.CanClick())
            {
                button = button2;
            }
        }
        if (mercOpts.GuardProtection.Value && (!noAttack || isAttack))
        {
            @event.Cancel();
            ResetButtonTimer(source, button);
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Mercenary, alpha: 0.5f));
        }

        var mercenary = guardMod.Mercenary;

        if (mercenary != null && source.AmOwner)
        {
            MercenaryRole.RpcGuarded(mercenary, target, mercOpts.GuardProtection.Value && (!noAttack || isAttack));
        }
    }
}