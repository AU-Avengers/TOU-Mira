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

public sealed class MorphlingMorphButton : TownOfUsRoleButton<MorphlingRole>, IAftermathableButton
{
    public override string Name => "Morph";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<MorphlingOptions>.Instance.MorphlingCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<MorphlingOptions>.Instance.MorphlingDuration;
    public override int MaxUses => (int)OptionGroupSingleton<MorphlingOptions>.Instance.MaxMorphs;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.MorphSprite;

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

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role is not { Sampled: null };
    }

    public override bool CanUse()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() || PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return ((Timer <= 0 && !EffectActive) || (EffectActive && Timer <= EffectDuration - 2f));
    }

    protected override void OnClick()
    {
        if (!EffectActive)
        {
            PlayerControl.LocalPlayer.RpcAddModifier<MorphlingMorphModifier>(Role.Sampled!);
            OverrideName("Unmorph");
            UsesLeft--;
            if (MaxUses != 0)
            {
                Button?.SetUsesRemaining(UsesLeft);
            }
        }
        else
        {
            PlayerControl.LocalPlayer.RpcRemoveModifier<MorphlingMorphModifier>();
            OverrideName("Morph");
        }
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();

        PlayerControl.LocalPlayer.RpcRemoveModifier<MorphlingMorphModifier>();
        OverrideName("Morph");
    }
}