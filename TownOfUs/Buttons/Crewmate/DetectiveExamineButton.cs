using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class DetectiveExamineButton : TownOfUsRoleButton<DetectiveTouRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleDetectiveExamine", "Examine");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Detective;
    public override float Cooldown => OptionGroupSingleton<DetectiveOptions>.Instance.ExamineCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.ExamineSprite;

    public override bool CanUse()
    {
        return base.CanUse() && Role.InvestigatingScene;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Role.ExaminePlayer(Target);
    }
}