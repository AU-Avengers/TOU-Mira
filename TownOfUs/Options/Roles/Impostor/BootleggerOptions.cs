using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class BootleggerOptions : AbstractRoleOptionGroup<BootleggerRole>
{
    public override string GroupName => "Bootlegger";

    public ModdedNumberOption RoleblockCooldown { get; } =
        new("Roleblock Cooldown", 22.5f, 15f, 120f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RoleblockDelayMin { get; } =
        new("Minimum Roleblock Delay", 1.5f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RoleblockDelayMax { get; } =
        new("Maximum Roleblock Delay", 5f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedEnumOption PoisonRoleblockTrigger { get; } =
        new("Poison Triggers On", (int)PoisonTrigger.OnDurationEnd, typeof(PoisonTrigger), ["Delay End", "Meeting Start", "Meeting End"]);

    public ModdedNumberOption ForcedPoisonDelay { get; } =
        new("Poison Delay", 15f, 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => (PoisonTrigger)OptionGroupSingleton<BootleggerOptions>.Instance.PoisonRoleblockTrigger.Value is PoisonTrigger.OnDurationEnd
        };
}

public enum PoisonTrigger
{
    OnDurationEnd,
    OnMeetingStart,
    OnMeetingEnd
}