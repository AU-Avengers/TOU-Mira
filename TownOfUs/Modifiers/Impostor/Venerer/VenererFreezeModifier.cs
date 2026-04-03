using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor.Venerer;

public sealed class VenererFreezeModifier(PlayerControl venerer) : TimedModifier, IVenererModifier
{
    public override string ModifierName => "Freeze";
    public override bool AutoStart => true;
    public override float Duration => OptionGroupSingleton<VenererOptions>.Instance.AbilityDuration;

    public PlayerControl Venerer { get; set; } = venerer;
    public float SpeedFactor { get; set; }

    public override void OnActivate()
    {
        var touAbilityEvent = new TouAbilityEvent(AbilityType.VenererFreezeAbility, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Player.HasDied() || Venerer.HasDied())
        {
            return;
        }

        var minFreezeSpeed = OptionGroupSingleton<VenererOptions>.Instance.MinFreezeSpeed;
        var freezeRadius = OptionGroupSingleton<VenererOptions>.Instance.FreezeRadius *
                           ShipStatus.Instance.MaxLightRadius;
        var rangeFromVenerer = (Player.GetTruePosition() - Venerer.GetTruePosition()).magnitude;

        SpeedFactor = 1f;

        if (rangeFromVenerer < freezeRadius)
        {
            SpeedFactor = Mathf.Lerp(minFreezeSpeed, 1f, rangeFromVenerer / freezeRadius);
        }
    }
}