using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class EngineerFixButton : TownOfUsRoleButton<EngineerTouRole>
{
    public override string Name => TouLocale.Get("TouRoleEngineerFix", "Fix");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Engineer;
    public override float Cooldown => 0.001f + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<EngineerOptions>.Instance.FixDelay + 0.01f;
    public override int MaxUses => (int)OptionGroupSingleton<EngineerOptions>.Instance.MaxFixes;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.FixButtonSprite;
    public override bool ShouldPauseInVent => false;

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        Button?.cooldownTimerText.gameObject.SetActive(false);
    }
    public override void ClickHandler()
    {
        if (!CanClick() || PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() ||
            PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return;
        }
        
        OnClick();

        if (HasEffect)
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
        var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();

        return base.CanUse() && system is { AnyActive: true };
    }

    protected override void OnClick()
    {
        OverrideName(TouLocale.Get("TouRoleEngineerFixing", "Fixing"));
        var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();

        if (system is not { AnyActive: true })
        {
            ResetCooldownAndOrEffect();
        }
    }

    public override void OnEffectEnd()
    {
        OverrideName(TouLocale.Get("TouRoleEngineerFix", "Fix"));
        var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();

        if (system is { AnyActive: true })
        {
            List<LoadableAsset<AudioClip>> audio = [TouAudio.EngiFix1, TouAudio.EngiFix2, TouAudio.EngiFix3];
            TouAudio.PlaySound(audio.Random()!, 4f);
            EngineerTouRole.EngineerFix(PlayerControl.LocalPlayer);
            
            if (LimitedUses)
            {
                UsesLeft--;
                Button?.SetUsesRemaining(UsesLeft);
                TownOfUsColors.UseBasic = false;
                if (TextOutlineColor != Color.clear)
                {
                    SetTextOutline(TextOutlineColor);
                    if (Button != null)
                    {
                        Button.usesRemainingSprite.color = TextOutlineColor;
                    }
                }

                TownOfUsColors.UseBasic = TownOfUsPlugin.UseCrewmateTeamColor.Value;
            }
        }
    }
}