using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class BarkeeperRoleblockButton : TownOfUsRoleButton<BarkeeperRole, PlayerControl>
{
    public override string Name => "Drink With";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Barkeeper;
    public override float Cooldown => OptionGroupSingleton<BarkeeperOptions>.Instance.RoleblockCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<BarkeeperOptions>.Instance.RoleblockDelay;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.RoleblockButtonSprite;
    public PlayerControl? _roleblockedTarget;

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

        OverrideName("Roleblocking");

        _roleblockedTarget = Target;

        if (PlayerControl.LocalPlayer.AmOwner)
        {
            var notif = Helpers.CreateAndShowNotification(
                $"<b>You chose to roleblock {_roleblockedTarget.CachedPlayerData.PlayerName}. They will be roleblocked in {OptionGroupSingleton<BarkeeperOptions>.Instance.RoleblockDelay} second(s)!</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Barkeeper.LoadAsset());
            notif.Text.SetOutlineThickness(0.35f);
        }
    }

    public override void OnEffectEnd()
    {
        OverrideName("Drink With");

        if (_roleblockedTarget == null) return;

        BarkeeperRole.RpcRoleblock(PlayerControl.LocalPlayer, _roleblockedTarget);
        _roleblockedTarget = null;
    }

}
