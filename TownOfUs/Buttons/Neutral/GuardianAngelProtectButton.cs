using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class GuardianAngelProtectButton : TownOfUsRoleButton<GuardianAngelTouRole>
{
    public override string Name => "Protect";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.GuardianAngel;
    public override float Cooldown => OptionGroupSingleton<GuardianAngelOptions>.Instance.ProtectCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<GuardianAngelOptions>.Instance.ProtectDuration;
    public override int MaxUses => (int)OptionGroupSingleton<GuardianAngelOptions>.Instance.MaxProtects;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.ProtectSprite;

    protected override void OnClick()
    {
        if (Role.Target == null || Role.Target.HasDied())
        {
            return;
        }
        
        Role.Target.RpcAddModifier<GuardianAngelProtectModifier>(PlayerControl.LocalPlayer);
    }
}