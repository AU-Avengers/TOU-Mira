using System.Globalization;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class HerbalistAbilityKillButton : TownOfUsRoleButton<HerbalistRole, PlayerControl>, IDiseaseableButton, IKillButton
{
    public override string Name => "Kill";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => PlayerControl.LocalPlayer.GetKillCooldown();
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;
    public static HerbalistAbilityHerbButton OtherHerbButton => CustomButtonSingleton<HerbalistAbilityHerbButton>.Instance;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && OtherHerbButton.CurrentAbility is HerbAbilities.Kill;
    }

    public override bool CanUse()
    {
        return base.CanUse() && OtherHerbButton.CurrentAbility is HerbAbilities.Kill;
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        Button!.usesRemainingSprite.sprite = TouAssets.AbilityCounterHerbsSprite.LoadAsset();
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
    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }
        PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
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

    public override PlayerControl? GetTarget()
    {
        return MiscUtils.GetImpostorTarget(Distance);
    }
}
