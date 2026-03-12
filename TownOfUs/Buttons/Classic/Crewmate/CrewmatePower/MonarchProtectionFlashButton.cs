using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class MonarchProtectionFlashButton : TownOfUsRoleButton<MonarchRole>
{
    public override string Name => "No Flash";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Monarch;
    public override float Cooldown => 3f;
    public override LoadableAsset<Sprite> Sprite => ProtectionButtons[0];
    public static MonarchOptions MonOptions => OptionGroupSingleton<MonarchOptions>.Instance;
    public RealFlash CurrentFlashType = RealFlash.NoFlash;

    public static List<LoadableAsset<Sprite>> ProtectionButtons { get; set; } = new()
    {
        TouImpAssets.FlashSprite,
        TouCrewAssets.BarrierSprite,
        TouCrewAssets.MedicSprite,
        TouNeutAssets.GuardSprite,
        TouCrewAssets.FortifySprite,
    };

    public static List<string> ProtectionText { get; set; } = new()
    {
        "No Flash",
        "Cleric Flash",
        "Medic Flash",
        "Merc Flash",
        "Warden Flash",
    };

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && MonOptions.CrewKnightsGrantKillImmunity;
    }

    public override bool CanUse()
    {
        return base.CanUse() && (ProtectionFlash)MonOptions.ProtectionFlashColor.Value is ProtectionFlash.Configurable;
    }

    protected override void OnClick()
    {
        var stepUp = (RealFlash)((int)CurrentFlashType + 1);
        if (Enum.IsDefined(stepUp))
        {
            CurrentFlashType = stepUp;
        }
        else
        {
            CurrentFlashType = RealFlash.NoFlash;
        }
        MonarchRole.RpcUpdateMonShield(PlayerControl.LocalPlayer, (int)CurrentFlashType);
    }

    public void SetShieldType(RealFlash type)
    {
        CurrentFlashType = type;
        OverrideSprite(ProtectionButtons[(int)CurrentFlashType].LoadAsset());
        OverrideName(ProtectionText[(int)CurrentFlashType]);
    }
}
