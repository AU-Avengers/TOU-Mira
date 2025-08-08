using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class ShroudEnshroudButton : TownOfUsRoleButton<ShroudRole, PlayerControl>
{
    public override string Name => "Enshroud";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Shroud;
    public override float Cooldown => OptionGroupSingleton<ShroudOptions>.Instance.EnshroudCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.EnshroudSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool ShouldPauseInVent => false;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null) return;

        Role.EnshroudedTarget = Target;

        if (PlayerControl.LocalPlayer.AmOwner)
        {
            var notif = Helpers.CreateAndShowNotification(
                $"<b>You enshrouded {Target.CachedPlayerData.PlayerName}.</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Shroud.LoadAsset());
            notif.Text.SetOutlineThickness(0.35f);
        }
    }

}
