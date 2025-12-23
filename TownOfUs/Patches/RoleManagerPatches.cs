using System.Collections;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Events;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class TouRoleManagerPatches
{
    private static readonly List<RoleTypes> CrewmateGhostRolePool = [];
    private static readonly List<RoleTypes> ImpostorGhostRolePool = [];
    private static readonly List<RoleTypes> CustomGhostRolePool = [];

    public static bool ReplaceRoleManager;
    private static List<int> LastImps { get; set; } = [];

    private static void GhostRoleSetup()
    {
        // var ghostRoles = MiscUtils.AllRegisteredRoles.Where(x => x.IsDead);
        var ghostRoles = MiscUtils.GetRegisteredGhostRoles();

        var text = $"GhostRoleSetup - ghostRoles Count: {ghostRoles.Count()}";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Warning, text);

        CrewmateGhostRolePool.Clear();
        ImpostorGhostRolePool.Clear();
        CustomGhostRolePool.Clear();

        foreach (var role in ghostRoles)
        {
            var ghostText = $"GhostRoleSetup - ghostRoles role NiceName: {role.GetRoleName()}";

            MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Warning, ghostText);

            var data = MiscUtils.GetAssignData(role.Role);

            switch (data.Chance)
            {
                case 100:
                    {
                        if (data.Count > 0)
                        {
                            if (role is ICustomRole { Team: ModdedRoleTeams.Custom })
                            {
                                CustomGhostRolePool.Add(role.Role);
                            }
                            else
                            {
                                switch (role.TeamType)
                                {
                                    case RoleTeamTypes.Crewmate:
                                        CrewmateGhostRolePool.Add(role.Role);
                                        break;
                                    case RoleTeamTypes.Impostor:
                                        ImpostorGhostRolePool.Add(role.Role);
                                        break;
                                }
                            }
                        }

                        break;
                    }
                case > 0:
                    {
                        if (data.Count > 0 && HashRandom.Next(101) < data.Chance)
                        {
                            if (role is ICustomRole { Team: ModdedRoleTeams.Custom })
                            {
                                CustomGhostRolePool.Add(role.Role);
                            }
                            else
                            {
                                switch (role.TeamType)
                                {
                                    case RoleTeamTypes.Crewmate:
                                        CrewmateGhostRolePool.Add(role.Role);
                                        break;
                                    case RoleTeamTypes.Impostor:
                                        ImpostorGhostRolePool.Add(role.Role);
                                        break;
                                }
                            }
                        }

                        break;
                    }
            }
        }

        CrewmateGhostRolePool.RemoveAll(x => x == (RoleTypes)RoleId.Get<HaunterRole>());
        CustomGhostRolePool.RemoveAll(x =>
            x == (RoleTypes)RoleId.Get<SpectreRole>() || x == (RoleTypes)RoleId.Get<SpectatorRole>());
    }

    /// <summary>
    /// Adjusts neutral role counts to ensure crewmates always outnumber neutrals.
    /// </summary>
    private static void AdjustNeutralCounts(ref int nbCount, ref int neCount, ref int nkCount, ref int noCount, int crewmateCount)
    {
        var roleOptions = OptionGroupSingleton<RoleOptions>.Instance;
        var minBenign = (int)roleOptions.MinNeutralBenign.Value;
        var minEvil = (int)roleOptions.MinNeutralEvil.Value;
        var minKilling = (int)roleOptions.MinNeutralKiller.Value;
        var minOutlier = (int)roleOptions.MinNeutralOutlier.Value;

        // Crew must always start out outnumbering neutrals, so subtract roles until that can be guaranteed.
        while (Math.Ceiling((double)crewmateCount / 2) <= nbCount + neCount + nkCount + noCount)
        {
            var totalNeutrals = nbCount + neCount + nkCount + noCount;
            if (totalNeutrals == 0)
            {
                break;
            }

            // This is one of the things I did change. The old code was actually biased and partially deterministic.
            // Over many games, it would silently favor removing certain neutral factions more often than others (for example, neutral benign was usually protected more than outlier).
            // Now, every faction has equal probability per subtraction. Statistically the most fair way to do it.
            // (I know this is nitpicky but I think it's better than just a list regardless)
            var factionIndices = new List<int> { 0, 1, 2, 3 };
            factionIndices.Shuffle();

            // Determine which factions can be subtracted
            var canSubtractBenign = nbCount > minBenign;
            var canSubtractEvil = neCount > minEvil;
            var canSubtractKilling = nkCount > minKilling;
            var canSubtractOutlier = noCount > minOutlier;
            var canSubtractAny = canSubtractBenign || canSubtractEvil || canSubtractKilling || canSubtractOutlier;

            // Try to subtract from a random faction that can be subtracted
            bool subtracted = false;
            foreach (var index in factionIndices)
            {
                switch (index)
                {
                    case 0 when nbCount > 0 && (canSubtractBenign || !canSubtractAny):
                        nbCount -= 1;
                        subtracted = true;
                        break;
                    case 1 when neCount > 0 && (canSubtractEvil || !canSubtractAny):
                        neCount -= 1;
                        subtracted = true;
                        break;
                    case 2 when nkCount > 0 && (canSubtractKilling || !canSubtractAny):
                        nkCount -= 1;
                        subtracted = true;
                        break;
                    case 3 when noCount > 0 && (canSubtractOutlier || !canSubtractAny):
                        noCount -= 1;
                        subtracted = true;
                        break;
                }

                if (subtracted)
                {
                    break;
                }
            }

            // Fallback: subtract from first available faction
            if (!subtracted)
            {
                if (nbCount > 0)
                {
                    nbCount -= 1;
                }
                else if (neCount > 0)
                {
                    neCount -= 1;
                }
                else if (nkCount > 0)
                {
                    nkCount -= 1;
                }
                else if (noCount > 0)
                {
                    noCount -= 1;
                }
            }
        }
    }

    /// <summary>
    /// Adds /up requested roles to the appropriate role pools.
    /// </summary>
    private static void AddUpRequestedRolesToPools(List<ushort> impRoles, List<ushort> crewRoles)
    {
        var upRequests = UpCommandRequests.GetAllRequests();
        foreach (var (playerName, _) in upRequests)
        {
            if (!UpCommandRequests.TryGetRequestRole(playerName, out var requestedRole))
            {
                continue;
            }

            var requestedRoleId = (ushort)requestedRole.Role;
            var targetPool = requestedRole.IsImpostor() ? impRoles : crewRoles;

            // Add to appropriate pool if not already present
            if (!targetPool.Contains(requestedRoleId))
            {
                targetPool.Add(requestedRoleId);
            }
        }
    }

    /// <summary>
    /// Assigns roles to players, handling /up requests first, then random assignment.
    /// </summary>
    private static void AssignRolesToPlayers(List<PlayerControl> players, List<ushort> roles, string teamName)
    {
        if (roles.Count == 0 || players.Count == 0)
        {
            return;
        }
        Warning($"Assigning {roles.Count} {teamName} roles to {players.Count} players...");

        // Shuffle both lists for better randomness and fairness
        players.Shuffle();
        roles.Shuffle();

        // Collect /up requested roles first
        var upRequestedRoles = new List<(ushort roleId, string playerName)>();
        foreach (var player in players.ToList())
        {
            if (UpCommandRequests.TryGetRequestRole(player.Data.PlayerName, out var requestedRole))
            {
                var requestedRoleId = (ushort)requestedRole.Role;
                // Only add if the role is in the pool and the player is still available
                if (roles.Contains(requestedRoleId) && players.Contains(player))
                {
                    upRequestedRoles.Add((requestedRoleId, player.Data.PlayerName));
                }
            }
        }

        // Shuffle up requests for fairness when multiple players request the same role
        upRequestedRoles.Shuffle();

        // Assign /up requested roles first
        foreach (var (roleId, playerName) in upRequestedRoles)
        {
            var player = players.FirstOrDefault(p => p.Data.PlayerName == playerName);
            if (player != null && roles.Contains(roleId))
            {
                AssignRoleToPlayer(player, roleId, true);
                UpCommandRequests.RemoveRequest(playerName);
                players.Remove(player);
                roles.Remove(roleId);
            }
        }

        // Assign remaining roles randomly to remaining players
        // Shuffle again before random assignment for better distribution
        roles.Shuffle();
        players.Shuffle();

        foreach (var role in roles)
        {
            if (players.Count == 0)
            {
                break;
            }

            // Use random selection from shuffled list for fairness
            var randomIndex = Random.RandomRangeInt(0, players.Count);
            var player = players[randomIndex];

            AssignRoleToPlayer(player, role, false);
            players.RemoveAt(randomIndex);
        }
    }

    /// <summary>
    /// Assigns a role to a player and logs the assignment.
    /// </summary>
    private static void AssignRoleToPlayer(PlayerControl player, ushort roleId, bool viaUpCommand)
    {
        player.RpcSetRole((RoleTypes)roleId);
        var roleName = RoleManager.Instance.GetRole((RoleTypes)roleId).GetRoleName();
        var source = viaUpCommand ? " (via /up)" : string.Empty;
        var roleText = $"SelectRoles - player: '{player.Data.PlayerName}', role: '{roleName}'{source}";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Warning, roleText);
    }

    /// <summary>
    /// Assigns vanilla roles (Crewmate/Impostor) to players who didn't receive a special role.
    /// </summary>
    private static void AssignVanillaRoles(List<PlayerControl> crewmates, List<PlayerControl> impostors)
    {
        foreach (var player in crewmates)
        {
            player.RpcSetRole(RoleTypes.Crewmate);
        }

        foreach (var player in impostors)
        {
            player.RpcSetRole(RoleTypes.Impostor);
        }
    }

    private static void AssignRoles(List<NetworkedPlayerInfo> infected)
    {
        var impCount = infected.Count;
        var impostors = MiscUtils.GetImpostors(infected);
        var crewmates = MiscUtils.GetCrewmates(impostors);

        // Calculate neutral role counts
        var roleOptions = OptionGroupSingleton<RoleOptions>.Instance;
        var nbCount = Random.RandomRange((int)roleOptions.MinNeutralBenign.Value,
            (int)roleOptions.MaxNeutralBenign.Value + 1);
        var neCount = Random.RandomRange((int)roleOptions.MinNeutralEvil.Value,
            (int)roleOptions.MaxNeutralEvil.Value + 1);
        var nkCount = Random.RandomRange((int)roleOptions.MinNeutralKiller.Value,
            (int)roleOptions.MaxNeutralKiller.Value + 1);
        var noCount = Random.RandomRange((int)roleOptions.MinNeutralOutlier.Value,
            (int)roleOptions.MaxNeutralOutlier.Value + 1);

        // Adjust neutral counts to ensure crewmates outnumber neutrals
        AdjustNeutralCounts(ref nbCount, ref neCount, ref nkCount, ref noCount, crewmates.Count);

        var excluded = MiscUtils.SpawnableRoles.Where(x => x is ISpawnChange { NoSpawn: true }).Select(x => x.Role);

        var impRoles =
            MiscUtils.GetMaxRolesToAssign(ModdedRoleTeams.Impostor, impCount, x => !excluded.Contains(x.Role));

        // Handle unique role constraint
        var uniqueRole = MiscUtils.SpawnableRoles.FirstOrDefault(x => x is ISpawnChange { NoSpawn: false });
        if (uniqueRole != null && impRoles.Contains(RoleId.Get(uniqueRole.GetType())))
        {
            impCount = 1;
            var impText = $"Removing Impostor Roles because of {uniqueRole.GetRoleName()}";
            MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Warning, impText);

            impRoles.RemoveAll(x => x != RoleId.Get(uniqueRole.GetType()));

            while (impostors.Count > impCount)
            {
                crewmates.Add(impostors.TakeFirst());
            }
        }

        // Get neutral and crewmate roles
        var nbRoles = MiscUtils.GetMaxRolesToAssign(RoleAlignment.NeutralBenign, nbCount);
        var neRoles = MiscUtils.GetMaxRolesToAssign(RoleAlignment.NeutralEvil, neCount);
        var nkRoles = MiscUtils.GetMaxRolesToAssign(RoleAlignment.NeutralKilling, nkCount);
        var noRoles = MiscUtils.GetMaxRolesToAssign(RoleAlignment.NeutralOutlier, noCount);

        var crewCount = crewmates.Count - nbRoles.Count - neRoles.Count - nkRoles.Count - noRoles.Count;
        var crewRoles = MiscUtils.GetMaxRolesToAssign(ModdedRoleTeams.Crewmate, crewCount);

        // Combine crewmate and neutral roles
        var crewAndNeutRoles = new List<ushort>();
        crewAndNeutRoles.AddRange(nbRoles);
        crewAndNeutRoles.AddRange(neRoles);
        crewAndNeutRoles.AddRange(nkRoles);
        crewAndNeutRoles.AddRange(noRoles);
        crewAndNeutRoles.AddRange(crewRoles);

        // Add /up requested roles to pools
        AddUpRequestedRolesToPools(impRoles, crewAndNeutRoles);

        // Assign roles to players
        AssignRolesToPlayers(crewmates, crewAndNeutRoles, "Crewmate/Neutral");
        AssignRolesToPlayers(impostors, impRoles, "Impostor");

        // Assign vanilla roles to remaining players
        AssignVanillaRoles(crewmates, impostors);
    }

    /// <summary>
    /// Builds the role list buckets from slot options.
    /// </summary>
    private static List<RoleListOption> BuildRoleListBuckets(int playerCount)
    {
        var opts = OptionGroupSingleton<RoleOptions>.Instance;
        var buckets = new List<RoleListOption>();
        var slotValues = new[]
        {
            opts.Slot1, opts.Slot2, opts.Slot3, opts.Slot4, opts.Slot5,
            opts.Slot6, opts.Slot7, opts.Slot8, opts.Slot9, opts.Slot10,
            opts.Slot11, opts.Slot12, opts.Slot13, opts.Slot14, opts.Slot15
        };

        // Add slots up to player count (max 15)
        var slotsToAdd = Math.Min(playerCount, 15);
        for (var i = 0; i < slotsToAdd; i++)
        {
            buckets.Add((RoleListOption)slotValues[i].Value);
        }

        // For players beyond 15, add random crew/non-imp roles
        if (playerCount > 15)
        {
            for (var i = 0; i < playerCount - 15; i++)
            {
                // Use better random distribution: 25% chance for CrewRandom, 75% for NonImp
                var random = Random.RandomRangeInt(0, 4);
                buckets.Add(random == 0 ? RoleListOption.CrewRandom : RoleListOption.NonImp);
            }
        }

        return buckets;
    }

    /// <summary>
    /// Adjusts role list buckets to match the required impostor count.
    /// </summary>
    private static void AdjustRoleListBucketsForImpostors(List<RoleListOption> buckets, List<RoleListOption> impBuckets, int requiredImpostors)
    {
        // Count current impostor slots
        var impCount = buckets.Count(bucket => impBuckets.Contains(bucket));
        var anySlots = buckets.Count(bucket => bucket == RoleListOption.Any);

        // Reduce impostor slots if too many
        while (impCount > requiredImpostors)
        {
            buckets.Shuffle();
            var lastImpIndex = buckets.FindLastIndex(bucket => impBuckets.Contains(bucket));
            if (lastImpIndex >= 0)
            {
                buckets.RemoveAt(lastImpIndex);
                buckets.Add(RoleListOption.NonImp);
                impCount -= 1;
            }
            else
            {
                break;
            }
        }

        // Increase impostor slots if too few
        while (impCount + anySlots < requiredImpostors)
        {
            buckets.Shuffle();
            if (buckets.Count > 0)
            {
                buckets.RemoveAt(0);
                buckets.Add(RoleListOption.ImpRandom);
                impCount += 1;
            }
            else
            {
                break;
            }
        }

        // Replace "Any" slots with appropriate roles
        while (buckets.Contains(RoleListOption.Any))
        {
            buckets.Shuffle();
            var anyIndex = buckets.FindLastIndex(bucket => bucket == RoleListOption.Any);
            if (anyIndex >= 0)
            {
                buckets.RemoveAt(anyIndex);
                if (impCount < requiredImpostors)
                {
                    buckets.Add(RoleListOption.ImpRandom);
                    impCount += 1;
                }
                else
                {
                    buckets.Add(RoleListOption.NonImp);
                }
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// Ensures role list buckets have at least one crew/neutral role.
    /// </summary>
    private static void EnsureCrewNeutralRoles(List<RoleListOption> buckets, List<RoleListOption> impBuckets, List<RoleListOption> crewNkBuckets)
    {
        var hasCrewNeutral = buckets.Any(bucket => crewNkBuckets.Contains(bucket));
        var hasNeutRandom = buckets.Contains(RoleListOption.NeutRandom);
        var hasNonImp = buckets.Contains(RoleListOption.NonImp);

        if (!hasCrewNeutral)
        {
            var replacementOptions = new List<RoleListOption> { RoleListOption.CrewRandom, RoleListOption.NeutKilling };
            replacementOptions.Shuffle();

            if (hasNeutRandom)
            {
                buckets.Remove(RoleListOption.NeutRandom);
                buckets.Add(RoleListOption.NeutKilling);
            }
            else if (hasNonImp)
            {
                buckets.Remove(RoleListOption.NonImp);
                buckets.Add(replacementOptions[0]);
            }
            else
            {
                // Remove a random non-impostor bucket and replace with crew/neutral
                buckets.Shuffle();
                var nonImpIndex = buckets.FindLastIndex(bucket => !impBuckets.Contains(bucket));
                if (nonImpIndex >= 0)
                {
                    buckets.RemoveAt(nonImpIndex);
                    buckets.Add(replacementOptions[0]);
                }
            }
        }
    }

    private static void AssignRolesFromRoleList(List<NetworkedPlayerInfo> infected)
    {
        var impostors = MiscUtils.GetImpostors(infected);
        var crewmates = MiscUtils.GetCrewmates(impostors);

        var crewRoles = new List<ushort>();
        var impRoles = new List<ushort>();

        var players = impostors.Count + crewmates.Count;

        // Define bucket categories
        List<RoleListOption> crewNkBuckets =
        [
            RoleListOption.CrewInvest, RoleListOption.CrewKilling, RoleListOption.CrewPower,
            RoleListOption.CrewProtective, RoleListOption.CrewSupport, RoleListOption.CrewCommon,
            RoleListOption.CrewSpecial, RoleListOption.CrewRandom, RoleListOption.NeutKilling
        ];
        List<RoleListOption> impBuckets =
        [
            RoleListOption.ImpConceal, RoleListOption.ImpKilling, RoleListOption.ImpPower,
            RoleListOption.ImpSupport, RoleListOption.ImpCommon, RoleListOption.ImpSpecial,
            RoleListOption.ImpRandom
        ];

        // Build and adjust role list buckets
        var buckets = BuildRoleListBuckets(players);
        AdjustRoleListBucketsForImpostors(buckets, impBuckets, impostors.Count);
        EnsureCrewNeutralRoles(buckets, impBuckets, crewNkBuckets);

        var impCount = buckets.Count(bucket => impBuckets.Contains(bucket));


        // Get all role lists with exclusion filter
        var excluded = MiscUtils.SpawnableRoles.Where(x => x is ISpawnChange { NoSpawn: true }).Select(x => x.Role).ToList();
        var exclusionFilter = new Func<RoleBehaviour, bool>(x => !excluded.Contains(x.Role));

        var crewInvestRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmateInvestigative, exclusionFilter);
        var crewKillingRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmateKilling, exclusionFilter);
        var crewProtectRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmateProtective, exclusionFilter);
        var crewPowerRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmatePower, exclusionFilter);
        var crewSupportRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmateSupport, exclusionFilter);
        var neutBenignRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralBenign, exclusionFilter);
        var neutEvilRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralEvil, exclusionFilter);
        var neutKillingRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralKilling, exclusionFilter);
        var neutOutlierRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralOutlier, exclusionFilter);
        var impConcealRoles = MiscUtils.GetRolesToAssign(RoleAlignment.ImpostorConcealing, exclusionFilter);
        var impKillingRoles = MiscUtils.GetRolesToAssign(RoleAlignment.ImpostorKilling, exclusionFilter);
        var impPowerRoles = MiscUtils.GetRolesToAssign(RoleAlignment.ImpostorPower, exclusionFilter);
        var impSupportRoles = MiscUtils.GetRolesToAssign(RoleAlignment.ImpostorSupport);

        // imp buckets
        impRoles.AddRange(MiscUtils.ReadFromBucket(buckets, impConcealRoles, RoleListOption.ImpConceal,
            RoleListOption.ImpCommon));

        var commonImpRoles = impConcealRoles;

        impRoles.AddRange(MiscUtils.ReadFromBucket(buckets, impSupportRoles, RoleListOption.ImpSupport,
            RoleListOption.ImpCommon));

        commonImpRoles.AddRange(impSupportRoles);

        impRoles.AddRange(MiscUtils.ReadFromBucket(buckets, impKillingRoles, RoleListOption.ImpKilling,
            RoleListOption.ImpSpecial));

        var specialImpRoles = impKillingRoles;

        impRoles.AddRange(MiscUtils.ReadFromBucket(buckets, impPowerRoles, RoleListOption.ImpPower,
            RoleListOption.ImpSpecial));

        specialImpRoles.AddRange(impPowerRoles);

        impRoles.AddRange(MiscUtils.ReadFromBucket(buckets, commonImpRoles, RoleListOption.ImpCommon,
            RoleListOption.ImpRandom));

        var randomImpRoles = commonImpRoles;

        impRoles.AddRange(MiscUtils.ReadFromBucket(buckets, specialImpRoles, RoleListOption.ImpSpecial,
            RoleListOption.ImpRandom));

        randomImpRoles.AddRange(specialImpRoles);

        impRoles.AddRange(MiscUtils.ReadFromBucket(buckets, randomImpRoles, RoleListOption.ImpRandom));

        // crew buckets
        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, crewInvestRoles, RoleListOption.CrewInvest,
            RoleListOption.CrewCommon));

        var commonCrewRoles = crewInvestRoles;

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, crewProtectRoles, RoleListOption.CrewProtective,
            RoleListOption.CrewCommon));

        commonCrewRoles.AddRange(crewProtectRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, crewSupportRoles, RoleListOption.CrewSupport,
            RoleListOption.CrewCommon));

        commonCrewRoles.AddRange(crewSupportRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, crewKillingRoles, RoleListOption.CrewKilling,
            RoleListOption.CrewSpecial));

        var specialCrewRoles = crewKillingRoles;

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, crewPowerRoles, RoleListOption.CrewPower,
            RoleListOption.CrewSpecial));

        specialCrewRoles.AddRange(crewPowerRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, commonCrewRoles, RoleListOption.CrewCommon,
            RoleListOption.CrewRandom));

        var randomCrewRoles = commonCrewRoles;

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, specialCrewRoles, RoleListOption.CrewSpecial,
            RoleListOption.CrewRandom));

        randomCrewRoles.AddRange(specialCrewRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, randomCrewRoles, RoleListOption.CrewRandom));

        var randomNonImpRoles = randomCrewRoles;

        // neutral buckets
        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, neutBenignRoles, RoleListOption.NeutBenign,
            RoleListOption.NeutCommon, RoleListOption.NeutWildcard));

        var commonNeutRoles = neutBenignRoles;

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, neutEvilRoles, RoleListOption.NeutEvil,
            RoleListOption.NeutCommon, RoleListOption.NeutWildcard));

        commonNeutRoles.AddRange(neutEvilRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, neutOutlierRoles, RoleListOption.NeutOutlier,
            RoleListOption.NeutSpecial, RoleListOption.NeutWildcard));

        var specialNeutRoles = neutOutlierRoles;

        var wildNeutRoles = new List<(ushort RoleType, int Chance)>();

        wildNeutRoles.AddRange(neutOutlierRoles);

        wildNeutRoles.AddRange(commonNeutRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, neutKillingRoles, RoleListOption.NeutKilling,
            RoleListOption.NeutSpecial));

        specialNeutRoles.AddRange(neutKillingRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, commonNeutRoles, RoleListOption.NeutCommon,
            RoleListOption.NeutRandom));

        var randomNeutRoles = commonNeutRoles;

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, specialNeutRoles, RoleListOption.NeutSpecial,
            RoleListOption.NeutRandom));

        randomNeutRoles.AddRange(specialNeutRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, wildNeutRoles, RoleListOption.NeutWildcard,
            RoleListOption.NeutRandom));

        randomNeutRoles.AddRange(wildNeutRoles);

        randomNeutRoles.AddRange(commonNeutRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, randomNeutRoles, RoleListOption.NeutRandom,
            RoleListOption.NonImp));

        randomNonImpRoles.AddRange(randomNeutRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, randomNonImpRoles, RoleListOption.NonImp));

        // Add /up requested roles to pools
        AddUpRequestedRolesToPools(impRoles, crewRoles);

        // Shuffle roles before handing them out for better randomness and fairness
        crewRoles.Shuffle();
        impRoles.Shuffle();

        // Select impostor roles (take up to impCount)
        var chosenImpRoles = impRoles.Take(impCount).ToList();

        // Ensure /up requested impostor roles are included
        foreach (var impostor in impostors.ToList())
        {
            if (UpCommandRequests.TryGetRequestRole(impostor.Data.PlayerName, out var requestedRole))
            {
                var requestedRoleId = (ushort)requestedRole.Role;
                if (requestedRole.IsImpostor() && !chosenImpRoles.Contains(requestedRoleId))
                {
                    // Add the requested role, removing a random one if at capacity
                    if (chosenImpRoles.Count >= impCount && chosenImpRoles.Count > 0)
                    {
                        var randomIndex = Random.RandomRangeInt(0, chosenImpRoles.Count);
                        chosenImpRoles.RemoveAt(randomIndex);
                    }
                    chosenImpRoles.Add(requestedRoleId);
                }
            }
        }

        chosenImpRoles = chosenImpRoles.Pad(impCount, (ushort)RoleTypes.Impostor);

        // Handle unique role constraint
        var uniqueRole = MiscUtils.SpawnableRoles.FirstOrDefault(x => x is ISpawnChange { NoSpawn: false });
        if (uniqueRole != null && chosenImpRoles.Contains(RoleId.Get(uniqueRole.GetType())))
        {
            impCount = 1;
            var impText = $"Removing Impostor Roles because of {uniqueRole.GetRoleName()}";
            MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Warning, impText);

            while (impostors.Count > impCount)
            {
                crewmates.Add(impostors.TakeFirst());
            }

            chosenImpRoles.RemoveAll(x => x != RoleId.Get(uniqueRole.GetType()));

            // Re-add /up requested role if it was removed
            foreach (var impostor in impostors.ToList())
            {
                if (UpCommandRequests.TryGetRequestRole(impostor.Data.PlayerName, out var requestedRole))
                {
                    var requestedRoleId = (ushort)requestedRole.Role;
                    if (requestedRole.IsImpostor() && !chosenImpRoles.Contains(requestedRoleId))
                    {
                        chosenImpRoles.Add(requestedRoleId);
                    }
                }
            }
        }

        // Assign roles to players
        AssignRolesToPlayers(impostors, chosenImpRoles, "Impostor");
        AssignRolesToPlayers(crewmates, crewRoles, "Crewmate/Neutral");

        // Assign vanilla roles to remaining players
        AssignVanillaRoles(crewmates, impostors);
    }

    public static void AssignTargets()
    {
        // This is a coroutine because otherwise, the game just assigns targets real badly like traitor egotist, exe being lovers with their targets, that sort of thing - Atony
        Coroutines.Start(CoAssignTargets());
    }

    public static IEnumerator CoAssignTargets()
    {
        foreach (var role in MiscUtils.SpawnableRoles.Where(x => x is IAssignableTargets)
                     .OrderBy(x => (x as IAssignableTargets)!.Priority))
        {
            if (role is IAssignableTargets assignRole)
            {
                assignRole.AssignTargets();
                yield return new WaitForSeconds(0.01f);
            }
        }

        foreach (var modifier in MiscUtils.AllModifiers.Where(x => x is IAssignableTargets)
                     .OrderBy(x => (x as IAssignableTargets)!.Priority))
        {
            if (modifier is IAssignableTargets assignMod)
            {
                assignMod.AssignTargets();
                yield return new WaitForSeconds(0.01f);
            }
        }

        GhostRoleSetup();
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    public static bool SelectRolesPatch(RoleManager __instance)
    {
        var assignmentType = (RoleSelectionMode)OptionGroupSingleton<RoleOptions>.Instance.RoleAssignmentType.Value;
        Error($"RoleManager.SelectRoles - ReplaceRoleManager: {ReplaceRoleManager} | Assignment type is set to {assignmentType.ToDisplayString()}!");
        GameManager.Instance.LogicOptions.SyncOptions();

        if (TutorialManager.InstanceExists || ReplaceRoleManager || GameManager.Instance.IsHideAndSeek() || assignmentType is RoleSelectionMode.Vanilla)
        {
            return true;
        }

        var random = new System.Random();

        var players = GameData.Instance.AllPlayers.ToArray()
            .Excluding(x => SpectatorRole.TrackedSpectators.Contains(x.PlayerName)).ToList();
        players.Shuffle();

        // TODO: PATCH THE GETTER FOR THIS!!!
        var impCount = GameOptionsManager.Instance.CurrentGameOptions.GetAdjustedNumImpostors(players.Count);
        List<NetworkedPlayerInfo> infected = [];

        var useBias = OptionGroupSingleton<RoleOptions>.Instance.LastImpostorBias;

        if (useBias && LastImps.Count > 0)
        {
            var biasPercent = OptionGroupSingleton<RoleOptions>.Instance.ImpostorBiasPercent.Value / 100f;
            while (infected.Count < impCount)
            {
                if (players.All(x => LastImps.Contains(x.ClientId)))
                {
                    var remainingImps = impCount - infected.Count;
                    players.Shuffle();
                    infected.AddRange(players.Where(x => !infected.Contains(x)).Take(remainingImps));
                    break;
                }

                var num = random.Next(players.Count);
                var player = players[num];
                var skip = LastImps.Contains(player.ClientId) && random.NextDouble() < biasPercent;

                if (infected.Contains(player) || skip)
                {
                    continue;
                }

                infected.Add(player);
            }
        }
        else
        {
            infected.AddRange(players.Take(impCount));
        }

        LastImps = [.. infected.Select(x => x.ClientId)];

        // Handle /up requests before role assignment
        // This ensures players are in the correct list (infected/crewmates) based on their requested role
        var upRequests = UpCommandRequests.GetAllRequests();
        foreach (var (playerName, roleName) in upRequests)
        {
            var playerInfo = players.FirstOrDefault(p => p.PlayerName == playerName);
            if (playerInfo == null)
            {
                continue;
            }

            if (!UpCommandRequests.TryGetRequestRole(playerName, out var requestedRole))
            {
                continue;
            }

            // Check if the role is Impostor-aligned using IsImpostor() method
            var isImpostorAligned = requestedRole.IsImpostor();
            var isNeutral = requestedRole.IsNeutral();

            if (isImpostorAligned)
            {
                // Force player into infected list (swap, don't add)
                if (!infected.Contains(playerInfo))
                {
                    // Remove from infected if at capacity, otherwise just add
                    if (infected.Count >= impCount && infected.Count > 0)
                    {
                        // Swap: remove a random impostor and add this player
                        var randomIndex = random.Next(infected.Count);
                        infected.RemoveAt(randomIndex);
                    }
                    infected.Add(playerInfo);
                }
            }
            else
            {
                // Force player out of infected (non-impostor roles)
                if (infected.Contains(playerInfo))
                {
                    infected.Remove(playerInfo);
                    // If we removed someone, we need to add someone else to maintain imp count
                    if (infected.Count < impCount)
                    {
                        var availablePlayers = players.Where(p => !infected.Contains(p) && p != playerInfo).ToList();
                        if (availablePlayers.Count > 0)
                        {
                            availablePlayers.Shuffle();
                            infected.Add(availablePlayers[0]);
                        }
                    }
                }
            }
        }

        if (assignmentType is RoleSelectionMode.RoleList)
        {
            AssignRolesFromRoleList(infected);
        }
        else
        {
            AssignRoles(infected);
        }

        return false;
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.First)]
    public static void AssignCustomModifiersPatch(RoleManager __instance)
    {
        var spectators = GameData.Instance.AllPlayers.ToArray()
            .Where(x => SpectatorRole.TrackedSpectators.Contains(x.PlayerName)).ToList();
        var specId = (RoleTypes)RoleId.Get<SpectatorRole>();

        foreach (var player in spectators)
        {
            player.Object.RpcSetRole(RoleTypes.Crewmate);
        }

        foreach (var player in spectators)
        {
            player.Object.RpcSetRole(specId);
        }

        if (OptionGroupSingleton<GeneralOptions>.Instance.RoundOneVictims)
        {
            var firstDead = GameData.Instance.AllPlayers.ToArray()
                .Where(x => FirstDeadPatch.FirstRoundPlayerNames.Contains(x.PlayerName) && !spectators.Contains(x)).ToList();

            foreach (var player in firstDead)
            {
                player.Object.RpcAddModifier<FirstRoundIndicator>();
            }
        }

        AssignTargets();
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
    [HarmonyPrefix]
    public static bool RpcSetRolePatch(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleType,
        [HarmonyArgument(1)] bool canOverrideRole = false)
    {
        if (AmongUsClient.Instance.AmClient)
        {
            __instance.StartCoroutine(__instance.CoSetRole(roleType, canOverrideRole));
        }

        var messageWriter =
            AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable);
        messageWriter.Write((ushort)roleType);
        messageWriter.Write(canOverrideRole);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);

        var changeRoleEvent = new ChangeRoleEvent(__instance, null, RoleManager.Instance.GetRole(roleType));
        MiraEventManager.InvokeEvent(changeRoleEvent);

        return false;
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
    [HarmonyPrefix]
    public static bool AssignRoleOnDeathPatch(RoleManager __instance, PlayerControl player, bool specialRolesAllowed)
    {
        // Note: I know this is a one-to-one recreation of the AssignRoleOnDeath function, but for some reason,
        // the original won't spawn the Spectre and just spawns Neutral Ghost instead

        var text = $"AssignRoleOnDeathPatch - Player: '{player.Data.PlayerName}', specialRolesAllowed: {specialRolesAllowed}";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Warning, text);

        if (player == null || !player.Data.IsDead || !player.CanGetGhostRole())
        // Message($"AssignRoleOnDeathPatch - !player.Data.IsDead: '{!player.Data.IsDead}'");
        {
            return false;
        }

        if ( /*!player.Data.Role.IsImpostor && */specialRolesAllowed)
        // Message($"AssignRoleOnDeathPatch - !player.Data.Role.IsImpostor: '{!player.Data.Role.IsImpostor}' specialRolesAllowed: {specialRolesAllowed}");
        {
            RoleManager.TryAssignSpecialGhostRoles(player, player.IsImpostor());
        }

        if (!RoleManager.IsGhostRole(player.Data.Role.Role))
        // Message($"AssignRoleOnDeathPatch - !RoleManager.IsGhostRole(player.Data.Role.Role): '{!RoleManager.IsGhostRole(player.Data.Role.Role)}'");
        {
            player.RpcSetRole(player.Data.Role.DefaultGhostRole);
        }

        return false;
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.TryAssignSpecialGhostRoles))]
    [HarmonyPrefix]
    public static bool TryAssignSpecialGhostRolesPatch(RoleManager __instance, PlayerControl player)
    {
        var text = $"TryAssignSpecialGhostRolesPatch - Player: '{player.Data.PlayerName}'";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Warning, text);

        var ghostRole = RoleTypes.CrewmateGhost;

        if (player.IsCrewmate() && CrewmateGhostRolePool.Count > 0)
        {
            ghostRole = CrewmateGhostRolePool.TakeFirst();
        }
        else if (player.IsImpostor() && ImpostorGhostRolePool.Count > 0)
        {
            ghostRole = ImpostorGhostRolePool.TakeFirst();
        }
        else if (player.IsNeutral() && CustomGhostRolePool.Count > 0)
        {
            ghostRole = CustomGhostRolePool.TakeFirst();
        }

        if (ghostRole != RoleTypes.CrewmateGhost && ghostRole != RoleTypes.ImpostorGhost &&
            ghostRole != (RoleTypes)RoleId.Get<NeutralGhostRole>())
        // var newRole = RoleManager.Instance.GetRole(ghostRole);
        // Message($"TryAssignSpecialGhostRolesPatch - ghostRoles role: {newRole.GetRoleName()}");
        {
            player.RpcChangeRole((ushort)ghostRole);
        }

        return false;
    }

    //[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SetRole))]
    //[HarmonyPostfix]
    //public static void SetRolePatch(RoleManager __instance, [HarmonyArgument(0)] PlayerControl targetPlayer, [HarmonyArgument(1)] RoleTypes roleType)
    //{
    //    GameHistory.RegisterRole(targetPlayer, targetPlayer.Data.Role);
    //}
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    [HarmonyPrefix]
    public static bool GetAdjustedImposters(IGameOptions __instance, ref int __result)
    {
        if (MiscUtils.CurrentGamemode() is not TouGamemode.Normal)
        {
            return true;
        }

        var assignmentType = (RoleSelectionMode)OptionGroupSingleton<RoleOptions>.Instance.RoleAssignmentType.Value;

        if (assignmentType is not RoleSelectionMode.RoleList)
        {
            return true;
        }

        var players = GameData.Instance.PlayerCount - SpectatorRole.TrackedSpectators.Count;
        var impostors = 0;
        var list = OptionGroupSingleton<RoleOptions>.Instance;
        var maxSlots = players < 15 ? players : 15;
        List<RoleListOption> impBuckets =
        [
            RoleListOption.ImpConceal, RoleListOption.ImpKilling, RoleListOption.ImpPower, RoleListOption.ImpSupport,
            RoleListOption.ImpCommon, RoleListOption.ImpSpecial, RoleListOption.ImpRandom
        ];
        List<RoleListOption> buckets = [];
        var anySlots = 0;

        for (var i = 0; i < maxSlots; i++)
        {
            var slotValue = i switch
            {
                0 => list.Slot1,
                1 => list.Slot2,
                2 => list.Slot3,
                3 => list.Slot4,
                4 => list.Slot5,
                5 => list.Slot6,
                6 => list.Slot7,
                7 => list.Slot8,
                8 => list.Slot9,
                9 => list.Slot10,
                10 => list.Slot11,
                11 => list.Slot12,
                12 => list.Slot13,
                13 => list.Slot14,
                14 => list.Slot15,
                _ => -1
            };
            buckets.Add((RoleListOption)slotValue);
        }


        foreach (var roleOption in buckets)
        {
            if (impBuckets.Contains(roleOption))
            {
                impostors += 1;
            }
            else if (roleOption == RoleListOption.Any)
            {
                anySlots += 1;
            }
        }

        int impProbability = (int)Math.Floor((double)players / anySlots * 5 / 3);
        for (int i = 0; i < anySlots; i++)
        {
            var random = Random.RandomRangeInt(0, 100);
            if (random < impProbability)
            {
                impostors += 1;
            }

            impProbability += 3;
        }

        if (players < 7 || impostors == 0)
        {
            impostors = 1;
        }
        else if (players < 10 && impostors > 2)
        {
            impostors = 2;
        }
        else if (players < 14 && impostors > 3)
        {
            impostors = 3;
        }
        else if (players < 19 && impostors > 4)
        {
            impostors = 4;
        }
        else if (impostors > 5)
        {
            impostors = 5;
        }

        __result = impostors;
        return false;
    }
}