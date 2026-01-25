using MiraAPI.Utilities.Assets;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class WardenFortifyButton : TownOfUsRoleButton<WardenRole, PlayerControl>
{
    public override string Name => TouLocale.GetParsed("TouRoleWardenFortify", "Fortify");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Warden;
    public override float Cooldown => Math.Clamp(MapCooldown, 0.001f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.FortifySprite;

    public override bool CanUse()
    {
        return base.CanUse() && Role is { Fortified: null };
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Warden Fortify: Target is null");
            return;
        }

        WardenRole.RpcWardenFortify(PlayerControl.LocalPlayer, Target);
    }
}