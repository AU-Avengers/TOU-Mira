using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class BootleggerOptions : AbstractRoleOptionGroup<BootleggerRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Bootlegger", "Bootlegger");

    public ModdedNumberOption RoleblockCooldown { get; } =
        new("TouOptionBarkeeperRoleblockCooldown", 22.5f, 15f, 120f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RoleblockDelayMin { get; } =
        new("TouOptionBarkeeperRoleblockDelayMin", 1.5f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RoleblockDelayMax { get; } =
        new("TouOptionBarkeeperRoleblockDelayMax", 5f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedEnumOption PoisonRoleblockTrigger { get; } =
        new("TouOptionBootleggerPoisonTriggersOn", (int)PoisonTrigger.OnDurationEnd, typeof(PoisonTrigger), ["TouOptionBootleggerPoisonTriggerEnumDelayEnd", "TouOptionBootleggerPoisonTriggerEnumMeetingStart", "TouOptionBootleggerPoisonTriggerEnumMeetingEnd"]);

    public ModdedNumberOption ForcedPoisonDelay { get; } =
        new("TouOptionBootleggerPoisonDelay", 15f, 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => (PoisonTrigger)OptionGroupSingleton<BootleggerOptions>.Instance.PoisonRoleblockTrigger.Value is PoisonTrigger.OnDurationEnd
        };

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>
        {
            RoleblockDelayMin.StringName,
            RoleblockDelayMax.StringName,
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        string[] array =
        [
            MiraLocaleManager.Get("TouOptionBarkeeperRoleblockDelaySummarized").Replace("<min>", RoleblockDelayMin.Value.ToString(TownOfUsPlugin.Culture)).Replace("<max>", RoleblockDelayMax.Value.ToString(TownOfUsPlugin.Culture))
        ];
        return array;
    }
}

public enum PoisonTrigger
{
    OnDurationEnd,
    OnMeetingStart,
    OnMeetingEnd
}