using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

// CODE REVIEW 22/2/2025 AEDT (D/M/Y)
// ---------------------------------
// Should link this to the effect duration of the button?
// ie: make this a base modifier and just remove it once the button is done...
// or make swooper function like this?
public sealed class SurvivorVestButton : TownOfUsRoleButton<SurvivorRole>
{
    public override string Name => TouLocale.Get("TouRoleSurvivorSafeguard", "Safeguard");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Survivor;
    public override float Cooldown => OptionGroupSingleton<SurvivorOptions>.Instance.VestCooldown;
    public override float EffectDuration => OptionGroupSingleton<SurvivorOptions>.Instance.VestDuration;
    public override int MaxUses => (int)OptionGroupSingleton<SurvivorOptions>.Instance.MaxVests;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.VestSprite;

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcAddModifier<SurvivorVestModifier>();
    }
}