using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class MedicShieldButton : TownOfUsRoleButton<MedicRole, PlayerControl>
{
    public bool CanChangeTarget = OptionGroupSingleton<MedicOptions>.Instance.ChangeTarget;
    public override string Name => TouLocale.GetParsed("TouRoleMedicShield", "Shield");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Medic;

    public override int MaxUses => OptionGroupSingleton<MedicOptions>.Instance.ChangeTarget
        ? (int)OptionGroupSingleton<MedicOptions>.Instance.MedicShieldUses
        : 0;

    public override float Cooldown => Math.Clamp(MapCooldown, 0.001f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.MedicSprite;

    public override bool CanUse()
    {
        return base.CanUse() && (Role is { Shielded: null } || CanChangeTarget);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Medic Shield: Target is null");
            return;
        }

        MedicRole.RpcMedicShield(PlayerControl.LocalPlayer, Target);
        CanChangeTarget = false;
    }
}