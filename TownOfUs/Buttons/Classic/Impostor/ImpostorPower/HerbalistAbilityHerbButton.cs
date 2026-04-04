using System.Globalization;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Impostor.Herbalist;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class HerbalistAbilityHerbButton : TownOfUsRoleButton<HerbalistRole, PlayerControl>
{
    public override string Name => "Kill";
    public string CurrentName = "Kill";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<HerbalistOptions>.Instance.HerbCooldown + MapCooldown, 5f, 120f);
    private PlayerControl? _selectedTarget;
    // -2 means the value isn't set, -1 means it is infinite.
    public int ExposeUsesLeft { get; set; } = -2;
    public int ConfuseUsesLeft { get; set; } = -2;
    public int ProtectUsesLeft { get; set; } = -2;

    public int CurrentHerbUses()
    {
        switch (CurrentAbility)
        {
            case HerbAbilities.Expose:
                return ExposeUsesLeft;
            case HerbAbilities.Confuse:
                return ConfuseUsesLeft;
            case HerbAbilities.Protect:
                return ProtectUsesLeft;
        }

        return -1;
    }

    public bool CurrentHerbsLimited => CurrentHerbUses() != -1;
    public string CurrentHerbsText => CurrentHerbUses().ToString(TownOfUsPlugin.Culture);

    public bool CanUseHerbs => !CurrentHerbsLimited || CurrentHerbUses() > 0;

    public override void ClickHandler()
    {
        if (CanClick() && !PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() &&
            !PlayerControl.LocalPlayer.HasModifier<DisabledModifier>())
        {
            if (CurrentHerbsLimited)
            {
                switch (CurrentAbility)
                {
                    case HerbAbilities.Expose:
                         ExposeUsesLeft--;
                         break;
                    case HerbAbilities.Confuse:
                         ConfuseUsesLeft--;
                         break;
                    case HerbAbilities.Protect:
                         ProtectUsesLeft--;
                         break;
                }
                Warning($"Expose Uses: {ExposeUsesLeft} | Confuse Uses: {ConfuseUsesLeft} | Protect Uses: {ProtectUsesLeft}");
            }
            Button!.OverrideText(CurrentHerbsLimited ? (CurrentName + " - " + CurrentHerbsText) : CurrentName);

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
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && CurrentAbility is not HerbAbilities.Kill;
    }

    public override bool CanUse()
    {
        return base.CanUse() && CanUseHerbs && CurrentAbility is not HerbAbilities.Kill;
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
    public override LoadableAsset<Sprite> Sprite => HerbButtons[0];
    public HerbAbilities CurrentAbility = HerbAbilities.Kill;

    public static List<LoadableAsset<Sprite>> HerbButtons { get; set; } = new()
    {
        TouAssets.KillSprite,
        TouImpAssets.HerbExposeSprite,
        TouImpAssets.HerbConfuseSprite,
        TouImpAssets.HerbProtectSprite,
    };

    public static List<string> ProtectionText { get; set; } = new()
    {
        "Kill",
        "Expose",
        "Confuse",
        "Protect",
    };

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        Button!.usesRemainingSprite.sprite = TouAssets.AbilityCounterKillSprite.LoadAsset();
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }
        switch (CurrentAbility)
        {
            case HerbAbilities.Expose:
                Target.RpcAddModifier<HerbalistExposedModifier>(PlayerControl.LocalPlayer, OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode);
                break;
            case HerbAbilities.Protect:
                Target.RpcAddModifier<HerbalistProtectionModifier>(PlayerControl.LocalPlayer);
                break;
        }

        _selectedTarget = Target;
    }

    public void UpdateMiniAbilityCooldown(float cooldown)
    {
        if (Button == null)
        {
            return;
        }
        Button.usesRemainingText.gameObject.SetActive(true);
        Button.usesRemainingSprite.gameObject.SetActive(true);
        Button.usesRemainingText.text = (int)cooldown + "<size=80%>s</size>";
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
        OverrideSprite(HerbButtons[(int)CurrentAbility].LoadAsset());
        OverrideName(ProtectionText[(int)CurrentAbility]);
    }
    public override void OverrideName(string name)
    {
        CurrentName = name;
        Button?.OverrideText(CurrentHerbsLimited ? (CurrentName + " - " + CurrentHerbsText) : CurrentName);
    }
    private static Func<HerbalistExposedModifier, bool> ExposedPredicate { get; } =
        msModifier => msModifier.Herbalist.AmOwner;
    
    private static Func<HerbalistConfusedModifier, bool> ConfusedPredicate { get; } =
        msModifier => msModifier.Herbalist.AmOwner;
    
    private static Func<HerbalistProtectionModifier, bool> ProtectedPredicate { get; } =
        msModifier => msModifier.Herbalist.AmOwner;

    public override PlayerControl? GetTarget()
    {
        var isFfa = OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode;
        if (CurrentAbility is HerbAbilities.Expose)
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => (isFfa || !x.IsImpostorAligned()) && !x.HasModifier(ExposedPredicate));
        }
        if (CurrentAbility is HerbAbilities.Confuse)
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => (isFfa || !x.IsImpostorAligned()) && !x.HasModifier(ConfusedPredicate));
        }
        if (CurrentAbility is HerbAbilities.Protect)
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.HasModifier(ProtectedPredicate));
        }
        return MiscUtils.GetImpostorTarget(Distance);
    }
}
