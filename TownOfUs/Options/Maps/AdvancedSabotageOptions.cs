using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class AdvancedSabotageOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.AdvancedSabotages");
    public override uint GroupPriority => 2;
    public override Color GroupColor => new Color32(173, 180, 179, 255);
    public ModdedToggleOption KillDuringCamoComms { get; set; } = new("TownOfUsMira.AdvancedSabo.Option.KillDuringCamoComms", true);

    public ModdedToggleOption CamoKillScreens { get; set; } = new("TownOfUsMira.AdvancedSabo.Option.CamoKillScreens", false);

    public ModdedToggleOption HidePlayerSizeInCamo { get; set; } = new("TownOfUsMira.AdvancedSabo.Option.HidePlayerSizeInCamo", false);

    public ModdedToggleOption HidePlayerSpeedInCamo { get; set; } = new("TownOfUsMira.AdvancedSabo.Option.HidePlayerSpeedInCamo", false);
}