using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.PolusImpostor;
using TownOfUs.Options.Roles.PolusImpostor;
using TownOfUs.Roles.TownOfPolus.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.TownofPolus.Impostor;

public sealed class PolusSwooperSwoopButton : TownOfUsRoleButton<PolusSwooperRole>, ILegacyButton
{
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override string Name => string.Empty;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<PolusSwooperOptions>.Instance.SwoopCooldown + MapCooldown, 5f, 120f);
    public override float EffectDuration => OptionGroupSingleton<PolusSwooperOptions>.Instance.SwoopDuration;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => PolusGgAssets.ButtonSwoop;

    public override bool ZeroIsInfinite { get; set; } = true;

    protected override void OnClick()
    {
        if (!EffectActive)
        {
            PlayerControl.LocalPlayer.RpcAddModifier<PolusSwoopModifier>();
            UsesLeft--;
            if (LimitedUses)
            {
                Button?.SetUsesRemaining(UsesLeft);
            }
        }
        else
        {
            OnEffectEnd();
        }
    }

    public override void OnEffectEnd()
    {
        if (!PlayerControl.LocalPlayer.HasModifier<PolusSwoopModifier>())
        {
            return;
        }

        PlayerControl.LocalPlayer.RpcRemoveModifier<PolusSwoopModifier>();
    }
}