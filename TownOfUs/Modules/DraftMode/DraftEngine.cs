using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private IEnumerator _hostDraftLoopCoroutine;
        private IEnumerator _watchDcCoroutine;
        private readonly UnityRng _rng = new();

        private readonly Dictionary<int, List<string>> _currentOffersBySlot = new();
        private readonly Dictionary<int, string> _slotGroupAssignments = new();
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
            _pool = DraftPoolBuilder.BuildPool(pidToSlot.Count);
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

        private (int maxImps, int maxNeuts) GetTargetLimits()
        {
            var impOpts = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
            var neutOpts = OptionGroupSingleton<RoleDraftNeutOptions>.Instance;

            int maxImps = impOpts != null ? Math.Max(0, (int)impOpts.MaxImpostors.Value) : int.MaxValue;
            int maxNeuts = neutOpts != null ? Math.Max(0, (int)neutOpts.MaxNeutrals.Value) : int.MaxValue;

            return (maxImps, maxNeuts);
        }

        private HashSet<string> GetAvoidNamesForTurn(int excludeSlot, bool ignoreConcurrentOffers = false, bool ignoreForce = false)
        {
            var avoid = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var assignedCountsById = new Dictionary<ushort, int>();

            int pickedImps = 0;
            int pickedNeuts = 0;
            bool exclusiveImpReserved = false;
            bool sharedImpReserved = false;

            foreach (var s in DraftManager.GetAllStates())
            {
                if (s.HasPicked && s.ChosenRoleId != 0)
                {
                    assignedCountsById[s.ChosenRoleId] = assignedCountsById.GetValueOrDefault(s.ChosenRoleId) + 1;

                    if (DraftRolePool.IsImpostorRoleId(s.ChosenRoleId)) pickedImps++;
                    else if (DraftRolePool.IsNeutralRoleId(s.ChosenRoleId)) pickedNeuts++;

                    if (DraftRolePool.IsExclusiveImpostorRoleId(s.ChosenRoleId)) exclusiveImpReserved = true;
                    else if (DraftRolePool.IsImpostorRoleId(s.ChosenRoleId)) sharedImpReserved = true;
                }
            }

            int offeredImps = 0;
            int offeredNeuts = 0;

            foreach (var kvp in _currentOffersBySlot)
            {
                if (kvp.Key == excludeSlot) continue;

                bool hasImp = false;
                bool hasNeut = false;

                foreach (var n in kvp.Value)
                {
                    if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                    var baseName = BaseRoleName(n);

                    if (!ignoreConcurrentOffers)
                    {
                        avoid.Add(n);
                        avoid.Add(baseName);
                        int groupPipeIdx = n.IndexOf('|');
                        if (groupPipeIdx >= 0)
                        {
                            string groupSuffix = n.Substring(groupPipeIdx);
                            foreach (var poolEntry in _pool)
                            {
                                if (poolEntry != null && poolEntry.EndsWith(groupSuffix, StringComparison.Ordinal))
                                {
                                    avoid.Add(poolEntry);
                                    avoid.Add(BaseRoleName(poolEntry));
                                }
                            }
                        }
                    }

                    if (DraftRolePool.IsImpostorRoleName(baseName))
                    {
                        hasImp = true;
                        if (DraftRolePool.IsExclusiveImpostorRoleName(baseName)) exclusiveImpReserved = true;
                        else sharedImpReserved = true;
                    }
                    else if (DraftRolePool.IsNeutralRoleName(baseName))
                    {
                        hasNeut = true;
                    }

                    var offeredId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { baseName });
                    assignedCountsById[offeredId] = assignedCountsById.GetValueOrDefault(offeredId) + 1;
                }

                if (hasImp) offeredImps++;
                if (hasNeut) offeredNeuts++;
            }

            var (maxImps, maxNeuts) = GetTargetLimits();

            int currentImps = pickedImps + offeredImps;
            int currentNeuts = pickedNeuts + offeredNeuts;

            bool forceImp = false;
            bool forceNeut = false;

            if (!ignoreForce)
            {
                int remainingUnpicked = DraftManager.GetAllStates().Count(s => !s.HasPicked);
                int neededImps = Math.Max(0, maxImps - pickedImps);
                int neededNeuts = Math.Max(0, maxNeuts - pickedNeuts);
                int totalNeeded = neededImps + neededNeuts;

                if (remainingUnpicked > 0 && remainingUnpicked <= totalNeeded)
                {
                    if (neededImps > 0) forceImp = true;
                    else if (neededNeuts > 0) forceNeut = true;
                }
            }

            bool blockImps = !forceImp && (currentImps >= maxImps || exclusiveImpReserved);
            bool blockNeuts = !forceNeut && (currentNeuts >= maxNeuts);

            if (blockImps || blockNeuts)
            {
                foreach (var n in _pool)
                {
                    if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                    var baseName = BaseRoleName(n);
                    if (blockImps && DraftRolePool.IsImpostorRoleName(baseName))
                    {
                        avoid.Add(n);
                        avoid.Add(baseName);
                    }
                    if (blockNeuts && DraftRolePool.IsNeutralRoleName(baseName))
                    {
                        avoid.Add(n);
                        avoid.Add(baseName);
                    }
                }
            }

            if (sharedImpReserved || exclusiveImpReserved)
            {
                foreach (var n in _pool)
                {
                    if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                    var baseName = BaseRoleName(n);
                    if (DraftRolePool.IsExclusiveImpostorRoleName(baseName))
                    {
                        avoid.Add(n);
                        avoid.Add(baseName);
                    }
                }
            }

            if (forceImp || forceNeut)
            {
                foreach (var n in _pool)
                {
                    if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                    var baseName = BaseRoleName(n);
                    bool isImp = DraftRolePool.IsImpostorRoleName(baseName);
                    bool isNeut = DraftRolePool.IsNeutralRoleName(baseName);
                    bool isCrew = !isImp && !isNeut;

                    if (isCrew) { avoid.Add(n); avoid.Add(baseName); }
                    else if (isImp && !forceImp) { avoid.Add(n); avoid.Add(baseName); }
                    else if (isNeut && !forceNeut) { avoid.Add(n); avoid.Add(baseName); }
                }
            }

            foreach (var n in _pool)
            {
                if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                var baseName = BaseRoleName(n);

                ushort baseId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { baseName });
                int currentCount = assignedCountsById.GetValueOrDefault(baseId);

                if (currentCount >= DraftRolePool.GetMaxCountForRoleName(baseName))
                {
                    avoid.Add(n);
                    avoid.Add(baseName);
                }
            }

            return avoid;
        }

        private List<string> GenerateOffersForSlot(int slot, ICollection<string> extraAvoid = null!)
        {
            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            int offered = Math.Max(1, (int)(roleOpts?.OfferedRolesCount.Value ?? 3));

            var avoidNames = GetAvoidNamesForTurn(slot);
            if (extraAvoid != null) avoidNames.UnionWith(extraAvoid);
            var offers = DraftPoolBuilder.GetOfferedRoles(_pool, _rng, avoidNames);

            if (offers.Count < offered)
            {
                var relaxedAvoid = GetAvoidNamesForTurn(slot, ignoreConcurrentOffers: true);
                if (extraAvoid != null) relaxedAvoid.UnionWith(extraAvoid);
                var relaxedOffers = DraftPoolBuilder.GetOfferedRoles(_pool, _rng, relaxedAvoid);
                offers = MergeOfferLists(offers, relaxedOffers, offered);
            }

            if (offers.Count < offered)
            {
                var strictAvoid = GetAvoidNamesForTurn(slot, ignoreConcurrentOffers: true, ignoreForce: true);
                if (extraAvoid != null) strictAvoid.UnionWith(extraAvoid);
                var strictOffers = DraftPoolBuilder.GetOfferedRoles(_pool, _rng, strictAvoid);
                offers = MergeOfferLists(offers, strictOffers, offered);
            }

            if (offers.Count < offered)
            {
                var fallbackAvoid = GetAvoidNamesForTurn(slot, ignoreConcurrentOffers: true, ignoreForce: true);
                if (extraAvoid != null) fallbackAvoid.UnionWith(extraAvoid);

                var anyCandidates = DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.Any))
                    ?.Where(n => !string.IsNullOrWhiteSpace(n) && !fallbackAvoid.Contains(n) && !fallbackAvoid.Contains(BaseRoleName(n)))
                    .ToList() ?? new List<string>();

                for (int i = anyCandidates.Count - 1; i > 0; i--)
                {
                    int j = _rng.NextInt(i + 1);
                    (anyCandidates[i], anyCandidates[j]) = (anyCandidates[j], anyCandidates[i]);
                }

                offers = MergeOfferLists(offers, anyCandidates, offered);
            }

            while (offers.Count < offered)
            {
                offers.Add("__RANDOM__");
            }

            return offers;
        }

        private static List<string> MergeOfferLists(List<string> primary, List<string> extra, int target)
        {
            var result = new List<string>(primary);
            var seen = new HashSet<string>(result.Select(BaseRoleName), StringComparer.OrdinalIgnoreCase);

            foreach (var n in extra)
            {
                if (result.Count >= target) break;
                if (string.IsNullOrEmpty(n)) continue;
                if (seen.Add(BaseRoleName(n))) result.Add(n);
            }

            return result;
        }

        private bool SetupTurn(int slot)
        {
            try
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Turn {_currentTurnNumber}: Starting turn for slot {slot}");

                var offers = GenerateOffersForSlot(slot);
                _currentOffersBySlot[slot] = offers;
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

                var state = DraftManager.GetStateForSlot(slot);
                var pickerId = state?.PlayerId ?? 0;

                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Announcing turn to picker {pickerId}");
                DraftNetworkHelper.SendTurnAnnouncement(slot, pickerId, pickedRoleCandidates, _currentTurnNumber);

                var turnDuration = (int)Mathf.Max(1f, OptionGroupSingleton<RoleOptions>.Instance.TurnDurationSeconds.Value);
                DraftManager.TurnDuration = turnDuration;

                if (state != null)
                {
                    state.PendingPickIndex = 255;
                    state.IsPickingNow = true;
                }

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
                var turnDuration = (int)Mathf.Max(1f, OptionGroupSingleton<RoleOptions>.Instance.TurnDurationSeconds.Value);
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
                        state.PendingPickIndex = 255;
                        ApplyPick(slot, index);
                        pending.Remove(slot);
                        continue;
                    }

                    if (!isBotOrDc[slot] && DraftManager.IsPlayerDisconnected(state.PlayerId))
                    {
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Slot {slot} disconnected mid-turn, skipping pick wait");
                        isBotOrDc[slot] = true;
                        deadlines[slot] = Mathf.Min(deadlines[slot], Time.time + 1f);
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
                    if (string.IsNullOrEmpty(roleName) || _pool.Contains(roleName)) continue;

                    _pool.Add(roleName);
                    MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
                        $"[DraftEngine] Slot {state.SlotNumber} disconnected after picking '{roleName}', returning it to pool");
                }

                yield return new WaitForSeconds(0.5f);
            }
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
                _pool.Remove(chosenName);
            }
        }

        private void ApplyPick(int slot, byte index, bool timedOut = false)
        {
            var state = DraftManager.GetStateForSlot(slot);
            if (state == null) return;

            var offers      = _currentOffersBySlot.TryGetValue(slot, out var o) ? o : new List<string>();
            string? chosenName = (index >= offers.Count) ? "__RANDOM__" : offers[index];

            if (chosenName != null && chosenName != "__RANDOM__" && !_pool.Remove(chosenName))
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
                    $"[DraftEngine] '{chosenName}' was already taken by a concurrent pick, falling back to random for slot {slot}");
                chosenName = null;
            }
            else if (chosenName != null && chosenName != "__RANDOM__")
            {
                RemovePickedSeatFromPool(chosenName);
            }

            ushort chosenRoleId;
            if (chosenName == "__RANDOM__" || chosenName == null)
            {
                var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
                var avoidForSlot = GetAvoidNamesForTurn(slot, ignoreConcurrentOffers: true);
                var eligibleRemaining = _pool.Where(r => !string.IsNullOrWhiteSpace(r) 
                    && !avoidForSlot.Contains(r) 
                    && !avoidForSlot.Contains(BaseRoleName(r))).ToList();

                if (roleOpts != null && roleOpts.UseRoleListForPool && _slotGroupAssignments.TryGetValue(slot, out var assignedGroup))
                {
                    var preferredRemaining = eligibleRemaining.Where(r => r != null && r.EndsWith(assignedGroup, StringComparison.Ordinal)).ToList();
                    if (preferredRemaining.Count > 0)
                        eligibleRemaining = preferredRemaining;
                }

                if (eligibleRemaining.Count > 0)
                {
                    var randomName = eligibleRemaining[_rng.NextInt(eligibleRemaining.Count)];
                    RemovePickedSeatFromPool(randomName);
                    chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { BaseRoleName(randomName) });
                }
                else
                {
                    var assignedCounts = new Dictionary<ushort, int>();
                    bool exclusiveImpAlreadyAssigned = false;
                    bool sharedImpAlreadyAssigned = false;
                    int currentImps = 0;
                    int currentNeuts = 0;
                    foreach (var s in DraftManager.GetAllStates())
                    {
                        if (s.HasPicked && s.ChosenRoleId != 0)
                        {
                            assignedCounts[s.ChosenRoleId] = assignedCounts.GetValueOrDefault(s.ChosenRoleId) + 1;

                            if (DraftRolePool.IsExclusiveImpostorRoleId(s.ChosenRoleId)) exclusiveImpAlreadyAssigned = true;
                            else if (DraftRolePool.IsImpostorRoleId(s.ChosenRoleId)) sharedImpAlreadyAssigned = true;

                            if (DraftRolePool.IsImpostorRoleId(s.ChosenRoleId)) currentImps++;
                            else if (DraftRolePool.IsNeutralRoleId(s.ChosenRoleId)) currentNeuts++;
                        }
                    }

                    foreach (var kvp in _currentOffersBySlot)
                    {
                        if (kvp.Key == slot) continue;
                        foreach (var n in kvp.Value)
                        {
                            if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                            var bn = BaseRoleName(n);
                            if (DraftRolePool.IsExclusiveImpostorRoleName(bn)) exclusiveImpAlreadyAssigned = true;
                            else if (DraftRolePool.IsImpostorRoleName(bn)) sharedImpAlreadyAssigned = true;
                        }
                    }

                    var (maxImps, maxNeuts) = GetTargetLimits();

                    Func<string, bool> fallbackFilter = n =>
                    {
                        var bn = BaseRoleName(n);
                        var id = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { bn });
                        if (assignedCounts.GetValueOrDefault(id) >= DraftRolePool.GetMaxCountForRoleName(bn)) return false;
                        if ((exclusiveImpAlreadyAssigned || sharedImpAlreadyAssigned) && DraftRolePool.IsExclusiveImpostorRoleName(bn)) return false;

                        bool isImp = DraftRolePool.IsImpostorRoleName(bn);
                        bool isNeut = DraftRolePool.IsNeutralRoleName(bn);
                        if (isImp && (currentImps >= maxImps || exclusiveImpAlreadyAssigned)) return false;
                        if (isNeut && currentNeuts >= maxNeuts) return false;
                        return true;
                    };

                    var anyNames = DraftRolePool.ResolveBucketToRoleNames(nameof(RoleListOption.Any))
                        ?.Where(n => !string.IsNullOrWhiteSpace(n))
                        .Where(fallbackFilter)
                        .ToList() ?? new List<string>();
                    if (anyNames.Count > 0)
                    {
                        var fallbackName = anyNames[_rng.NextInt(anyNames.Count)];
                        chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { BaseRoleName(fallbackName) });
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                            $"[DraftEngine] Pool exhausted for slot {slot}, assigned emergency fallback role id {chosenRoleId}");
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
            }

            if (chosenRoleId == 0)
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, $"[DraftEngine] Pick for slot {slot} resolved to role id 0 (chosen name: '{chosenName ?? "null"}')");

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Applied pick for slot {slot}: roleId {chosenRoleId}");

            state.PendingPickIndex = 255;
            _currentOffersBySlot.Remove(slot);
            DraftManager.ConfirmPick(slot, chosenRoleId);
            DraftNetworkHelper.BroadcastPickConfirmed(slot, chosenRoleId, timedOut);
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
                          ?? RoleManager.Instance?.GetRole((AmongUs.GameOptions.RoleTypes)s.ChosenRoleId)
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

            var offers = GenerateOffersForSlot(currentSlot, previousOffers);
            _currentOffersBySlot[currentSlot] = offers;

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
            DraftManager.Reset(cancelledBeforeCompletion: true);
            DraftNetworkHelper.BroadcastCancelDraft();
        }
    }
}
