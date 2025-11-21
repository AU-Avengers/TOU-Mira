using MiraAPI.GameOptions;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class TrackerArrowTargetModifier(PlayerControl owner, Color color, float update)
    : ArrowTargetModifier(owner, color, update)
{
    public override string ModifierName => "Tracker Arrow";

    public override void OnActivate()
    {
        base.OnActivate();

        if (Arrow == null)
        {
            return;
        }

        var spr = Arrow.gameObject.GetComponent<SpriteRenderer>();
        var r = Arrow.gameObject.AddComponent<BasicRainbowBehaviour>();

        r.AddRend(spr, Player.cosmetics.ColorId);
    }

    public override void OnDeath(DeathReason reason)
    {
        if (OptionGroupSingleton<SonarOptions>.Instance.SoundOnDeactivate && Owner.AmOwner)
        {
            TouAudio.PlaySound(TouAudio.TrackerDeactivateSound);
        }

        base.OnDeath(reason);
    }
}