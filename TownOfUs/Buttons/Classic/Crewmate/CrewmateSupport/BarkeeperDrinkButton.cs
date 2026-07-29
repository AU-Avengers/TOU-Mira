using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class BarkeeperRoleblockButton : TownOfUsRoleButton<BarkeeperRole, PlayerControl>
{
    public override string Name => "Roleblock";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Barkeeper;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<BarkeeperOptions>.Instance.RoleblockCooldown.Value + MapCooldown, 5f, 120f);
    public override float EffectDuration => SelectedDuration;

    public float SelectedDuration = 0.001f;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.CleanseSprite;
    private PlayerControl? _roleblockedTarget;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    public override void ClickHandler()
    {
        if (CanClick())
        {
            var opts = OptionGroupSingleton<BarkeeperOptions>.Instance;
            SelectedDuration = UnityEngine.Random.RandomRange(opts.RoleblockDelayMin.Value, opts.RoleblockDelayMax.Value);
        }
        base.ClickHandler();
    }

    public LobbyNotificationMessage? NotifMessage;
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
            NotifMessage = Helpers.CreateAndShowNotification(
                $"<b>You chose to roleblock {_roleblockedTarget.CachedPlayerData.PlayerName}.</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Barkeeper.LoadAsset());
            NotifMessage.Text.SetOutlineThickness(0.35f);
        }
    }

    public override void OnEffectEnd()
    {
        OverrideName("Roleblock");

        if (_roleblockedTarget == null) return;

        BarkeeperRole.RpcRoleblock(PlayerControl.LocalPlayer, _roleblockedTarget);
        _roleblockedTarget = null;
    }

}
