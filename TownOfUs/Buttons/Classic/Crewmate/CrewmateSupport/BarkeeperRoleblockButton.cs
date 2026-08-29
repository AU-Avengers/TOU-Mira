using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class BarkeeperRoleblockButton : TownOfUsRoleButton<BarkeeperRole, PlayerControl>
{
    public override string Name => MiraLocaleManager.Get("TouRoleBarkeeperRoleblock");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Barkeeper;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<BarkeeperOptions>.Instance.RoleblockCooldown.Value + MapCooldown, 5f, 120f);
    public override float EffectDuration => SelectedDuration;

    public float SelectedDuration = 0.001f;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.RoleblockSprite;
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

        OverrideName(MiraLocaleManager.Get("TouRoleBarkeeperRoleblocking"));

        _roleblockedTarget = Target;

        NotifMessage = Helpers.CreateAndShowNotification($"<b>{MiraLocaleManager.Get("TouRoleBarkeeperRoleblockChosen") .Replace("<player>", _roleblockedTarget.CachedPlayerData.PlayerName)}</b>",
        Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Barkeeper.LoadAsset());
        NotifMessage.Text.SetOutlineThickness(0.35f);
        CustomButtonSingleton<BarkeeperSpillButton>.Instance.ResetCooldownAndOrEffect();
    }

    public override void OnEffectEnd()
    {
        OverrideName(MiraLocaleManager.Get("TouRoleBarkeeperRoleblock"));

        if (_roleblockedTarget == null) return;

        BarkeeperRole.RpcRoleblock(PlayerControl.LocalPlayer, _roleblockedTarget);
        _roleblockedTarget = null;
    }

}
