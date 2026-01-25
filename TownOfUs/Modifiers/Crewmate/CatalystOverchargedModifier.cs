using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class CatalystOverchargedModifier(PlayerControl catalyst) : BaseModifier
{
    public override string ModifierName => "Overcharged";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Catalyst;
    public override bool HideOnUi => false;
    public override string GetDescription() => "You are overcharged.\nYour cooldown decreases faster!";

    public PlayerControl Catalyst { get; } = catalyst;

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }

    public override void FixedUpdate()
    {
        if (!Player.AmOwner) return;
        
        var value = OptionGroupSingleton<CatalystOptions>.Instance.OverchargedMultiplier - 1;
        
        foreach (var ability in CustomButtonManager.Buttons)
        {
            if (ability.EffectActive) continue;
            if (ability.TimerPaused) continue;

            ability.DecreaseTimer(Time.deltaTime * value);
        }

        Player.killTimer -= Time.deltaTime * value;
    }
}