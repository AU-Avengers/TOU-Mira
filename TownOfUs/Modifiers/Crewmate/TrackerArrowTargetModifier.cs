using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class TrackerArrowTargetModifier(PlayerControl owner, Color color, float update)
    : PingTargetModifier(owner, color, update)
{
    public override string ModifierName => "Sonar Arrow";

    public override void OnActivate()
    {
        base.OnActivate();

        if (Arrow == null)
        {
            return;
        }

        var spr = Arrow.gameObject.GetComponent<SpriteRenderer>();
        spr.color = Color.white;
        var materialColor =
            Player.cosmetics.currentBodySprite.BodySprite.material.GetColor(ShaderID.BodyColor);
        spr.material = HatManager.Instance.PlayerMaterial;

        PlayerMaterial.SetColors(materialColor, spr);
        spr.material.SetColor(ShaderID.VisorColor, materialColor);
        spr.material.SetColor(ShaderID.BackColor, materialColor);
        spr.material.SetColor(ShaderID.BodyColor, materialColor);
        var r = Arrow.gameObject.GetComponent<RainbowBehaviour>();

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