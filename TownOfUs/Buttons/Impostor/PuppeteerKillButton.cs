using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using TownOfUs.Networking;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class PuppeteerKillButton : TownOfUsKillRoleButton<PuppeteerRole, PlayerControl>, IDiseaseableButton,
    IKillButton
{
    private string _ctrlKillName = "Control Kill";
    private string _killName = "Kill";
    public override string Name => _killName;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => PlayerControl.LocalPlayer.GetKillCooldown();
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;
    private static PuppeteerControlButton ControlButton => CustomButtonSingleton<PuppeteerControlButton>.Instance;

    public override bool ZeroIsInfinite { get; set; } = true;

    public void ToggleControlText(bool control)
    {
        if (control)
        {
            OverrideName(_ctrlKillName);
        }
        else
        {
            OverrideName(_killName);
        }
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        _killName = TranslationController.Instance.GetStringWithDefault(StringNames.KillLabel, "Kill");
        _ctrlKillName = TouLocale.GetParsed("TouRolePuppeteerKIll", "Control Kill");
    }

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override PlayerControl? GetTarget()
    {
        if (ControlButton.EffectActive && Role.Controlled != null)
        {
            return Role.Controlled.GetClosestLivingPlayer(
                false,
                Distance,
                predicate: plr =>
                    plr != null &&
                    plr != PlayerControl.LocalPlayer &&
                    !plr.HasDied() &&
                    !plr.IsInTargetingAnimState() &&
                    !plr.IsImpostorAligned());
        }
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(
            false,
            Distance,
            predicate: plr => plr != null && !plr.HasDied() && !plr.IsInTargetingAnimState());
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Puppeteer Kill: Target is null");
            return;
        }

        if (Role.Controlled != null)
        {
            PlayerControl.LocalPlayer.RpcFramedMurder(
                Target,
                Role.Controlled,
                causeOfDeath: "PuppetControl");
        }
        else
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
        }
    }
}