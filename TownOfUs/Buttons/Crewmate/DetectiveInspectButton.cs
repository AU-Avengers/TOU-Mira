using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Components;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class DetectiveInspectButton : TownOfUsRoleButton<ForensicRole, CrimeSceneComponent>
{
    public override string Name => TouLocale.GetParsed("TouRoleForensicInspect", "Inspect");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Detective;
    public override float Cooldown => Math.Clamp(MapCooldown, 1f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.InspectSprite;

    public override CrimeSceneComponent? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestObjectOfType<CrimeSceneComponent>(Distance,
            Helpers.CreateFilter(Constants.NotShipMask));
    }

    public override void SetOutline(bool active)
    {
        // placeholder
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Role.InvestigatingScene = Target;
        Role.InvestigatedPlayers.AddRange(Target.GetScenePlayers());
        var notif1 = Helpers.CreateAndShowNotification(
            $"{TouLocale.GetParsed("TouRoleForensicInspectNotif").Replace("<player>", $"{TownOfUsColors.Detective.ToTextColor()}{Target.DeadPlayer!.Data.PlayerName}</color>")}",
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Forensic.LoadAsset());
        notif1.AdjustNotification();
    }
}