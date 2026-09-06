using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class AdvancedUtilityOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.AdvancedUtility");
    public override uint GroupPriority => 2;
    public override Color GroupColor => new Color32(173, 180, 179, 255);

    public ModdedNumberOption TasksToUseAdmin { get; set; } = new("TownOfUsMira.AdvancedUtils.Option.TasksToUseAdmin", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption TasksToUseCams { get; set; } = new("TownOfUsMira.AdvancedUtils.Option.TasksToUseCams", 2f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption TasksToUseDoorlog { get; set; } = new("TownOfUsMira.AdvancedUtils.Option.TasksToUseDoorlog", 0f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption TasksToUseVitals { get; set; } = new("TownOfUsMira.AdvancedUtils.Option.TasksToUseVitals", 3f, 0f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedToggleOption TasksOnPortables { get; set; } = new("TownOfUsMira.AdvancedUtils.Option.TasksRequiredOnPortableUtils", true);

}