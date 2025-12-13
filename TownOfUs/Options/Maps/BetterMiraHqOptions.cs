using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Maps;

public sealed class BetterMiraHqOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.CustomOne;
    public override string GroupName => "Better Mira HQ";
    public override uint GroupPriority => 4;
    public override Color GroupColor => new Color32(255, 128, 100, 255);

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

    public ModdedEnumOption BetterVentNetwork { get; set; } = new("TouOptionBetterMiraHqVentNetwork",
        (int)MiraVentMode.Normal, typeof(MiraVentMode),
        [
            "TouOptionBetterMiraHqVentModeEnumNormal", "TouOptionBetterMiraHqVentModeEnumThreeGroups", "TouOptionBetterMiraHqVentModeEnumFourGroups"
        ]);

    [ModdedToggleOption("TouOptionBetterMapsChangeSaboTimers")]
    public bool ChangeSaboTimers { get; set; } = true;

    public ModdedNumberOption SaboCountdownOxygen { get; set; } = new("TouOptionBetterMapsSaboCountdownOxygen", 45f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterMiraHqOptions>.Instance.ChangeSaboTimers
    };

    public ModdedNumberOption SaboCountdownReactor { get; set; } = new("TouOptionBetterMapsSaboCountdownReactor", 45f, 15f, 90f,
        5f, MiraNumberSuffixes.Seconds, "0.#")
    {
        Visible = () =>
            OptionGroupSingleton<BetterMiraHqOptions>.Instance.ChangeSaboTimers
    };
}

public enum MiraVentMode
{
    Normal,
    ThreeGroups,
    FourGroups
}