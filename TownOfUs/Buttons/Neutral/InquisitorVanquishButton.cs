using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class InquisitorVanquishButton : TownOfUsKillRoleButton<InquisitorRole, PlayerControl>, IDiseaseableButton,
    IKillButton
{
    public override string Name => TouLocale.GetParsed("TouRoleInquisitorVanquish", "Vanquish");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Inquisitor;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<InquisitorOptions>.Instance.VanquishCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.InquisKillSprite;

    public bool Usable { get; set; } =
        OptionGroupSingleton<InquisitorOptions>.Instance.FirstRoundUse || TutorialManager.InstanceExists;

    public override bool ZeroIsInfinite { get; set; } = true;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override bool CanUse()
    {
        return base.CanUse() && Usable && Role.CanVanquish;
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

        PlayerControl.LocalPlayer.RpcAltCustomMurder(Target, false);
    }
}