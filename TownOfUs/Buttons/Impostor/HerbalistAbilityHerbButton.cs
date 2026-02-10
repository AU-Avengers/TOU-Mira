using System.Globalization;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Impostor.Herbalist;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class HerbalistAbilityHerbButton : TownOfUsRoleButton<HerbalistRole, PlayerControl>
{
    public override string Name => "Kill";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<HerbalistOptions>.Instance.HerbCooldown + MapCooldown, 5f, 120f);
    public PlayerControl? _selectedTarget;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && CurrentAbility is not HerbAbilities.Kill;
    }

    public override bool CanUse()
    {
        return base.CanUse() && CurrentAbility is not HerbAbilities.Kill;
    }

    public override float EffectDuration
    {
        get
        {
            if (CurrentAbility is HerbAbilities.Confuse)
            {
                return Mathf.Clamp(OptionGroupSingleton<HerbalistOptions>.Instance.ConfuseDelay, 0.5f, 30f);
            }
            return 0.0001f;
        }
    }
    public override LoadableAsset<Sprite> Sprite => ProtectionButtons[0];
    public HerbAbilities CurrentAbility = HerbAbilities.Kill;

    public static List<LoadableAsset<Sprite>> ProtectionButtons { get; set; } = new()
    {
        TouAssets.KillSprite,
        TouImpAssets.BlackmailSprite,
        TouImpAssets.HypnotiseButtonSprite,
        //TouImpAssets.FlashSprite,
        TouCrewAssets.BarrierSprite,
    };

    public static List<string> ProtectionText { get; set; } = new()
    {
        "Kill",
        "Expose",
        "Confuse",
        //"Glamour",
        "Protect",
    };

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }
        switch (CurrentAbility)
        {
            case HerbAbilities.Expose:
                Target.RpcAddModifier<HerbalistExposedModifier>(PlayerControl.LocalPlayer);
                break;
            case HerbAbilities.Protect:
                Target.RpcAddModifier<HerbalistProtectionModifier>(PlayerControl.LocalPlayer);
                break;
        }

        _selectedTarget = Target;
    }

    public void UpdateCooldownHandler(PlayerControl playerControl)
    {
        if (Timer >= 0)
        {
            var shouldPauseInVent = ShouldPauseInVent && PlayerControl.LocalPlayer.inVent && !EffectActive;

            if (!TimerPaused && !OptionGroupSingleton<VanillaTweakOptions>.Instance.CanPauseCooldown &&
                (!shouldPauseInVent || EffectActive))
            {
                Timer -= Time.deltaTime;
            }
        }
        else if (HasEffect && EffectActive)
        {
            EffectActive = false;
            Timer = Cooldown;
            OnEffectEnd();
        }

        if (Button != null)
        {
            if (EffectActive)
            {
                Button.SetFillUp(Timer, EffectDuration);

                Button.cooldownTimerText.text =
                    Timer.ToString(CooldownTimerFormatString, NumberFormatInfo.InvariantInfo);
                Button.cooldownTimerText.gameObject.SetActive(true);
            }
            else
            {
                Button.SetCooldownFormat(Timer, Cooldown, CooldownTimerFormatString);
            }
        }
    }

    public override void OnEffectEnd()
    {
        if (_selectedTarget == null)
        {
            _selectedTarget = null;
            return;
        }
        if (CurrentAbility is HerbAbilities.Confuse)
        {
            _selectedTarget.RpcAddModifier<HerbalistConfusedModifier>(PlayerControl.LocalPlayer);
        }

        _selectedTarget = null;
    }

    public void CycleAbility()
    {
        var stepUp = (HerbAbilities)((int)CurrentAbility + 1);
        if (Enum.IsDefined(stepUp))
        {
            CurrentAbility = stepUp;
        }
        else
        {
            CurrentAbility = HerbAbilities.Kill;
        }
        OverrideSprite(ProtectionButtons[(int)CurrentAbility].LoadAsset());
        OverrideName(ProtectionText[(int)CurrentAbility]);
    }
    
    private static Func<HerbalistExposedModifier, bool> ExposedPredicate { get; } =
        msModifier => msModifier.Herbalist.AmOwner;
    
    private static Func<HerbalistConfusedModifier, bool> ConfusedPredicate { get; } =
        msModifier => msModifier.Herbalist.AmOwner;
    
    private static Func<HerbalistProtectionModifier, bool> ProtectedPredicate { get; } =
        msModifier => msModifier.Herbalist.AmOwner;

    public override PlayerControl? GetTarget()
    {
        if (CurrentAbility is HerbAbilities.Expose)
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsImpostorAligned() && !x.HasModifier(ExposedPredicate));
        }
        if (CurrentAbility is HerbAbilities.Confuse)
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsImpostorAligned() && !x.HasModifier(ConfusedPredicate));
        }
        if (CurrentAbility is HerbAbilities.Protect)
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.HasModifier(ProtectedPredicate));
        }
        return MiscUtils.GetImpostorTarget(Distance);
    }
}
