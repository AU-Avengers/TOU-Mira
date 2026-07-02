using UnityEngine;
using TownOfUs.Patches.DraftMode;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using TownOfUs.Options;
using MiraAPI.GameOptions;

namespace TownOfUs.Modules.DraftMode
{
    [RegisterInIl2Cpp]
    public class DraftEngineBehaviour : MonoBehaviour
    {
        public DraftEngineBehaviour(IntPtr ptr) : base(ptr) { }

        public static DraftEngineBehaviour Instance { get; private set; }

        private List<string> _pool = new();
        private readonly List<int> _slotOrder = new();
        private int _currentTurnNumber;
        private int _totalSlots;
        private int _turnIndex;
        private bool _running;
        private readonly System.Random _rng = new System.Random();

        private List<string> _currentOffers = new();

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
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, "[DraftEngine] Already running, aborting");
                return;
            }

            _pool = DraftPoolBuilder.BuildPool();
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

            // Set state locally
            DraftManager.SetDraftStateFromHost(totalSlots, pidToSlot.Keys.ToList(), _slotOrder);
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Draft state set locally");

            // Broadcast to clients
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Broadcasting slot notifications");
            DraftNetworkHelper.BroadcastSlotNotifications(totalSlots, pidToSlot);

            // Start the draft loop
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Starting draft loop coroutine");
            Coroutines.Start(HostDraftLoop());
        }

        private IEnumerator HostDraftLoop()
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] HostDraftLoop started");

            while (_running && _turnIndex < _slotOrder.Count)
            {
                bool turnSetupSuccess = SetupTurn();
                
                if (!turnSetupSuccess)
                {
                    _turnIndex++;
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                yield return CoWaitForPickOrTimeout(_slotOrder[_turnIndex]);

                _turnIndex++;
                yield return new WaitForSeconds(0.5f);
            }

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Draft complete");
            FinishDraft();
        }

        private bool SetupTurn()
        {
            try
            {
                _currentTurnNumber++;
                int slot = _slotOrder[_turnIndex];

                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Turn {_currentTurnNumber}: Starting turn for slot {slot}");

                _currentOffers = DraftPoolBuilder.GetOfferedRoles(_rng);
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Generated {_currentOffers.Count} role offers");

                var pickedRoleCandidates = new List<ushort>();
                foreach (var bucket in _currentOffers)
                {
                    var resolved = DraftRolePool.ResolveBucketToRoleNames(bucket);
                    var roleId = DraftRolePool.ChooseRepresentativeRoleId(resolved);
                    if (roleId == 0)
                        MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, $"[DraftEngine] Bucket '{bucket}' failed to resolve to a role id (resolved names: [{string.Join(", ", resolved)}]) - check DraftRolePool name matching for this role");
                    pickedRoleCandidates.Add(roleId);
                }

                var state = DraftManager.GetStateForSlot(slot);
                var pickerId = state?.PlayerId ?? 0;

                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Announcing turn to picker {pickerId}");
                DraftNetworkHelper.SendTurnAnnouncement(slot, pickerId, pickedRoleCandidates, _currentTurnNumber);

                var turnDuration = (int)Mathf.Max(1f, OptionGroupSingleton<RoleOptions>.Instance.TurnDurationSeconds.Value);
                DraftManager.TurnDuration = turnDuration;
                DraftManager.TurnTimeLeft = turnDuration;

                var endTime = Time.time + turnDuration;
                DraftManager.SetClientTurn(_currentTurnNumber, slot);

                if (state != null)
                {
                    state.PendingPickIndex = 255;
                    state.IsPickingNow = true;
                }

                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Waiting {turnDuration}s for pick");
                return true;
            }
            catch (Exception e)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftEngine] Exception during turn setup: {e}");
                return false;
            }
        }

        private IEnumerator CoWaitForPickOrTimeout(int slot)
        {
            var state = DraftManager.GetStateForSlot(slot);
            var turnDuration = (int)Mathf.Max(1f, OptionGroupSingleton<RoleOptions>.Instance.TurnDurationSeconds.Value);
            var endTime = Time.time + turnDuration;

            while (Time.time < endTime && _running)
            {
                DraftManager.TurnTimeLeft = Mathf.Max(0f, endTime - Time.time);

                if (state != null && state.PendingPickIndex != 255 && !state.HasPicked)
                {
                    var index = state.PendingPickIndex;
                    MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Pick received: index {index}");
                    state.PendingPickIndex = 255;
                    ApplyPick(slot, index);
                    yield break;
                }
                yield return null;
            }

            if (state != null && state.PendingPickIndex == 255 && !state.HasPicked)
            {
                var index = (byte)_rng.Next(Math.Max(1, _currentOffers.Count));
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Auto-picking index {index} (timeout)");
                ApplyPick(slot, index);
            }
        }

        public void RequestReroll(byte playerId)
        {
            if (!_running) return;
            if (_turnIndex >= _slotOrder.Count) return;

            var currentSlot = _slotOrder[_turnIndex];
            var state = DraftManager.GetStateForSlot(currentSlot);
            if (state == null || state.PlayerId != playerId || state.HasPicked) return;

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Reroll requested by player {playerId}");

            _currentOffers = DraftPoolBuilder.GetOfferedRoles(_rng);
            var pickedRoleCandidates = new List<ushort>();
            foreach (var bucket in _currentOffers)
            {
                var resolved = DraftRolePool.ResolveBucketToRoleNames(bucket);
                var roleId = DraftRolePool.ChooseRepresentativeRoleId(resolved);
                pickedRoleCandidates.Add(roleId);
            }

            state.PendingPickIndex = 255;
            DraftNetworkHelper.SendTurnAnnouncement(currentSlot, playerId, pickedRoleCandidates, _currentTurnNumber);
        }

        private void ApplyPick(int slot, byte index)
        {
            var state = DraftManager.GetStateForSlot(slot);
            if (state == null) return;

            var idx = Math.Max(0, Math.Min(_currentOffers.Count - 1, index));
            var chosenBucket = _currentOffers.Count > idx ? _currentOffers[idx] : null;
            var resolved = chosenBucket != null ? DraftRolePool.ResolveBucketToRoleNames(chosenBucket) : new List<string>();
            var chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(resolved);

            if (chosenRoleId == 0)
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, $"[DraftEngine] Pick for slot {slot} resolved to role id 0 (bucket '{chosenBucket}'), this player will not get a proper role assignment");

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Applied pick for slot {slot}: roleId {chosenRoleId}");

            state.PendingPickIndex = 255;
            DraftManager.ConfirmPick(slot, chosenRoleId);
            DraftNetworkHelper.BroadcastPickConfirmed(slot, chosenRoleId);
        }

        private void FinishDraft()
        {
            _running = false;
            var recapEntries = new List<RecapEntry>();
            foreach (var s in DraftManager.GetAllStates())
            {
                var roleName = DraftRolePool.GetRoleNameFromId(s.ChosenRoleId) ?? s.ForcedRoleName ?? "Unknown";
                recapEntries.Add(new RecapEntry(s.SlotNumber, roleName));
            }

            var showRecap = OptionGroupSingleton<RoleOptions>.Instance.DraftRecap.Value != DraftRecapMode.Nothing;
            
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Draft finished, recap={showRecap}");
            DraftApplier.StorePendingDraftStates(DraftManager.GetAllStates());
            DraftNetworkHelper.BroadcastRecap(recapEntries, showRecap);
            Coroutines.Start(CoAutoStartGame());
        }

        private static IEnumerator CoAutoStartGame()
        {
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
            GameStartManager.Instance.BeginGame();
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