using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class MonarchKnightButton : TownOfUsRoleButton<MonarchRole, PlayerControl>
{
    public override string Name => "Knight";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Monarch;
    public override float Cooldown => OptionGroupSingleton<MonarchOptions>.Instance.KnightCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<MonarchOptions>.Instance.KnightDelay;
    public override int MaxUses => (int)OptionGroupSingleton<MonarchOptions>.Instance.MaxKnights;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.KnightButtonSprite;
    public PlayerControl? _knightedTarget;

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

        OverrideName("Knighting");

        _knightedTarget = Target;

        if (PlayerControl.LocalPlayer.AmOwner)
        {
            var notif = Helpers.CreateAndShowNotification(
                $"<b>You chose to knight {_knightedTarget.CachedPlayerData.PlayerName}. They will be knighted in {OptionGroupSingleton<MonarchOptions>.Instance.KnightDelay} second(s)!</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Monarch.LoadAsset());
            notif.Text.SetOutlineThickness(0.35f);
        }
    }

public override void OnEffectEnd()
{
    OverrideName("Knight");

    if (_knightedTarget == null) return;

    MonarchRole.RpcKnight(PlayerControl.LocalPlayer, _knightedTarget);
    _knightedTarget = null;
}

}
