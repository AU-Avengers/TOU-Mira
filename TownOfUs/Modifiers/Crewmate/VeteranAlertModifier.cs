using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class VeteranAlertModifier : TimedModifier
{
    public override float Duration => OptionGroupSingleton<VeteranOptions>.Instance.AlertDuration;
    public override string ModifierName => "Alerted";
    public override bool HideOnUi => true;
    public List<PlayerControl> AttackedPlayers { get; set; } = [];
    public bool WasAttacked(PlayerControl player) => AttackedPlayers.Contains(player);

    public override void OnActivate()
    {
        base.OnActivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.VeteranAlert, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeactivate()
    {
        base.OnActivate();

        // This gets applied after alerting isn't possible, incase vet is left with one use in a 1v1 scenario
        if (Player.Data.Role is VeteranRole vet)
        {
            vet.Alerts--;
        }
    }
    public void MarkPlayer(PlayerControl attacker)
    {
        if (!AttackedPlayers.Contains(attacker))
        {
            AttackedPlayers.Add(attacker);
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        ModifierComponent?.RemoveModifier(this);
    }
}