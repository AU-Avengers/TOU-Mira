using MiraAPI.GameOptions;
using MiraAPI.Networking;
using TownOfUs.Options.Roles.PolusNeutral;
using TownOfUs.Roles.TownOfPolus.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.TownOfPolus.Neutral;

public sealed class PolusSerialKillerKillButton : TownOfUsKillRoleButton<PolusSerialKillerRole, PlayerControl>, IKillButton, ILegacyButton
{
    public override string Name => string.Empty;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.PolusSerialKiller;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<PolusSerialKillerOptions>.Instance.KillCooldown + MapCooldown - CooldownReduction, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => LegacyVanillaAssets.KillSprite;
    public override bool ShouldPauseInVent => false;
    public float CooldownReduction;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Serial Killer: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}