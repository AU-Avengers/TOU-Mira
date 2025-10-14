using System.Collections;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modules.Anims;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

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
        
        var text = TouLocale.GetParsed("TouRoleSpellslingerWarningNotif").Replace("<role>", $"{TownOfUsColors.ImpSoft.ToTextColor()}{TouLocale.Get("TouRoleSpellslinger")}</color>");

        var notif1 = Helpers.CreateAndShowNotification(
            text.Replace("<time>", $"{(int)OptionGroupSingleton<SpellslingerOptions>.Instance.HexBombDuration}"),
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Spellslinger.LoadAsset());
        notif1.AdjustNotification();
    }

    [HideFromIl2Cpp]
    private IEnumerator CoFlash()
    {
        var wait = new WaitForSeconds(1f);
        var playSound = false;
        while (_sabotage.TimeRemaining > 0)
        {
            var disableBlare = (MeetingHud.Instance != null || ExileController.Instance != null);
            if (_sabotage.Stage == HexBombStage.Countdown)
            {
                HudManager.Instance.FullScreen.color = new Color(0.38f, 0.2f, 0f, playSound ? 0.18f : 0.34f);
                HudManager.Instance.FullScreen.gameObject.SetActive(true);
                HudManager.Instance.PlayerCam.shakeAmount = 0.03f;
                HudManager.Instance.PlayerCam.shakePeriod = 16f;

                playSound = !playSound;
                if (playSound && !disableBlare)
                {
                    SoundManager.Instance.StopSound(TouAudio.HexBombAlarmSound.LoadAsset());
                    SoundManager.Instance.PlaySound(TouAudio.HexBombAlarmSound.LoadAsset(), false, 3f);
                }
            }
            else if (_sabotage.Stage == HexBombStage.SpellslingerDead)
            {
                HudManager.Instance.FullScreen.color = new Color(Palette.CrewmateBlue.r, Palette.CrewmateBlue.g, Palette.CrewmateBlue.b, playSound ? 0.18f : 0.34f);
                HudManager.Instance.FullScreen.gameObject.SetActive(true);
                HudManager.Instance.PlayerCam.shakeAmount = 0f;
                HudManager.Instance.PlayerCam.shakePeriod = 1f;

                playSound = !playSound;
                if (playSound)
                {
                    SoundManager.Instance.StopSound(TouAudio.HexBombAlarmSound.LoadAsset());
                    SoundManager.Instance.PlaySound(TouAudio.HexBombAlarmSound.LoadAsset(), false, 0.1f);
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
                    var deathAnim = AnimStore.SpawnAnimBody(PlayerControl.LocalPlayer, TouAssets.HexBombDeathPrefab.LoadAsset());
                    var redBg = Object.Instantiate(HudManager.Instance.FullScreen,
                        deathAnim.transform.GetParent().transform);
                    deathAnim.name = "Disintegrate Animation";
                    deathAnim.SetActive(false);
                    var deathRend = deathAnim.GetComponent<SpriteRenderer>();
                    deathRend.color = new Color(0f, 0f, 0f, 0.17254903f);
                    redBg.color = new Color(1f, 0f, 0f, 0.37254903f);
                    deathAnim.transform.localPosition += new Vector3(-0.4f, 0.1f, redBg.transform.localPosition.z - 100f);
                    redBg.transform.localScale *= 20f;
                    deathAnim.gameObject.layer = redBg.gameObject.layer;
                    if (PlayerControl.LocalPlayer.HasModifier<GiantModifier>())
                    {
                        deathAnim.transform.localScale /= 0.7f;
                    }
                    else if (PlayerControl.LocalPlayer.HasModifier<MiniModifier>())
                    {
                        deathAnim.transform.localScale *= 0.7f;
                    }
                    SoundManager.Instance.StopSound(TouAudio.HexBombAlarmSound.LoadAsset());
                    SoundManager.Instance.PlaySound(TouAudio.HexBombDetonateSound.LoadAsset(), false, 1f);
                    HudManager.Instance.FullScreen.gameObject.SetActive(true);
                    HudManager.Instance.FullScreen.color = new Color(1f, 0f, 0f, 0.37254903f);
                    deathAnim.SetActive(true);
                    yield return MiscUtils.FadeInDualRenderers(redBg, deathRend, 0.01f, 0.03f, 5f);
                }
                _triggeredHexBomb = true;
            }
            else
            {
                HudManager.Instance.FullScreen.color = new Color(1f, 0f, 0f, 0.37254903f);
                if (!HudManager.Instance.FullScreen.gameObject.activeSelf && !disableBlare)
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
            case HexBombStage.Countdown:
                text = $"The Spellslinger is unleashing a Hex Bomb!\n{(int)_sabotage.TimeRemaining + 1} seconds left!";
                break;
            case HexBombStage.SpellslingerDead:
                color = Palette.CrewmateBlue;
                text = $"The Spellslinger has perished!";
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
        }
        DataManager.Settings.Gameplay.ScreenShake = _ogShakeEnabled;
        HudManager.Instance.PlayerCam.shakeAmount = _ogShakeAmt;
        HudManager.Instance.PlayerCam.shakePeriod = _ogShakePeriod;

        _isComplete = true;
        PlayerControl.LocalPlayer.RemoveTask(this);
    }
}
