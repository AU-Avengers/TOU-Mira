using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class GuardianAegisButton : TownOfUsRoleButton<GuardianRole>
{
    public override string Name => "Aegis";
    public override string Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Guardian;
    public override float Cooldown => 1;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.FortifySprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool ShouldPauseInVent => false;

    protected override void OnClick()
    {
        var guardian = PlayerControl.LocalPlayer.GetRole<GuardianRole>();

        if (guardian != null)
        {
            guardian.OpenAegisMenu();
        }
    }
}