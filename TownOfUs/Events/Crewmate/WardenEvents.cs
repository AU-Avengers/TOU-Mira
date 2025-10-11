using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class WardenEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        var wardenForts = ModifierUtils.GetActiveModifiers<WardenFortifiedModifier>();

        if (!wardenForts.Any())
        {
            return;
        }

        foreach (var mod in wardenForts)
        {
            var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
            var show = OptionGroupSingleton<WardenOptions>.Instance.ShowFortified;

            var showShieldedEveryone = show == FortifyOptions.Everyone;
            var showShieldedSelf = PlayerControl.LocalPlayer.PlayerId == mod.Player.PlayerId &&
                                   show is FortifyOptions.Self or FortifyOptions.SelfAndWarden;
            var showShieldedWarden = PlayerControl.LocalPlayer.PlayerId == mod.Warden.PlayerId &&
                                     show is FortifyOptions.Warden or FortifyOptions.SelfAndWarden;

            var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x =>
                x.ParentId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);
            var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x =>
                x.PlayerId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);

            mod.ShowFort = showShieldedEveryone || showShieldedSelf || showShieldedWarden ||
                           (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body);
        }
    }

    [RegisterEvent(-1)]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        // Logger<TownOfUsPlugin>.Error("WardenEvents KillButtonClickHandler");
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var source = PlayerControl.LocalPlayer;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick())
        {
            return;
        }

        CheckForWardenFortify(@event, source, target);
    }

    [RegisterEvent(-1)]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        CheckForWardenFortify(@event, source, target);
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var victim = @event.Target;

        foreach (var warden in CustomRoleUtils.GetActiveRolesOfType<WardenRole>())
        {
            if (victim == warden.Fortified)
            {
                warden.Clear();
            }
        }
    }

    private static void CheckForWardenFortify(MiraCancelableEvent @event, PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        if (!target.HasModifier<WardenFortifiedModifier>() || source == target ||
            (source.TryGetModifier<IndirectAttackerModifier>(out var indirect) && indirect.IgnoreShield))
        {
            return;
        }

        @event.Cancel();

        // The reason this exists is that otherwise, players can brute force through the warden fort if they spam fast enough
        if (@event is MiraButtonClickEvent buttonClick)
        {
            var button = buttonClick.Button as CustomActionButton<PlayerControl>;
            if (button != null)
            {
                button.Timer += 1f;
            }
        }

        if (@event is BeforeMurderEvent && source.IsImpostor())
        {
            source.SetKillTimer(source.killTimer + 1f);
        }

        // Find the warden which fortified the target
        var warden = target.GetModifier<WardenFortifiedModifier>()?.Warden.GetRole<WardenRole>();

        if (warden != null && source.AmOwner)
        {
            WardenRole.RpcWardenNotify(warden.Player, source, target);
        }
    }
}