using System.Collections;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
public sealed class HexBombSabotageTask(nint cppPtr) : PlayerTask(cppPtr)
{
    public override int TaskStep => !IsComplete ? 0 : 1;
    public override bool IsComplete => _isComplete;
    private bool _isComplete;
    private bool _triggeredHexBomb;
    private HexBombSabotageSystem _sabotage;
    private Coroutine? _flash;

    public override bool ValidConsole(Console console)
    {
        return false;
    }

    private void FixedUpdate()
    {
        if (IsComplete) return;

        if (!_sabotage.IsActive)
        {
            Complete();
        }
    }

    private float _ogShakeAmt;
    private bool _ogShakeEnabled;
    private float _ogShakePeriod;
    private bool _even;

    public override void Initialize()
    {
        _sabotage = ShipStatus.Instance.Systems[(SystemTypes)HexBombSabotageSystem.SabotageId]
            .Cast<HexBombSabotageSystem>();
        _flash ??= HudManager.Instance.StartCoroutine(CoFlash().WrapToIl2Cpp());

        _ogShakeEnabled = DataManager.Settings.Gameplay.ScreenShake;
        _ogShakeAmt = HudManager.Instance.PlayerCam.shakeAmount;
        _ogShakePeriod = HudManager.Instance.PlayerCam.shakePeriod;
        DataManager.Settings.Gameplay.ScreenShake = true;
    }

    [HideFromIl2Cpp]
    private IEnumerator CoFlash()
    {
        var wait = new WaitForSeconds(1f);
        var playSound = true;
        while (_sabotage.TimeRemaining > 0)
        {
            if (_sabotage.Stage == HexBombStage.Countdown)
            {
                HudManager.Instance.FullScreen.color = new Color(0.38f, 0.2f, 0f, playSound ? 0.18f : 0.34f);
                HudManager.Instance.FullScreen.gameObject.SetActive(true);
                HudManager.Instance.PlayerCam.shakeAmount = 0.01f;
                HudManager.Instance.PlayerCam.shakePeriod = 16f;

                playSound = !playSound;
                if (playSound)
                {
                    SoundManager.Instance.StopSound(TouAudio.HexBombAlarmSound.LoadAsset());
                    SoundManager.Instance.PlaySound(TouAudio.HexBombAlarmSound.LoadAsset(), false, 3f);
                }
            }
            else if (_sabotage.Stage == HexBombStage.Finished)
            {
                if (_triggeredHexBomb)
                {
                    // Do nothing
                }
                else
                {
                    HudManager.Instance.FullScreen.gameObject.SetActive(true);
                    HudManager.Instance.FullScreen.color = new Color(1f, 0f, 0f, 0.37254903f);
                    yield return MiscUtils.FadeIn(HudManager.Instance.FullScreen);
                }
                _triggeredHexBomb = true;
            }
            else
            {
                HudManager.Instance.FullScreen.color = new Color(1f, 0f, 0f, 0.37254903f);
                if (!HudManager.Instance.FullScreen.gameObject.activeSelf)
                {
                    SoundManager.Instance.StopSound(TouAudio.HexBombAlarmSound.LoadAsset());
                    SoundManager.Instance.PlaySound(TouAudio.HexBombAlarmSound.LoadAsset(), false, 3f);
                }
                HudManager.Instance.FullScreen.gameObject.SetActive(!HudManager.Instance.FullScreen.gameObject.activeSelf);
            }
            yield return wait;
        }
    }

    public override void AppendTaskText(Il2CppSystem.Text.StringBuilder sb)
    {
        _even = !_even;
        var color = _even ? Color.yellow : Color.red;
        if (_sabotage.Stage == HexBombStage.Countdown)
        {
            color = _even ? new Color(0.7f, 0.5f, 0f) : Color.red;
        }

        var text = "The Hex Bomb has been triggered!";
        switch (_sabotage.Stage)
        {
            case HexBombStage.Warning:
                text = $"The Spellslinger is unleashing a Hex Bomb!\nYou have {(int)_sabotage.TimeRemaining + 1} before it begins!";
                break;
            case HexBombStage.Countdown:
                text = $"Find the Spellslinger before time is up!\n{(int)_sabotage.TimeRemaining + 1} seconds left!";
                break;
        }

        sb.AppendLine($"{color.ToTextColor()}\n{text}</color>");
    }

    public override void Complete()
    {
        if (_flash != null)
        {
            HudManager.Instance.StopCoroutine(_flash);
            _flash = null;
            HudManager.Instance.FullScreen.gameObject.SetActive(false);
            SoundManager.Instance.StopSound(TouAudio.HexBombAlarmSound.LoadAsset());
            DataManager.Settings.Gameplay.ScreenShake = _ogShakeEnabled;
            HudManager.Instance.PlayerCam.shakeAmount = _ogShakeAmt;
            HudManager.Instance.PlayerCam.shakePeriod = _ogShakePeriod;
        }

        _isComplete = true;
        PlayerControl.LocalPlayer.RemoveTask(this);
    }
}
