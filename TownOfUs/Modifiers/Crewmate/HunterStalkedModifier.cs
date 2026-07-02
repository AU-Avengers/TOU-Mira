using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class HunterStalkedModifier(PlayerControl hunter) : TimedModifier
{
    public override string ModifierName => "Stalked";
    public override bool HideOnUi => true;
    public override float Duration => OptionGroupSingleton<HunterOptions>.Instance.HunterStalkDuration;
    public PlayerControl Hunter { get; set; } = hunter;

    public override void OnActivate()
    {
        base.OnActivate();
        var touAbilityEvent = new TouAbilityEvent(AbilityType.HunterStalk, Hunter, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (PlayerControl.LocalPlayer.Data.Role is HunterRole)
        {
            Player?.cosmetics.SetOutline(true, TownOfUsColors.Hunter);
        }
    }

    public override void OnDeactivate()
    {
        Player.cosmetics.SetOutline(false, TownOfUsColors.Hunter);
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.cosmetics.SetOutline(false, TownOfUsColors.Hunter);
    }
}