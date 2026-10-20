using MiraAPI.Events;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Events.TouEvents;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class OracleConfessModifier(PlayerControl oracle, int faction) : BaseModifier
{
    public override string ModifierName => "Confess";
    public override bool HideOnUi => true;
    public PlayerControl Oracle { get; } = oracle;
    public ModdedRoleTeams RevealedFaction { get; set; }
    public bool ConfessToAll { get; set; }

    public override void OnActivate()
    {
        base.OnActivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.OracleConfess, Oracle, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (faction == 0)
        {
            RevealedFaction = ModdedRoleTeams.Crewmate;
        }
        else if (faction == 1)
        {
            RevealedFaction = ModdedRoleTeams.Custom;
        }
        else
        {
            RevealedFaction = ModdedRoleTeams.Impostor;
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        RemoveSelf();
    }
}