using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class CampButton : TownOfUsRoleButton<DeputyRole, PlayerControl>, ILegacyCapable
{
    public bool Usable = true;
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.DeputyCamp", "Camp");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Deputy;
    public override float Cooldown => Math.Clamp(MapCooldown, 0.001f, 120f);
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyCrewAssets.CampButtonSprite : TouCrewAssets.CampButtonSprite;

    public override bool CanUse()
    {
        return base.CanUse() && Usable;
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return base.IsTargetValid(target) && !target?.HasModifier<DeputyCampedModifier>() == true;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Camp: Target is null");
            return;
        }

        var player = ModifierUtils.GetPlayersWithModifier<DeputyCampedModifier>(x => x.Deputy.AmOwner).FirstOrDefault();

        player?.RpcRemoveModifier<DeputyCampedModifier>();

        Target.RpcAddModifier<DeputyCampedModifier>(PlayerControl.LocalPlayer);
        Usable = false;
        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.DeputyCampNotif").Replace("<player>", $"{TownOfUsColors.Deputy.ToTextColor()}{Target.Data.PlayerName}</color>")}</b>", Color.white,
            new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Deputy.LoadAsset());
        notif1.AdjustNotification();
    }
}