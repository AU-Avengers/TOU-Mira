using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class BarkeeperOptions : AbstractRoleOptionGroup<BarkeeperRole>
{
    public override string GroupName => "Barkeeper";

    public ModdedNumberOption RoleblockCooldown { get; } =
        new("TouOptionBarkeeperRoleblockCooldown", 22.5f, 15f, 120f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RoleblockDelayMin { get; } =
        new("TouOptionBarkeeperRoleblockDelayMin", 3f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RoleblockDelayMax { get; } =
        new("TouOptionBarkeeperRoleblockDelayMax", 5f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);
}