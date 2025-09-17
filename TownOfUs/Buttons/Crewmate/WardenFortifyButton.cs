using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class WardenFortifyButton : TownOfUsRoleButton<WardenRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleWardenFortify", "Fortify");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Warden;
    public override float Cooldown => 0.001f + MapCooldown;
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
            Logger<TownOfUsPlugin>.Error("Warden Fortify: Target is null");
            return;
        }

        WardenRole.RpcWardenFortify(PlayerControl.LocalPlayer, Target);
    }
}