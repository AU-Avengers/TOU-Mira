using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class AdvancedSabotageOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Advanced Sabotages";
    public override uint GroupPriority => 2;
    public override Color GroupColor => new Color32(173, 180, 179, 255);
    public ModdedToggleOption KillDuringCamoComms { get; set; } = new("TouOptionAdvancedSaboKillDuringCamoComms", true);

    public ModdedToggleOption CamoKillScreens { get; set; } = new("TouOptionAdvancedSaboCamoKillScreens", false);

    public ModdedToggleOption HidePlayerSizeInCamo { get; set; } = new("TouOptionAdvancedSaboHidePlayerSizeInCamo", false);

    public ModdedToggleOption HidePlayerSpeedInCamo { get; set; } = new("TouOptionAdvancedSaboHidePlayerSpeedInCamo", false);
}