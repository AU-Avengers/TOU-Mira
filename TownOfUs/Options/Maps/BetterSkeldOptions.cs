using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterSkeldOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Skeld";
    public override uint GroupPriority => 3;
    public override Color GroupColor => new Color32(188, 206, 200, 255);

    [ModdedNumberOption("TouOptionBetterMapsSpeedMultiplier", 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float SpeedMultiplier { get; set; } = 1f;

    [ModdedNumberOption("TouOptionBetterMapsCrewVisionMultiplier", 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float CrewVisionMultiplier { get; set; } = 1f;
    
    [ModdedNumberOption("TouOptionBetterMapsImpVisionMultiplier", 0.25f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float ImpVisionMultiplier { get; set; } = 1f;

    [ModdedNumberOption("TouOptionBetterMapsCooldownOffset", -15f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CooldownOffset { get; set; } = 0f;

    [ModdedNumberOption("TouOptionBetterMapsOffsetShortTasks", -5f, 5f)]
    public float OffsetShortTasks { get; set; } = 0f;

    [ModdedNumberOption("TouOptionBetterMapsOffsetLongTasks", -3f, 3f)]
    public float OffsetLongTasks { get; set; } = 0f;

    public ModdedEnumOption SkeldDoorType { get; set; } = new("TouOptionBetterSkeldDoorType", (int)MapDoorType.Skeld, typeof(MapDoorType),
    [
        "TouOptionBetterDoorsEnumSkeld", "TouOptionBetterDoorsEnumPolus", "TouOptionBetterDoorsEnumAirship",
        "TouOptionBetterDoorsEnumFungle", "TouOptionBetterDoorsEnumSubmerged", "TouOptionBetterDoorsEnumNoDoors",
        "TouOptionBetterDoorsEnumRandom"
    ]);

    [ModdedToggleOption("TouOptionBetterMapsChangeSaboTimers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownOxygen { get; set; } = new("TouOptionBetterMapsSaboCountdownOxygen", 30f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterSkeldOptions>.Instance.ChangeSaboTimers
    };

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("TouOptionBetterMapsSaboCountdownReactor", 30f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterSkeldOptions>.Instance.ChangeSaboTimers
    };
}