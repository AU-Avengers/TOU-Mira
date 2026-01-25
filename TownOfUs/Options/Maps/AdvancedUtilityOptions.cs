using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class AdvancedUtilityOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Advanced Utilities";
    public override uint GroupPriority => 2;
    public override Color GroupColor => new Color32(173, 180, 179, 255);

    public ModdedNumberOption TasksToUseAdmin { get; set; } = new("TouOptionAdvancedUtilTasksToUseAdmin", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption TasksToUseCams { get; set; } = new("TouOptionAdvancedUtilTasksToUseCams", 2f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption TasksToUseDoorlog { get; set; } = new("TouOptionAdvancedUtilTasksToUseDoorlog", 0f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption TasksToUseVitals { get; set; } = new("TouOptionAdvancedUtilTasksToUseVitals", 3f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedToggleOption TasksOnPortables { get; set; } = new("TouOptionAdvancedUtilTasksRequiredOnPortableUtils", true);

}