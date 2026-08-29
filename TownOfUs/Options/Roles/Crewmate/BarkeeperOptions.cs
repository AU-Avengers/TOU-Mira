using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class BarkeeperOptions : AbstractRoleOptionGroup<BarkeeperRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => "Barkeeper";

    public ModdedNumberOption RoleblockCooldown { get; } =
        new("TouOptionBarkeeperRoleblockCooldown", 22.5f, 15f, 120f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RoleblockDelayMin { get; } =
        new("TouOptionBarkeeperRoleblockDelayMin", 3f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RoleblockDelayMax { get; } =
        new("TouOptionBarkeeperRoleblockDelayMax", 5f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption SpillDelay { get; } =
        new("TouOptionBarkeeperSpillDelay", 5f, 2.5f, 15f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption SpillBuffDuration { get; } =
        new("TouOptionBarkeeperSpillBuffDuration", 20f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption SpillCleanUpDuration { get; } =
        new("TouOptionBarkeeperSpillCleanUpDuration", 30f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption SpillEffectDuration { get; } =
        new("TouOptionBarkeeperSpillEffectDuration", 20f, 7.5f, 45f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption SpillEffectBuffMultiplier { get; } =
        new("TouOptionBarkeeperSpillEffectBuffMultiplier", 1.2f, 1.05f, 2f, 0.05f, MiraNumberSuffixes.Multiplier);

    public ModdedNumberOption SpillEffectDebuffMultiplier { get; } =
        new("TouOptionBarkeeperSpillEffectDebuffMultiplier", 0.8f, 0.25f, 0.95f, 0.05f, MiraNumberSuffixes.Multiplier);

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>
        {
            RoleblockDelayMin.StringName,
            RoleblockDelayMax.StringName,
            SpillEffectDuration.StringName,
            SpillEffectBuffMultiplier.StringName,
            SpillEffectDebuffMultiplier.StringName,
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        string[] array =
        [
            MiraLocaleManager.Get("TouOptionBarkeeperRoleblockDelaySummarized")
                .Replace("<min>", RoleblockDelayMin.Value.ToString(TownOfUsPlugin.Culture)).Replace("<max>",
                    RoleblockDelayMax.Value.ToString(TownOfUsPlugin.Culture)),
            MiraLocaleManager.Get("TouOptionBarkeeperSpillDurationOptionsSummarized")
                .Replace("<duration>", SpillEffectDuration.Value.ToString(TownOfUsPlugin.Culture)).Replace("<fast>",
                    SpillEffectBuffMultiplier.Value.ToString(TownOfUsPlugin.Culture)).Replace("<slow>",
                    SpillEffectDebuffMultiplier.Value.ToString(TownOfUsPlugin.Culture))
        ];
        return array;
    }
}