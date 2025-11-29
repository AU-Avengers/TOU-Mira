using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.HnsCrewmate;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.HnsCrewmate;
using TownOfUs.Roles.HnsCrewmate;
using UnityEngine;

namespace TownOfUs.Buttons.HnsCrewmate;

public sealed class ChameleonSwoopButton : TownOfUsRoleButton<HnsChameleonRole>
{
    public override Color TextOutlineColor => TownOfUsColors.Chameleon;
    public override string Name => TouLocale.GetParsed("HnsRoleChameleonSwoop", "Swoop");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<HnsChameleonOptions>.Instance.SwoopCooldown + MapCooldown, 5f, 120f);
    public override float EffectDuration => OptionGroupSingleton<HnsChameleonOptions>.Instance.SwoopDuration;
    public override int MaxUses => (int)OptionGroupSingleton<HnsChameleonOptions>.Instance.MaxSwoops;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.CrewSwoopSprite;

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
        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() || PlayerControl.LocalPlayer
                .GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return ((Timer <= 0 && !EffectActive && (!LimitedUses || UsesLeft > 0)) ||
                (EffectActive && Timer <= EffectDuration - 2f));
    }

    protected override void OnClick()
    {
        if (!EffectActive)
        {
            PlayerControl.LocalPlayer.RpcAddModifier<HnsChameleonSwoopModifier>();
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
        if (!PlayerControl.LocalPlayer.HasModifier<HnsChameleonSwoopModifier>())
        {
            return;
        }

        PlayerControl.LocalPlayer.RpcRemoveModifier<HnsChameleonSwoopModifier>();
    }
}