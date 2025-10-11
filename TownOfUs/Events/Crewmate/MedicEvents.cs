using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Buttons;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class MedicEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (PlayerControl.LocalPlayer.Data.Role is MedicRole)
        {
            MedicRole.OnRoundStart();
        }

        var medicShields = ModifierUtils.GetActiveModifiers<MedicShieldModifier>();

        if (!medicShields.Any())
        {
            return;
        }

        foreach (var mod in medicShields)
        {
            var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
            var showShielded = OptionGroupSingleton<MedicOptions>.Instance.ShowShielded;

            var showShieldedEveryone = showShielded == MedicOption.Everyone;
            var showShieldedSelf = PlayerControl.LocalPlayer.PlayerId == mod.Player.PlayerId &&
                                   showShielded is MedicOption.Shielded or MedicOption.ShieldedAndMedic;
            var showShieldedMedic = PlayerControl.LocalPlayer.PlayerId == mod.Medic.PlayerId &&
                                    showShielded is MedicOption.Medic or MedicOption.ShieldedAndMedic;

            var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x =>
                x.ParentId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);
            var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x =>
                x.PlayerId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);

            mod.ShowShield = showShieldedEveryone || showShieldedSelf || showShieldedMedic ||
                             (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body);
        }
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (CheckForMedicShield(@event, source, target))
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

        if (CheckForMedicShield(@event, source, target))
        {
            ResetButtonTimer(source, button);
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var victim = @event.Target;

        foreach (var medic in CustomRoleUtils.GetActiveRolesOfType<MedicRole>())
        {
            if (victim == medic.Shielded)
            {
                medic.Clear();
            }
        }

        if (victim.TryGetModifier<MedicShieldModifier>(out var medMod)
            && PlayerControl.LocalPlayer.Data.Role is MedicRole
            && medMod.Medic.AmOwner)
        {
            CustomButtonSingleton<MedicShieldButton>.Instance.CanChangeTarget = true;
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled == null)
        {
            return;
        }

        if (exiled.TryGetModifier<MedicShieldModifier>(out var medMod)
            && PlayerControl.LocalPlayer.Data.Role is MedicRole
            && medMod.Medic.AmOwner)
        {
            CustomButtonSingleton<MedicShieldButton>.Instance.CanChangeTarget = true;
        }
    }

    [RegisterEvent]
    public static void PlayerLeaveEventHandler(PlayerLeaveEvent @event)
    {
        var player = @event.ClientData.Character;

        if (!player)
        {
            return;
        }

        if (player && player.TryGetModifier<MedicShieldModifier>(out var medMod)
                   && PlayerControl.LocalPlayer.Data.Role is MedicRole
                   && medMod.Medic.AmOwner)
        {
            CustomButtonSingleton<MedicShieldButton>.Instance.CanChangeTarget = true;
        }
    }

    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        if (@event.Target == null)
        {
            return;
        }

        if (@event.Reporter.Data.Role is MedicRole medic && @event.Reporter.AmOwner)
        {
            medic.Report(@event.Target.PlayerId);
        }
    }

    private static bool CheckForMedicShield(MiraCancelableEvent @event, PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return false;
        }

        if (!target.HasModifier<MedicShieldModifier>() ||
            source == null ||
            target.PlayerId == source.PlayerId ||
            (source.TryGetModifier<IndirectAttackerModifier>(out var indirect) && indirect.IgnoreShield))
        {
            return false;
        }

        @event.Cancel();

        var medic = target.GetModifier<MedicShieldModifier>()?.Medic.GetRole<MedicRole>();

        if (medic != null && source.AmOwner)
        {
            MedicRole.RpcMedicShieldAttacked(medic.Player, source, target);
        }

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