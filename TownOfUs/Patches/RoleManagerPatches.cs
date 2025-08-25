using System.Collections;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
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
        // var ghostRoles = RoleManager.Instance.AllRoles.Where(x => x.IsDead);
        var ghostRoles = MiscUtils.GetRegisteredGhostRoles();

        if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Warning($"GhostRoleSetup - ghostRoles Count: {ghostRoles.Count()}");
        CrewmateGhostRolePool.Clear();
        ImpostorGhostRolePool.Clear();
        CustomGhostRolePool.Clear();

        foreach (var role in ghostRoles)
        {
            if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Warning($"GhostRoleSetup - ghostRoles role NiceName: {role.NiceName}");
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
        CustomGhostRolePool.RemoveAll(x => x == (RoleTypes)RoleId.Get<PhantomTouRole>());
    }

    private static void AssignRoles(List<NetworkedPlayerInfo> infected)
    {
        var impCount = infected.Count;
        var impostors = MiscUtils.GetImpostors(infected);
        var crewmates = MiscUtils.GetCrewmates(impostors);

        var nbCount = Random.RandomRange((int)OptionGroupSingleton<RoleOptions>.Instance.MinNeutralBenign.Value,
            (int)OptionGroupSingleton<RoleOptions>.Instance.MaxNeutralBenign.Value + 1);
        var neCount = Random.RandomRange((int)OptionGroupSingleton<RoleOptions>.Instance.MinNeutralEvil.Value,
            (int)OptionGroupSingleton<RoleOptions>.Instance.MaxNeutralEvil.Value + 1);
        var nkCount = Random.RandomRange((int)OptionGroupSingleton<RoleOptions>.Instance.MinNeutralKiller.Value,
            (int)OptionGroupSingleton<RoleOptions>.Instance.MaxNeutralKiller.Value + 1);

        var factions = new List<string> { "Benign", "Evil", "Killing" };

        // Crew must always start out outnumbering neutrals, so subtract roles until that can be guaranteed.
        while (Math.Ceiling((double)crewmates.Count / 2) <= nbCount + neCount + nkCount)
        {
            var canSubtractBenign = CanSubtract(nbCount,
                (int)OptionGroupSingleton<RoleOptions>.Instance.MinNeutralBenign.Value);
            var canSubtractEvil =
                CanSubtract(neCount, (int)OptionGroupSingleton<RoleOptions>.Instance.MinNeutralEvil.Value);
            var canSubtractKilling = CanSubtract(nkCount,
                (int)OptionGroupSingleton<RoleOptions>.Instance.MinNeutralKiller.Value);
            var canSubtractNone = !canSubtractBenign && !canSubtractEvil && !canSubtractKilling;

            factions.Shuffle();
            switch (factions[0])
            {
                case "Benign":
                    if (nbCount > 0 && (canSubtractBenign || canSubtractNone))
                    {
                        nbCount -= 1;
                        break;
                    }

                    goto case "Evil";
                case "Evil":
                    if (neCount > 0 && (canSubtractEvil || canSubtractNone))
                    {
                        neCount -= 1;
                        break;
                    }

                    goto case "Killing";
                case "Killing":
                    if (nkCount > 0 && (canSubtractKilling || canSubtractNone))
                    {
                        nkCount -= 1;
                        break;
                    }

                    goto default;
                default:
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

                    break;
            }

            if (nbCount + neCount + nkCount == 0)
            {
                break;
            }
        }

        var excluded = MiscUtils.AllRoles.Where(x => x is ISpawnChange { NoSpawn: true }).Select(x => x.Role);

        var impRoles =
            MiscUtils.GetMaxRolesToAssign(ModdedRoleTeams.Impostor, impCount, x => !excluded.Contains(x.Role));

        var uniqueRole = MiscUtils.AllRoles.FirstOrDefault(x => x is ISpawnChange { NoSpawn: false });
        if (uniqueRole != null && impRoles.Contains(RoleId.Get(uniqueRole.GetType())))
        {
            impCount = 1;
            
            if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Warning($"Removing Impostor Roles because of {uniqueRole.NiceName}");
            
            impRoles.RemoveAll(x => x != RoleId.Get(uniqueRole.GetType()));

            while (impostors.Count > impCount)
            {
                crewmates.Add(impostors.TakeFirst());
            }
        }

        var nbRoles = MiscUtils.GetMaxRolesToAssign(RoleAlignment.NeutralBenign, nbCount);
        var neRoles = MiscUtils.GetMaxRolesToAssign(RoleAlignment.NeutralEvil, neCount);
        var nkRoles = MiscUtils.GetMaxRolesToAssign(RoleAlignment.NeutralKilling, nkCount);

        var crewCount = crewmates.Count - nbRoles.Count - neRoles.Count - nkRoles.Count;

        Func<RoleBehaviour, bool>? crewFilter = null;

        if ((MapNames)GameOptionsManager.Instance.GameHostOptions.MapId == MapNames.Fungle)
        {
            crewFilter = x => x.Role != (RoleTypes)RoleId.Get<SpyRole>();
        }

        var crewRoles = MiscUtils.GetMaxRolesToAssign(ModdedRoleTeams.Crewmate, crewCount, crewFilter);

        var crewAndNeutRoles = new List<ushort>();
        crewAndNeutRoles.AddRange(nbRoles);
        crewAndNeutRoles.AddRange(neRoles);
        crewAndNeutRoles.AddRange(nkRoles);
        crewAndNeutRoles.AddRange(crewRoles);
        crewAndNeutRoles.Shuffle();

        foreach (var role in crewAndNeutRoles)
        {
            var num = HashRandom.FastNext(crewmates.Count);
            var player = crewmates[num];

            player.RpcSetRole((RoleTypes)role);

            crewmates.RemoveAt(num);

            if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Warning($"SelectRoles - player: '{player.Data.PlayerName}', role: '{RoleManager.Instance.GetRole((RoleTypes)role).NiceName}'");
        }

        foreach (var role in impRoles)
        {
            var num = HashRandom.FastNext(impostors.Count);
            var player = impostors[num];

            player.RpcSetRole((RoleTypes)role);

            impostors.RemoveAt(num);

            if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Warning($"SelectRoles - player: '{player.Data.PlayerName}', role: '{RoleManager.Instance.GetRole((RoleTypes)role).NiceName}'");
        }

        foreach (var player in crewmates)
        {
            player.RpcSetRole(RoleTypes.Crewmate);
        }

        foreach (var player in impostors)
        {
            player.RpcSetRole(RoleTypes.Impostor);
        }

        static bool CanSubtract(int faction, int minFaction)
        {
            return faction > minFaction;
        }
    }

    private static void AssignRolesFromRoleList(List<NetworkedPlayerInfo> infected)
    {
        var impostors = MiscUtils.GetImpostors(infected);
        var crewmates = MiscUtils.GetCrewmates(impostors);

        var crewRoles = new List<ushort>();
        var impRoles = new List<ushort>();

        var opts = OptionGroupSingleton<RoleOptions>.Instance;

        // sort out bad lists
        var players = impostors.Count + crewmates.Count;
        List<RoleListOption> crewNkBuckets =
        [
            RoleListOption.CrewInvest, RoleListOption.CrewKilling, RoleListOption.CrewPower,
            RoleListOption.CrewProtective,
            RoleListOption.CrewSupport, RoleListOption.CrewCommon, RoleListOption.CrewSpecial,
            RoleListOption.CrewRandom, RoleListOption.NeutKilling
        ];
        List<RoleListOption> impBuckets =
        [
            RoleListOption.ImpConceal, RoleListOption.ImpKilling, RoleListOption.ImpPower, RoleListOption.ImpSupport,
            RoleListOption.ImpCommon, RoleListOption.ImpSpecial, RoleListOption.ImpRandom
        ];
        List<RoleListOption> buckets =
        [
            (RoleListOption)opts.Slot1.Value, (RoleListOption)opts.Slot2.Value, (RoleListOption)opts.Slot3.Value,
            (RoleListOption)opts.Slot4.Value
        ];
        var impCount = 0;
        var anySlots = 0;

        if (players > 4)
        {
            buckets.Add((RoleListOption)opts.Slot5.Value);
        }

        if (players > 5)
        {
            buckets.Add((RoleListOption)opts.Slot6.Value);
        }

        if (players > 6)
        {
            buckets.Add((RoleListOption)opts.Slot7.Value);
        }

        if (players > 7)
        {
            buckets.Add((RoleListOption)opts.Slot8.Value);
        }

        if (players > 8)
        {
            buckets.Add((RoleListOption)opts.Slot9.Value);
        }

        if (players > 9)
        {
            buckets.Add((RoleListOption)opts.Slot10.Value);
        }

        if (players > 10)
        {
            buckets.Add((RoleListOption)opts.Slot11.Value);
        }

        if (players > 11)
        {
            buckets.Add((RoleListOption)opts.Slot12.Value);
        }

        if (players > 12)
        {
            buckets.Add((RoleListOption)opts.Slot13.Value);
        }

        if (players > 13)
        {
            buckets.Add((RoleListOption)opts.Slot14.Value);
        }

        if (players > 14)
        {
            buckets.Add((RoleListOption)opts.Slot15.Value);
        }

        if (players > 15)
        {
            for (var i = 0; i < players - 15; i++)
            {
                var random = Random.RandomRangeInt(0, 4);
                buckets.Add(random == 0 ? RoleListOption.CrewRandom : RoleListOption.NonImp);
            }
        }

        // imp issues
        foreach (var roleOption in buckets)
        {
            if (impBuckets.Contains(roleOption))
            {
                impCount += 1;
            }
            else if (roleOption == RoleListOption.Any)
            {
                anySlots += 1;
            }
        }

        while (impCount > impostors.Count)
        {
            buckets.Shuffle();
            buckets.Remove(buckets.FindLast(x => impBuckets.Contains(x)));
            buckets.Add(RoleListOption.NonImp);
            impCount -= 1;
        }

        while (impCount + anySlots < impostors.Count)
        {
            buckets.Shuffle();
            buckets.RemoveAt(0);
            buckets.Add(RoleListOption.ImpRandom);
            impCount += 1;
        }

        while (buckets.Contains(RoleListOption.Any))
        {
            buckets.Shuffle();
            buckets.Remove(buckets.FindLast(x => x == RoleListOption.Any));
            if (impCount < impostors.Count)
            {
                buckets.Add(RoleListOption.ImpRandom);
                impCount += 1;
            }
            else
            {
                buckets.Add(RoleListOption.NonImp);
            }
        }

        // crew and neut issues
        var noChange = false;
        var nonImp = false;
        var randNeut = false;

        foreach (var roleOption in buckets)
        {
            if (crewNkBuckets.Contains(roleOption))
            {
                noChange = true;
                break;
            }

            if (roleOption == RoleListOption.NeutRandom)
            {
                randNeut = true;
                break;
            }

            if (roleOption == RoleListOption.NonImp)
            {
                nonImp = true;
                break;
            }
        }

        if (!noChange)
        {
            List<RoleListOption> add = [RoleListOption.CrewRandom, RoleListOption.NeutKilling];
            add.Shuffle();

            if (randNeut)
            {
                buckets.Remove(RoleListOption.NeutRandom);
                buckets.Add(RoleListOption.NeutKilling);
            }
            else if (nonImp)
            {
                buckets.Remove(RoleListOption.NonImp);
                buckets.Add(add[0]);
            }
            else
            {
                buckets.Remove(buckets.FindLast(x => !impBuckets.Contains(x)));
                buckets.Add(add[0]);
            }
        }

        Func<RoleBehaviour, bool>? crewFilter = null;
        if ((MapNames)GameOptionsManager.Instance.GameHostOptions.MapId == MapNames.Fungle)
        {
            crewFilter = x => x.Role != (RoleTypes)RoleId.Get<SpyRole>();
        }

        var excluded = MiscUtils.AllRoles.Where(x => x is ISpawnChange { NoSpawn: true }).Select(x => x.Role).ToList();

        var crewInvestRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmateInvestigative, crewFilter);
        var crewKillingRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmateKilling);
        var crewProtectRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmateProtective);
        var crewPowerRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmatePower);
        var crewSupportRoles = MiscUtils.GetRolesToAssign(RoleAlignment.CrewmateSupport);
        var neutBenignRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralBenign);
        var neutEvilRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralEvil);
        var neutKillingRoles = MiscUtils.GetRolesToAssign(RoleAlignment.NeutralKilling);
        var impConcealRoles = MiscUtils.GetRolesToAssign(RoleAlignment.ImpostorConcealing);
        var impKillingRoles =
            MiscUtils.GetRolesToAssign(RoleAlignment.ImpostorKilling, x => !excluded.Contains(x.Role));
        var impPowerRoles =
            MiscUtils.GetRolesToAssign(RoleAlignment.ImpostorPower, x => !excluded.Contains(x.Role));
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
            RoleListOption.NeutCommon));

        var commonNeutRoles = neutBenignRoles;

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, neutEvilRoles, RoleListOption.NeutEvil,
            RoleListOption.NeutCommon));

        commonNeutRoles.AddRange(neutEvilRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, neutKillingRoles, RoleListOption.NeutKilling,
            RoleListOption.NeutRandom));

        var randomNeutRoles = neutKillingRoles;

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, commonNeutRoles, RoleListOption.NeutCommon,
            RoleListOption.NeutRandom));

        randomNeutRoles.AddRange(commonNeutRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, randomNeutRoles, RoleListOption.NeutRandom,
            RoleListOption.NonImp));

        randomNonImpRoles.AddRange(randomNeutRoles);

        crewRoles.AddRange(MiscUtils.ReadFromBucket(buckets, randomNonImpRoles, RoleListOption.NonImp));

        // Shuffle roles before handing them out.
        // This should ensure a statistically equal chance of all permutations of roles.
        crewRoles.Shuffle();
        impRoles.Shuffle();

        var chosenImpRoles = impRoles.Take(impCount).ToList();
        chosenImpRoles = chosenImpRoles.Pad(impCount, (ushort)RoleTypes.Impostor);

        var uniqueRole = MiscUtils.AllRoles.FirstOrDefault(x => x is ISpawnChange { NoSpawn: false });
        if (uniqueRole != null && chosenImpRoles.Contains(RoleId.Get(uniqueRole.GetType())))
        {
            impCount = 1;
            
            if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Warning($"Removing Impostor Roles because of {uniqueRole.NiceName}");

            while (impostors.Count > impCount)
            {
                crewmates.Add(impostors.TakeFirst());
            }

            chosenImpRoles.RemoveAll(x => x != RoleId.Get(uniqueRole.GetType()));
        }

        foreach (var role in chosenImpRoles)
        {
            var num = HashRandom.FastNext(impostors.Count);
            var player = impostors[num];

            player.RpcSetRole((RoleTypes)role);

            impostors.RemoveAt(num);
            
            if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Warning($"SelectRoles - player: '{player.Data.PlayerName}', role: '{RoleManager.Instance.GetRole((RoleTypes)role).NiceName}'");
        }

        foreach (var role in crewRoles)
        {
            var num = HashRandom.FastNext(crewmates.Count);
            var player = crewmates[num];

            player.RpcSetRole((RoleTypes)role);

            crewmates.RemoveAt(num);
            
            if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Warning($"SelectRoles - player: '{player.Data.PlayerName}', role: '{RoleManager.Instance.GetRole((RoleTypes)role).NiceName}'");
        }

        // Assign vanilla roles to anyone who did not receive a role.
        foreach (var player in crewmates)
        {
            player.RpcSetRole(RoleTypes.Crewmate);
        }

        foreach (var player in impostors)
        {
            player.RpcSetRole(RoleTypes.Impostor);
        }
    }

    public static void AssignTargets()
    {
        // This is a coroutine because otherwise, the game just assigns targets real badly like traitor egotist, exe being lovers with their targets, that sort of thing - Atony
        Coroutines.Start(CoAssignTargets());
    }

    public static IEnumerator CoAssignTargets()
    {
        foreach (var role in MiscUtils.AllRoles.Where(x => x is IAssignableTargets)
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
        if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Error($"RoleManager.SelectRoles - ReplaceRoleManager: {ReplaceRoleManager}");

        if (TutorialManager.InstanceExists || ReplaceRoleManager)
        {
            return true;
        }

        //Logger<TownOfUsPlugin>.Error($"RoleManager.SelectRoles 2");

        var random = new System.Random();

        var players = GameData.Instance.AllPlayers.ToArray().ToList();
        players.Shuffle();

        var impCount = GameOptionsManager.Instance.CurrentGameOptions.GetAdjustedNumImpostors(players.Count);
        List<NetworkedPlayerInfo> infected = [];

        var useBias = OptionGroupSingleton<RoleOptions>.Instance.LastImpostorBias;

        if (useBias && LastImps.Count > 0)
        {
            var biasPercent = OptionGroupSingleton<RoleOptions>.Instance.ImpostorBiasPercent.Value / 100f;
            while (infected.Count < impCount)
            {
                if (players.All(x=> LastImps.Contains(x.ClientId)))
                {
                    var remainingImps = impCount - infected.Count;
                    players.Shuffle();
                    infected.AddRange(players.Where(x=>!infected.Contains(x)).Take(remainingImps));
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
        if (OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled)
        {
            AssignRolesFromRoleList(infected);
        }
        else
        {
            AssignRoles(infected);
        }

        AssignTargets();

        return false;
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
        // Note: I know this is a like for like recreation of the AssignRoleOnDeath function but for some reason
        // the original won't spawn the Phantom and just spawns Neutral Ghost instead

        if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Warning($"AssignRoleOnDeathPatch - Player: '{player.Data.PlayerName}', specialRolesAllowed: {specialRolesAllowed}");
        if (player == null || !player.Data.IsDead)
            // Logger<TownOfUsPlugin>.Message($"AssignRoleOnDeathPatch - !player.Data.IsDead: '{!player.Data.IsDead}'");
        {
            return false;
        }

        if (/*!player.Data.Role.IsImpostor && */specialRolesAllowed && !player.HasModifier<BasicGhostModifier>())
            // Logger<TownOfUsPlugin>.Message($"AssignRoleOnDeathPatch - !player.Data.Role.IsImpostor: '{!player.Data.Role.IsImpostor}' specialRolesAllowed: {specialRolesAllowed}");
        {
            RoleManager.TryAssignSpecialGhostRoles(player);
        }

        if (!RoleManager.IsGhostRole(player.Data.Role.Role))
            // Logger<TownOfUsPlugin>.Message($"AssignRoleOnDeathPatch - !RoleManager.IsGhostRole(player.Data.Role.Role): '{!RoleManager.IsGhostRole(player.Data.Role.Role)}'");
        {
            player.RpcSetRole(player.Data.Role.DefaultGhostRole);
        }

        return false;
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.TryAssignSpecialGhostRoles))]
    [HarmonyPrefix]
    public static bool TryAssignSpecialGhostRolesPatch(RoleManager __instance, PlayerControl player)
    {
        if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Warning($"TryAssignSpecialGhostRolesPatch - Player: '{player.Data.PlayerName}'");
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
            // Logger<TownOfUsPlugin>.Message($"TryAssignSpecialGhostRolesPatch - ghostRoles role: {newRole.NiceName}");
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
        if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek) return true;
        if (!OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled) return true;

        var players = GameData.Instance.PlayerCount;
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
            if (impBuckets.Contains(roleOption)) impostors += 1;
            else if (roleOption == RoleListOption.Any) anySlots += 1;
        }

        int impProbability = (int)Math.Floor((double)players / anySlots * 5 / 3);
        for (int i = 0; i < anySlots; i++)
        {
            var random = Random.RandomRangeInt(0, 100);
            if (random < impProbability) impostors += 1;
            impProbability += 3;
        }

        if (players < 7 || impostors == 0) impostors = 1;
        else if (players < 10 && impostors > 2) impostors = 2;
        else if (players < 14 && impostors > 3) impostors = 3;
        else if (players < 19 && impostors > 4) impostors = 4;
        else if (impostors > 5) impostors = 5;
        __result = impostors;
        return false;
    }
}