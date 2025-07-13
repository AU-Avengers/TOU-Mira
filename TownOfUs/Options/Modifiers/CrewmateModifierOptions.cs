﻿using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class CrewmateModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Crewmate Modifiers";
    public override Color GroupColor => Palette.CrewmateRoleHeaderBlue;
    public override bool ShowInModifiersMenu => true;
    public override uint GroupPriority => 2;

    [ModdedNumberOption("Aftermath Amount", 0, 5)]
    public float AftermathAmount { get; set; } = 0;

    public ModdedNumberOption AftermathChance { get; } =
        new("Aftermath Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.AftermathAmount > 0
        };

    [ModdedNumberOption("Bait Amount", 0, 5)]
    public float BaitAmount { get; set; } = 0;

    public ModdedNumberOption BaitChance { get; } = new("Bait Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.BaitAmount > 0
    };

    [ModdedNumberOption("Celebrity Amount", 0, 5)]
    public float CelebrityAmount { get; set; } = 0;

    public ModdedNumberOption CelebrityChance { get; } =
        new("Celebrity Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.CelebrityAmount > 0
        };

    [ModdedNumberOption("Diseased Amount", 0, 5)]
    public float DiseasedAmount { get; set; } = 0;

    public ModdedNumberOption DiseasedChance { get; } =
        new("Diseased Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.DiseasedAmount > 0
        };

    [ModdedNumberOption("Frosty Amount", 0, 5)]
    public float FrostyAmount { get; set; } = 0;

    public ModdedNumberOption FrostyChance { get; } =
        new("Frosty Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.FrostyAmount > 0
        };

    [ModdedNumberOption("Multitasker Amount", 0, 5)]
    public float MultitaskerAmount { get; set; } = 0;

    public ModdedNumberOption MultitaskerChance { get; } =
        new("Multitasker Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.MultitaskerAmount > 0
        };

    [ModdedNumberOption("Noisemaker Amount", 0, 5)]
    public float NoisemakerAmount { get; set; } = 0;

    public ModdedNumberOption NoisemakerChance { get; } =
        new("Noisemaker Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.NoisemakerAmount > 0
        };

    [ModdedNumberOption("Rotting Amount", 0, 5)]
    public float RottingAmount { get; set; } = 0;

    public ModdedNumberOption RottingChance { get; } =
        new("Rotting Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.RottingAmount > 0
        };

    [ModdedNumberOption("Taskmaster Amount", 0, 5)]
    public float TaskmasterAmount { get; set; } = 0;

    public ModdedNumberOption TaskmasterChance { get; } =
        new("Taskmaster Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.TaskmasterAmount > 0
        };

    [ModdedNumberOption("Torch Amount", 0, 5)]
    public float TorchAmount { get; set; } = 0;

    public ModdedNumberOption TorchChance { get; } = new("Torch Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.TorchAmount > 0
    };
}