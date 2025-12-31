using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using System.Globalization;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Networking;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
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
        PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.GetKillCooldown() : 10f;
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
        _infectName = TouLocale.GetParsed("TouRoleParasiteOvertake", "Overtake");
        _killName = TouLocale.GetParsed("TouRoleParasiteDecay", "Kill");
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

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is ParasiteRole;
    }

    public override bool CanUse()
    {
        if (PlayerControl.LocalPlayer.Data?.Role is not ParasiteRole pr)
        {
            return false;
        }

        if (pr.Controlled != null)
        {
            if (!CanUseWhileControlling())
            {
                return false;
            }

            var controlled = pr.Controlled;
            if (controlled == null ||
                controlled.Data == null ||
                controlled.HasDied() ||
                controlled.Data.Disconnected ||
                !ParasiteControlState.IsControlled(controlled.PlayerId, out _))
            {
                if (controlled != null)
                {
                    ParasiteRole.RpcParasiteEndControl(PlayerControl.LocalPlayer, controlled);
                }
                return false;
            }

            if (pr.GetOvertakeKillLockoutRemainingSeconds() > 0f)
            {
                return false;
            }
            return true;
        }

        return base.CanUse() && Target != null && Timer <= 0;
    }

    public override bool CanClick()
    {
        if (PlayerControl.LocalPlayer?.Data?.Role is ParasiteRole pr && pr.Controlled != null)
        {
            return CanUse();
        }

        return base.CanClick();
    }

    private static bool CanUseWhileControlling()
    {
        if (PlayerControl.LocalPlayer == null)
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

        // Respect ability blocks (Glitch hack / disabled modifiers).
        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() ||
            PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
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

        if (target.HasModifier<MedicShieldModifier>() && target.PlayerId != local.PlayerId)
        {
            var medic = target.GetModifier<MedicShieldModifier>()?.Medic.GetRole<MedicRole>();
            if (medic != null && (TutorialManager.InstanceExists || local.AmOwner))
            {
                MedicRole.RpcMedicShieldAttacked(medic.Player, local, target);
            }
            return true;
        }

        if (target.TryGetModifier<ClericBarrierModifier>(out var barrier) && target.PlayerId != local.PlayerId)
        {
            var cleric = barrier?.Cleric?.GetRole<ClericRole>();
            if (cleric != null && (TutorialManager.InstanceExists || local.AmOwner))
            {
                ClericRole.RpcClericBarrierAttacked(cleric.Player, local, target);
            }
            return true;
        }

        if (target.TryGetModifier<MagicMirrorModifier>(out var mirror) && target.PlayerId != local.PlayerId)
        {
            var mirrorcaster = mirror?.Mirrorcaster?.GetRole<MirrorcasterRole>();
            if (mirrorcaster != null && (TutorialManager.InstanceExists || local.AmOwner))
            {
                MirrorcasterRole.RpcMagicMirrorAttacked(mirrorcaster.Player, local, target);
            }
            return true;
        }

        return target.GetModifiers<BaseShieldModifier>().Any();
    }

    public override PlayerControl? GetTarget()
    {
        if (PlayerControl.LocalPlayer.Data?.Role is not ParasiteRole pr)
        {
            return null;
        }

        if (pr.Controlled != null)
        {
            return pr.Controlled;
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(
            true,
            Distance,
            predicate: plr =>
                plr != null &&
                plr != PlayerControl.LocalPlayer &&
                !plr.HasDied() &&
                !plr.IsImpostorAligned() &&
                !plr.GetModifiers<BaseModifier>().Any(x => x is IUncontrollable) &&
                !plr.HasModifier<TownOfUs.Modifiers.Impostor.ParasiteInfectedModifier>());
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        // Freeze effect/cooldown timers during the initial control sync grace window.
        // This prevents "burning" duration/cooldown while the victim is intentionally immobilized.
        TimerPaused = false;
        var localRole = PlayerControl.LocalPlayer?.Data?.Role as ParasiteRole;
        if (localRole?.Controlled != null &&
            ParasiteControlState.IsControlled(localRole.Controlled.PlayerId, out _) &&
            ParasiteControlState.IsInInitialGrace(localRole.Controlled.PlayerId))
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
                if (Button.graphic.material != null)
                {
                    Button.graphic.material.SetFloat("_Desat", 0f);
                }
            }
            if (Button.buttonLabelText != null)
            {
                Button.buttonLabelText.color = Palette.EnabledColor;
            }
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
        if (local?.Data?.Role is ParasiteRole pr && pr.Controlled != null)
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

            if (Button.graphic != null)
            {
                Button.graphic.sprite = TouAssets.KillSprite.LoadAsset();
            }
        }
        else
        {
            OverrideName(_infectName);
            ClearAutoDecayCountdownVisual();

            if (Button.graphic != null)
            {
                Button.graphic.sprite = TouImpAssets.OvertakeSprite.LoadAsset();
            }
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
        if (PlayerControl.LocalPlayer?.Data?.Role is ParasiteRole pr && pr.Controlled != null)
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
            Button.usesRemainingSprite.sprite = TouAssets.TimerImpSprite.LoadAsset();
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

        if (pr.Controlled != null &&
            pr.Controlled.Data != null &&
            !pr.Controlled.HasDied() &&
            !pr.Controlled.Data.Disconnected &&
            ParasiteControlState.IsControlled(pr.Controlled.PlayerId, out _))
        {
            if (pr.GetOvertakeKillLockoutRemainingSeconds() > 0f)
            {
                return;
            }

            var target = pr.Controlled;
            if (!target.HasDied())
            {
                PlayerControl.LocalPlayer.RpcSpecialMurder(
                    target,
                    teleportMurderer: false,
                    showKillAnim: false,
                    causeOfDeath: "Parasite");
            }

            ParasiteRole.RpcParasiteEndControl(PlayerControl.LocalPlayer, target);
            return;
        }

        if (pr.Controlled != null)
        {
            ParasiteRole.RpcParasiteEndControl(PlayerControl.LocalPlayer, pr.Controlled);
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