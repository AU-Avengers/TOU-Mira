using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class HypnotistHypnotizeButton : TownOfUsRoleButton<HypnotistRole, PlayerControl>,
    IAftermathablePlayerButton
{
    public override string Name => TouLocale.Get("TouRoleHypnotistHypnotize", "Hypnotize");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<HypnotistOptions>.Instance.HypnotiseCooldown;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.HypnotiseButtonSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && role is HypnotistRole hypno && !hypno.HysteriaActive;
    }

    public override bool CanUse()
    {
        return base.CanUse() && !Role.HysteriaActive;
    }

    public void AftermathHandler()
    {
        PlayerControl.LocalPlayer.RpcAddModifier<HypnotisedModifier>(PlayerControl.LocalPlayer);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Target.RpcAddModifier<HypnotisedModifier>(PlayerControl.LocalPlayer);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance, false,
            player => !player.HasModifier<HypnotisedModifier>());
    }
}