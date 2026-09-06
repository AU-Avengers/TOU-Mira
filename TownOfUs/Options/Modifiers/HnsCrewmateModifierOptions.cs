using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class HnsCrewmateModifierOptions : AbstractOptionGroup
{
    public override string GroupName => MiraLocaleManager.Get("HnsOptionTitleHiderModifiers");
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution() is RoleDistribution.HideAndSeek;
    public override Color GroupColor => Palette.CrewmateRoleHeaderBlue;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 0;

    public ModdedNumberOption FrostyAmount { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.FrostyAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption FrostyChance { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.FrostyChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.FrostyAmount > 0
        };

    public ModdedNumberOption GiantAmount { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.GiantAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption GiantChance { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.GiantChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.GiantAmount > 0
        };

    public ModdedNumberOption MiniAmount { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.MiniAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption MiniChance { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.MiniChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.MiniAmount > 0
        };

    public ModdedNumberOption MultitaskerAmount { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.MultitaskerAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption MultitaskerChance { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.MultitaskerChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.MultitaskerAmount > 0
        };

    public ModdedNumberOption ObliviousAmount { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.ObliviousAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ObliviousChance { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.ObliviousChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.ObliviousAmount > 0
        };

    public ModdedNumberOption TransporterAmount { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.TransporterAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption TransporterChance { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.TransporterChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.TransporterAmount > 0
        };

    /*public ModdedNumberOption WaryAmount { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.WaryAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption WaryChance { get; } =
        new("TownOfUsMira.HideAndSeek.Modifier.Option.WaryChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.WaryAmount > 0
        };*/
}