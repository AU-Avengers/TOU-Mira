using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterPolusOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Polus";
    public override uint GroupPriority => 5;
    public override Color GroupColor => new Color32(157, 146, 198, 255);

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

    public ModdedEnumOption PolusDoorType { get; set; } = new("TouOptionBetterPolusDoorType", (int)MapDoorType.Polus, typeof(MapDoorType),
    [
        "TouOptionBetterDoorsEnumSkeld", "TouOptionBetterDoorsEnumPolus", "TouOptionBetterDoorsEnumAirship",
        "TouOptionBetterDoorsEnumFungle", "TouOptionBetterDoorsEnumSubmerged", "TouOptionBetterDoorsEnumNoDoors",
        "TouOptionBetterDoorsEnumRandom"
    ]);

    [ModdedToggleOption("TouOptionBetterPolusVentNetwork")]
    public bool BPVentNetwork { get; set; } = false;

    [ModdedToggleOption("TouOptionBetterPolusVitalsInLab")]
    public bool BPVitalsInLab { get; set; } = false;

    [ModdedToggleOption("TouOptionBetterPolusTempInDeathValley")]
    public bool BPTempInDeathValley { get; set; } = false;

    [ModdedToggleOption("TouOptionBetterPolusSwapWikiAndChart")]
    public bool BPSwapWifiAndChart { get; set; } = false;

    [ModdedToggleOption("TouOptionBetterPolusMoveToiletVent")]
    public bool MoveToiletVent { get; set; } = false;

    [ModdedToggleOption("TouOptionBetterMapsChangeSaboTimers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("TouOptionBetterMapsSaboCountdownSeismicStabilizer", 60f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterPolusOptions>.Instance.ChangeSaboTimers
    };
}