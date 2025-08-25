using TownOfUs.Modules.RainbowMod;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor;

public sealed class AmbusherArrowTargetModifier(PlayerControl owner, Color color, float update)
    : ArrowTargetModifier(owner, color, update)
{
    public override string ModifierName => "Ambusher Arrow";

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

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeath(DeathReason reason)
    {
        TouAudio.PlaySound(TouAudio.TrackerDeactivateSound);
        base.OnDeath(reason);
    }
}