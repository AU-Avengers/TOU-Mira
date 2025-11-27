using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class DoomsayerObserveButton : TownOfUsRoleButton<DoomsayerRole, PlayerControl>
{
    public override string Name => TouLocale.GetParsed("TouRoleDoomsayerObserve", "Observe");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Doomsayer;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<DoomsayerOptions>.Instance.ObserveCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.Observe;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && !OptionGroupSingleton<DoomsayerOptions>.Instance.CantObserve;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: x => !x.HasModifier<DoomsayerObservedModifier>());
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        ModifierUtils.GetPlayersWithModifier<DoomsayerObservedModifier>()
            .Do(x => x.RemoveModifier<DoomsayerObservedModifier>());

        Target.AddModifier<DoomsayerObservedModifier>();

        var notif1 = Helpers.CreateAndShowNotification(
            TouLocale.GetParsed("TouRoleDoomsayerObserveNotif").Replace("<player>", $"{TownOfUsColors.Doomsayer.ToTextColor()}{Target.Data.PlayerName}</color>"),
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Doomsayer.LoadAsset());

        notif1.AdjustNotification();
    }
}