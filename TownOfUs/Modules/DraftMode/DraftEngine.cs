using System.Collections;
using UnityEngine;
using TownOfUs.Patches.DraftMode;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using TownOfUs.Options;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;

namespace TownOfUs.Modules.DraftMode
{
    [RegisterInIl2Cpp]
    public class DraftEngineBehaviour(IntPtr iPtr) : MonoBehaviour(iPtr)
    {
        public static DraftEngineBehaviour Instance { get; private set; }

        private List<string> _pool = new();
        private readonly List<int> _slotOrder = new();
        private int _currentTurnNumber;
        private int _totalSlots;
        private int _turnIndex;
        private bool _running;
        private int _draftSessionId;
        private IEnumerator? _hostDraftLoopCoroutine;
        private IEnumerator? _watchDcCoroutine;
        private IRng _rng = new UnityRng();

        private readonly Dictionary<int, List<string>> _currentOffersBySlot = new();
        private readonly Dictionary<int, List<string>> _reservedSeatsBySlot = new();
        private readonly Dictionary<int, List<ushort>> _offeredRoleIdsBySlot = new();
        private readonly Dictionary<int, string> _slotGroupAssignments = new();
        private readonly HashSet<int> _processingPickSlots = new();
        private readonly HashSet<int> _reclaimedSlots = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Initialized");
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null!;
        }

        [HideFromIl2Cpp]
        public void StartHostDraft(int totalSlots, Dictionary<byte, int> pidToSlot)
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] StartHostDraft called");

            if (!AmongUsClient.Instance.AmHost)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, "[DraftEngine] Not host, aborting");
                return;
            }

            _draftSessionId++;
            _running = false;
            if (_hostDraftLoopCoroutine != null)
            {
                Coroutines.Stop(_hostDraftLoopCoroutine);
                _hostDraftLoopCoroutine = null;
            }
            if (_watchDcCoroutine != null)
            {
                Coroutines.Stop(_watchDcCoroutine);
                _watchDcCoroutine = null;
            }

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Building draft pool");
            _rng = DeterministicRng.CreateRandomlySeeded();
            _pool = DraftPoolBuilder.BuildPool(pidToSlot.Count, _rng);
            if (_pool == null || _pool.Count == 0)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, "[DraftEngine] Pool is empty, aborting and starting game normally");
                Coroutines.Start(CoAutoStartGame());
                return;
            }

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Pool contains {_pool.Count} entries");

            _slotOrder.Clear();
            _slotOrder.AddRange(pidToSlot.Values.OrderBy(x => x));
            _totalSlots = totalSlots;
            _turnIndex = 0;
            _currentTurnNumber = 0;
            _running = true;

            _slotGroupAssignments.Clear();
            _reclaimedSlots.Clear();
            _reservedSeatsBySlot.Clear();

            DraftManager.SetDraftStateFromHost(totalSlots, pidToSlot.Keys.ToList(), pidToSlot.Values.ToList());
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Draft state set locally");
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Broadcasting slot notifications");
            DraftNetworkHelper.BroadcastSlotNotifications(totalSlots, pidToSlot);
            DraftCancelButton.Show();

            _hostDraftLoopCoroutine = HostDraftLoop();
            _watchDcCoroutine = CoWatchForDisconnectedPickers();
            Coroutines.Start(_hostDraftLoopCoroutine);
            Coroutines.Start(_watchDcCoroutine);
        }

        [HideFromIl2Cpp]
        public void TryApplySubmittedPick(byte playerId, byte index)
        {
            if (!AmongUsClient.Instance.AmHost || !_running) return;

            var state = DraftManager.GetStateForPlayer(playerId);
            if (state == null || state.HasPicked || state.ChosenRoleId != 0) return;
            if (!state.IsPickingNow) return;
            if (state.PendingPickIndex == 255 && state.PendingPickTurnNumber < 0) return;

            var slot = state.SlotNumber;
            if (slot <= 0) return;

            if (state.PendingPickTurnNumber != _currentTurnNumber)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftEngine] Ignoring submitted pick for slot {slot} because it belongs to turn {state.PendingPickTurnNumber}, current turn {_currentTurnNumber}");
                return;
            }

            ApplyPick(slot, index);
        }

        [HideFromIl2Cpp]
        private IEnumerator HostDraftLoop()
        {
            int currentSession = _draftSessionId;
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] HostDraftLoop started for session {currentSession}");

            while (_running && _draftSessionId == currentSession && _turnIndex < _slotOrder.Count)
            {
                int concurrency = Math.Max(1, Math.Min(2, (int)OptionGroupSingleton<RoleOptions>.Instance.ConcurrentPicks.Value));
                int batchSize   = Math.Min(concurrency, _slotOrder.Count - _turnIndex);

                _currentTurnNumber++;
                _currentOffersBySlot.Clear();

                var activeSlots = new List<int>();
                for (int i = 0; i < batchSize; i++)
                {
                    var slot = _slotOrder[_turnIndex + i];
                    if (SetupTurn(slot))
                        activeSlots.Add(slot);
                    else
                        ApplyPick(slot, 255);
                }

                if (activeSlots.Count == 0)
                {
                    _turnIndex += Math.Max(1, batchSize);
                    yield return null;
                    continue;
                }

                yield return CoWaitForBatch(activeSlots, currentSession);

                _turnIndex += batchSize;
                yield return new WaitForSeconds(0.5f);
            }

            if (!_running || _draftSessionId != currentSession)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Draft loop session {currentSession} exited, skipping FinishDraft");
                yield break;
            }

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Draft complete");

            foreach (var s in DraftManager.GetAllStates())
            {
                if (s.HasPicked) continue;
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, $"[DraftEngine] Slot {s.SlotNumber} never picked, applying fallback pick before finishing");
                ApplyPick(s.SlotNumber, 255);
            }

            FinishDraft();
        }

        private static string BaseRoleName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            int pipeIdx = name.IndexOf('|');
            return pipeIdx >= 0 ? name.Substring(0, pipeIdx) : name;
        }

        private static (int maxImps, int maxNeuts) GetTargetLimits()
        {
            var impOpts = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
            var neutOpts = OptionGroupSingleton<RoleDraftNeutOptions>.Instance;

            int maxImps = impOpts != null ? Math.Max(0, (int)impOpts.MaxImpostors.Value) : int.MaxValue;
            int maxNeuts = neutOpts != null ? Math.Max(0, (int)neutOpts.MaxNeutrals.Value) : int.MaxValue;

            return (maxImps, maxNeuts);
        }

        private static bool UseRoleListMode => OptionGroupSingleton<RoleOptions>.Instance?.UseRoleListForPool ?? false;
        private int CountDistinctPoolSeats()
        {
            var seatKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var n in _pool)
            {
                if (string.IsNullOrEmpty(n)) continue;
                int pipeIdx = n.IndexOf('|');
                seatKeys.Add(pipeIdx >= 0 ? n.Substring(pipeIdx) : n);
            }
            return seatKeys.Count;
        }

        private bool IsBackedByPoolSeat(string baseName)
        {
            if (string.IsNullOrEmpty(baseName)) return false;
            foreach (var n in _pool)
            {
                if (n != null && BaseRoleName(n).Equals(baseName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static string NormalizeRoleNameKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            int pipeIdx = name.IndexOf('|');
            if (pipeIdx >= 0) name = name.Substring(0, pipeIdx);
            return name.Trim().ToLowerInvariant();
        }

        private sealed class DraftSlotContext
        {
            public HashSet<string> AvoidNames { get; } = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<ushort, int> AssignedCountsById { get; } = new();
            public Dictionary<string, int> AssignedCountsByName { get; } = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, ushort> RepresentativeIds { get; } = new(StringComparer.OrdinalIgnoreCase);
            public int PickedImps;
            public int PickedNeuts;
            public int OfferedImps;
            public int OfferedNeuts;
            public bool ExclusiveImpReserved;
            public bool SharedImpReserved;
            public bool ForceImp;
            public bool ForceNeut;
            public int MaxImps;
            public int MaxNeuts;
            public int RemainingUnpicked;
            public int RemainingSeats;
            public int CurrentImps => PickedImps + OfferedImps;
            public int CurrentNeuts => PickedNeuts + OfferedNeuts;

            public ushort GetRepresentativeId(string baseName)
            {
                if (RepresentativeIds.TryGetValue(baseName, out var id)) return id;
                id = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { baseName });
                RepresentativeIds[baseName] = id;
                return id;
            }
        }

        private DraftSlotContext BuildSlotContext(int excludeSlot, bool ignoreConcurrentOffers = false, bool ignoreForce = false)
        {
            var context = new DraftSlotContext();
            var (maxImps, maxNeuts) = GetTargetLimits();
            context.MaxImps = maxImps;
            context.MaxNeuts = maxNeuts;

            var allStates = DraftManager.GetAllStates();
            foreach (var s in allStates)
            {
                if (s.HasPicked && s.ChosenRoleId != 0)
                {
                    context.AssignedCountsById[s.ChosenRoleId] = context.AssignedCountsById.GetValueOrDefault(s.ChosenRoleId) + 1;
                    var assignedId = s.ChosenRoleId.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    context.AvoidNames.Add(assignedId);

                    var roleName = DraftRolePool.GetRoleNameFromId(s.ChosenRoleId);
                    if (!string.IsNullOrEmpty(roleName))
                    {
                        var norm = NormalizeRoleNameKey(roleName);
                        context.AssignedCountsByName[norm] = context.AssignedCountsByName.GetValueOrDefault(norm) + 1;
                        context.AvoidNames.Add(roleName);
                        context.AvoidNames.Add(BaseRoleName(roleName));
                    }

                    if (DraftRolePool.IsImpostorRoleId(s.ChosenRoleId) || DraftRolePool.IsImpostorRoleName(roleName)) context.PickedImps++;
                    else if (DraftRolePool.IsNeutralRoleId(s.ChosenRoleId) || DraftRolePool.IsNeutralRoleName(roleName)) context.PickedNeuts++;

                    if (DraftRolePool.IsExclusiveImpostorRoleId(s.ChosenRoleId) || DraftRolePool.IsExclusiveImpostorRoleName(roleName)) context.ExclusiveImpReserved = true;
                    else if (DraftRolePool.IsImpostorRoleId(s.ChosenRoleId) || DraftRolePool.IsImpostorRoleName(roleName)) context.SharedImpReserved = true;
                }
            }

            foreach (var kvp in _currentOffersBySlot)
            {
                if (kvp.Key == excludeSlot) continue;
                if (ignoreConcurrentOffers) continue;

                bool hasImp = false;
                bool hasNeut = false;
                _offeredRoleIdsBySlot.TryGetValue(kvp.Key, out var offeredIdsForSlot);

                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    var n = kvp.Value[i];
                    if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                    var baseName = BaseRoleName(n);

                    if (!ignoreConcurrentOffers)
                    {
                        context.AvoidNames.Add(n);
                        context.AvoidNames.Add(baseName);

                        int groupPipeIdx = n.IndexOf('|');
                        if (groupPipeIdx >= 0)
                        {
                            string groupSuffix = n.Substring(groupPipeIdx);
                            foreach (var poolEntry in _pool)
                            {
                                if (poolEntry != null && poolEntry.EndsWith(groupSuffix, StringComparison.Ordinal))
                                {
                                    context.AvoidNames.Add(poolEntry);
                                    context.AvoidNames.Add(BaseRoleName(poolEntry));
                                }
                            }
                        }
                    }

                    if (DraftRolePool.IsImpostorRoleName(baseName))
                    {
                        hasImp = true;
                        if (DraftRolePool.IsExclusiveImpostorRoleName(baseName)) context.ExclusiveImpReserved = true;
                        else context.SharedImpReserved = true;
                    }
                    else if (DraftRolePool.IsNeutralRoleName(baseName))
                    {
                        hasNeut = true;
                    }

                    ushort offeredId = offeredIdsForSlot != null && i < offeredIdsForSlot.Count
                        ? offeredIdsForSlot[i]
                        : context.GetRepresentativeId(baseName);

                    if (offeredId != 0)
                        context.AssignedCountsById[offeredId] = context.AssignedCountsById.GetValueOrDefault(offeredId) + 1;
                    var norm = NormalizeRoleNameKey(baseName);
                    context.AssignedCountsByName[norm] = context.AssignedCountsByName.GetValueOrDefault(norm) + 1;
                }

                if (hasImp) context.OfferedImps++;
                if (hasNeut) context.OfferedNeuts++;
            }

            if (!ignoreForce)
            {
                context.RemainingUnpicked = allStates.Count(s => !s.HasPicked);
                int neededImps = Math.Max(0, context.MaxImps - context.PickedImps);
                int neededNeuts = Math.Max(0, context.MaxNeuts - context.PickedNeuts);
                int totalNeeded = neededImps + neededNeuts;

                if (context.RemainingUnpicked > 0 && context.RemainingUnpicked <= totalNeeded)
                {
                    if (neededImps > 0) context.ForceImp = true;
                    if (neededNeuts > 0) context.ForceNeut = true;
                }
            }

            context.RemainingSeats = CountDistinctPoolSeats();
            bool blockImps = !context.ForceImp && (context.CurrentImps >= context.MaxImps || context.ExclusiveImpReserved);
            bool blockNeuts = !context.ForceNeut && (context.CurrentNeuts >= context.MaxNeuts);

            foreach (var n in _pool)
            {
                if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                var baseName = BaseRoleName(n);
                bool isImp = DraftRolePool.IsImpostorRoleName(baseName);
                bool isNeut = DraftRolePool.IsNeutralRoleName(baseName);
                bool isCrew = !isImp && !isNeut;

                if (blockImps && isImp)
                {
                    context.AvoidNames.Add(n);
                    context.AvoidNames.Add(baseName);
                }
                if (blockNeuts && isNeut)
                {
                    context.AvoidNames.Add(n);
                    context.AvoidNames.Add(baseName);
                }
                if ((context.SharedImpReserved || context.ExclusiveImpReserved) && DraftRolePool.IsExclusiveImpostorRoleName(baseName))
                {
                    context.AvoidNames.Add(n);
                    context.AvoidNames.Add(baseName);
                }
                if ((context.ForceImp || context.ForceNeut) && (isCrew || (isImp && !context.ForceImp) || (isNeut && !context.ForceNeut)))
                {
                    context.AvoidNames.Add(n);
                    context.AvoidNames.Add(baseName);
                }

                ushort baseId = context.GetRepresentativeId(baseName);
                int countById = baseId != 0 ? context.AssignedCountsById.GetValueOrDefault(baseId) : 0;
                int countByName = context.AssignedCountsByName.GetValueOrDefault(NormalizeRoleNameKey(baseName));
                int currentCount = Math.Max(countById, countByName);

                if (baseId != 0 && currentCount >= DraftRolePool.GetMaxCountForRoleName(baseName))
                {
                    context.AvoidNames.Add(n);
                    context.AvoidNames.Add(baseName);
                }
            }

            return context;
        }

        [HideFromIl2Cpp]
        private (int pickedImps, int pickedNeuts, int offeredImps, int offeredNeuts, bool exclusiveImpReserved, bool sharedImpReserved, Dictionary<ushort, int> assignedCountsById, Dictionary<string, int> assignedCountsByName) GetDraftStats(int excludeSlot)
        {
            var context = BuildSlotContext(excludeSlot, ignoreConcurrentOffers: true, ignoreForce: true);
            return (context.PickedImps, context.PickedNeuts, context.OfferedImps, context.OfferedNeuts, context.ExclusiveImpReserved, context.SharedImpReserved, context.AssignedCountsById, context.AssignedCountsByName);
        }

        [HideFromIl2Cpp]
        private bool IsRoleAllowedForSlot(string candidate, int slot, bool ignoreConcurrentOffers = false, bool ignoreForce = false, DraftSlotContext? context = null)
        {
            if (string.IsNullOrWhiteSpace(candidate) || candidate == "__RANDOM__") return false;
            var baseName = BaseRoleName(candidate);
            context ??= BuildSlotContext(slot, ignoreConcurrentOffers, ignoreForce);
            if (context.AvoidNames.Contains(candidate) || context.AvoidNames.Contains(baseName)) return false;

            bool isImp = DraftRolePool.IsImpostorRoleName(baseName);
            bool isNeut = DraftRolePool.IsNeutralRoleName(baseName);

            if (!UseRoleListMode)
            {
                if ((context.ForceImp && context.ForceNeut && !isImp && !isNeut)
                    || (context.ForceImp && !isImp)
                    || (context.ForceNeut && !isNeut))
                {
                    return false;
                }

                if (!context.ForceImp && isImp && (context.CurrentImps >= context.MaxImps || context.ExclusiveImpReserved)) return false;
                if (!context.ForceNeut && isNeut && context.CurrentNeuts >= context.MaxNeuts) return false;
            }
            else if (!ignoreForce)
            {
                int remainingUnpicked = DraftManager.GetAllStates().Count(s => !s.HasPicked);
                int remainingSeats = CountDistinctPoolSeats();
                if (remainingUnpicked > 0 && remainingUnpicked <= remainingSeats && !IsBackedByPoolSeat(baseName))
                    return false;
            }

            if (DraftRolePool.IsExclusiveImpostorRoleName(baseName) && (context.ExclusiveImpReserved || context.SharedImpReserved)) return false;

            var roleId = context.GetRepresentativeId(baseName);
            int currentCountById = context.AssignedCountsById.GetValueOrDefault(roleId);
            int currentCountByName = context.AssignedCountsByName.GetValueOrDefault(NormalizeRoleNameKey(baseName));
            int currentCount = Math.Max(currentCountById, currentCountByName);

            if (currentCount >= DraftRolePool.GetMaxCountForRoleName(baseName)) return false;
            return true;
        }

        [HideFromIl2Cpp]
        private HashSet<string> GetAvoidNamesForTurn(int excludeSlot, bool ignoreConcurrentOffers = false, bool ignoreForce = false, DraftSlotContext? context = null)
        {
            context ??= BuildSlotContext(excludeSlot, ignoreConcurrentOffers, ignoreForce);
            return new HashSet<string>(context.AvoidNames, StringComparer.OrdinalIgnoreCase);
        }

        [HideFromIl2Cpp]
        private List<string> GenerateOffersForSlot(int slot, ICollection<string> extraAvoid = null!)
        {
            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            int offered = Math.Max(1, (int)(roleOpts?.OfferedRolesCount.Value ?? 3));
            var context = BuildSlotContext(slot);
            var avoidNames = new HashSet<string>(context.AvoidNames, StringComparer.OrdinalIgnoreCase);
            if (extraAvoid != null) avoidNames.UnionWith(extraAvoid);

            var offers = DraftPoolBuilder.GetOfferedRoles(_pool, _rng, avoidNames)
                .Where(o => !string.IsNullOrWhiteSpace(o) && o != "__RANDOM__")
                .Where(o => IsRoleAllowedForSlot(o, slot, ignoreConcurrentOffers: false, context: context))
                .ToList();

            var relaxedContext = BuildSlotContext(slot, ignoreConcurrentOffers: true);
            var relaxedAvoid = new HashSet<string>(relaxedContext.AvoidNames, StringComparer.OrdinalIgnoreCase);
            if (extraAvoid != null) relaxedAvoid.UnionWith(extraAvoid);

            if (offers.Count < offered)
            {
                var relaxedOffers = DraftPoolBuilder.GetOfferedRoles(_pool, _rng, relaxedAvoid)
                    .Where(o => !string.IsNullOrWhiteSpace(o) && o != "__RANDOM__")
                    .Where(o => IsRoleAllowedForSlot(o, slot, ignoreConcurrentOffers: false, context: context))
                    .ToList();
                offers = MergeOfferLists(offers, relaxedOffers, offered);
            }

            if (offers.Count < offered)
            {
                var fallbackAvoid = new HashSet<string>(relaxedContext.AvoidNames, StringComparer.OrdinalIgnoreCase);
                if (extraAvoid != null) fallbackAvoid.UnionWith(extraAvoid);

                var anyCandidates = DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.Any))
                    ?.Where(n => !string.IsNullOrWhiteSpace(n))
                    .Where(n => IsBackedByPoolSeat(BaseRoleName(n)))
                    .Where(n => IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: true, context: relaxedContext))
                    .Where(n => !fallbackAvoid.Contains(n) && !fallbackAvoid.Contains(BaseRoleName(n)))
                    .ToList() ?? new List<string>();

                for (int i = anyCandidates.Count - 1; i > 0; i--)
                {
                    int j = _rng.NextInt(i + 1);
                    (anyCandidates[i], anyCandidates[j]) = (anyCandidates[j], anyCandidates[i]);
                }

                offers = MergeOfferLists(offers, anyCandidates, offered);
            }

            if (offers.Count == 0)
            {
                var rawPoolCandidates = _pool
                    .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                    .Where(n => !avoidNames.Contains(n) && !avoidNames.Contains(BaseRoleName(n)))
                    .Where(n => IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: true, context: relaxedContext))
                    .GroupBy(n => BaseRoleName(n), StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

                if (rawPoolCandidates.Count == 0)
                {
                    rawPoolCandidates = _pool
                        .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                        .Where(n => IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: true, context: relaxedContext))
                        .GroupBy(n => BaseRoleName(n), StringComparer.OrdinalIgnoreCase)
                        .Select(g => g.First())
                        .ToList();
                }

                for (int i = rawPoolCandidates.Count - 1; i > 0; i--)
                {
                    int j = _rng.NextInt(i + 1);
                    (rawPoolCandidates[i], rawPoolCandidates[j]) = (rawPoolCandidates[j], rawPoolCandidates[i]);
                }

                offers = MergeOfferLists(offers, rawPoolCandidates, Math.Max(1, offered));
            }

            var filtered = offers
                .Where(o => !string.IsNullOrWhiteSpace(o) && o != "__RANDOM__")
                .Where(o => IsRoleAllowedForSlot(o, slot, ignoreConcurrentOffers: false, context: context))
                .Where(o => context.GetRepresentativeId(BaseRoleName(o)) != 0)
                .ToList();

            if (filtered.Count == 0)
            {
                filtered = new List<string>();
            }

            if (filtered.Count < offered)
            {
                var relaxedOffers = DraftPoolBuilder.GetOfferedRoles(_pool, _rng, relaxedAvoid);

                var anyCandidates = DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.Any))
                    ?.Where(n => !string.IsNullOrWhiteSpace(n))
                    .Where(n => IsBackedByPoolSeat(BaseRoleName(n)))
                    .Where(n => IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: true, context: relaxedContext))
                    .Where(n => !relaxedAvoid.Contains(n) && !relaxedAvoid.Contains(BaseRoleName(n)))
                    .ToList() ?? new List<string>();

                var prioritized = new List<string>();
                prioritized.AddRange(offers);
                prioritized.AddRange(relaxedOffers);
                prioritized.AddRange(anyCandidates);
                prioritized.AddRange(_pool.Where(n => !string.IsNullOrWhiteSpace(n)).GroupBy(BaseRoleName, StringComparer.OrdinalIgnoreCase).Select(g => g.First()));

                filtered = MergeOfferLists(filtered, prioritized, offered);
            }

            if (filtered.Count > offered) filtered = filtered.Take(offered).ToList();

            return filtered;
        }

        private static List<string> MergeOfferLists(List<string> primary, List<string> extra, int target)
        {
            var result = new List<string>(primary);
            var seenBaseNames = new HashSet<string>(result.Select(BaseRoleName), StringComparer.OrdinalIgnoreCase);
            foreach (var n in extra)
            {
                if (result.Count >= target) break;
                if (string.IsNullOrEmpty(n)) continue;
                var baseName = BaseRoleName(n);
                if (seenBaseNames.Add(baseName))
                {
                    result.Add(n);
                }
            }

            foreach (var n in extra)
            {
                if (result.Count >= target) break;
                if (string.IsNullOrEmpty(n)) continue;
                result.Add(n);
            }

            return result;
        }

        private bool SetupTurn(int slot)
        {
            try
            {
                var state = DraftManager.GetStateForSlot(slot);
                var pickerId = state?.PlayerId ?? 0;

                if (state == null || DraftManager.IsPlayerDisconnected(pickerId))
                {
                    MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Slot {slot} (player {pickerId}) is disconnected prior to turn, instantly auto-picking fallback role");
                    ApplyPick(slot, 255, timedOut: true);
                    return false;
                }

                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Turn {_currentTurnNumber}: Starting turn for slot {slot}");

                var offers = GenerateOffersForSlot(slot);
                if (offers == null) offers = new List<string>();

                if (offers.Count == 0)
                {
                    MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                        $"[DraftEngine] No legal offers generated for slot {slot}; leaving turn open so the picker can inspect the empty offer state");
                }

                if (offers.Count == 0)
                {
                    var fallbackContext = BuildSlotContext(slot, ignoreConcurrentOffers: true);
                    var fallbackCandidates = _pool
                        .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                        .Where(n => !fallbackContext.AvoidNames.Contains(n) && !fallbackContext.AvoidNames.Contains(BaseRoleName(n)))
                        .Where(n => IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: true, context: fallbackContext))
                        .GroupBy(n => BaseRoleName(n), StringComparer.OrdinalIgnoreCase)
                        .Select(g => g.First())
                        .ToList();

                    if (fallbackCandidates.Count == 0)
                    {
                        fallbackCandidates = DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.Any))
                            ?.Where(n => !string.IsNullOrWhiteSpace(n))
                            .Where(n => IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: true, context: fallbackContext))
                            .GroupBy(n => BaseRoleName(n), StringComparer.OrdinalIgnoreCase)
                            .Select(g => g.First())
                            .ToList() ?? new List<string>();
                    }

                    if (fallbackCandidates.Count > 0)
                    {
                        for (int i = fallbackCandidates.Count - 1; i > 0; i--)
                        {
                            int j = _rng.NextInt(i + 1);
                            (fallbackCandidates[i], fallbackCandidates[j]) = (fallbackCandidates[j], fallbackCandidates[i]);
                        }

                        offers = MergeOfferLists(new List<string>(), fallbackCandidates, Math.Max(1, (int)(OptionGroupSingleton<RoleOptions>.Instance?.OfferedRolesCount.Value ?? 3)));
                    }
                }


            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            int offeredLimit = Math.Max(1, (int)(roleOpts?.OfferedRolesCount.Value ?? 3));
            if (offers.Count > offeredLimit)
            {
                offers = offers.Take(offeredLimit).ToList();
            }
                _currentOffersBySlot[slot] = offers;
                _reservedSeatsBySlot[slot] = ReserveSeatsForOffers(offers);
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Generated {offers.Count} role offers for slot {slot}");

                var pickedRoleCandidates = new List<ushort>();
                foreach (var roleName in offers)
                {
                    ushort roleId;
                    if (roleName == "__RANDOM__")
                    {
                        roleId = 0;
                    }
                    else
                    {
                        roleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { BaseRoleName(roleName) });
                        if (roleId == 0)
                            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                                $"[DraftEngine] Role name '{roleName}' failed to resolve to a role id");
                    }
                    pickedRoleCandidates.Add(roleId);
                }

                _offeredRoleIdsBySlot[slot] = new List<ushort>(pickedRoleCandidates);

                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Announcing turn to picker {pickerId}");
                DraftNetworkHelper.SendTurnAnnouncement(slot, pickerId, pickedRoleCandidates, _currentTurnNumber);

                var turnDuration = (int)Mathf.Max(1f, OptionGroupSingleton<RoleOptions>.Instance?.TurnDurationSeconds.Value ?? 1f);
                DraftManager.TurnDuration = turnDuration;

                state.PendingPickIndex = 255;
                state.PendingPickTurnNumber = _currentTurnNumber;
                state.IsPickingNow = true;

                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Waiting {turnDuration}s for pick (slot {slot})");
                return true;
            }
            catch (Exception e)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftEngine] Exception during turn setup for slot {slot}: {e}");
                return false;
            }
        }

        [HideFromIl2Cpp]
        private IEnumerator CoWaitForBatch(List<int> activeSlots, int currentSession)
        {
            var deadlines = new Dictionary<int, float>();
            var isBotOrDc = new Dictionary<int, bool>();
            var pending   = new HashSet<int>(activeSlots);

            foreach (var slot in activeSlots)
            {
                var state = DraftManager.GetStateForSlot(slot);
                var turnDuration = (int)Mathf.Max(1f, OptionGroupSingleton<RoleOptions>.Instance?.TurnDurationSeconds.Value ?? 1f);
                bool botDc = state != null && DraftManager.IsPlayerDisconnected(state.PlayerId);
                var waitSeconds = botDc ? Mathf.Min(1f, turnDuration) : turnDuration;
                deadlines[slot] = Time.time + waitSeconds;
                isBotOrDc[slot] = botDc;
            }

            while (pending.Count > 0 && _running && _draftSessionId == currentSession)
            {
                float maxRemaining = 0f;

                foreach (var slot in pending.ToList())
                {
                    var state = DraftManager.GetStateForSlot(slot);
                    if (state == null || state.HasPicked)
                    {
                        pending.Remove(slot);
                        continue;
                    }

                    if (state.PendingPickIndex != 255)
                    {
                        var index = state.PendingPickIndex;
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Pick received for slot {slot}: index {index}");
                        ApplyPick(slot, index);
                        state.PendingPickIndex = 255;
                        pending.Remove(slot);
                        continue;
                    }

                    if (!isBotOrDc[slot] && DraftManager.IsPlayerDisconnected(state.PlayerId))
                    {
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Slot {slot} disconnected mid-turn, instantly auto-picking fallback role");
                        isBotOrDc[slot] = true;
                        ApplyPick(slot, 255, timedOut: true);
                        pending.Remove(slot);
                        continue;
                    }

                    var remaining = deadlines[slot] - Time.time;
                    if (remaining <= 0f)
                    {
                        var reason  = isBotOrDc[slot] ? "bot/disconnected" : "timeout";
                        var offers  = _currentOffersBySlot.TryGetValue(slot, out var o) ? o : new List<string>();
                        var autoIndex = (byte)_rng.NextInt(Math.Max(1, offers.Count));
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Auto-picking index {autoIndex} for slot {slot} ({reason})");
                        ApplyPick(slot, autoIndex, timedOut: true);
                        pending.Remove(slot);
                        continue;
                    }

                    maxRemaining = Mathf.Max(maxRemaining, remaining);
                }

                DraftManager.TurnTimeLeft = maxRemaining;
                yield return null;
            }
        }

        [HideFromIl2Cpp]
        private IEnumerator CoWatchForDisconnectedPickers()
        {
            int currentSession = _draftSessionId;

            while (_running && _draftSessionId == currentSession)
            {
                foreach (var state in DraftManager.GetAllStates())
                {
                    if (!state.HasPicked || state.ChosenRoleId == 0) continue;
                    if (_reclaimedSlots.Contains(state.SlotNumber)) continue;
                    if (!DraftManager.IsPlayerDisconnected(state.PlayerId)) continue;

                    _reclaimedSlots.Add(state.SlotNumber);

                    var roleName = DraftRolePool.GetRoleNameFromId(state.ChosenRoleId);
                    state.ChosenRoleId = 0;
                    state.HasPicked = false;

                    if (!string.IsNullOrEmpty(roleName) && !_pool.Contains(roleName))
                    {
                        _pool.Add(roleName);
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
                            $"[DraftEngine] Slot {state.SlotNumber} disconnected after picking '{roleName}', returning it to pool");
                    }
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        private List<string> ReserveSeatsForOffers(List<string> offers)
        {
            var reserved = new List<string>();
            if (offers == null) return reserved;

            foreach (var offerName in offers)
            {
                if (string.IsNullOrEmpty(offerName) || offerName == "__RANDOM__") continue;

                if (_pool.Remove(offerName))
                {
                    reserved.Add(offerName);
                    continue;
                }

                var baseName = BaseRoleName(offerName);
                var matchIdx = _pool.FindIndex(x => !string.IsNullOrEmpty(x) && BaseRoleName(x).Equals(baseName, StringComparison.OrdinalIgnoreCase));
                if (matchIdx >= 0)
                {
                    var removed = _pool[matchIdx];
                    _pool.RemoveAt(matchIdx);
                    reserved.Add(removed);
                }
            }

            return reserved;
        }

        private void ReleaseReservedSeats(int slot)
        {
            if (_reservedSeatsBySlot.TryGetValue(slot, out var reserved))
            {
                foreach (var r in reserved)
                {
                    if (string.IsNullOrEmpty(r)) continue;
                    _pool.Add(r);
                }
                _reservedSeatsBySlot.Remove(slot);
            }

            _offeredRoleIdsBySlot.Remove(slot);
        }

        private void ConsumeReservedSeat(int slot, string? chosenName)
        {
            if (string.IsNullOrWhiteSpace(chosenName) || chosenName == "__RANDOM__") return;

            if (_reservedSeatsBySlot.TryGetValue(slot, out var reserved) && reserved != null)
            {
                var match = reserved.FirstOrDefault(x =>
                    string.Equals(x, chosenName, StringComparison.OrdinalIgnoreCase) ||
                    BaseRoleName(x).Equals(BaseRoleName(chosenName), StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    reserved.Remove(match);
                    if (reserved.Count == 0)
                        _reservedSeatsBySlot.Remove(slot);
                    return;
                }
            }

            if (HasSeatInPool(chosenName))
            {
                RemovePickedSeatFromPool(chosenName);
            }
        }

        private bool HasSeatInPool(string chosenName)
        {
            if (string.IsNullOrEmpty(chosenName) || chosenName == "__RANDOM__") return false;
            int pipeIdx = chosenName.IndexOf('|');
            if (pipeIdx >= 0)
            {
                string slotSuffix = chosenName.Substring(pipeIdx);
                return _pool.Any(x => x != null && x.EndsWith(slotSuffix, StringComparison.Ordinal));
            }

            return _pool.Any(x =>
                string.Equals(x, chosenName, StringComparison.OrdinalIgnoreCase) ||
                BaseRoleName(x).Equals(chosenName, StringComparison.OrdinalIgnoreCase));
        }

        private void RemovePickedSeatFromPool(string chosenName)
        {
            if (string.IsNullOrEmpty(chosenName) || chosenName == "__RANDOM__")
            {
                if (!string.IsNullOrEmpty(chosenName)) _pool.Remove(chosenName);
                return;
            }

            int pipeIdx = chosenName.IndexOf('|');
            if (pipeIdx >= 0)
            {
                string slotSuffix = chosenName.Substring(pipeIdx);
                _pool.RemoveAll(x => x != null && x.EndsWith(slotSuffix, StringComparison.Ordinal));
            }
            else
            {
                var matchIdx = _pool.FindIndex(x => !string.IsNullOrEmpty(x) && BaseRoleName(x).Equals(chosenName, StringComparison.OrdinalIgnoreCase));
                if (matchIdx >= 0)
                {
                    _pool.RemoveAt(matchIdx);
                }
                else
                {
                    _pool.Remove(chosenName);
                }
            }
        }

        private void RemoveAllSeatsForBaseName(string baseName)
        {
            if (string.IsNullOrEmpty(baseName)) return;

            _pool.RemoveAll(x => !string.IsNullOrEmpty(x) && BaseRoleName(x).Equals(baseName, StringComparison.OrdinalIgnoreCase));
            var slots = _reservedSeatsBySlot.Keys.ToList();
            foreach (var slot in slots)
            {
                if (_reservedSeatsBySlot.TryGetValue(slot, out var reserved))
                {
                    reserved.RemoveAll(x => !string.IsNullOrEmpty(x) && BaseRoleName(x).Equals(baseName, StringComparison.OrdinalIgnoreCase));
                    if (reserved.Count == 0) _reservedSeatsBySlot.Remove(slot);
                }
            }
        }

        private void ApplyPick(int slot, byte index, bool timedOut = false)
        {
            if (_processingPickSlots.Contains(slot)) return;
            _processingPickSlots.Add(slot);

            try
            {
            var state = DraftManager.GetStateForSlot(slot);
            if (state == null) return;
            if (state.HasPicked && state.ChosenRoleId != 0)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
                    $"[DraftEngine] Skipping apply for slot {slot} because it already has role {state.ChosenRoleId}");
                return;
            }

            if (index != 255 && (!state.IsPickingNow || state.PendingPickTurnNumber != _currentTurnNumber))
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftEngine] Ignoring stale pick for slot {slot}: turn {state.PendingPickTurnNumber}, current {_currentTurnNumber}");
                return;
            }

            if (index != 255 && state.PendingPickIndex != index && !timedOut)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftEngine] Ignoring pick for slot {slot} because the stored index {state.PendingPickIndex} does not match the submitted index {index}");
                return;
            }

            var offers = _currentOffersBySlot.TryGetValue(slot, out var o) ? o : new List<string>();
            string? chosenName = (index >= offers.Count) ? "__RANDOM__" : offers[index];
            ReleaseReservedSeats(slot);
            var validationContext = BuildSlotContext(slot, ignoreConcurrentOffers: true);

            if (chosenName != null && chosenName != "__RANDOM__" && !IsRoleAllowedForSlot(chosenName, slot, ignoreConcurrentOffers: true, context: validationContext))
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
                    $"[DraftEngine] '{chosenName}' is no longer allowed for slot {slot}, falling back to a legal role");
                chosenName = null;
            }

            ushort chosenRoleId = 0;
            if (index != 255 && _offeredRoleIdsBySlot.TryGetValue(slot, out var offeredIds) && index < offeredIds.Count)
            {
                if (!string.IsNullOrEmpty(chosenName) && chosenName != "__RANDOM__")
                {
                    chosenRoleId = offeredIds[index];
                    ConsumeReservedSeat(slot, chosenName);
                    _offeredRoleIdsBySlot.Remove(slot);
                    if (chosenRoleId == 0)
                    {
                        chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { BaseRoleName(chosenName) });
                        if (chosenRoleId == 0)
                        {
                            chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { "Crewmate" });
                        }
                    }
                }
                else
                {
                    _offeredRoleIdsBySlot.Remove(slot);
                    chosenRoleId = 0;
                }
            }
            else if (chosenName == "__RANDOM__" || chosenName == null)
            {
                bool isDc = DraftManager.IsPlayerDisconnected(state.PlayerId);
                var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
                var eligibleRemaining = _pool.Where(r => !string.IsNullOrWhiteSpace(r)
                    && IsRoleAllowedForSlot(r, slot, ignoreConcurrentOffers: true))
                    .Where(r => !isDc || (!DraftRolePool.IsImpostorRoleName(BaseRoleName(r)) && !DraftRolePool.IsNeutralRoleName(BaseRoleName(r))))
                    .ToList();

                if (roleOpts != null && roleOpts.UseRoleListForPool && _slotGroupAssignments.TryGetValue(slot, out var assignedGroup))
                {
                    var preferredRemaining = eligibleRemaining.Where(r => r != null && r.EndsWith(assignedGroup, StringComparison.Ordinal)).ToList();
                    if (preferredRemaining.Count > 0)
                        eligibleRemaining = preferredRemaining;
                }

                if (eligibleRemaining.Count > 0)
                {
                    var attempts = new List<string>(eligibleRemaining);
                    while (attempts.Count > 0)
                    {
                        var idx = _rng.NextInt(attempts.Count);
                        var randomName = attempts[idx];
                        attempts.RemoveAt(idx);
                        var repId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { BaseRoleName(randomName) });
                        if (repId == 0) continue;
                        RemovePickedSeatFromPool(randomName);
                        chosenRoleId = repId;
                        break;
                    }
                }
                else
                {
                    var eligibleContext = BuildSlotContext(slot, ignoreConcurrentOffers: true, ignoreForce: true);
                    var anyNames = DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.Any))
                        ?.Where(n => !string.IsNullOrWhiteSpace(n))
                        .Where(n => IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: true, ignoreForce: true, context: eligibleContext))
                        .Where(n => !isDc || (!DraftRolePool.IsImpostorRoleName(BaseRoleName(n)) && !DraftRolePool.IsNeutralRoleName(BaseRoleName(n))))
                        .ToList() ?? new List<string>();

                    if (anyNames.Count > 0)
                    {
                        var attempts = new List<string>(anyNames);
                        while (attempts.Count > 0)
                        {
                            var idx = _rng.NextInt(attempts.Count);
                            var fallbackName = attempts[idx];
                            attempts.RemoveAt(idx);
                            var repId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { BaseRoleName(fallbackName) });
                            if (repId == 0) continue;
                            chosenRoleId = repId;
                            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                                $"[DraftEngine] Pool exhausted for slot {slot}, assigned emergency fallback role id {chosenRoleId}");
                            break;
                        }
                    }
                    else
                    {
                        chosenRoleId = 0;
                    }
                }
            }
            else
            {
                chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { BaseRoleName(chosenName) });
                _offeredRoleIdsBySlot.Remove(slot);
                if (chosenRoleId == 0)
                {
                    chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { "Crewmate" });
                }
            }

            string? pickedRoleName = null;
            if (chosenRoleId != 0)
            {
                var statsEx = GetDraftStats(slot);
                int currentCountById = statsEx.assignedCountsById.GetValueOrDefault(chosenRoleId);
                pickedRoleName = DraftRolePool.GetRoleNameFromId(chosenRoleId);
                int maxAllowed = !string.IsNullOrEmpty(pickedRoleName) ? DraftRolePool.GetMaxCountForRoleName(BaseRoleName(pickedRoleName)) : int.MaxValue;
                if (currentCountById >= maxAllowed)
                {
                    MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                        $"[DraftEngine] Chosen role id {chosenRoleId} for slot {slot} exceeds max allowed (current={currentCountById}, max={maxAllowed}), falling back");
                    chosenRoleId = 0;
                }
            }
            else
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, $"[DraftEngine] Pick for slot {slot} resolved to role id 0 (chosen name: '{chosenName ?? "null"}')");
            }

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Applied pick for slot {slot}: roleId {chosenRoleId}");

            state.PendingPickIndex = 255;
            _currentOffersBySlot.Remove(slot);
            pickedRoleName = DraftRolePool.GetRoleNameFromId(chosenRoleId);
            if (!string.IsNullOrEmpty(pickedRoleName))
            {
                RemoveAllSeatsForBaseName(BaseRoleName(pickedRoleName));
            }

            DraftManager.ConfirmPick(slot, chosenRoleId);
            DraftNetworkHelper.BroadcastPickConfirmed(slot, chosenRoleId, timedOut);
            }
            finally
            {
                _processingPickSlots.Remove(slot);
            }
        }

        private void FinishDraft()
        {
            _running = false;

            var recapMode = OptionGroupSingleton<RoleOptions>.Instance?.DraftRecap.Value ?? DraftRecapMode.Nothing;

            var recapEntries = new List<RecapEntry>();
            foreach (var s in DraftManager.GetAllStates())
            {
                var roleName = DraftRolePool.GetRoleNameFromId(s.ChosenRoleId) ?? s.ForcedRoleName ?? "Unknown";

                RoleBehaviour? roleBehaviour = null;
                try
                {
                    roleBehaviour = s.ChosenRoleId != 0
                        ? MiscUtils.GetRegisteredRole((AmongUs.GameOptions.RoleTypes)s.ChosenRoleId)
                        : null;
                }
                catch
                {
                    // ignored
                }

                string teamLabel = "Unknown";
                Color roleColor = Color.white;

                if (roleBehaviour != null)
                {
                    if (recapMode == DraftRecapMode.Faction || recapMode == DraftRecapMode.Alignment)
                    {
                        teamLabel = DraftUiManager.GetTeamLabel(roleBehaviour).ToUpperInvariant() ?? "Unknown";
                        roleColor = MiscUtils.GetRoleFactionColor(roleBehaviour, true);
                    }
                    else if (recapMode == DraftRecapMode.Role)
                    {
                        teamLabel = roleBehaviour.GetRoleName()?.ToUpperInvariant() ?? "Unknown";
                        roleColor = roleBehaviour.TeamColor;
                    }
                }
                else
                {
                    if (recapMode == DraftRecapMode.Role)
                        teamLabel = roleName.ToUpperInvariant();
                }

                string colorHex  = ColorUtility.ToHtmlStringRGB(roleColor);

                recapEntries.Add(new RecapEntry(s.SlotNumber, roleName, teamLabel, colorHex));
            }

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Draft finished, recapMode={recapMode}");
            DraftApplier.StorePendingDraftStates(DraftManager.GetAllStates());
            DraftNetworkHelper.BroadcastRecap(recapEntries, recapMode);
            Coroutines.Start(CoAutoStartGame(recapMode != DraftRecapMode.Nothing ? 6f : 0f));
        }

        private static IEnumerator CoAutoStartGame(float delay = 0f)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (!AmongUsClient.Instance.AmHost)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, "[DraftEngine] No longer host");
                yield break;
            }

            if (GameStartManager.Instance == null)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, "[DraftEngine] GameStartManager not found");
                yield break;
            }

            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Joined)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, "[DraftEngine] Not in joined state");
                yield break;
            }

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Auto-starting game");

            GameStartPatch.SkipIntercept = true;
            int orig = GameStartManager.Instance.MinPlayers;
            try
            {
                GameStartManager.Instance.ResetStartState();
                GameStartManager.Instance.MinPlayers = 1;
                GameStartManager.Instance.BeginGame();
                GameStartManager.Instance.countDownTimer = 0f;
            }
            catch (System.Exception ex)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftEngine] Exception during GameStartManager.BeginGame: {ex}");
            }
            finally
            {
                GameStartManager.Instance.MinPlayers = orig;
            }

            float timeout = 10f;
            while (AmongUsClient.Instance != null && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Joined && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
            GameStartPatch.SkipIntercept = false;
        }

        public void RequestShuffle(byte playerId)
        {
            if (!_running) return;

            var state = DraftManager.GetStateForPlayer(playerId);
            if (state == null || state.HasPicked || !state.IsPickingNow) return;

            var currentSlot = state.SlotNumber;
            if (!_currentOffersBySlot.TryGetValue(currentSlot, out var previousOffers)) return;

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Shuffle requested by player {playerId}");

            ReleaseReservedSeats(currentSlot);

            var offers = GenerateOffersForSlot(currentSlot, previousOffers);
            _currentOffersBySlot[currentSlot] = offers;
            _reservedSeatsBySlot[currentSlot] = ReserveSeatsForOffers(offers);

            var pickedRoleCandidates = new List<ushort>();
            foreach (var roleName in offers)
            {
                ushort roleId;
                if (roleName == "__RANDOM__")
                {
                    roleId = 0;
                }
                else
                {
                    roleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { BaseRoleName(roleName) });
                }
                pickedRoleCandidates.Add(roleId);
            }

            DraftNetworkHelper.SendTurnAnnouncement(currentSlot, playerId, pickedRoleCandidates, _currentTurnNumber);
        }

        public void CancelDraft()
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] CancelDraft called");
            _draftSessionId++;
            _running = false;
            if (_hostDraftLoopCoroutine != null)
            {
                Coroutines.Stop(_hostDraftLoopCoroutine);
                _hostDraftLoopCoroutine = null;
            }
            if (_watchDcCoroutine != null)
            {
                Coroutines.Stop(_watchDcCoroutine);
                _watchDcCoroutine = null;
            }

            _currentOffersBySlot.Clear();
            _reservedSeatsBySlot.Clear();
            DraftManager.Reset(cancelledBeforeCompletion: true);
            DraftNetworkHelper.BroadcastCancelDraft();
        }
    }
}
