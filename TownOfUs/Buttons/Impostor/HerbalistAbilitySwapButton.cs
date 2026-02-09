using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class HerbalistAbilitySwapButton : TownOfUsRoleButton<HerbalistRole>
{
    public override string Name => "Change Herb";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.0001f;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.TraitorSelect;
    protected override void OnClick()
    {
        CustomButtonSingleton<HerbalistAbilityButton>.Instance.CycleAbility();
    }
}
