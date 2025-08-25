using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class SwooperSwoopButton : TownOfUsRoleButton<SwooperRole>, IAftermathableButton
{
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override string Name => "Swoop";
    public override string Keybind => Keybinds.SecondaryAction;
    public override float Cooldown => OptionGroupSingleton<SwooperOptions>.Instance.SwoopCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<SwooperOptions>.Instance.SwoopDuration;
    public override int MaxUses => (int)OptionGroupSingleton<SwooperOptions>.Instance.MaxSwoops;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.SwoopSprite;

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        if (EffectActive)
        {
            Timer = Cooldown;
            EffectActive = false;
        }
        else if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = Cooldown;
        }
    }

    public override bool CanUse()
    {
        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() || PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return ((Timer <= 0 && !EffectActive && (MaxUses == 0 || UsesLeft > 0)) || (EffectActive && Timer <= EffectDuration - 2f));
    }

    protected override void OnClick()
    {
        if (!EffectActive)
        {
            PlayerControl.LocalPlayer.RpcAddModifier<SwoopModifier>();
            UsesLeft--;
            if (MaxUses != 0)
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
        if (!PlayerControl.LocalPlayer.HasModifier<SwoopModifier>())
        {
            return;
        }

        PlayerControl.LocalPlayer.RpcRemoveModifier<SwoopModifier>();
    }
}