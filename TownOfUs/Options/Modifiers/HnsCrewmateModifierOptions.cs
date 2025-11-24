using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class HnsCrewmateModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Hider Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.HideAndSeek;
    public override Color GroupColor => Palette.CrewmateRoleHeaderBlue;
    public override bool ShowInModifiersMenu => true;
    public override uint GroupPriority => 0;

    public ModdedNumberOption FrostyAmount { get; } =
        new("Frosty Amount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption FrostyChance { get; } =
        new("Frosty Chance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.FrostyAmount > 0
        };

    public ModdedNumberOption GiantAmount { get; } =
        new("Giant Amount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption GiantChance { get; } =
        new("Giant Chance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.GiantAmount > 0
        };

    public ModdedNumberOption MiniAmount { get; } =
        new("Mini Amount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption MiniChance { get; } =
        new("Mini Chance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.MiniAmount > 0
        };

    public ModdedNumberOption MultitaskerAmount { get; } =
        new("Multitasker Amount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption MultitaskerChance { get; } =
        new("Multitasker Chance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.MultitaskerAmount > 0
        };

    public ModdedNumberOption ObliviousAmount { get; } =
        new("Oblivious Amount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ObliviousChance { get; } =
        new("Oblivious Chance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.ObliviousAmount > 0
        };

    /*public ModdedNumberOption TransporterAmount { get; } =
        new("Transporter Amount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption TransporterChance { get; } =
        new("Transporter Chance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.TransporterAmount > 0
        };

    public ModdedNumberOption WaryAmount { get; } =
        new("Wary Amount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption WaryChance { get; } =
        new("Wary Chance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.WaryAmount > 0
        };*/
}