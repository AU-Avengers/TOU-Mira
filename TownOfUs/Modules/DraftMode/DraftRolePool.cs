using AmongUs.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Roles;

namespace TownOfUs.Modules.DraftMode;

public sealed record DraftCopy(ushort RoleId, RoleAlignment Alignment, int Chance, int Weight);

public static class DraftRolePool
{
    public static readonly RoleAlignment[] Crew =
    [
        RoleAlignment.CrewmateInvestigative, RoleAlignment.CrewmateKilling, RoleAlignment.CrewmateProtective,
        RoleAlignment.CrewmatePower, RoleAlignment.CrewmateSupport
    ];

    public static readonly RoleAlignment[] Impostor =
    [
        RoleAlignment.ImpostorConcealing, RoleAlignment.ImpostorKilling, RoleAlignment.ImpostorPower,
        RoleAlignment.ImpostorSupport
    ];

    public static readonly RoleAlignment[] Neutral =
    [
        RoleAlignment.NeutralBenign, RoleAlignment.NeutralEvil, RoleAlignment.NeutralKilling,
        RoleAlignment.NeutralOutlier
    ];

    public static List<DraftCopy> Build()
    {
        var blocked = MiscUtils.SpawnableRoles
            .Where(x => x is ISpawnChange { NoSpawn: true })
            .Select(x => x.Role)
            .ToHashSet();

        bool Allowed(RoleBehaviour role) => !blocked.Contains(role.Role) && role is not ITownOfUsRole { IsDraftable: false };

        var pool = new List<DraftCopy>();
        foreach (var alignment in Crew.Concat(Impostor).Concat(Neutral))
        {
            foreach (var (_, roleType, chance) in MiscUtils.GetRolesToAssign(alignment, Allowed))
            {
                if (chance <= 0)
                {
                    continue;
                }

                var weight = MiscUtils.GetRegisteredRole((RoleTypes)roleType) is IDoubleDraftRole { IsDoubleDraftRole: true } ? 2 : 1;
                pool.Add(new DraftCopy(roleType, alignment, chance, weight));
            }
        }

        return pool;
    }

    public static string GetRoleNameFromId(ushort id)
    {
        var role = MiscUtils.GetRegisteredRole((RoleTypes)id);
        return (role?.GetRoleName() ?? role?.NiceName)!;
    }

    public static bool IsImpostorRoleId(ushort id) => MiscUtils.GetRegisteredRole((RoleTypes)id)?.IsImpostor() ?? false;

    public static bool IsNeutralRoleId(ushort id) => MiscUtils.GetRegisteredRole((RoleTypes)id)?.IsNeutral() ?? false;
}
