using UnityEngine;
using TownOfUs.Patches.DraftMode;
using System.Collections;
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
        private readonly UnityRng _rng = new();

        private readonly Dictionary<int, List<string>> _currentOffersBySlot = new();

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
            if (Instance == this) Instance = null;
        }

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

            DraftManager.SetDraftStateFromHost(totalSlots, pidToSlot.Keys.ToList(), pidToSlot.Values.ToList());
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Draft state set locally");
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Broadcasting slot notifications");
            DraftNetworkHelper.BroadcastSlotNotifications(totalSlots, pidToSlot);
            DraftCancelButton.Show();
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Starting draft loop coroutine");
            Coroutines.Start(HostDraftLoop());
        }

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
                }

                if (activeSlots.Count == 0)
                {
                    _turnIndex += Math.Max(1, batchSize);
                    yield return new WaitForSeconds(1f);
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
            FinishDraft();
        }
        private HashSet<string> CollectOfferedNamesForOtherActiveSlots(int excludeSlot)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in _currentOffersBySlot)
            {
                if (kvp.Key == excludeSlot) continue;
                foreach (var n in kvp.Value)
                    if (!string.IsNullOrEmpty(n) && n != "__RANDOM__")
                        names.Add(n);
            }
            return names;
        }

        private bool SetupTurn(int slot)
        {
            try
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Turn {_currentTurnNumber}: Starting turn for slot {slot}");

                var avoidNames = CollectOfferedNamesForOtherActiveSlots(slot);
                var offers = DraftPoolBuilder.GetOfferedRoles(_pool, _rng, avoidNames);
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

            }

            return false;
        }

        private void ApplyPick(int slot, byte index)
        {
            var state = DraftManager.GetStateForSlot(slot);
            if (state == null) return;

            var offers      = _currentOffersBySlot.TryGetValue(slot, out var o) ? o : new List<string>();
            var idx         = Math.Max(0, Math.Min(offers.Count - 1, index));
            var chosenName  = offers.Count > idx ? offers[idx] : null;

            if (chosenName != null && chosenName != "__RANDOM__")
            {
                if (!_pool.Remove(chosenName))
                {
                    MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info,
                        $"[DraftEngine] '{chosenName}' was already taken by a concurrent pick, falling back to random for slot {slot}");
                    chosenName = null;
                }
            }

            ushort chosenRoleId;
            if (chosenName == "__RANDOM__" || chosenName == null)
            {
                var remaining = _pool.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
                if (remaining.Count > 0)
                {
                    var randomName = remaining[_rng.NextInt(remaining.Count)];
                    _pool.Remove(randomName);
                    chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { randomName });
                }
                else
                {
                    chosenRoleId = 0;
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

                RoleBehaviour roleBehaviour = null!;
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

                string teamLabel = "";
                Color roleColor = Color.white;
                if (recapMode == DraftRecapMode.Faction)
                {
                    teamLabel = DraftUiManager.GetTeamLabel(roleBehaviour).ToUpperInvariant() ?? "Unknown";
                    roleColor = MiscUtils.GetRoleFactionColor(roleBehaviour);
                }
                else if (recapMode == DraftRecapMode.Alignment)
                {
                    teamLabel = DraftUiManager.GetTeamLabel(roleBehaviour).ToUpperInvariant() ?? "Unknown";
                    roleColor = MiscUtils.GetRoleFactionColor(roleBehaviour);

                }
                else if (recapMode == DraftRecapMode.Role)
                {
                    teamLabel = roleBehaviour.GetRoleName().ToUpperInvariant() ?? "Unknown";
                    roleColor = roleBehaviour.TeamColor;
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

        public void CancelDraft()
        {
            if (!_running) return;
            _running = false;
            DraftManager.Reset(cancelledBeforeCompletion: true);
            DraftNetworkHelper.BroadcastCancelDraft();
        }
    }
}