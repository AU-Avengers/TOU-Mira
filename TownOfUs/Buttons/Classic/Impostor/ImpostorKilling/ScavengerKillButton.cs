using MiraAPI.Networking;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class ScavengerKillButton : TownOfUsKillRoleButton<ScavengerRole, PlayerControl>, IDiseaseableButton,
    IKillButton, ILegacyCapable
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.KillLabel, "Kill");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => PlayerControl.LocalPlayer.GetKillCooldown();

    public override LoadableAsset<Sprite> Sprite =>
        LegacyAssets.IsLegacy ? LegacyVanillaAssets.KillSprite : TouAssets.KillSprite;

    public override bool ZeroIsInfinite { get; set; } = true;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override PlayerControl? GetTarget()
    {
        return MiscUtils.GetImpostorTarget(Distance);
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        UpdateScavengeCounter();

        base.FixedUpdate(playerControl);
    }

    private void UpdateScavengeCounter()
    {
        if (Button == null)
        {
            return;
        }

        var scavenger = Role;
        var showTimer = scavenger && scavenger.Scavenging && scavenger.TimeRemaining > 0f;

        Button.usesRemainingText.gameObject.SetActive(showTimer);
        Button.usesRemainingSprite.gameObject.SetActive(showTimer);

        if (showTimer)
        {
            Button.usesRemainingText.text = Mathf.CeilToInt(scavenger.TimeRemaining) + "<size=80%>s</size>";
        }
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
    }
}