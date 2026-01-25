using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class DetectiveExamineButton : TownOfUsRoleButton<ForensicRole, PlayerControl>
{
    public override string Name => TouLocale.GetParsed("TouRoleForensicExamine", "Examine");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Detective;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<ForensicOptions>.Instance.ExamineCooldown + MapCooldown, 5f, 120f);
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