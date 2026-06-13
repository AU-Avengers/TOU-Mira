using MiraAPI.GameOptions;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class SonarHeartbeatTargetModifier(PlayerControl owner, Color color, float update)
    : PingTargetModifier(owner, color, update)
{
    public override string ModifierName => "Sonar Heartbeat";

    public override void OnActivate()
    {
        base.OnActivate();

        if (Arrow == null)
        {
            return;
        }

        var playerColor = Palette.PlayerColors[Player.GetDefaultAppearance().ColorId];
        var spr = Arrow.gameObject.GetComponent<SpriteRenderer>();
        spr.color = playerColor;
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