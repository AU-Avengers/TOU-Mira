using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers;

public class ScatterModifier(float time) : TimedModifier
{
    private readonly List<Vector3> _locations = [];
    private Image? scatterBar;
    private TextMeshProUGUI? scatterText;
    private GameObject? scatterUI;
    private float soundTimer = 1f;
    public override string ModifierName => TouLocale.Get(TouNames.Scatter, "Scatter");
    public override float Duration => time;
    public override bool AutoStart => false;
    public override bool HideOnUi => true;
    public override bool RemoveOnComplete => true;

    public override string GetDescription()
    {
        var roundedTime = (int)Math.Round(Math.Max(TimeRemaining, 0), 0);

        var textColor = roundedTime switch
        {
            > 10 => Color.green,
            > 5 => Color.yellow,
            _ => Color.red
        };

        return $"{textColor.ToTextColor()}<size=80%>{roundedTime}s</size></color>";
    }

    public override void OnActivate()
    {
        base.OnActivate();

        //Logger<TownOfUsPlugin>.Error($"ScatterModifier.OnActivate");

        if (!Player.AmOwner)
        {
            return;
        }

        scatterUI = Object.Instantiate(TouAssets.ScatterUI.LoadAsset(), HudManager.Instance.transform);
        scatterUI.transform.localPosition = new Vector3(-3.22f, 2.26f, -10f);
        scatterUI!.SetActive(false);

        scatterText = scatterUI.transform.FindChild("ScatterCanvas").FindChild("ScatterText").gameObject
            .GetComponent<TextMeshProUGUI>();
        scatterText.text = $"Scatter: {Duration}s";
        scatterText!.gameObject.SetActive(false);

        scatterBar = scatterUI.transform.FindChild("ScatterCanvas").FindChild("ScatterBar").gameObject
            .GetComponent<Image>();
        scatterBar.fillAmount = 1f;

        var scatterIcon = scatterUI.transform.FindChild("ScatterCanvas").FindChild("ScatterIcon").gameObject
            .GetComponent<Image>();
        scatterIcon.sprite = Player.Data.Role.RoleIconSolid;
    }

    public override void OnMeetingStart()
    {
        ResetTimer();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        //Logger<TownOfUsPlugin>.Error($"Scatter - !Player.AmOwner: {!Player.AmOwner} !TimerActive: {!TimerActive} Player.HasDied(): {Player.HasDied()} MeetingHud.Instance: {MeetingHud.Instance} ScatterEvents.Intro: {ScatterEvents.Intro}");

        if (!Player.AmOwner || !TimerActive || Player.HasDied() || MeetingHud.Instance)
        {
            soundTimer = 1f;
            TimeRemaining = Duration;

            scatterUI!.SetActive(false);
            scatterText!.gameObject.SetActive(false);

            return;
        }

        var roundedTime = (int)Math.Round(Math.Max(TimeRemaining, 0f), 0f);

        var textColor = roundedTime switch
        {
            > 10 => Color.green,
            > 5 => Color.yellow,
            _ => Color.red
        };

        if (scatterText != null)
        {
            scatterText.text = $"Scatter: {textColor.ToTextColor()}{roundedTime}s</color>";
        }

        if (scatterBar != null)
        {
            scatterBar.fillAmount = Math.Clamp(TimeRemaining / Duration, 0f, 1f);
            scatterBar.color = textColor;
        }

        if (roundedTime <= 11f)
        {
            soundTimer -= Time.fixedDeltaTime;

            if (soundTimer <= 0f)
            {
                var num = roundedTime / 10f;
                var pitch = 1.5f - num / 2f;
                SoundManager.Instance.PlaySoundImmediate(
                    GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideCountdownSFX, false, 1f, pitch,
                    SoundManager.Instance.SfxChannel);
                soundTimer = 1f;
            }
        }

        scatterUI!.SetActive(true);
        scatterText!.gameObject.SetActive(true);

        foreach (var location in _locations)
        {
            var magnitude = (location - Player.transform.localPosition).magnitude;
            if (magnitude < 5f)
            {
                return;
            }
        }

        TimeRemaining = Duration;

        _locations.Insert(0, Player.transform.localPosition);
        if (_locations.Count > 3)
        {
            _locations.RemoveAt(3);
        }
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        soundTimer = 1f;
        TimeRemaining = Duration;

        scatterUI!.SetActive(false);
        scatterText!.gameObject.SetActive(false);

        if (scatterUI?.gameObject != null)
        {
            scatterUI.gameObject.Destroy();
        }
    }

    public override void OnTimerComplete()
    {
        if (Player.AmOwner && !Player.HasDied())
        {
            Player.RpcCustomMurder(Player);
        }
    }

    public void OnRoundStart()
    {
        //Logger<TownOfUsPlugin>.Error($"Scatter - OnRoundStart");

        ResetTimer();
        ResumeTimer();
    }
}