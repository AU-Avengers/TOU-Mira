using UnityEngine;
using TownOfUs.Patches.DraftMode;
using System.Collections;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using TownOfUs.Options;
using System.Threading.Tasks;
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

            if (!_running)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftEngine] Draft loop exited due to cancellation, skipping FinishDraft");
                yield break;
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

                _currentOffers = DraftPoolBuilder.GetOfferedRoles(_pool, _rng);
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Generated {_currentOffers.Count} role offers");

                var pickedRoleCandidates = new List<ushort>();
                foreach (var roleName in _currentOffers)
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

            bool isBotOrDc = state != null && IsBotOrDisconnected(state.PlayerId);
            var waitSeconds = isBotOrDc ? Mathf.Min(1f, turnDuration) : turnDuration;
            var endTime = Time.time + waitSeconds;

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

            if (!_running) yield break;

            if (state != null && state.PendingPickIndex == 255 && !state.HasPicked)
            {
                var index = (byte)_rng.Next(Math.Max(1, _currentOffers.Count));
                var reason = isBotOrDc ? "bot/disconnected" : "timeout";
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Auto-picking index {index} ({reason})");
                ApplyPick(slot, index);
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

        public void RequestReroll(byte playerId)
        {
            if (!_running) return;
            if (_turnIndex >= _slotOrder.Count) return;

            var currentSlot = _slotOrder[_turnIndex];
            var state = DraftManager.GetStateForSlot(currentSlot);
            if (state == null || state.PlayerId != playerId || state.HasPicked) return;

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Reroll requested by player {playerId}");

            _currentOffers = DraftPoolBuilder.GetOfferedRoles(_pool, _rng);
            var pickedRoleCandidates = new List<ushort>();
            foreach (var roleName in _currentOffers)
            {
                ushort roleId = roleName == "__RANDOM__"
                    ? (ushort)0
                    : DraftRolePool.ChooseRepresentativeRoleId(new List<string> { roleName });
                pickedRoleCandidates.Add(roleId);
            }

            state.PendingPickIndex = 255;
            DraftNetworkHelper.SendTurnAnnouncement(currentSlot, playerId, pickedRoleCandidates, _currentTurnNumber);
        }

        private void ApplyPick(int slot, byte index)
        {
            var state = DraftManager.GetStateForSlot(slot);
            if (state == null) return;

            var idx          = Math.Max(0, Math.Min(_currentOffers.Count - 1, index));
            var chosenName   = _currentOffers.Count > idx ? _currentOffers[idx] : null;

            if (chosenName != null && chosenName != "__RANDOM__")
                _pool.Remove(chosenName);

            ushort chosenRoleId;
            if (chosenName == "__RANDOM__" || chosenName == null)
            {

                var remaining = _pool.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
                chosenRoleId = (ushort)(remaining.Count > 0
                    ? DraftRolePool.ChooseRepresentativeRoleId(new List<string> { remaining[_rng.Next(remaining.Count)] })
                    : 0);
            }
            else
            {
                chosenRoleId = DraftRolePool.ChooseRepresentativeRoleId(new List<string> { chosenName });
            }

            if (chosenRoleId == 0)
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning, $"[DraftEngine] Pick for slot {slot} resolved to role id 0 (chosen name: '{chosenName ?? "null"}'), this player will not get a proper role assignment");

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"[DraftEngine] Applied pick for slot {slot}: roleId {chosenRoleId}");

            state.PendingPickIndex = 255;
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

                RoleBehaviour roleBehaviour = null;
                try
                {
                    roleBehaviour = s.ChosenRoleId != 0
                        ? MiscUtils.GetRegisteredRole((AmongUs.GameOptions.RoleTypes)s.ChosenRoleId)
                          ?? RoleManager.Instance?.GetRole((AmongUs.GameOptions.RoleTypes)s.ChosenRoleId)
                        : null;
                }
                catch {  }

                string teamLabel = "";
                Color roleColor = Color.white;
                if (recapMode == DraftRecapMode.Faction)
                {
                    teamLabel = DraftUiManager.GetBroadFaction(roleBehaviour).ToUpperInvariant() ?? "Unknown";
                    if (teamLabel != null && teamLabel.Contains("Impostor", System.StringComparison.OrdinalIgnoreCase))
                        ColorUtility.TryParseHtmlString("#FF0000", out roleColor);
                    else if (teamLabel != null && teamLabel.Contains("Neutral", System.StringComparison.OrdinalIgnoreCase))
                        ColorUtility.TryParseHtmlString("#717171", out roleColor);
                    else
                        ColorUtility.TryParseHtmlString("#5BD7E4", out roleColor);
                }
                else if (recapMode == DraftRecapMode.Alignment)
                {
                    teamLabel = DraftUiManager.GetTeamLabel(roleBehaviour).ToUpperInvariant() ?? "Unknown";
                    if (teamLabel != null && teamLabel.Contains("Impostor", System.StringComparison.OrdinalIgnoreCase))
                        ColorUtility.TryParseHtmlString("#FF0000", out roleColor);
                    else if (teamLabel != null && teamLabel.Contains("Neutral", System.StringComparison.OrdinalIgnoreCase))
                        ColorUtility.TryParseHtmlString("#717171", out roleColor);
                    else
                        ColorUtility.TryParseHtmlString("#5BD7E4", out roleColor);

                }
                else if (recapMode == DraftRecapMode.Role)
                {
                    teamLabel = roleBehaviour?.NiceName.ToUpperInvariant() ?? "Unknown";
                    roleColor = DraftUiManager.GetRoleColor(roleBehaviour);
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
