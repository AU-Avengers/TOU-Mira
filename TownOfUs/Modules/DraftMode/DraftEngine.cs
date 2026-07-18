using UnityEngine;
using TownOfUs.Patches.DraftMode;
using System.Collections;
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
        private readonly HashSet<int> _specialEligibleSlots = new();
        private int _totalNeutralGroupsInPool;
        private int _totalImpostorGroupsInPool;
        private int _currentTurnNumber;
        private int _totalSlots;
        private int _turnIndex;
        private bool _running;
        private readonly UnityRng _rng = new();

        private readonly Dictionary<int, List<string>> _currentOffersBySlot = new();
        private readonly Dictionary<int, string> _slotGroupAssignments = new();

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

            if (_running)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, "[DraftEngine] Draft already running!");
                return;
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

            _specialEligibleSlots.Clear();
            var eligiblePool = new List<int>(_slotOrder);
            for (int i = eligiblePool.Count - 1; i > 0; i--)
            {
                int j = _rng.NextInt(i + 1);
                (eligiblePool[i], eligiblePool[j]) = (eligiblePool[j], eligiblePool[i]);
            }
            int specialUnits = CountSpecialUnits(_pool);
            for (int i = 0; i < Math.Min(specialUnits, eligiblePool.Count); i++)
                _specialEligibleSlots.Add(eligiblePool[i]);
            _totalNeutralGroupsInPool = CountGroupsByPredicate(_pool, DraftRolePool.IsNeutralRoleName);
            _totalImpostorGroupsInPool = CountGroupsByPredicate(_pool, DraftRolePool.IsImpostorRoleName);

            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            if (roleOpts != null && roleOpts.UseRoleListForPool)
            {
                _slotGroupAssignments.Clear();
                var groups = GetRemainingPoolGroups(_pool);
                var shuffledGroups = new List<string>(groups);
                for (int i = shuffledGroups.Count - 1; i > 0; i--)
                {
                    int j = _rng.NextInt(i + 1);
                    (shuffledGroups[i], shuffledGroups[j]) = (shuffledGroups[j], shuffledGroups[i]);
                }
                
                for (int i = 0; i < Math.Min(_slotOrder.Count, shuffledGroups.Count); i++)
                {
                    _slotGroupAssignments[_slotOrder[i]] = shuffledGroups[i];
                }
            }

            DraftManager.SetDraftStateFromHost(totalSlots, pidToSlot.Keys.ToList(), pidToSlot.Values.ToList());
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Draft state set locally");
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Broadcasting slot notifications");
            DraftNetworkHelper.BroadcastSlotNotifications(totalSlots, pidToSlot);
            DraftCancelButton.Show();
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Starting draft loop coroutine");
            Coroutines.Start(HostDraftLoop());
        }

        [HideFromIl2Cpp]
        private IEnumerator HostDraftLoop()
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] HostDraftLoop started");

            while (_running && _turnIndex < _slotOrder.Count)
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

                yield return CoWaitForBatch(activeSlots);

                _turnIndex += batchSize;
                yield return new WaitForSeconds(0.5f);
            }

            if (!_running)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Draft loop exited due to cancellation, skipping FinishDraft");
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
        private HashSet<string> GetAvoidNamesForTurn(int excludeSlot, bool ignoreConcurrentOffers = false)
        {
            var avoid = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var assignedCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            int currentImps = 0;
            int currentNeuts = 0;
            int guaranteedImps = 0;
            int guaranteedNeuts = 0;
            bool exclusiveImpReserved = false;
            bool sharedImpReserved = false;

            foreach (var s in DraftManager.GetAllStates())
            {
                if (s.HasPicked && s.ChosenRoleId != 0)
                {
                    var roleName = DraftRolePool.GetRoleNameFromId(s.ChosenRoleId) ?? s.ForcedRoleName;
                    if (!string.IsNullOrEmpty(roleName))
                    {
                        assignedCounts[roleName] = assignedCounts.GetValueOrDefault(roleName) + 1;

                        if (DraftRolePool.IsImpostorRoleName(roleName))
                        {
                            currentImps++;
                            guaranteedImps++;
                        }
                        else if (DraftRolePool.IsNeutralRoleName(roleName))
                        {
                            currentNeuts++;
                            guaranteedNeuts++;
                        }

                        if (DraftRolePool.IsExclusiveImpostorRoleName(roleName)) exclusiveImpReserved = true;
                        else if (DraftRolePool.IsImpostorRoleName(roleName)) sharedImpReserved = true;
                    }
                }
            }

            foreach (var kvp in _currentOffersBySlot)
            {
                if (kvp.Key == excludeSlot) continue;

                bool hasImp = false;
                bool hasNeut = false;
                bool hasExclusiveImp = false;
                bool hasSharedImp = false;

                bool allImp = true;
                bool allNeut = true;
                int offerCount = 0;

                foreach (var n in kvp.Value)
                {
                    if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                    offerCount++;
                    
                    if (!ignoreConcurrentOffers)
                    {
                        avoid.Add(n);
                        int groupPipeIdx = n.IndexOf('|');
                        if (groupPipeIdx >= 0)
                        {
                            string groupSuffix = n.Substring(groupPipeIdx);
                            foreach (var poolEntry in _pool)
                            {
                                if (poolEntry != null && poolEntry.EndsWith(groupSuffix, StringComparison.Ordinal))
                                    avoid.Add(poolEntry);
                            }
                        }
                    }

                    if (DraftRolePool.IsImpostorRoleName(n)) hasImp = true;
                    else allImp = false;

                    if (DraftRolePool.IsNeutralRoleName(n)) hasNeut = true;
                    else allNeut = false;

                    if (DraftRolePool.IsExclusiveImpostorRoleName(n)) hasExclusiveImp = true;
                    else if (DraftRolePool.IsImpostorRoleName(n)) hasSharedImp = true;
                }

                if (offerCount > 0)
                {
                    if (allImp) guaranteedImps++;
                    if (allNeut) guaranteedNeuts++;
                }

                if (hasImp) currentImps++;
                if (hasNeut) currentNeuts++;
                if (hasExclusiveImp) exclusiveImpReserved = true;
                if (hasSharedImp) sharedImpReserved = true;
            }

            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            int maxImps;
            int maxNeuts;

            if (roleOpts != null && !roleOpts.UseRoleListForPool)
            {
                var impOpts = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
                var neutOpts = OptionGroupSingleton<RoleDraftNeutOptions>.Instance;

                maxImps = impOpts != null ? Math.Max(0, (int)impOpts.MaxImpostors.Value) : int.MaxValue;
                maxNeuts = neutOpts != null ? Math.Max(0, (int)neutOpts.MaxNeutrals.Value) : int.MaxValue;
            }
            else
            {
                maxImps = _totalImpostorGroupsInPool;
                maxNeuts = _totalNeutralGroupsInPool;
            }

            bool forceImp = false;
            bool forceNeut = false;

            if (_specialEligibleSlots.Contains(excludeSlot))
            {
                var slotState = DraftManager.GetStateForSlot(excludeSlot);
                if (slotState != null && !slotState.HasPicked)
                {
                    int impShortfall = exclusiveImpReserved ? 0 : Math.Max(0, maxImps - guaranteedImps);
                    int neutShortfall = Math.Max(0, maxNeuts - guaranteedNeuts);
                    int totalShortfall = impShortfall + neutShortfall;

                    int remainingSpecialSlots = _specialEligibleSlots.Count(s =>
                    {
                        var st = DraftManager.GetStateForSlot(s);
                        return st != null && !st.HasPicked;
                    });
                    
                    int activeSpecialSlots = 0;
                    foreach (var kvp in _currentOffersBySlot)
                    {
                        if (kvp.Key == excludeSlot) continue;
                        if (_specialEligibleSlots.Contains(kvp.Key))
                        {
                            activeSpecialSlots++;
                        }
                    }
                    
                    int uncommittedSpecialSlots = remainingSpecialSlots - activeSpecialSlots;

                    if (totalShortfall > 0 && uncommittedSpecialSlots <= totalShortfall)
                    {
                        if (impShortfall > 0) forceImp = true;
                        if (neutShortfall > 0) forceNeut = true;
                    }
                }
            }

            bool blockImps = !forceImp && (currentImps >= maxImps || exclusiveImpReserved);
            bool blockNeuts = !forceNeut && (currentNeuts >= maxNeuts);

            if (blockImps || blockNeuts)
            {
                foreach (var n in _pool)
                {
                    if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                    if (blockImps && DraftRolePool.IsImpostorRoleName(n)) avoid.Add(n);
                    if (blockNeuts && DraftRolePool.IsNeutralRoleName(n)) avoid.Add(n);
                }
            }

            if (sharedImpReserved)
            {
                foreach (var n in _pool)
                {
                    if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                    if (DraftRolePool.IsExclusiveImpostorRoleName(n)) avoid.Add(n);
                }
            }

            if (forceImp || forceNeut)
            {
                foreach (var n in _pool)
                {
                    if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                    bool isImp = DraftRolePool.IsImpostorRoleName(n);
                    bool isNeut = DraftRolePool.IsNeutralRoleName(n);
                    bool isCrew = !isImp && !isNeut;

                    if (isCrew) avoid.Add(n);
                    else if (isImp && !forceImp) avoid.Add(n);
                    else if (isNeut && !forceNeut) avoid.Add(n);
                }
            }

            if (!_specialEligibleSlots.Contains(excludeSlot))
            {
                foreach (var n in _pool)
                {
                    if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                    if (DraftRolePool.IsImpostorRoleName(n) || DraftRolePool.IsNeutralRoleName(n))
                        avoid.Add(n);
                }
            }

            foreach (var n in _pool)
            {
                if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                var baseName = n;
                int pipeIdx = baseName.IndexOf('|');
                if (pipeIdx >= 0) baseName = baseName.Substring(0, pipeIdx);
                
                int currentCount = assignedCounts.GetValueOrDefault(baseName);
                if (currentCount > 0)
                {
                    var matchedNames = assignedCounts.Keys.Where(k => DraftRolePool.ChooseRepresentativeRoleId(new List<string> { k }) == DraftRolePool.ChooseRepresentativeRoleId(new List<string> { baseName }));
                    currentCount = matchedNames.Sum(k => assignedCounts[k]);
                }
                
                if (currentCount >= DraftRolePool.GetMaxCountForRoleName(baseName))
                {
                    avoid.Add(n);
                }
            }

            return avoid;
        }

        private static int CountSpecialUnits(List<string> pool)
        {
            var groups = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in pool)
            {
                if (string.IsNullOrEmpty(entry)) continue;
                int pipeIdx = entry.IndexOf('|');
                string tag = pipeIdx >= 0 ? entry.Substring(pipeIdx) : entry;
                bool isSpecial = DraftRolePool.IsImpostorRoleName(entry) || DraftRolePool.IsNeutralRoleName(entry);
                if (!groups.ContainsKey(tag))
                    groups[tag] = isSpecial;
                else if (isSpecial)
                    groups[tag] = true;
            }
            return groups.Values.Count(v => v);
        }

        private static int CountGroupsByPredicate(List<string> pool, Func<string, bool> predicate)
        {
            var groups = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in pool)
            {
                if (string.IsNullOrEmpty(entry)) continue;
                int pipeIdx = entry.IndexOf('|');
                string tag = pipeIdx >= 0 ? entry.Substring(pipeIdx) : entry;
                bool matches = predicate(entry);
                if (!groups.ContainsKey(tag))
                    groups[tag] = matches;
                else if (matches)
                    groups[tag] = true;
            }
            return groups.Values.Count(v => v);
        }

        private HashSet<string> GetConcurrentOfferAvoid(int excludeSlot)
        {
            var avoid = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in _currentOffersBySlot)
            {
                if (kvp.Key == excludeSlot) continue;
                foreach (var n in kvp.Value)
                {
                    if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                    avoid.Add(n);
                    int pipeIdx = n.IndexOf('|');
                    if (pipeIdx < 0) continue;
                    string suffix = n.Substring(pipeIdx);
                    foreach (var poolEntry in _pool)
                    {
                        if (poolEntry != null && poolEntry.EndsWith(suffix, StringComparison.Ordinal))
                            avoid.Add(poolEntry);
                    }
                }
            }
            return avoid;
        }

        private static List<string> GetRemainingPoolGroups(List<string> pool)
        {
            var groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in pool)
            {
                if (string.IsNullOrEmpty(entry)) continue;
                int pipeIdx = entry.IndexOf('|');
                groups.Add(pipeIdx >= 0 ? entry.Substring(pipeIdx) : entry);
            }
            return groups.ToList();
        }

        private static int CountRemainingPoolGroups(List<string> pool)
        {
            return GetRemainingPoolGroups(pool).Count;
        }

        private List<string> GenerateOffersForSlot(int slot)
        {
            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            if (roleOpts != null && roleOpts.UseRoleListForPool && _slotGroupAssignments.TryGetValue(slot, out var assignedGroup))
            {
                var restrictedPool = _pool.Where(p => p != null && p.EndsWith(assignedGroup, StringComparison.Ordinal)).ToList();
                var strictAvoid = GetAvoidNamesForTurn(slot);
                var strictOffers = DraftPoolBuilder.GetOfferedRoles(restrictedPool, _rng, strictAvoid);
                if (strictOffers.Count > 0)
                {
                    MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Strict role list mode: slot {slot} assigned group {assignedGroup}");
                    return strictOffers;
                }
            }

            int remainingPlayers = DraftManager.GetAllStates().Count(s => !s.HasPicked);
            int remainingGroups = CountRemainingPoolGroups(_pool);

            if (remainingGroups > 0 && remainingPlayers <= remainingGroups)
            {
                var avoid = GetConcurrentOfferAvoid(slot);
                var allRemainingGroups = GetRemainingPoolGroups(_pool);
                var validGroups = allRemainingGroups.Where(g => !avoid.Any(a => a != null && a.EndsWith(g, StringComparison.Ordinal))).ToList();

                if (validGroups.Count > 0)
                {
                    var chosenGroup = validGroups[_rng.NextInt(validGroups.Count)];
                    var restrictedPool = _pool.Where(p => p != null && p.EndsWith(chosenGroup, StringComparison.Ordinal)).ToList();
                    
                    var healOffers = DraftPoolBuilder.GetOfferedRoles(restrictedPool, _rng, avoid);
                    if (healOffers.Count > 0)
                    {
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
                            $"[DraftEngine] Self-heal active for slot {slot}: assigned group {chosenGroup}");
                        return healOffers;
                    }
                }
            }

            var avoidNames = GetAvoidNamesForTurn(slot);
            var offers = DraftPoolBuilder.GetOfferedRoles(_pool, _rng, avoidNames);
            if (offers.Count == 0)
            {
                var relaxedAvoid = GetAvoidNamesForTurn(slot, ignoreConcurrentOffers: true);
                offers = DraftPoolBuilder.GetOfferedRoles(_pool, _rng, relaxedAvoid);
            }

            if (offers.Count == 0)
            {
                offers = DraftPoolBuilder.GetOfferedRoles(_pool, _rng, null!);
            }

            return offers;
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
                        roleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { roleName });
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
        private IEnumerator CoWaitForBatch(List<int> activeSlots)
        {
            var deadlines = new Dictionary<int, float>();
            var isBotOrDc = new Dictionary<int, bool>();
            var pending   = new HashSet<int>(activeSlots);

            foreach (var slot in activeSlots)
            {
                var state = DraftManager.GetStateForSlot(slot);
                var turnDuration = (int)Mathf.Max(1f, OptionGroupSingleton<RoleOptions>.Instance.TurnDurationSeconds.Value);
                bool botDc = state != null && IsBotOrDisconnected(state.PlayerId);
                var waitSeconds = botDc ? Mathf.Min(1f, turnDuration) : turnDuration;
                deadlines[slot] = Time.time + waitSeconds;
                isBotOrDc[slot] = botDc;
            }

            while (pending.Count > 0 && _running)
            {
                float maxRemaining = 0f;

                foreach (var slot in pending.ToList())
                {
                    var state = DraftManager.GetStateForSlot(slot);
                    if (state == null)
                    {
                        pending.Remove(slot);
                        continue;
                    }

                    if (state.HasPicked)
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

                    var remaining = deadlines[slot] - Time.time;
                    if (remaining <= 0f)
                    {
                        var reason  = isBotOrDc[slot] ? "bot/disconnected" : "timeout";
                        var offers  = _currentOffersBySlot.TryGetValue(slot, out var o) ? o : new List<string>();
                        var autoIndex = (byte)_rng.NextInt(Math.Max(1, offers.Count));
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Auto-picking index {autoIndex} for slot {slot} ({reason})");
                        ApplyPick(slot, autoIndex);
                        pending.Remove(slot);
                        continue;
                    }

                    maxRemaining = Mathf.Max(maxRemaining, remaining);
                }

                DraftManager.TurnTimeLeft = maxRemaining;
                yield return null;
            }
        }

        private static bool IsBotOrDisconnected(byte playerId)
        {
            var player = PlayerControl.AllPlayerControls?.ToArray()
                .FirstOrDefault(p => p != null && p.PlayerId == playerId);

            if (player == null) return true;
            if (player.Data == null || player.Data.Disconnected) return true;

            try
            {
                var client = AmongUsClient.Instance?.GetClient(player.OwnerId);
                if (client == null) return true;
            }
            catch
            {
                //ignored
            }

            return false;
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

        private void ApplyPick(int slot, byte index)
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
                if (roleOpts != null && roleOpts.UseRoleListForPool && _slotGroupAssignments.TryGetValue(slot, out var assignedGroup))
                {
                    var eligibleRemaining = _pool.Where(p => p != null && p.EndsWith(assignedGroup, StringComparison.Ordinal)).ToList();
                    if (eligibleRemaining.Count > 0)
                    {
                        var randomName = eligibleRemaining[_rng.NextInt(eligibleRemaining.Count)];
                        RemovePickedSeatFromPool(randomName);
                        chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { randomName });
                    }
                    else
                    {
                        chosenRoleId = 0;
                    }
                }
                else
                {
                    var remaining = _pool.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
                    var avoidForSlot = GetAvoidNamesForTurn(slot, ignoreConcurrentOffers: true);
                    var eligibleRemaining = remaining.Where(r => !avoidForSlot.Contains(r)).ToList();

                    if (eligibleRemaining.Count > 0)
                    {
                        var randomName = eligibleRemaining[_rng.NextInt(eligibleRemaining.Count)];
                        RemovePickedSeatFromPool(randomName);
                        chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { randomName });
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

                            var rn = DraftRolePool.GetRoleNameFromId(s.ChosenRoleId) ?? s.ForcedRoleName;
                            if (!string.IsNullOrEmpty(rn))
                            {
                                if (DraftRolePool.IsExclusiveImpostorRoleName(rn)) exclusiveImpAlreadyAssigned = true;
                                else if (DraftRolePool.IsImpostorRoleName(rn)) sharedImpAlreadyAssigned = true;

                                if (DraftRolePool.IsImpostorRoleName(rn)) currentImps++;
                                else if (DraftRolePool.IsNeutralRoleName(rn)) currentNeuts++;
                            }
                        }
                    }

                    foreach (var kvp in _currentOffersBySlot)
                    {
                        if (kvp.Key == slot) continue;
                        foreach (var n in kvp.Value)
                        {
                            if (string.IsNullOrEmpty(n) || n == "__RANDOM__") continue;
                            if (DraftRolePool.IsExclusiveImpostorRoleName(n)) exclusiveImpAlreadyAssigned = true;
                            else if (DraftRolePool.IsImpostorRoleName(n)) sharedImpAlreadyAssigned = true;
                        }
                    }

                    var roleOptsFallback = OptionGroupSingleton<RoleOptions>.Instance;
                    int maxImps;
                    int maxNeuts;
                    if (roleOptsFallback != null && !roleOptsFallback.UseRoleListForPool)
                    {
                        var impOptsFallback = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
                        var neutOptsFallback = OptionGroupSingleton<RoleDraftNeutOptions>.Instance;
                        maxImps = impOptsFallback != null ? Math.Max(0, (int)impOptsFallback.MaxImpostors.Value) : int.MaxValue;
                        maxNeuts = neutOptsFallback != null ? Math.Max(0, (int)neutOptsFallback.MaxNeutrals.Value) : int.MaxValue;
                    }
                    else
                    {
                        maxImps = _totalImpostorGroupsInPool;
                        maxNeuts = _totalNeutralGroupsInPool;
                    }

                    bool slotEligibleForSpecial = _specialEligibleSlots.Contains(slot);

                    Func<string, bool> fallbackFilter = n =>
                    {
                        var id = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { n });
                        if (assignedCounts.GetValueOrDefault(id) >= DraftRolePool.GetMaxCountForRoleName(n)) return false;
                        if (exclusiveImpAlreadyAssigned && DraftRolePool.IsImpostorRoleName(n)) return false;
                        if (sharedImpAlreadyAssigned && DraftRolePool.IsExclusiveImpostorRoleName(n)) return false;

                        bool isImp = DraftRolePool.IsImpostorRoleName(n);
                        bool isNeut = DraftRolePool.IsNeutralRoleName(n);
                        if ((isImp || isNeut) && !slotEligibleForSpecial) return false;
                        if (isImp && currentImps >= maxImps) return false;
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
                        chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { fallbackName });
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                            $"[DraftEngine] Pool exhausted for slot {slot}, assigned emergency fallback role id {chosenRoleId}");
                    }
                    else
                    {
                        chosenRoleId = 0;
                    }
                }
                }
            }
            else
            {
                chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { chosenName });
            }

            if (chosenRoleId == 0)
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, $"[DraftEngine] Pick for slot {slot} resolved to role id 0 (chosen name: '{chosenName ?? "null"}'), this player will not get a proper role assignment");

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Applied pick for slot {slot}: roleId {chosenRoleId}");

            state.PendingPickIndex = 255;
            _currentOffersBySlot.Remove(slot);
            DraftManager.ConfirmPick(slot, chosenRoleId);
            DraftNetworkHelper.BroadcastPickConfirmed(slot, chosenRoleId);
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
            GameStartManager.Instance.MinPlayers = 1;
            try
            {
                GameStartManager.Instance.BeginGame();
            }
            catch (System.Exception ex)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftEngine] Exception during GameStartManager.BeginGame: {ex}");
            }
            GameStartManager.Instance.countDownTimer = 0f;
            GameStartManager.Instance.MinPlayers = orig;
            yield return null;
            GameStartPatch.SkipIntercept = false;
        }

        public void RequestReroll(byte playerId)
        {
            if (!_running) return;

            var state = DraftManager.GetStateForPlayer(playerId);
            if (state == null || state.HasPicked || !state.IsPickingNow) return;

            var currentSlot = state.SlotNumber;
            if (!_currentOffersBySlot.ContainsKey(currentSlot)) return;

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Reroll requested by player {playerId}");

            var offers = GenerateOffersForSlot(currentSlot);
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
                    roleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { roleName });
                }
                pickedRoleCandidates.Add(roleId);
            }

            DraftNetworkHelper.SendTurnAnnouncement(currentSlot, playerId, pickedRoleCandidates, _currentTurnNumber);
        }

        public void CancelDraft()
        {
            if (!_running) return;
            _running = false;
            DraftManager.Reset(cancelledBeforeCompletion: true);
            DraftNetworkHelper.BroadcastCancelDraft();
        }
    }
}