using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral.NeutralEvil;
using TownOfUs.Interfaces;
using TownOfUs.Utilities;

namespace TownOfUs.Options;

public class DemolitionistOptions : AbstractRoleOptionGroup<DemolitionistRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist", "Demolitionist");

    public ModdedNumberOption SabotagesToWin { get; } = new(
        MiraLocaleManager.Get("TownOfUsMira.Options.Demolitionist.SabotagesToWin"), 1f, 1f, 4f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption PlantCooldown { get; } = new(
        MiraLocaleManager.Get("TownOfUsMira.Options.Demolitionist.PlantCooldown"), 30f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption PlantToSabotageDelay { get; } = new(
        MiraLocaleManager.Get("TownOfUsMira.Options.Demolitionist.PlantToSabotageDelay"), 3f, 1f, 10f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption SabotageDurationSkeld { get; } =
        new(MiraLocaleManager.Get("TownOfUsMira.Options.Demolitionist.SabotageDuration.Skeld"), 20f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapDurationOption(ExpandedMapNames.Skeld),
        };

    public ModdedNumberOption SabotageDurationMiraHQ { get; } =
        new(MiraLocaleManager.Get("TownOfUsMira.Options.Demolitionist.SabotageDuration.MiraHQ"), 20f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapDurationOption(ExpandedMapNames.MiraHq),
        };

    public ModdedNumberOption SabotageDurationPolus { get; } =
        new(MiraLocaleManager.Get("TownOfUsMira.Options.Demolitionist.SabotageDuration.Polus"), 20f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapDurationOption(ExpandedMapNames.Polus),
        };

    public ModdedNumberOption SabotageDurationFungle { get; } =
        new(MiraLocaleManager.Get("TownOfUsMira.Options.Demolitionist.SabotageDuration.Fungle"), 30f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapDurationOption(ExpandedMapNames.Fungle),
        };

    public ModdedNumberOption SabotageDurationAirship { get; } =
        new(MiraLocaleManager.Get("TownOfUsMira.Options.Demolitionist.SabotageDuration.Airship"), 60f, 10f, 180f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapDurationOption(ExpandedMapNames.Airship),
        };

    public float SabotageDuration => GetSabotageDurationOptionForMap(MiscUtils.GetCurrentMap).Value;

    public ModdedEnumOption SabotageStyle { get; } = new(
        MiraLocaleManager.Get("TownOfUsMira.Options.Demolitionist.SabotageStyle"),
        (int)DemolitionistSabotageStyle.Numpad,
        typeof(DemolitionistSabotageStyle));

    public ModdedNumberOption PlantTime { get; } = new(
        MiraLocaleManager.Get("TownOfUsMira.Options.Demolitionist.PlantTime"), 5f, 2f, 10f, 1f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<DemolitionistOptions>.Instance.IsTimedSabotageStyle,
    };

    public ModdedNumberOption DefuseTime { get; } = new(
        MiraLocaleManager.Get("TownOfUsMira.Options.Demolitionist.DefuseTime"), 5f, 2f, 10f, 1f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<DemolitionistOptions>.Instance.IsTimedSabotageStyle,
    };

    [ModdedToggleOption("TownOfUsMira.Options.Demolitionist.CanVent")]
    public bool CanVent { get; set; } = false;

    [ModdedToggleOption("TownOfUsMira.Options.Demolitionist.DisableExplodedConsoles")]
    public bool DisableExplodedConsoles { get; set; } = true;

    [ModdedToggleOption("TownOfUsMira.Options.Demolitionist.ExplosionKillsDefusers")]
    public bool ExplosionKillsDefusers { get; set; } = false;

    [ModdedEnumOption("TownOfUsMira.Options.Demolitionist.WinOutcome", typeof(AltWinResult), 
    [
        "TownOfUsMira.Options.Demolitionist.WinOutcome.EndsGame",
        "TownOfUsMira.Options.Demolitionist.WinOutcome.KillOnePlayer",
        "TownOfUsMira.Options.Demolitionist.WinOutcome.Nothing"
    ]
    )]
    public AltWinResult WinOutcome { get; set; } = AltWinResult.EndsGame;

    public bool IsTimedSabotageStyle => (DemolitionistSabotageStyle)SabotageStyle.Value is DemolitionistSabotageStyle.Timed;

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        ShipStatus.Instance != null
            ? new HashSet<StringNames>
            {
                SabotageDurationSkeld.StringName,
                SabotageDurationMiraHQ.StringName,
                SabotageDurationPolus.StringName,
                SabotageDurationFungle.StringName,
                SabotageDurationAirship.StringName,
            }
            : new HashSet<StringNames>();

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        if (ShipStatus.Instance == null)
        {
            return [];
        }

        var option = GetSabotageDurationOptionForMap(MiscUtils.GetCurrentMap);
        var valueStr = FormatWikiNumberValue(option);
        var title = TranslationController.Instance != null
            ? TranslationController.Instance.GetString(option.StringName)
            : option.StringName.ToString();

        return new[] { $"{title}: {valueStr}" };
    }

    private ModdedNumberOption GetSabotageDurationOptionForMap(ExpandedMapNames map) =>
        map switch
        {
            ExpandedMapNames.MiraHq => SabotageDurationMiraHQ,
            ExpandedMapNames.Polus => SabotageDurationPolus,
            ExpandedMapNames.Fungle => SabotageDurationFungle,
            ExpandedMapNames.Airship => SabotageDurationAirship,
            _ => SabotageDurationSkeld,
        };

    private static bool ShouldShowMapDurationOption(ExpandedMapNames mapOption)
    {
        if (ShipStatus.Instance == null)
        {
            return true;
        }

        return mapOption == GetMapDurationOptionKey(MiscUtils.GetCurrentMap);
    }

    private static ExpandedMapNames GetMapDurationOptionKey(ExpandedMapNames currentMap) =>
        currentMap switch
        {
            ExpandedMapNames.MiraHq => ExpandedMapNames.MiraHq,
            ExpandedMapNames.Polus => ExpandedMapNames.Polus,
            ExpandedMapNames.Fungle => ExpandedMapNames.Fungle,
            ExpandedMapNames.Airship => ExpandedMapNames.Airship,
            _ => ExpandedMapNames.Skeld,
        };

    private static string FormatWikiNumberValue(ModdedNumberOption numberOption)
    {
        var optionStr = numberOption.Data.GetValueString(numberOption.Value);
        if (optionStr.Contains(".000"))
        {
            optionStr = optionStr.Replace(".000", "");
        }
        else if (optionStr.Contains(".00"))
        {
            optionStr = optionStr.Replace(".00", "");
        }
        else if (optionStr.Contains(".0"))
        {
            optionStr = optionStr.Replace(".0", "");
        }

        return optionStr;
    }
}

public enum DemolitionistSabotageStyle
{
    Timed,
    Numpad,
}
