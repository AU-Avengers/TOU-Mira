using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class AdvancedSabotageOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Advanced Sabotages";
    public override uint GroupPriority => 1;
    public override Color GroupColor => new Color32(173, 180, 179, 255);
    
    [ModdedToggleOption("Camouflage Comms")]
    public bool CamouflageComms { get; set; } = true;

    public ModdedToggleOption KillDuringCamoComms { get; set; } = new("Kill Anyone During Camouflage", true)
    {
        Visible = () => OptionGroupSingleton<AdvancedSabotageOptions>.Instance.CamouflageComms
    };

    public ModdedToggleOption CamoKillScreens { get; set; } = new("Camouflage Kill Screens During Comms", false)
    {
        Visible = () => OptionGroupSingleton<AdvancedSabotageOptions>.Instance.CamouflageComms
    };

    public ModdedToggleOption HidePlayerSizeInCamo { get; set; } = new("Camouflage Hides Player Size", false)
    {
        Visible = () => OptionGroupSingleton<AdvancedSabotageOptions>.Instance.CamouflageComms
    };

    public ModdedToggleOption HidePlayerSpeedInCamo { get; set; } = new("Camouflage Hides Player Speed", false)
    {
        Visible = () => OptionGroupSingleton<AdvancedSabotageOptions>.Instance.CamouflageComms
    };
}