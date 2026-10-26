using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using System.Globalization;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class ParasiteOvertakeButton : TownOfUsKillRoleButton<ParasiteRole, PlayerControl>, IDiseaseableButton, IKillButton
{
    private string _infectName = "Overtake";
    private string _killName = "Kill";
    private bool _isProcessingClick;
    private Sprite? _defaultCounterSprite;
    private Vector3 _defaultCounterScale;
    private Vector3 _defaultCounterEuler;
    private Vector3 _defaultButtonLocalPos;
    private bool _hasCapturedButtonPos;
    public override string Name => _infectName;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float EffectDuration => OptionGroupSingleton<ParasiteOptions>.Instance.ControlDuration;
    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<ParasiteOptions>.Instance.OvertakeCooldown + MapCooldown + GetKillCooldownDelta(), 5f, 120f);
    public override float InitialCooldown =>
        PlayerControl.LocalPlayer ? PlayerControl.LocalPlayer.GetKillCooldown() : 10f;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.OvertakeSprite;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    private static float GetKillCooldownDelta()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null ||
            GameOptionsManager.Instance == null ||
            GameOptionsManager.Instance.CurrentGameOptions == null)
        {
            return 0f;
        }

        var baseKill = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        var effectiveKill = local.GetKillCooldown();
        return effectiveKill - (baseKill + MapCooldown);
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        _infectName = MiraLocaleManager.Get("TownOfUsMira.Role.ParasiteOvertake", "Overtake");
        _killName = MiraLocaleManager.Get("TownOfUsMira.Role.ParasiteDecay", "Kill");
        OverrideName(_infectName);

        _hasCapturedButtonPos = false;

        if (Button?.usesRemainingSprite != null)
        {
            _defaultCounterSprite = Button.usesRemainingSprite.sprite;
            _defaultCounterScale = Button.usesRemainingSprite.transform.localScale;
            _defaultCounterEuler = Button.usesRemainingSprite.transform.localEulerAngles;

            if (_defaultCounterScale == Vector3.zero)
            {
                _defaultCounterScale = Vector3.one;
            }
        }
    }

    public override bool CanUse()
    {
        if (!Role)
        {
            return false;
        }

        if (Role.Controlled != null)
        {
            if (!CanUseWhileControlling())
            {
                return false;
            }

            var controlled = Role.Controlled;
            if (controlled == null ||
                controlled.Data == null ||
                controlled.HasDied() ||
                controlled.Data.Disconnected ||
                !ParasiteRole.ControlState.IsControlled(controlled.PlayerId, out _))
            {
                if (controlled != null)
                {
                    ParasiteRole.RpcParasiteEndControl(PlayerControl.LocalPlayer, controlled, controlled.transform.position, false);
                }
                return false;
            }

            if (Role.GetOvertakeKillLockoutRemainingSeconds() > 0f)
            {
                return false;
            }
            return true;
        }

        return base.CanUse() && Target != null && Timer <= 0;
    }

    public override bool CanClick()
    {
        if (Role && Role.Controlled != null)
        {
            return CanUse();
        }

        return base.CanClick();
    }

    private static bool CanUseWhileControlling()
    {
        if (!PlayerControl.LocalPlayer)
        {
            return false;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return false;
        }

        if (MeetingHud.Instance || HudManager.Instance.Chat.IsOpenOrOpening)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasDied())
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasModifier<DisabledModifier>(x => !x.CanUseAbilities))
        {
            return false;
        }

        return true;
    }

    private static bool IsProtectedFromOvertake(PlayerControl target)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || target == null)
        {
            return false;
        }

        if (target.TryGetModifier<MedicShieldModifier>(out var medicMod) && target.PlayerId != local.PlayerId)
        {
            var medic = medicMod.Medic.GetRole<MedicRole>();
            if (medic != null && (TutorialManager.InstanceExists || local.AmOwner))
            {
                MedicRole.RpcMedicShieldAttacked(local, medic.Player, target);
            }
            return true;
        }

        if (target.TryGetModifier<ClericBarrierModifier>(out var barrier) && target.PlayerId != local.PlayerId)
        {
            var cleric = barrier?.Cleric?.GetRole<ClericRole>();
            if (cleric != null && (TutorialManager.InstanceExists || local.AmOwner))
            {
                ClericRole.RpcClericBarrierAttacked(local, cleric.Player, target);
            }
            return true;
        }

        if (target.TryGetModifier<MagicMirrorModifier>(out var mirror) && target.PlayerId != local.PlayerId)
        {
            var mirrorcaster = mirror?.Mirrorcaster?.GetRole<MirrorcasterRole>();
            if (mirrorcaster != null && (TutorialManager.InstanceExists || local.AmOwner))
            {
                MirrorcasterRole.RpcMagicMirrorAttacked(local, mirrorcaster.Player, target);
            }
            return true;
        }

        return target.HasModifier<BaseShieldModifier>();
    }

    public override PlayerControl? GetTarget()
    {
        if (PlayerControl.LocalPlayer.Data?.Role is not ParasiteRole { Controlled: var controlled })
        {
            return null;
        }

        if (controlled != null)
        {
            return controlled;
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(
            true,
            Distance,
            predicate: plr =>
                plr != null &&
                !plr.AmOwner &&
                !plr.HasDied() &&
                !plr.IsImpostorAligned() &&
                !plr.IsInTargetingAnimState() &&
                !plr.HasModifierOfType<IUncontrollable>() &&
                !plr.HasModifier<TownOfUs.Modifiers.Impostor.ParasiteInfectedModifier>());
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        TimerPaused = false;
        var localRole = PlayerControl.LocalPlayer?.Data?.Role as ParasiteRole;
        if (localRole?.Controlled != null &&
            ParasiteRole.ControlState.IsControlled(localRole.Controlled.PlayerId, out _) &&
            ParasiteRole.ControlState.IsInInitialGrace(localRole.Controlled.PlayerId))
        {
            TimerPaused = true;
        }

        base.FixedUpdateHandler(playerControl);

        if (localRole != null && localRole.Controlled != null && Button != null && Button.gameObject != null)
        {
            var lockoutRemaining = localRole.GetOvertakeKillLockoutRemainingSeconds();

            if (Button.cooldownTimerText != null && Button.cooldownTimerText.gameObject != null)
            {
                if (lockoutRemaining > 0f)
                {
                    Button.cooldownTimerText.text =
                        lockoutRemaining.ToString(CooldownTimerFormatString, NumberFormatInfo.InvariantInfo);
                    Button.cooldownTimerText.gameObject.SetActive(true);
                }
                else
                {
                    Button.cooldownTimerText.gameObject.SetActive(false);
                }
            }

            if (lockoutRemaining > 0f)
            {
                Button.SetDisabled();
                return;
            }
            else
            {
                Button.SetEnabled();
            }
            if (Button.graphic != null)
            {
                Button.graphic.color = Palette.EnabledColor;
                Button.graphic.material?.SetFloat("_Desat", 0f);
            }
            Button.buttonLabelText?.color = Palette.EnabledColor;
        }
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (Button == null || Button.gameObject == null)
        {
            base.FixedUpdate(playerControl);
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local?.Data?.Role is ParasiteRole { Controlled: not null })
        {
            var duration = OptionGroupSingleton<ParasiteOptions>.Instance.ControlDuration;
            if (duration > 0f)
            {
                OverrideName(_killName);

                var remaining = Mathf.Max(0f, EffectActive ? Timer : duration);
                UpdateAutoDecayCountdownVisual(remaining);

            }
            else
            {
                OverrideName(_killName);
                ClearAutoDecayCountdownVisual();
            }

            Button.graphic?.sprite = TouAssets.KillSprite;
        }
        else
        {
            OverrideName(_infectName);
            ClearAutoDecayCountdownVisual();

            Button.graphic?.sprite = TouImpAssets.OvertakeSprite;
        }

        base.FixedUpdate(playerControl);
    }

    public void StartControlEffectIfEnabled()
    {
        var duration = OptionGroupSingleton<ParasiteOptions>.Instance.ControlDuration;
        if (duration <= 0f)
        {
            EffectActive = false;
            return;
        }

        EffectActive = true;
        Timer = duration;
    }

    public void StopControlEffectAndApplyCooldown()
    {
        var wasEffectActive = EffectActive;
        EffectActive = false;

        if (wasEffectActive)
        {
            SetTimer(Cooldown);
        }
        else
        {
            SetTimer(Mathf.Max(Timer, Cooldown));
        }
    }

    public override void OnEffectEnd()
    {
        if (PlayerControl.LocalPlayer?.Data?.Role is ParasiteRole { Controlled: not null } pr)
        {
            pr.KillControlledFromTimer();
        }
    }

    private void UpdateAutoDecayCountdownVisual(float remainingSeconds)
    {
        if (Button == null)
        {
            return;
        }

        if (Button.usesRemainingSprite != null)
        {
            Button.usesRemainingSprite.sprite = TouAssets.TimerImpSprite;
            Button.usesRemainingSprite.gameObject.SetActive(true);

            var endUrgency = Mathf.Clamp01((5f - remainingSeconds) / 5f);
            var pulseAmp = Mathf.Lerp(0.003f, 0.012f, endUrgency);
            var pulseSpeed = Mathf.Lerp(1.5f, 3.0f, endUrgency);
            var pulse = 1f + pulseAmp * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f);

            Button.usesRemainingSprite.transform.localScale = _defaultCounterScale * pulse;
            Button.usesRemainingSprite.transform.localEulerAngles = _defaultCounterEuler;
        }

        if (Button.usesRemainingText != null)
        {
            Button.usesRemainingText.text =
                Mathf.CeilToInt(remainingSeconds).ToString(CultureInfo.InvariantCulture);
            Button.usesRemainingText.gameObject.SetActive(true);
        }

        if (remainingSeconds <= 5f)
        {
            if (!_hasCapturedButtonPos)
            {
                _defaultButtonLocalPos = Button.transform.localPosition;
                _hasCapturedButtonPos = true;
            }

            var urgency = Mathf.Clamp01((5f - remainingSeconds) / 5f);
            var amp = Mathf.Lerp(0.01f, 0.06f, urgency);
            var speed = Mathf.Lerp(18f, 35f, urgency);
            var nx = Mathf.PerlinNoise(Time.time * speed, 0.123f) - 0.5f;
            var ny = Mathf.PerlinNoise(0.456f, Time.time * speed) - 0.5f;
            Button.transform.localPosition = _defaultButtonLocalPos + new Vector3(nx * amp, ny * amp, 0f);
        }
        else
        {
            _hasCapturedButtonPos = false;
        }
    }

    private void ClearAutoDecayCountdownVisual()
    {
        if (Button == null)
        {
            return;
        }

        if (_hasCapturedButtonPos)
        {
            Button.transform.localPosition = _defaultButtonLocalPos;
            _hasCapturedButtonPos = false;
        }

        if (Button.usesRemainingSprite != null)
        {
            if (_defaultCounterSprite != null)
            {
                Button.usesRemainingSprite.sprite = _defaultCounterSprite;
            }

            Button.usesRemainingSprite.transform.localScale = _defaultCounterScale;
            Button.usesRemainingSprite.transform.localEulerAngles = _defaultCounterEuler;
            Button.usesRemainingSprite.gameObject.SetActive(false);
        }

        Button.usesRemainingText?.gameObject.SetActive(false);
    }

    public override void ClickHandler()
    {
        // Otherwise it clicks twice for whatever fucking reason I really don't know
        // If anyone else knows how to fix this properly please tell me
        // I've been at it for hours. Hours I say
        // No sleep
        // I'm not even having fun anymore
        // It was all fun and games haha you press Q after one game it insta kills haha         // Now it's just pain
        // Please help
        if (_isProcessingClick)
        {
            return;
        }
        _isProcessingClick = true;

        try
        {
            if (PlayerControl.LocalPlayer.Data?.Role is not ParasiteRole)
            {
                return;
            }

            if (!CanClick())
            {
                return;
            }

            OnClick();
            Button?.SetDisabled();
        }
        finally
        {
            Coroutines.Start(ResetProcessingFlag());
        }
    }

    private System.Collections.IEnumerator ResetProcessingFlag()
    {
        yield return new WaitForSeconds(0.2f);
        _isProcessingClick = false;
    }

    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data?.Role is not ParasiteRole pr)
        {
            return;
        }

        var controlled = pr.Controlled;
        if (controlled != null)
        {
            var toKill = controlled.Data != null &&
                         !controlled.HasDied() &&
                         !controlled.Data.Disconnected &&
                         ParasiteRole.ControlState.IsControlled(controlled.PlayerId, out _);

            if (toKill)
            {
                if (pr.GetOvertakeKillLockoutRemainingSeconds() > 0f)
                {
                    return;
                }

                if (controlled.IsInTargetingAnimState() || controlled.inVent)
                {
                    return;
                }
            }

            ParasiteRole.RpcParasiteEndControl(PlayerControl.LocalPlayer, controlled, controlled.transform.position, toKill);
            return;
        }

        if (Target == null)
        {
            return;
        }

        if (IsProtectedFromOvertake(Target))
        {
            SetTimer(OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset);
            return;
        }

        ParasiteRole.RpcParasiteControl(PlayerControl.LocalPlayer, Target);
    }
}