using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral.NeutralOutlier;

namespace TownOfUs.Options;

public enum DuelistWinType
{
    WinAlone,
    LeaveInVictory,
}

public enum DuelSpawnType
{
    Close,
    Far,
    Random,
}

public class DuelistOptions : AbstractRoleOptionGroup<DuelistRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Duelist", "Duelist");

    public ModdedNumberOption DuelCooldown { get; } = new(
        MiraLocaleManager.Get("TownOfUsMira.Options.Duelist.DuelCooldown"), 40f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption DuelSpeed { get; } = new(
        MiraLocaleManager.Get("TownOfUsMira.Options.Duelist.DuelSpeed"), 1.10f, 1.00f, 1.50f, 0.05f, MiraNumberSuffixes.Multiplier);

    public ModdedNumberOption DuelsToWin { get; } = new(
        MiraLocaleManager.Get("TownOfUsMira.Options.Duelist.DuelsToWin"), 4f, 1f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption DuelsLostToDie { get; } = new(
        MiraLocaleManager.Get("TownOfUsMira.Options.Duelist.DuelsLostToDie"), 2f, 1f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedEnumOption WinType { get; } = new(
        MiraLocaleManager.Get("TownOfUsMira.Options.Duelist.WinType"), (int)DuelistWinType.WinAlone, typeof(DuelistWinType),
        [
            MiraLocaleManager.Get("TownOfUsMira.Options.Duelist.WinType.WinAlone"),
            MiraLocaleManager.Get("TownOfUsMira.Options.Duelist.WinType.LeaveInVictory")
        ]
        );

    public ModdedEnumOption SpawnType { get; } = new(
        MiraLocaleManager.Get("TownOfUsMira.Options.Duelist.SpawnType"), (int)DuelSpawnType.Close, typeof(DuelSpawnType),
        [
            MiraLocaleManager.Get("TownOfUsMira.Options.Duelist.SpawnType.Close"),
            MiraLocaleManager.Get("TownOfUsMira.Options.Duelist.SpawnType.Far"),
            MiraLocaleManager.Get("TownOfUsMira.Options.Duelist.SpawnType.Random")
        ]);
}
