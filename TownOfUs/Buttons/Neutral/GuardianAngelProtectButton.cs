using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class GuardianAngelProtectButton : TownOfUsRoleButton<FairyRole>
{
    public override string Name => TouLocale.GetParsed("TouRoleFairyProtect", "Protect");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.GuardianAngel;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<FairyOptions>.Instance.ProtectCooldown + MapCooldown, 5f, 120f);
    public override float EffectDuration => OptionGroupSingleton<FairyOptions>.Instance.ProtectDuration;
    public override int MaxUses => (int)OptionGroupSingleton<FairyOptions>.Instance.MaxProtects;
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