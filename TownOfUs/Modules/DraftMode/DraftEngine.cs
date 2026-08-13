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
        private readonly HashSet<int> _guaranteedTurnSchedule = new();
        private int _currentTurnNumber;
        private int _totalSlots;
        private bool _running;
        private int _draftSessionId;
        private IEnumerator? _hostDraftLoopCoroutine;
        private IEnumerator? _watchDcCoroutine;
        private IRng _rng = new UnityRng();

        private readonly Dictionary<int, List<string>> _currentOffersBySlot = new();
        private readonly Dictionary<int, List<string>> _reservedSeatsBySlot = new();
        private readonly Dictionary<int, List<ushort>> _offeredRoleIdsBySlot = new();
        private readonly HashSet<int> _processingPickSlots = new();
        private readonly HashSet<int> _reclaimedSlots = new();
        private readonly Dictionary<int, HashSet<string>> _seenBaseNamesBySlot = new();
        private readonly HashSet<int> _allowedImpSlots = new();
        private readonly HashSet<int> _allowedNeutSlots = new();

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
            StopDraftCoroutines();

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

            _allowedImpSlots.Clear();
            _allowedNeutSlots.Clear();

            // Every slot may receive Evil/Neutral. The distribution is now handled
            // by the position tilt, soft floor spread, and Impostor nudge below.
            for (int slotNum = 1; slotNum <= totalSlots; slotNum++)
            {
                _allowedImpSlots.Add(slotNum);
                _allowedNeutSlots.Add(slotNum);
            }

            _currentTurnNumber = 0;
            _running = true;

            BuildGuaranteedTurnSchedule();

            _reclaimedSlots.Clear();
            _reservedSeatsBySlot.Clear();

            DraftManager.SetDraftStateFromHost(totalSlots, pidToSlot.Keys.ToList(), pidToSlot.Values.ToList());
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Draft state set locally");
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Broadcasting slot notifications");
            DraftNetworkHelper.BroadcastSlotNotifications(totalSlots, pidToSlot);
            DraftCancelButton.Show();

            BeginHostLoop();
        }

        private void BeginHostLoop()
        {
            _hostDraftLoopCoroutine = HostDraftLoop();
            _watchDcCoroutine = CoWatchForDisconnectedPickers();
            Coroutines.Start(_hostDraftLoopCoroutine);
            Coroutines.Start(_watchDcCoroutine);
        }

        private void StopDraftCoroutines()
        {
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

            while (_running && _draftSessionId == currentSession)
            {
                var pendingSlots = GetPendingSlots();

                if (pendingSlots.Count == 0)
                {
                    break;
                }

                int concurrency = Math.Max(1, Math.Min(2, (int)OptionGroupSingleton<RoleOptions>.Instance.ConcurrentPicks.Value));
                int batchSize   = Math.Min(concurrency, pendingSlots.Count);

                _currentTurnNumber++;
                _currentOffersBySlot.Clear();

                var activeSlots = new List<int>();
                for (int i = 0; i < batchSize; i++)
                {
                    var slot = pendingSlots[i];
                    if (SetupTurn(slot))
                        activeSlots.Add(slot);
                    else
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                            $"[DraftEngine] Turn setup failed for slot {slot}, will retry next pass instead of skipping their turn");
                }

                if (activeSlots.Count == 0)
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                yield return CoWaitForBatch(activeSlots, currentSession);
                yield return new WaitForSeconds(0.5f);
            }

            if (!_running || _draftSessionId != currentSession)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Draft loop session {currentSession} exited, skipping FinishDraft");
                yield break;
            }

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Draft complete, checking for late reclaims");

            while (_running && _draftSessionId == currentSession)
            {
                var lateSlots = GetPendingSlots();

                if (lateSlots.Count == 0) break;

                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftEngine] {lateSlots.Count} slot(s) reclaimed right as the draft was finishing, giving them a real turn instead of instant random");

                _currentTurnNumber++;
                _currentOffersBySlot.Clear();

                var lateActiveSlots = new List<int>();
                foreach (var slot in lateSlots)
                {
                    if (SetupTurn(slot))
                        lateActiveSlots.Add(slot);
                    else
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                            $"[DraftEngine] Turn setup failed for slot {slot}, will retry next pass instead of skipping their turn");
                }

                if (lateActiveSlots.Count > 0)
                {
                    yield return CoWaitForBatch(lateActiveSlots, currentSession);
                }

                yield return new WaitForSeconds(0.5f);
            }

            FinishDraft();
        }

        private static string BaseRoleName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            int pipeIdx = name.IndexOf('|');
            return pipeIdx >= 0 ? name.Substring(0, pipeIdx) : name;
        }

        [HideFromIl2Cpp]
        private List<int> GetPendingSlots()
        {
            return _slotOrder
                .Where(slot =>
                {
                    var state = DraftManager.GetStateForSlot(slot);
                    return state != null && !state.HasPicked;
                })
                .ToList();
        }

        private void BuildGuaranteedTurnSchedule()
        {
            _guaranteedTurnSchedule.Clear();

            int guaranteedCount = _pool
                .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                .Select(BaseRoleName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(n => DraftRolePool.GetChanceForRoleName(n) >= 100);

            if (guaranteedCount == 0) return;

            int concurrency = Math.Max(1, Math.Min(2, (int)OptionGroupSingleton<RoleOptions>.Instance.ConcurrentPicks.Value));
            int estimatedTurns = Math.Max(1, (int)Math.Ceiling(_slotOrder.Count / (double)concurrency));

            foreach (var turnIndex in _rng.NextSpreadIndices(guaranteedCount, estimatedTurns))
            {
                _guaranteedTurnSchedule.Add(turnIndex + 1);
            }
        }

        private bool DecideAllowGuaranteedThisTurn()
        {
            if (_guaranteedTurnSchedule.Contains(_currentTurnNumber)) return true;

            int guaranteedRemaining = _pool
                .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                .Select(BaseRoleName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(n => DraftRolePool.GetChanceForRoleName(n) >= 100);

            if (guaranteedRemaining == 0) return false;
            int turnsRemaining = GetPendingSlots().Count;
            return turnsRemaining <= guaranteedRemaining;
        }

        private static (int maxImps, int maxNeuts) GetTargetLimits()
        {
            if (UseRoleListMode)
            {
                var rl = OptionGroupSingleton<RoleDraftRoleListOptions>.Instance;
                if (rl != null)
                {
                    RoleListOption[] slots =
                    [
                        rl.Slot1.Value,  rl.Slot2.Value,  rl.Slot3.Value,
                        rl.Slot4.Value,  rl.Slot5.Value,  rl.Slot6.Value,
                        rl.Slot7.Value,  rl.Slot8.Value,  rl.Slot9.Value,
                        rl.Slot10.Value, rl.Slot11.Value, rl.Slot12.Value,
                        rl.Slot13.Value, rl.Slot14.Value, rl.Slot15.Value,
                    ];

                    int numPlayers = Instance != null && Instance._totalSlots > 0
                        ? Instance._totalSlots
                        : Math.Max(1, DraftManager.GetAllStates().Count);
                    if (numPlayers <= 0 && AmongUsClient.Instance != null && PlayerControl.AllPlayerControls != null)
                    {
                        numPlayers = PlayerControl.AllPlayerControls.Count;
                    }
                    if (numPlayers <= 0) numPlayers = 15;

                    int activeSlots = Math.Max(1, Math.Min(numPlayers, slots.Length));

                    int impSlots = 0;
                    int neutSlots = 0;
                    int anySlots = 0;
                    int nonImpSlots = 0;

                    for (int i = 0; i < activeSlots; i++)
                    {
                        var opt = slots[i];
                        if (DraftRolePool.IsImpostorRoleListOption(opt)) impSlots++;
                        else if (DraftRolePool.IsNeutralRoleListOption(opt)) neutSlots++;
                        else if (opt == RoleListOption.NonImp) nonImpSlots++;
                        else if (opt == RoleListOption.Any) anySlots++;
                    }

                    int maxImps = impSlots + anySlots;
                    int maxNeuts = neutSlots + nonImpSlots + anySlots;
                    return (maxImps, maxNeuts);
                }
            }

            var impOpts = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
            var neutOpts = OptionGroupSingleton<RoleDraftNeutOptions>.Instance;

            int maxImpsManual = impOpts != null ? Math.Max(0, (int)impOpts.MaxImpostors.Value) : int.MaxValue;
            int maxNeutsManual = neutOpts != null ? Math.Max(0, (int)neutOpts.MaxNeutrals.Value) : int.MaxValue;

            return (maxImpsManual, maxNeutsManual);
        }

        private static bool UseRoleListMode => OptionGroupSingleton<RoleOptions>.Instance?.UseRoleListForPool ?? false;
        private int CountDistinctPoolSeats()
        {
            var seatKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var n in _pool)
            {
                if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                int pipeIdx = n.IndexOf('|');
                seatKeys.Add(pipeIdx >= 0 ? n.Substring(pipeIdx) : n);
            }
            return seatKeys.Count;
        }

        private static bool IsSameDoubleDraftGroup(string aBaseName, string bBaseName)
        {
            bool aIsImp = DraftRolePool.IsImpostorRoleName(aBaseName);
            bool bIsImp = DraftRolePool.IsImpostorRoleName(bBaseName);
            if (aIsImp || bIsImp) return aIsImp && bIsImp;

            var aAlignment = DraftRolePool.GetRoleAlignment(aBaseName);
            var bAlignment = DraftRolePool.GetRoleAlignment(bBaseName);
            return aAlignment.HasValue && bAlignment.HasValue && aAlignment.Value == bAlignment.Value;
        }

        private int CountDistinctPoolSeatsForGroup(string candidateBaseName)
        {

            var seatKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var n in _pool)
            {
                if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                var entryBaseName = BaseRoleName(n);
                if (!IsSameDoubleDraftGroup(candidateBaseName, entryBaseName)) continue;

                int pipeIdx = n.IndexOf('|');
                seatKeys.Add(pipeIdx >= 0 ? n.Substring(pipeIdx) : n);
            }
            return seatKeys.Count;
        }

        private void ConsumeExtraSeatForDoubleDraft(string pickedBaseName, int excludeSlot)
        {
            string? target = null;
            foreach (var n in _pool)
            {
                if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                if (!IsSameDoubleDraftGroup(pickedBaseName, BaseRoleName(n))) continue;
                target = n;
                break;
            }

            if (target != null)
            {
                RemovePickedSeatFromPool(target);
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
                    $"[DraftEngine] '{pickedBaseName}' is double-draft; also consumed matching seat '{target}' from the pool");
            }
            else
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftEngine] '{pickedBaseName}' is double-draft but no second matching seat was found to consume");
            }
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

        [HideFromIl2Cpp]
        private DraftSlotContext BuildSlotContext(int excludeSlot, bool ignoreConcurrentOffers = false, bool ignoreForce = false)
        {
            var context = new DraftSlotContext();
            var (maxImps, maxNeuts) = GetTargetLimits();
            context.MaxImps = maxImps;
            context.MaxNeuts = maxNeuts;

            var allStates = DraftManager.GetAllStates();
            foreach (var s in allStates){
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

        bool isImp = DraftRolePool.IsImpostorRoleId(s.ChosenRoleId) || 
                     (!string.IsNullOrEmpty(roleName) && DraftRolePool.IsImpostorRoleName(roleName));
                     
        bool isNeut = DraftRolePool.IsNeutralRoleId(s.ChosenRoleId) || 
                      (!string.IsNullOrEmpty(roleName) && DraftRolePool.IsNeutralRoleName(roleName));

        bool isDoubleDraftPicked = DraftRolePool.IsDoubleDraftRoleId(s.ChosenRoleId) ||
                                  (!string.IsNullOrEmpty(roleName) && DraftRolePool.IsDoubleDraftRoleName(roleName));
        int pickedWeight = isDoubleDraftPicked ? 2 : 1;

        if (isImp)
        {
            context.PickedImps += pickedWeight;
        }
        else if (isNeut)
        {
            context.PickedNeuts += pickedWeight;
        }
    }
}
            foreach (var kvp in _currentOffersBySlot)
            {
                if (kvp.Key == excludeSlot) continue;
                if (ignoreConcurrentOffers) continue;

                int impCount = 0;
                int neutCount = 0;
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
                        var offerWeight = DraftRolePool.IsDoubleDraftRoleName(baseName) ? 2 : 1;
                        impCount = Math.Max(impCount, offerWeight);
                    }
                    else if (DraftRolePool.IsNeutralRoleName(baseName))
                    {
                        var offerWeight = DraftRolePool.IsDoubleDraftRoleName(baseName) ? 2 : 1;
                        neutCount = Math.Max(neutCount, offerWeight);
                    }

                    ushort offeredId = offeredIdsForSlot != null && i < offeredIdsForSlot.Count
                        ? offeredIdsForSlot[i]
                        : context.GetRepresentativeId(baseName);

                    if (offeredId != 0)
                        context.AssignedCountsById[offeredId] = context.AssignedCountsById.GetValueOrDefault(offeredId) + 1;
                    var norm = NormalizeRoleNameKey(baseName);
                    context.AssignedCountsByName[norm] = context.AssignedCountsByName.GetValueOrDefault(norm) + 1;
                }

                context.OfferedImps += impCount;
                context.OfferedNeuts += neutCount;
            }

            if (!ignoreForce)
            {
                context.RemainingUnpicked = allStates.Count(s => !s.HasPicked);
                int neededImps = Math.Max(0, context.MaxImps - context.PickedImps);
                int neededNeuts = Math.Max(0, context.MaxNeuts - context.PickedNeuts);
                int totalNeeded = neededImps + neededNeuts;

                if (context.RemainingUnpicked > 0 && context.RemainingUnpicked <= totalNeeded)
                {
                    // The current picker can only be locked to one faction. Prefer the
                    // larger remaining deficit; the next picker will re-evaluate the other.
                    if (neededImps >= neededNeuts && neededImps > 0)
                        context.ForceImp = true;
                    else if (neededNeuts > 0)
                        context.ForceNeut = true;
                }
            }

            context.RemainingSeats = CountDistinctPoolSeats();
            bool blockImps = !context.ForceImp && context.CurrentImps >= context.MaxImps;
            bool blockNeuts = !context.ForceNeut && context.CurrentNeuts >= context.MaxNeuts;

            foreach (var n in _pool)
            {
                if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                var baseName = BaseRoleName(n);
                bool isImp = DraftRolePool.IsImpostorRoleName(baseName);
                bool isNeut = DraftRolePool.IsNeutralRoleName(baseName);
                bool isCrew = !isImp && !isNeut;
                int candidateWeight = DraftRolePool.IsDoubleDraftRoleName(baseName) ? 2 : 1;

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
                if (!context.ForceImp && isImp && context.CurrentImps + candidateWeight > context.MaxImps)
                {
                    context.AvoidNames.Add(n);
                    context.AvoidNames.Add(baseName);
                }
                if (!context.ForceNeut && isNeut && context.CurrentNeuts + candidateWeight > context.MaxNeuts)
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
        private (int pickedImps, int pickedNeuts, int offeredImps, int offeredNeuts, Dictionary<ushort, int> assignedCountsById, Dictionary<string, int> assignedCountsByName) GetDraftStats(int excludeSlot)
        {
            var context = BuildSlotContext(excludeSlot, ignoreConcurrentOffers: true, ignoreForce: true);
            return (context.PickedImps, context.PickedNeuts, context.OfferedImps, context.OfferedNeuts, context.AssignedCountsById, context.AssignedCountsByName);
        }

        // Role offers are fully mixed across every configured bucket rather than
        // restricted to a single bucket per player - overall category counts are
        // still enforced via GetTargetLimits() and CountDistinctPoolSeatsForGroup,
        // so this intentionally always allows a role to be offered for any slot.
        private static bool RoleMatchesSlotBucket(string baseName, int slot) => true;

        [HideFromIl2Cpp]
        private bool IsRoleAllowedForSlot(string candidate, int slot, bool ignoreConcurrentOffers = false, bool ignoreForce = false, DraftSlotContext context = null!, bool ignoreSlotBucket = false)
        {
            if (string.IsNullOrWhiteSpace(candidate) || candidate == "__RANDOM__") return false;
            var baseName = BaseRoleName(candidate);

            ushort resolvedId = DraftRolePool.ResolveRoleIdFromName(baseName);

            if (resolvedId == (ushort)AmongUs.GameOptions.RoleTypes.Crewmate || 
                resolvedId == (ushort)AmongUs.GameOptions.RoleTypes.Impostor)
            {
                return false;
            }

            bool isImp = DraftRolePool.IsImpostorRoleId(resolvedId) || DraftRolePool.IsImpostorRoleName(baseName);

            context ??= BuildSlotContext(slot, ignoreConcurrentOffers, ignoreForce);
            if (context.AvoidNames.Contains(candidate) || context.AvoidNames.Contains(baseName)) return false;
            bool isNeut = DraftRolePool.IsNeutralRoleName(baseName);
            int candidateWeight = DraftRolePool.IsDoubleDraftRoleName(baseName) ? 2 : 1;

            // Evil/Neutral eligibility is no longer tied to pre-assigned slot buckets.
            // Position and spread probabilities decide when those roles are offered.
            if (!ignoreSlotBucket && !RoleMatchesSlotBucket(baseName, slot)) return false;

            if ((context.ForceImp && context.ForceNeut && !isImp && !isNeut)
                || (context.ForceImp && !isImp)
                || (context.ForceNeut && !isNeut))
            {
                return false;
            }

            if (!context.ForceImp && isImp && context.CurrentImps + candidateWeight > context.MaxImps) return false;
            if (!context.ForceNeut && isNeut && context.CurrentNeuts + candidateWeight > context.MaxNeuts) return false;
            if (candidateWeight == 2 && CountDistinctPoolSeatsForGroup(baseName) < 2) return false;

            if (UseRoleListMode && !ignoreForce)
            {
                int remainingUnpicked = DraftManager.GetAllStates().Count(s => !s.HasPicked);
                int remainingSeats = CountDistinctPoolSeats();
                if (remainingUnpicked > 0 && remainingUnpicked <= remainingSeats && !IsBackedByPoolSeat(baseName))
                    return false;
            }

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

            // Hard floor: when the remaining players can no longer satisfy the
            // configured Evil/Neutral floor, the whole offer is locked to that faction.
            DraftFaction? lockedFaction = GetHardFloorFaction(context);

            // Soft floor spread: before the hard deadline, occasionally lock an offer
            // to the faction with the largest remaining deficit. This is the important
            // part that prevents the last few picks from receiving all the Evil roles.
            if (!lockedFaction.HasValue)
                lockedFaction = GetSoftFloorFaction(context, slot);

            var allowed = _pool
                .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                .Where(n => !avoidNames.Contains(n) && !avoidNames.Contains(BaseRoleName(n)))
                .Where(n => IsRoleAllowedForSlot(
                    n, slot,
                    ignoreConcurrentOffers: false,
                    ignoreForce: lockedFaction.HasValue,
                    context: context))
                .GroupBy(BaseRoleName, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            // If a floor lock was selected, only show roles from that faction.
            if (lockedFaction.HasValue)
            {
                var lockedCandidates = allowed
                    .Where(n => GetRoleFaction(n) == lockedFaction.Value)
                    .ToList();

                if (lockedCandidates.Count > 0)
                    return BuildDiverseOffer(lockedCandidates, offered);
            }

            if (allowed.Count == 0)
            {
                var fallbackContext = BuildSlotContext(slot, ignoreConcurrentOffers: true, ignoreForce: true);
                allowed = _pool
                    .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                    .Where(n => !avoidNames.Contains(n) && !avoidNames.Contains(BaseRoleName(n)))
                    .Where(n => IsRoleAllowedForSlot(
                        n, slot, ignoreConcurrentOffers: true, ignoreForce: true, context: fallbackContext))
                    .GroupBy(BaseRoleName, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();
            }

            if (allowed.Count == 0)
                return BuildGuaranteedOfferFallback(offered, avoidNames, slot, context);

            var result = new List<string>();
            var usedAlignments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Position tilt:
            // first slot = 1.15x evil chance, middle = 1.0x, last = 0.85x.
            // The minimum Evil count is deliberately zero, so first/last are never
            // forced into a faction just because of their position.
            double positionTilt = PositionTilt(slot, _totalSlots);
            double evilChance = Math.Min(0.95, EvilOfferChance * positionTilt);

            var nonCrew = allowed.Where(IsEvilRole).ToList();
            var crew = allowed.Where(n => !IsEvilRole(n)).ToList();

            // Soft Impostor Nudge:
            // calculate the chance from Impostors still needed divided by players left.
            // This makes Impostors progressively more likely as the draft advances
            // instead of front-loading them onto the first few players.
            bool addImpostor = false;
            if (SoftImpostorNudge &&
                context.PickedImps < context.MaxImps)
            {
                int impDeficit = Math.Max(0, context.MaxImps - context.PickedImps);
                int playersLeft = Math.Max(1, context.RemainingUnpicked);
                double density = _totalSlots > 0
                    ? (double)Math.Max(1, context.MaxImps) / _totalSlots
                    : 0.20;
                double boost = Math.Pow(0.20 / Math.Max(0.04, density), ImpostorSpreadPower);
                boost = Math.Min(4.0, Math.Max(0.4, boost));
                double nudgeProb = Math.Min(1.0, boost * impDeficit / playersLeft);

                if (nudgeProb < 1.0)
                    nudgeProb = Math.Min(0.99, nudgeProb * positionTilt);

                var impCandidates = allowed
                    .Where(n => DraftRolePool.IsImpostorRoleName(BaseRoleName(n)))
                    .ToList();

                addImpostor = impCandidates.Count > 0 && _rng.NextDouble() < nudgeProb;
                if (addImpostor)
                {
                    var imp = PickDiverseRole(impCandidates, usedAlignments);
                    result.Add(imp);
                }
            }

            // General Evil/Neutral offer chance. It may add one or more Evil cards,
            // but never forces an Evil card solely because a player picked early.
            int maxEvil = Math.Min(nonCrew.Count, Math.Min(offered, 3));
            for (int i = result.Count; i < maxEvil; i++)
            {
                if (_rng.NextDouble() >= evilChance) break;

                var evilCandidates = nonCrew
                    .Where(n => !result.Contains(n))
                    .Where(n => !addImpostor || !DraftRolePool.IsImpostorRoleName(BaseRoleName(n)))
                    .ToList();

                if (evilCandidates.Count == 0) break;

                var pick = PickDiverseRole(evilCandidates, usedAlignments);
                result.Add(pick);
            }

            // Fill the rest with Crew first, using alignment diversity. If Crew runs
            // out, use any remaining legal role.
            while (result.Count < offered && crew.Count > 0)
            {
                var candidates = crew.Where(n => !result.Contains(n)).ToList();
                if (candidates.Count == 0) break;

                var pick = PickDiverseRole(candidates, usedAlignments);
                result.Add(pick);
                crew.RemoveAll(n => string.Equals(BaseRoleName(n), BaseRoleName(pick), StringComparison.OrdinalIgnoreCase));
            }

            while (result.Count < offered)
            {
                var remaining = allowed
                    .Where(n => !result.Contains(n))
                    .ToList();
                if (remaining.Count == 0) break;

                result.Add(PickDiverseRole(remaining, usedAlignments));
            }

            // If diversity was too restrictive because the pool is small, use the
            // normal builder as a final top-up before the generic fallback.
            if (result.Count < offered)
            {
                var topUp = DraftPoolBuilder.GetOfferedRoles(
                        allowed, _rng,
                        new HashSet<string>(result.Select(BaseRoleName), StringComparer.OrdinalIgnoreCase),
                        DecideAllowGuaranteedThisTurn())
                    .Where(n => !string.IsNullOrWhiteSpace(n) && !result.Contains(n))
                    .ToList();

                result = MergeOfferLists(result, topUp, offered);
            }

            if (result.Count > offered)
                result = result.Take(offered).ToList();

            return result;
        }

        private enum DraftFaction
        {
            Crewmate,
            Impostor,
            Neutral
        }

        // Tuned to be a small positional advantage rather than a guarantee.
        private const double PositionEdge = 0.15;
        private const double EvilOfferChance = 0.45;
        private const double FloorSpreadBias = 0.50;
        private const bool SoftImpostorNudge = true;
        private const double ImpostorSpreadPower = 1.0;

        private double PositionTilt(int slot, int totalSlots)
        {
            if (PositionEdge <= 0 || totalSlots <= 1) return 1.0;

            double pos = (double)(slot - 1) / Math.Max(1, totalSlots - 1);
            pos = Math.Max(0.0, Math.Min(1.0, pos));
            double tilt = 1.0 + PositionEdge * (0.5 - pos) * 2.0;
            return Math.Max(0.35, Math.Min(1.65, tilt));
        }

        private DraftFaction GetRoleFaction(string roleName)
        {
            var baseName = BaseRoleName(roleName);
            if (DraftRolePool.IsImpostorRoleName(baseName))
                return DraftFaction.Impostor;
            if (DraftRolePool.IsNeutralRoleName(baseName))
                return DraftFaction.Neutral;
            return DraftFaction.Crewmate;
        }

        private bool IsEvilRole(string roleName)
        {
            var baseName = BaseRoleName(roleName);
            return DraftRolePool.IsImpostorRoleName(baseName) ||
                   DraftRolePool.IsNeutralRoleName(baseName);
        }

        private string GetDiversityKey(string roleName)
        {
            var baseName = BaseRoleName(roleName);
            var alignment = DraftRolePool.GetRoleAlignment(baseName);
            if (alignment.HasValue)
                return alignment.Value.ToString();

            return GetRoleFaction(baseName).ToString();
        }

        private string PickDiverseRole(List<string> candidates, HashSet<string> usedAlignments)
        {
            if (candidates == null || candidates.Count == 0)
                return string.Empty;

            var distinct = candidates
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .GroupBy(BaseRoleName, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            if (distinct.Count == 0)
                return string.Empty;

            var preferred = distinct
                .Where(n => !usedAlignments.Contains(GetDiversityKey(n)))
                .ToList();

            var pool = preferred.Count > 0 ? preferred : distinct;
            var chosen = PickWeightedRoleName(pool);
            usedAlignments.Add(GetDiversityKey(chosen));
            return chosen;
        }

        private string PickWeightedRoleName(List<string> candidates)
        {
            if (candidates == null || candidates.Count == 0)
                return string.Empty;
            if (candidates.Count == 1)
                return candidates[0];

            int total = 0;
            var weights = new int[candidates.Count];
            for (int i = 0; i < candidates.Count; i++)
            {
                weights[i] = Math.Max(1, DraftRolePool.GetChanceForRoleName(BaseRoleName(candidates[i])));
                total += weights[i];
            }

            int roll = _rng.NextInt(total);
            int cumulative = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                    return candidates[i];
            }

            return candidates[^1];
        }

        private List<string> BuildDiverseOffer(List<string> candidates, int offered)
        {
            var result = new List<string>();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var remaining = candidates
                .GroupBy(BaseRoleName, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            while (result.Count < offered && remaining.Count > 0)
            {
                var pick = PickDiverseRole(remaining, used);
                if (string.IsNullOrEmpty(pick)) break;

                result.Add(pick);
                remaining.RemoveAll(n =>
                    string.Equals(BaseRoleName(n), BaseRoleName(pick), StringComparison.OrdinalIgnoreCase));
            }

            // A locked offer is intentionally faction-pure, but it should still contain
            // different alignments whenever the faction has them.
            return result;
        }

        private DraftFaction? GetHardFloorFaction(DraftSlotContext context)
        {
            int neededImps = Math.Max(0, context.MaxImps - context.PickedImps);
            int neededNeuts = Math.Max(0, context.MaxNeuts - context.PickedNeuts);
            int totalNeeded = neededImps + neededNeuts;

            if (context.RemainingUnpicked <= 0 || totalNeeded <= 0)
                return null;

            if (context.RemainingUnpicked <= totalNeeded)
            {
                if (neededImps >= neededNeuts && neededImps > 0)
                    return DraftFaction.Impostor;
                if (neededNeuts > 0)
                    return DraftFaction.Neutral;
            }

            return null;
        }

        private DraftFaction? GetSoftFloorFaction(DraftSlotContext context, int slot)
        {
            int neededImps = Math.Max(0, context.MaxImps - context.PickedImps);
            int neededNeuts = Math.Max(0, context.MaxNeuts - context.PickedNeuts);
            int totalNeeded = neededImps + neededNeuts;

            if (totalNeeded <= 0 || context.RemainingUnpicked <= totalNeeded)
                return null;

            double density = (double)totalNeeded / Math.Max(1, _totalSlots);
            double adaptive = Math.Max(0.0, Math.Min(0.5, 1.5 * (density - 0.35)));
            double spread = FloorSpreadBias * adaptive;
            double probability = spread * totalNeeded / Math.Max(1, context.RemainingUnpicked);

            if (_rng.NextDouble() >= probability)
                return null;

            if (neededImps > 0 && neededNeuts > 0)
            {
                double impWeight = (double)neededImps / totalNeeded;
                return _rng.NextDouble() < impWeight
                    ? DraftFaction.Impostor
                    : DraftFaction.Neutral;
            }

            if (neededImps > 0) return DraftFaction.Impostor;
            if (neededNeuts > 0) return DraftFaction.Neutral;
            return null;
        }

        [HideFromIl2Cpp]
        private List<string> BuildGuaranteedOfferFallback(int targetCount, HashSet<string> avoidNames, int slot, DraftSlotContext context)
        {
            var allowedPoolCandidates = _pool
                .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                .Where(n => !avoidNames.Contains(n) && !avoidNames.Contains(BaseRoleName(n)))
                .Where(n => IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: false, ignoreForce: false, context: context))
                .GroupBy(n => BaseRoleName(n), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            if (allowedPoolCandidates.Count == 0)
            {
                allowedPoolCandidates = _pool
                    .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                    .Where(n => IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: false, ignoreForce: false, context: context))
                    .GroupBy(n => BaseRoleName(n), StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();
            }

            if (allowedPoolCandidates.Count == 0)
            {
                allowedPoolCandidates = _pool
                    .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                    .Where(n => IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: false, ignoreForce: false, context: context, ignoreSlotBucket: true))
                    .GroupBy(n => BaseRoleName(n), StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();
            }

            if (allowedPoolCandidates.Count == 0)
            {
                allowedPoolCandidates = DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.Any))
                    ?.Where(n => !string.IsNullOrWhiteSpace(n))
                    .Where(n => IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: false, ignoreForce: false, context: context, ignoreSlotBucket: true))
                    .GroupBy(n => BaseRoleName(n), StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList() ?? new List<string>();
            }

            if (allowedPoolCandidates.Count == 0)
            {
                var fallbackId = DraftRolePool.GetAnyUsableRoleId();
                var fallbackName = fallbackId != 0 ? DraftRolePool.GetRoleNameFromId(fallbackId) : null;
                if (!string.IsNullOrEmpty(fallbackName))
                {
                    allowedPoolCandidates.Add(fallbackName);
                }
            }

            var guaranteed = DraftPoolBuilder.GetOfferedRoles(allowedPoolCandidates, _rng, new HashSet<string>(StringComparer.OrdinalIgnoreCase))
                .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                .ToList();

            if (guaranteed.Count == 0 && allowedPoolCandidates.Count > 0)
            {
                guaranteed.Add(allowedPoolCandidates[0]);
            }

            return guaranteed.Take(Math.Max(1, targetCount)).ToList();
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

                _seenBaseNamesBySlot[slot] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                    offers = BuildGuaranteedOfferFallback(
                        Math.Max(1, (int)(OptionGroupSingleton<RoleOptions>.Instance?.OfferedRolesCount.Value ?? 3)),
                        fallbackContext.AvoidNames,
                        slot,
                        fallbackContext);
                }

            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            int offeredLimit = Math.Max(1, (int)(roleOpts?.OfferedRolesCount.Value ?? 3));
            if (offers.Count > offeredLimit)
            {
                offers = offers.Take(offeredLimit).ToList();
            }
                _currentOffersBySlot[slot] = offers;
                _reservedSeatsBySlot[slot] = ReserveSeatsForOffers(offers);
                _seenBaseNamesBySlot[slot].UnionWith(offers
                    .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                    .Select(BaseRoleName));
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Generated {offers.Count} role offers for slot {slot}");

                var pickedRoleCandidates = new List<ushort>();
                var offeredRoleNames = new List<string>();
                foreach (var roleName in offers)
                {
                    ushort roleId;
                    if (roleName == "__RANDOM__")
                    {
                        roleId = 0;
                    }
                    else
                    {
                        roleId = DraftRolePool.ResolveRoleIdFromName(BaseRoleName(roleName));
                        if (roleId == 0)
                            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                                $"[DraftEngine] Role name '{roleName}' failed to resolve to a role id");
                    }
                    pickedRoleCandidates.Add(roleId);
                    offeredRoleNames.Add(roleName ?? string.Empty);
                }

                _offeredRoleIdsBySlot[slot] = new List<ushort>(pickedRoleCandidates);

                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Announcing turn to picker {pickerId}");
                DraftNetworkHelper.SendTurnAnnouncement(slot, pickerId, pickedRoleCandidates, offeredRoleNames, _currentTurnNumber);

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
                deadlines[slot] = Time.time + turnDuration;
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
                        var reason = isBotOrDc[slot] ? "bot/disconnected" : "timeout";
                        var offers = _currentOffersBySlot.TryGetValue(slot, out var o) ? o : new List<string>();
                        var autoIndex = (byte)_rng.NextInt(Math.Max(1, offers.Count));
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Auto-picking index {autoIndex} for slot {slot} due to {reason}");
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

        [HideFromIl2Cpp]
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
            RemoveAllSeatsForBaseName(baseName, _pool);
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

        private static void RemoveAllSeatsForBaseName(string baseName, List<string> pool)
        {
            if (string.IsNullOrEmpty(baseName)) return;
            pool.RemoveAll(x => !string.IsNullOrEmpty(x) && BaseRoleName(x).Equals(baseName, StringComparison.OrdinalIgnoreCase));
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

            bool wasDoubleDraftBlocked = false;
            if (chosenName != null && chosenName != "__RANDOM__" && !IsRoleAllowedForSlot(chosenName, slot, ignoreConcurrentOffers: false, context: validationContext))
            {
                wasDoubleDraftBlocked = DraftRolePool.IsDoubleDraftRoleName(BaseRoleName(chosenName));
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
                    $"[DraftEngine] '{chosenName}' is no longer allowed for slot {slot}, falling back to a legal role");
                chosenName = null;
            }

            ushort chosenRoleId = 0;

            if (chosenName == "__RANDOM__" || chosenName == null)
            {
                bool isDc = DraftManager.IsPlayerDisconnected(state.PlayerId);
                var strictValidationContext = BuildSlotContext(slot, ignoreConcurrentOffers: false, ignoreForce: true);
                var eligibleRemaining = _pool.Where(r => !string.IsNullOrWhiteSpace(r)
                    && IsRoleAllowedForSlot(r, slot, ignoreConcurrentOffers: false, ignoreForce: true, context: strictValidationContext))
                    .Where(r => !isDc || (!DraftRolePool.IsImpostorRoleName(BaseRoleName(r)) && !DraftRolePool.IsNeutralRoleName(BaseRoleName(r))))
                    .ToList();

                if (wasDoubleDraftBlocked)
                {
                    var crewOnly = eligibleRemaining.Where(r => !DraftRolePool.IsImpostorRoleName(BaseRoleName(r)) && !DraftRolePool.IsNeutralRoleName(BaseRoleName(r))).ToList();
                    if (crewOnly.Count > 0) eligibleRemaining = crewOnly;
                }

                if (eligibleRemaining.Count > 0)
                {
                    var attempts = new List<string>(eligibleRemaining);
                    while (attempts.Count > 0)
                    {
                        var idx = _rng.NextInt(attempts.Count);
                        var randomName = attempts[idx];
                        attempts.RemoveAt(idx);
                        var repId = DraftRolePool.ResolveRoleIdFromName(BaseRoleName(randomName));
                        if (repId == 0) continue;
                        RemovePickedSeatFromPool(randomName);
                        chosenRoleId = repId;
                        break;
                    }
                }
                else
                {
                    var eligibleContext = BuildSlotContext(slot, ignoreConcurrentOffers: false, ignoreForce: true);
                    var anyNames = DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.Any))
                        ?.Where(n => !string.IsNullOrWhiteSpace(n))
                        .Where(n => IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: false, ignoreForce: true, context: eligibleContext, ignoreSlotBucket: true))
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
                            var repId = DraftRolePool.ResolveRoleIdFromName(BaseRoleName(fallbackName));
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

                _offeredRoleIdsBySlot.Remove(slot);
            }
            else if (index != 255 && _offeredRoleIdsBySlot.TryGetValue(slot, out var offeredIds) && index < offeredIds.Count)
            {
                var offeredBaseName = BaseRoleName(chosenName);
                chosenRoleId = offeredIds[index];
                if (chosenRoleId == 0 || DraftRolePool.GetRoleNameFromId(chosenRoleId) == null)
                {
                    chosenRoleId = DraftRolePool.ResolveRoleIdFromName(offeredBaseName);
                }

                if (chosenRoleId == 0)
                {
                    chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { "Crewmate" });
                }

                ConsumeReservedSeat(slot, chosenName);
                _offeredRoleIdsBySlot.Remove(slot);
            }
            else
            {
                chosenRoleId = DraftRolePool.ResolveRoleIdFromName(BaseRoleName(chosenName));
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
                if (string.IsNullOrEmpty(pickedRoleName))
                {
                    pickedRoleName = BaseRoleName(chosenName ?? string.Empty);
                }

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

            if (chosenRoleId == 0 || MiscUtils.GetRegisteredRole((AmongUs.GameOptions.RoleTypes)chosenRoleId) == null)
            {
                var emergencyId = DraftRolePool.GetAnyUsableRoleId();
                if (emergencyId == 0 || MiscUtils.GetRegisteredRole((AmongUs.GameOptions.RoleTypes)emergencyId) == null)
                {
                    emergencyId = (ushort)AmongUs.GameOptions.RoleTypes.Engineer;
                }

                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftEngine] Slot {slot} had no resolvable role (id {chosenRoleId}), force-assigning emergency role id {emergencyId} to avoid an Unknown role");
                chosenRoleId = emergencyId;
            }

            state.PendingPickIndex = 255;
            _currentOffersBySlot.Remove(slot);
            pickedRoleName = DraftRolePool.GetRoleNameFromId(chosenRoleId);
            if (!string.IsNullOrEmpty(pickedRoleName))
            {
                var finalBaseName = BaseRoleName(pickedRoleName);
                RemoveAllSeatsForBaseName(finalBaseName);

                if (DraftRolePool.IsDoubleDraftRoleId(chosenRoleId) || DraftRolePool.IsDoubleDraftRoleName(finalBaseName))
                {
                    ConsumeExtraSeatForDoubleDraft(finalBaseName, slot);
                }
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

            var offeredCount = Math.Max(1, (int)(OptionGroupSingleton<RoleOptions>.Instance?.OfferedRolesCount.Value ?? 3));

            if (!_seenBaseNamesBySlot.TryGetValue(currentSlot, out var seen))
            {
                seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _seenBaseNamesBySlot[currentSlot] = seen;
            }

            var justShownBaseNames = previousOffers
                .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                .Select(BaseRoleName)
                .ToList();
            seen.UnionWith(justShownBaseNames);
            var offers = GenerateOffersForSlot(currentSlot, seen);
            offers = FinalizeShuffleOffers(currentSlot, offers, seen, offeredCount, previousOffers);

            _currentOffersBySlot[currentSlot] = offers;
            seen.UnionWith(offers
                .Where(n => !string.IsNullOrWhiteSpace(n) && n != "__RANDOM__")
                .Select(BaseRoleName));
            _reservedSeatsBySlot[currentSlot] = ReserveSeatsForOffers(offers);

            var pickedRoleCandidates = new List<ushort>();
            var offeredRoleNames = new List<string>();
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
                offeredRoleNames.Add(roleName ?? string.Empty);
            }

            _offeredRoleIdsBySlot[currentSlot] = new List<ushort>(pickedRoleCandidates);

            DraftNetworkHelper.SendTurnAnnouncement(currentSlot, playerId, pickedRoleCandidates, offeredRoleNames, _currentTurnNumber);
        }

        [HideFromIl2Cpp]
        private List<string> FinalizeShuffleOffers(int slot, List<string> offers, HashSet<string> seenBaseNames, int offeredCount, List<string> previousOffers)
        {
            var result = new List<string>((offers ?? new List<string>()).Where(n => !string.IsNullOrWhiteSpace(n)));
            var usedBaseNames = new HashSet<string>(result.Select(BaseRoleName), StringComparer.OrdinalIgnoreCase);

            DraftSlotContext? validationContext = null;
            DraftSlotContext EnsureContext() => validationContext ??= BuildSlotContext(slot, ignoreConcurrentOffers: true);

            for (var i = result.Count - 1; i >= 0; i--)
            {
                var baseName = BaseRoleName(result[i]);
                if (!seenBaseNames.Contains(baseName)) continue;

                var replacement = _pool.FirstOrDefault(n =>
                    !string.IsNullOrWhiteSpace(n) &&
                    !seenBaseNames.Contains(BaseRoleName(n)) &&
                    !usedBaseNames.Contains(BaseRoleName(n)) &&
                    IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: true, context: EnsureContext()));

                if (replacement != null)
                {
                    usedBaseNames.Remove(baseName);
                    usedBaseNames.Add(BaseRoleName(replacement));
                    result[i] = replacement;
                }
                else
                {
                    usedBaseNames.Remove(baseName);
                    result.RemoveAt(i);
                }
            }

            if (result.Count < offeredCount)
            {
                foreach (var n in _pool)
                {
                    if (result.Count >= offeredCount) break;
                    if (string.IsNullOrWhiteSpace(n)) continue;
                    var baseName = BaseRoleName(n);
                    if (usedBaseNames.Contains(baseName) || seenBaseNames.Contains(baseName)) continue;
                    if (!IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: true, context: EnsureContext())) continue;

                    result.Add(n);
                    usedBaseNames.Add(baseName);
                }
            }

            if (result.Count < offeredCount)
            {
                foreach (var n in _pool)
                {
                    if (result.Count >= offeredCount) break;
                    if (string.IsNullOrWhiteSpace(n)) continue;
                    var baseName = BaseRoleName(n);
                    if (usedBaseNames.Contains(baseName)) continue;
                    if (!IsRoleAllowedForSlot(n, slot, ignoreConcurrentOffers: true, context: EnsureContext())) continue;

                    result.Add(n);
                    usedBaseNames.Add(baseName);
                }
            }

            if (result.Count == 0 && previousOffers != null && previousOffers.Count > 0)
            {
                result = new List<string>(previousOffers.Where(n => !string.IsNullOrWhiteSpace(n)));
            }

            return result;
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