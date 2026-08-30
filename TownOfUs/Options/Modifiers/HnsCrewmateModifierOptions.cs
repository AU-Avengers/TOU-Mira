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
        new("HnsOptionFrostyAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption FrostyChance { get; } =
        new("HnsOptionFrostyChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.FrostyAmount > 0
        };

    public ModdedNumberOption GiantAmount { get; } =
        new("HnsOptionGiantAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption GiantChance { get; } =
        new("HnsOptionGiantChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.GiantAmount > 0
        };

    public ModdedNumberOption MiniAmount { get; } =
        new("HnsOptionMiniAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption MiniChance { get; } =
        new("HnsOptionMiniChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.MiniAmount > 0
        };

    public ModdedNumberOption MultitaskerAmount { get; } =
        new("HnsOptionMultitaskerAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption MultitaskerChance { get; } =
        new("HnsOptionMultitaskerChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.MultitaskerAmount > 0
        };

    public ModdedNumberOption ObliviousAmount { get; } =
        new("HnsOptionObliviousAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ObliviousChance { get; } =
        new("HnsOptionObliviousChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.ObliviousAmount > 0
        };

    public ModdedNumberOption TransporterAmount { get; } =
        new("HnsOptionTransporterAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption TransporterChance { get; } =
        new("HnsOptionTransporterChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.TransporterAmount > 0
        };

    /*public ModdedNumberOption WaryAmount { get; } =
        new("HnsOptionWaryAmount", 1f, 0f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption WaryChance { get; } =
        new("HnsOptionWaryChance", 10f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.WaryAmount > 0
        };*/
}