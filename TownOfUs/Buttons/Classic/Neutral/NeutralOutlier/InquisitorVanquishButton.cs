using MiraAPI.GameOptions;
using MiraAPI.Networking;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class InquisitorVanquishButton : TownOfUsKillRoleButton<InquisitorRole, PlayerControl>, IDiseaseableButton,
    IKillButton, ILegacyCapable
{
    public override string Name => MiraLocaleManager.Get("TouRoleInquisitorVanquish", "Vanquish");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Inquisitor;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<InquisitorOptions>.Instance.VanquishCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyVanillaAssets.KillSprite : TouNeutAssets.InquisKillSprite;

    public override bool UsableFirstRound => OptionGroupSingleton<InquisitorOptions>.Instance.FirstRoundUse;

    public override bool ZeroIsInfinite { get; set; } = true;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override bool CanUse()
    {
        return base.CanUse() && Role.CanVanquish;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Inquisitor Vanquish: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
    }
}