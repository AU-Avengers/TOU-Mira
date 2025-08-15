using MiraAPI.Events;
using MiraAPI.Modifiers.Types;
using TownOfUs.Events.TouEvents;
using TownOfUs.Roles.Neutral;
using UnityEngine;
using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Neutral;

namespace TownOfUs.Modifiers.Neutral;

public sealed class PredatorStaringModifier(PlayerControl predator) : TimedModifier
{
    public override string ModifierName => "Staring";
    public override bool HideOnUi => true;
    public override float Duration => OptionGroupSingleton<PredatorOptions>.Instance.PredatorStareDuration;
    public PlayerControl Predator { get; set; } = predator;

    public override void OnActivate()
    {
        base.OnActivate();
        var touAbilityEvent = new TouAbilityEvent(AbilityType.PredatorStare, Predator, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (PlayerControl.LocalPlayer.Data.Role is PredatorRole)
        {
            Player?.cosmetics.SetOutline(true, new Il2CppSystem.Nullable<Color>(TownOfUsColors.Predator));
        }
    }

    public override void OnDeactivate()
    {
        Player.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(TownOfUsColors.Predator));
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(TownOfUsColors.Predator));
    }
}