using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class BlackmailerBlackmailButton : TownOfUsRoleButton<BlackmailerRole, PlayerControl>,
    IAftermathablePlayerButton
{
    public override string Name => TouLocale.Get("TouRoleBlackmailerBlackmail", "Blackmail");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<BlackmailerOptions>.Instance.BlackmailCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<BlackmailerOptions>.Instance.MaxBlackmails;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.BlackmailSprite;

    public void AftermathHandler()
    {
        BlackmailerRole.RpcBlackmail(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer);
        Timer = 60f;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        BlackmailerRole.RpcBlackmail(PlayerControl.LocalPlayer, Target);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false,
            player => !player.HasModifier<BlackmailedModifier>() && !player.HasModifier<BlackmailSparedModifier>());
    }
}