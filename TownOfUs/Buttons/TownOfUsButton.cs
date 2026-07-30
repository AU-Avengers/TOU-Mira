using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using System.Globalization;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Maps;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Buttons;

#pragma warning disable S3060
[MiraIgnore]
public abstract class TownOfUsButton : CustomActionButton
{
    public override string Name => string.Empty;

    public static float MapCooldown =>
        TownOfUsMapOptions.GetMapBasedCooldownDifference();

    public override bool ZeroIsInfinite { get; set; }

    public override float InitialCooldown => 10;
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override string CooldownTimerFormatString =>
        Timer <= 10f && LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.PreciseCooldownsToggle.Value
            ? "0.0"
            : "0";

    public virtual bool Disabled { get; set; }
    public virtual bool UsableInDeath => false;
    public virtual bool UsableFirstRound => true;
    public virtual bool ShouldPauseInVent => true;
    public SpriteRenderer FirstRoundLock;

    public void CreateRoundLockIcon()
    {
        var hackedSprite = new GameObject("RoundOneLockSprite");
        hackedSprite.transform.SetParent(Button!.transform);
        hackedSprite.transform.localPosition = new Vector3(0, 0, -10f);
        hackedSprite.gameObject.layer = Button!.gameObject.layer;

        var render = hackedSprite.AddComponent<SpriteRenderer>();
        render.sprite = TouAssets.FirstRoundLockSprite.LoadAsset();
        FirstRoundLock = render;

        SetRoundLockActive(false);
    }

    public void SetRoundLockActive(bool isActive)
    {
        if (FirstRoundLock && FirstRoundLock.gameObject)
        {
            FirstRoundLock.gameObject.SetActive(isActive);
            FirstRoundLock.enabled = isActive;
        }
    }

    public PassiveButton PassiveComp { get; set; }

    public virtual int ConsoleBind()
    {
        var bind = -1;
        if (Keybind == Keybinds.PrimaryAction)
        {
            bind = Keybinds.PrimaryConsole;
        }
        else if (Keybind == Keybinds.SecondaryAction)
        {
            bind = Keybinds.SecondaryConsole;
        }
        else if (Keybind == Keybinds.ModifierAction)
        {
            bind = Keybinds.ModifierConsole;
        }
        else if (Keybind == Keybinds.VentAction)
        {
            bind = Keybinds.VentConsole;
        }

        return bind;
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        if (Timer >= 0)
        {
            var shouldPauseInVent = ShouldPauseInVent && PlayerControl.LocalPlayer.inVent && !EffectActive;
            
            if (!TimerPaused && !OptionGroupSingleton<VanillaTweakOptions>.Instance.CanPauseCooldown && (!shouldPauseInVent || EffectActive))
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

        if (Button)
        {
            if (CanUse())
            {
                Button!.SetEnabled();
            }
            else
            {
                Button!.SetDisabled();
            }

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

        FixedUpdate(playerControl);
    }

    public override void SetActive(bool visible, RoleBehaviour role)
    {
        Button?.ToggleVisible(visible && Enabled(role) && !role.Player.HasDied());
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        if (Button == null)
        {
            Error($"Button is null for {GetType().FullName}");
            return;
        }

        CreateRoundLockIcon();

        Button.usesRemainingSprite.sprite = this is ILegacyCapable && LegacyAssets.IsLegacy ? TouAssets.BlankSprite.LoadAsset() : TouAssets.AbilityCounterBasicSprite.LoadAsset();

        TownOfUsColors.UseBasic = false;
        if (TextOutlineColor != Color.clear)
        {
            SetTextOutline(TextOutlineColor);
            Button.usesRemainingSprite.color = TextOutlineColor;
        }

        TownOfUsColors.UseBasic =
            LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance.UseCrewmateTeamColorToggle.Value;

        PassiveComp = Button.GetComponent<PassiveButton>();
    }

    public override void SetUses(int amount)
    {
        base.SetUses(amount);
        TownOfUsColors.UseBasic = false;
        if (TextOutlineColor != Color.clear)
        {
            SetTextOutline(TextOutlineColor);
            Button!.usesRemainingSprite.color = TextOutlineColor;
        }

        TownOfUsColors.UseBasic =
            LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance.UseCrewmateTeamColorToggle.Value;
    }

    public override bool CanUse()
    {
        if (!PlayerControl.LocalPlayer)
        {
            return false;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return false;
        }

        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (!UsableFirstRound && DeathEventHandlers.CurrentRound == 1 && !TutorialManager.InstanceExists)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasDied() && !UsableInDeath)
        {
            return false;
        }

        if (!PlayerControl.LocalPlayer.CanMove ||
            PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return base.CanUse();
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (MeetingHud.Instance)
        {
            return;
        }

        Button?.gameObject.SetActive(HudManager.Instance.UseButton.isActiveAndEnabled ||
                                     HudManager.Instance.PetButton.isActiveAndEnabled);
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        if (LimitedUses)
        {
            UsesLeft--;
            Button?.SetUsesRemaining(UsesLeft);
            TownOfUsColors.UseBasic = false;
            if (TextOutlineColor != Color.clear)
            {
                SetTextOutline(TextOutlineColor);
                Button?.usesRemainingSprite.color = TextOutlineColor;
            }

            TownOfUsColors.UseBasic = LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance
                .UseCrewmateTeamColorToggle.Value;
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
}

[MiraIgnore]
public abstract class TownOfUsTargetButton<T> : CustomActionButton<T> where T : MonoBehaviour
{
    public override string Name => string.Empty;

    public static float MapCooldown =>
        TownOfUsMapOptions.GetMapBasedCooldownDifference();

    public override float InitialCooldown => 10;
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override string CooldownTimerFormatString =>
        Timer <= 10f && LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.PreciseCooldownsToggle.Value
            ? "0.0"
            : "0";

    public virtual bool Disabled { get; set; }
    public virtual bool ShouldPauseInVent => true;
    public virtual bool UsableInDeath => false;
    public virtual bool UsableFirstRound => true;
    public SpriteRenderer FirstRoundLock;

    public void CreateRoundLockIcon()
    {
        var hackedSprite = new GameObject("RoundOneLockSprite");
        hackedSprite.transform.SetParent(Button!.transform);
        hackedSprite.transform.localPosition = new Vector3(0, 0, -10f);
        hackedSprite.gameObject.layer = Button!.gameObject.layer;

        var render = hackedSprite.AddComponent<SpriteRenderer>();
        render.sprite = TouAssets.FirstRoundLockSprite.LoadAsset();
        FirstRoundLock = render;

        SetRoundLockActive(false);
    }

    public void SetRoundLockActive(bool isActive)
    {
        if (FirstRoundLock && FirstRoundLock.gameObject)
        {
            FirstRoundLock.gameObject.SetActive(isActive);
            FirstRoundLock.enabled = isActive;
        }
    }

    public PassiveButton PassiveComp { get; set; }

    public virtual int ConsoleBind()
    {
        var bind = -1;
        if (Keybind == Keybinds.PrimaryAction)
        {
            bind = Keybinds.PrimaryConsole;
        }
        else if (Keybind == Keybinds.SecondaryAction)
        {
            bind = Keybinds.SecondaryConsole;
        }
        else if (Keybind == Keybinds.ModifierAction)
        {
            bind = Keybinds.ModifierConsole;
        }
        else if (Keybind == Keybinds.VentAction)
        {
            bind = Keybinds.VentConsole;
        }

        return bind;
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        if (Timer >= 0)
        {
            var shouldPauseInVent = ShouldPauseInVent && PlayerControl.LocalPlayer.inVent && !EffectActive;
            
            if (!TimerPaused && !OptionGroupSingleton<VanillaTweakOptions>.Instance.CanPauseCooldown && (!shouldPauseInVent || EffectActive))
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

        if (Button)
        {
            if (CanUse())
            {
                Button!.SetEnabled();
            }
            else
            {
                Button!.SetDisabled();
            }

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

        FixedUpdate(playerControl);
    }

    public override void SetActive(bool visible, RoleBehaviour role)
    {
        Button?.ToggleVisible(visible && Enabled(role) && !role.Player.HasDied());
    }

    public override bool CanUse()
    {
        if (TimeLordRewindSystem.IsRewinding)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasDied() && !UsableInDeath)
        {
            return false;
        }

        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (!UsableFirstRound && DeathEventHandlers.CurrentRound == 1 && !TutorialManager.InstanceExists)
        {
            return false;
        }

        if (!PlayerControl.LocalPlayer.CanMove ||
            PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return base.CanUse();
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        if (Button == null)
        {
            Error($"Button is null for {GetType().FullName}");
            return;
        }

        CreateRoundLockIcon();

        if (this is ILegacyCapable && LegacyAssets.IsLegacy)
        {
            Button.usesRemainingSprite.sprite = TouAssets.BlankSprite.LoadAsset();
        }
        else
        {
            Button.usesRemainingSprite.sprite = typeof(T) switch
            {
                Type t when t == typeof(Vent) => TouAssets.AbilityCounterVentSprite.LoadAsset(),
                Type t when t == typeof(DeadBody) => TouAssets.AbilityCounterBodySprite.LoadAsset(),
                Type t when t == typeof(PlayerControl) => TouAssets.AbilityCounterPlayerSprite.LoadAsset(),
                _ => TouAssets.AbilityCounterBasicSprite.LoadAsset(),
            };
        }

        TownOfUsColors.UseBasic = false;
        if (TextOutlineColor != Color.clear)
        {
            SetTextOutline(TextOutlineColor);
            Button.usesRemainingSprite.color = TextOutlineColor;
        }

        TownOfUsColors.UseBasic =
            LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance.UseCrewmateTeamColorToggle.Value;

        PassiveComp = Button.GetComponent<PassiveButton>();
    }

    public override void SetUses(int amount)
    {
        base.SetUses(amount);
        TownOfUsColors.UseBasic = false;
        if (TextOutlineColor != Color.clear)
        {
            SetTextOutline(TextOutlineColor);
            Button!.usesRemainingSprite.color = TextOutlineColor;
        }

        TownOfUsColors.UseBasic =
            LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance.UseCrewmateTeamColorToggle.Value;
    }

    public override void ClickHandler()
    {
        if (CanClick())
        {
            if (LimitedUses)
            {
                UsesLeft--;
                Button?.SetUsesRemaining(UsesLeft);
                TownOfUsColors.UseBasic = false;
                if (TextOutlineColor != Color.clear)
                {
                    SetTextOutline(TextOutlineColor);
                    Button?.usesRemainingSprite.color = TextOutlineColor;
                }

                TownOfUsColors.UseBasic = LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance
                    .UseCrewmateTeamColorToggle.Value;
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
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (MeetingHud.Instance)
        {
            return;
        }

        Button?.gameObject.SetActive(HudManager.Instance.UseButton.isActiveAndEnabled ||
                                     HudManager.Instance.PetButton.isActiveAndEnabled);
    }
}

[MiraIgnore]
public abstract class TownOfUsRoleButton<TRole> : TownOfUsButton where TRole : RoleBehaviour
{
    public TRole Role => PlayerControl.LocalPlayer.GetRole<TRole>()!;

    public override bool Enabled(RoleBehaviour? role)
    {
        return !Disabled && role is TRole && Role;
    }

    protected virtual bool ShouldTrackKillCooldown()
    {
        return false;
    }
}

[MiraIgnore]
public abstract class TownOfUsRoleButton<TRole, TTarget> : TownOfUsTargetButton<TTarget>
    where TTarget : MonoBehaviour where TRole : RoleBehaviour
{
    public TRole Role => PlayerControl.LocalPlayer.GetRole<TRole>()!;

    public override bool Enabled(RoleBehaviour? role)
    {
        return !Disabled && role is TRole && Role;
    }

    protected virtual bool ShouldTrackKillCooldown()
    {
        return false;
    }

    public override void SetOutline(bool active)
    {
        if (Target != null && !PlayerControl.LocalPlayer.HasDied())
        {
            if (Target is PlayerControl target)
            {
                target.cosmetics.currentBodySprite.BodySprite.SetOutline(active ? Role.TeamColor : null);
            }
            else if (Target is DeadBody body)
            {
                body.bodyRenderers.Do(x => x.SetOutline(active ? Role.TeamColor : null));
            }
            else if (Target is Vent vent)
            {
                vent.SetOutline(active, true, Role.TeamColor);
            }
        }
    }

    public override bool IsTargetValid(TTarget? target)
    {
        if (target is PlayerControl playerTarget)
        {
            return base.IsTargetValid(target) && !playerTarget.inVent &&
                   !playerTarget.GetModifiers<DisabledModifier>().Any(mod => !mod.CanBeInteractedWith) &&
                   !SpectatorRole.TrackedSpectators.Contains(playerTarget.Data.PlayerName);
        }

        return base.IsTargetValid(target);
    }
}

public interface IAftermathablePlayerButton : IAftermathableButton
{
    PlayerControl? Target { get; set; }
}

public interface IAftermathableBodyButton : IAftermathableButton
{
    DeadBody? Target { get; set; }
}

public interface IAftermathableButton
{
    void AftermathHandler();
}

public interface IDiseaseableButton
{
    void SetDiseasedTimer(float multiplier);
}

public interface IKillButton
{
}

public interface ILegacyCapable
{
}

/// <summary>
/// Base class for role buttons that need kill cooldown tracking.
/// Buttons implementing IKillButton or IDiseaseableButton should inherit from this.
/// </summary>
[MiraIgnore]
public abstract class TownOfUsKillRoleButton<TRole> : TownOfUsRoleButton<TRole> where TRole : RoleBehaviour
{
    protected override bool ShouldTrackKillCooldown()
    {
        return true;
    }
}

/// <summary>
/// Base class for role buttons with targets that need kill cooldown tracking.
/// Buttons implementing IKillButton or IDiseaseableButton should inherit from this.
/// </summary>
[MiraIgnore]
public abstract class TownOfUsKillRoleButton<TRole, TTarget> : TownOfUsRoleButton<TRole, TTarget>
    where TTarget : MonoBehaviour where TRole : RoleBehaviour
{
    protected override bool ShouldTrackKillCooldown()
    {
        return true;
    }
}

/// <summary>
/// Base class for role buttons with Vent targets that do not use <see cref="VentButton"/> 
/// or <see cref="HudManager.ImpostorVentButton"/> as a base.
/// <para/>
/// Utilizies the vanilla system for getting its Vent targets, as well as handling outlines.
/// </summary>
[MiraIgnore]
public abstract class TownOfUsVentRoleButton<TRole> : TownOfUsRoleButton<TRole, Vent> where TRole : RoleBehaviour
{
    public override Vent? GetTarget()
    {
        return HudManager.Instance.ImpostorVentButton.currentTarget;
    }

    public override bool CanUse()
    {
        if (TimeLordRewindSystem.IsRewinding)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasDied())
        {
            return false;
        }

        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        var newTarget = GetTarget();
        Target = IsTargetValid(newTarget) ? newTarget : null;

        return (PlayerControl.LocalPlayer.inVent || Timer <= 0 && Target != null) &&
            (!LimitedUses || UsesLeft > 0);
    }
}
#pragma warning restore S3060