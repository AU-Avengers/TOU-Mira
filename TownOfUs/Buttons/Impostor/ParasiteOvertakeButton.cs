using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using System.Globalization;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class ParasiteOvertakeButton : TownOfUsRoleButton<ParasiteRole, PlayerControl>, IDiseaseableButton, IKillButton
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
    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<ParasiteOptions>.Instance.InfectCooldown + MapCooldown + GetKillCooldownDelta(), 5f, 120f);
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
            if (pr.Controlled.Data == null ||
                pr.Controlled.HasDied() ||
                pr.Controlled.Data.Disconnected ||
                !ParasiteControlState.IsControlled(pr.Controlled.PlayerId, out _))
            {
                ParasiteRole.RpcParasiteEndControl(PlayerControl.LocalPlayer, pr.Controlled);
                return false;
            }
            return base.CanUse();
        }

        return base.CanUse() && Target != null && Timer <= 0;
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
                !plr.HasModifier<TownOfUs.Modifiers.Impostor.ParasiteInfectedModifier>());
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        base.FixedUpdateHandler(playerControl);

        var local = PlayerControl.LocalPlayer;
        if (local?.Data?.Role is ParasiteRole pr && pr.Controlled != null && Button != null && Button.gameObject != null)
        {
            if (Button.cooldownTimerText != null && Button.cooldownTimerText.gameObject != null)
            {
                Button.cooldownTimerText.gameObject.SetActive(false);
            }
            Button.SetEnabled();
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

                var remaining = Mathf.Max(0f, pr.ControlTimer);
                UpdateAutoDecayCountdownVisual(remaining, duration);

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

    private void UpdateAutoDecayCountdownVisual(float remainingSeconds, float durationSeconds)
    {
        if (Button == null)
        {
            return;
        }

        if (Button.usesRemainingSprite != null)
        {
            Button.usesRemainingSprite.sprite = TouAssets.TimerImpSprite.LoadAsset();
            Button.usesRemainingSprite.gameObject.SetActive(true);

            var t = Mathf.Clamp01(1f - (remainingSeconds / durationSeconds));
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
            if (PlayerControl.LocalPlayer.Data?.Role is not ParasiteRole pr)
            {
                return;
            }

            if (!CanClick())
            {
                return;
            }

            OnClick();
            Button?.SetDisabled();

            if (pr.Controlled == null)
            {
                Timer = Mathf.Max(Timer, Cooldown);
            }
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
            var target = pr.Controlled;
            if (!target.HasDied())
            {
                PlayerControl.LocalPlayer.RpcSpecialMurder(
                    target,
                    teleportMurderer: false,
                    showKillAnim: false,
                    causeOfDeath: "Parasite");
            }

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

        if (Target.HasModifier<MedicShieldModifier>() &&
            Target.PlayerId != PlayerControl.LocalPlayer.PlayerId)
        {
            var medic = Target.GetModifier<MedicShieldModifier>()?.Medic.GetRole<MedicRole>();
            if (medic != null && (TutorialManager.InstanceExists || PlayerControl.LocalPlayer.AmOwner))
            {
                MedicRole.RpcMedicShieldAttacked(medic.Player, PlayerControl.LocalPlayer, Target);
            }

            SetTimer(OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset);
            return;
        }

        ParasiteRole.RpcParasiteControl(PlayerControl.LocalPlayer, Target);
    }
}