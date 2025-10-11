using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class PlaguebearerInfectButton : TownOfUsRoleButton<PlaguebearerRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRolePlaguebearerInfect", "Infect");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Plaguebearer;
    public override float Cooldown => OptionGroupSingleton<PlaguebearerOptions>.Instance.InfectCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.InfectSprite;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: plr => !plr.HasModifier<PlaguebearerInfectedModifier>());
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Plaguebearer Infect: Target is null");
            return;
        }

        PlaguebearerRole.RpcCheckInfected(PlayerControl.LocalPlayer, Target);
    }
}