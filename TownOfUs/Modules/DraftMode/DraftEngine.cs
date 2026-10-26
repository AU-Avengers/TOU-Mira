using System.Collections;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using Reactor.Utilities;
using TownOfUs.Options;
using TownOfUs.Patches.DraftMode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Modules.DraftMode;

public static class DraftEngine
{
    private const float SubmissionGrace = 1f;
    private const float ReadyResendDelay = 2.5f;
    private const float MinShuffleWindow = 5f;
    private const float PostDraftCountdown = 5f;
    private const float OptionalHostileOfferChance = 0.5f;
    private const float DiversityPenalty = 0.3f;

    private static readonly Dictionary<int, DraftCopy> Picked = [];
    private static readonly Dictionary<int, List<ushort>> Offers = [];
    private static readonly Dictionary<int, HashSet<ushort>> Seen = [];
    private static readonly Dictionary<int, int> ShufflesUsed = [];
    private static readonly Dictionary<int, float> AnnouncedAt = [];
    private static readonly HashSet<int> Resent = [];
    private static readonly Dictionary<int, DraftSpreadOffer> SpreadOffers = [];
    private static readonly Dictionary<ushort, RoleBehaviour?> Roles = [];

    private static List<DraftCopy> _basePool = [];
    private static List<DraftCopy> _pool = [];
    private static DraftPlan _plan = null!;
    private static bool _running;
    private static int _session;
    private static int _turn;
    private static int _doubleDraftCrewSlotsRemaining;

    private static RoleOptions Opts => OptionGroupSingleton<RoleOptions>.Instance;

    private static RoleBehaviour? RoleOf(ushort id)
    {
        if (!Roles.TryGetValue(id, out var role))
        {
            Roles[id] = role = MiscUtils.GetRegisteredRole((RoleTypes)id);
        }

        return role;
    }

    private static bool Active(int session) => _running && _session == session && DraftManager.IsDraftActive;

    public static void Start(List<PlayerControl> players)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        Stop();
        _basePool = DraftRolePool.Build();
        if (_basePool.Count == 0)
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, "[DraftEngine] Role pool is empty, starting the game normally");
            Coroutines.Start(StartGame(0f));
            return;
        }

        var slots = Enumerable.Range(1, players.Count).OrderBy(_ => Random.value).ToList();
        var playerSlots = new Dictionary<byte, int>();
        for (var i = 0; i < players.Count; i++)
        {
            playerSlots[players[i].PlayerId] = slots[i];
        }

        Roles.Clear();
        _plan = DraftPlan.Create(players.Count, _basePool);
        _pool = [.. _basePool];
        _turn = 0;
        _doubleDraftCrewSlotsRemaining = 0;
        _running = true;

        DraftManager.Begin(playerSlots);
        DraftNetworkHelper.BroadcastStart(playerSlots);
        Coroutines.Start(Run(_session));
    }

    public static void Cancel()
    {
        Stop();
        DraftNetworkHelper.BroadcastCancelDraft();
    }

    public static void OnPick(byte playerId, int index, ushort roleId)
    {
        if (!_running) return;

        var state = DraftManager.GetStateForPlayer(playerId);
        if (state is not { IsPickingNow: true, HasPicked: false } || !Offers.TryGetValue(state.SlotNumber, out var offer)) return;

        var slot = state.SlotNumber;
        if (roleId == 0 && index == offer.Count && Opts.ShowRandomOption)
        {
            var offeredRoles = offer
                .Select(id => _pool.FirstOrDefault(copy => copy.RoleId == id))
                .Where(copy => copy != null)
                .Cast<DraftCopy>()
                .ToList();
            Commit(slot, Choose(offeredRoles, 1).FirstOrDefault(), false);
            return;
        }

        if (index < 0 || index >= offer.Count || offer[index] != roleId)
        {
            Announce(slot);
            return;
        }

        var copy = _pool.FirstOrDefault(x => x.RoleId == roleId);
        if (copy == null)
        {
            Offer(slot);
            return;
        }

        Commit(slot, copy, false);
    }

    public static void RequestShuffle(byte playerId)
    {
        if (!_running) return;

        var state = DraftManager.GetStateForPlayer(playerId);
        if (state is not { IsPickingNow: true, HasPicked: false }) return;

        var slot = state.SlotNumber;
        if (ShufflesUsed.GetValueOrDefault(slot) >= Opts.ShufflesPerPlayer) return;

        ShufflesUsed[slot] = ShufflesUsed.GetValueOrDefault(slot) + 1;
        if (DraftManager.TurnDuration <= MinShuffleWindow && DraftManager.TurnTimeLeft < MinShuffleWindow)
        {
            DraftManager.TurnTimeLeft = MinShuffleWindow;
        }

        Offer(slot);
    }

    private static void Stop()
    {
        _session++;
        _running = false;
        Picked.Clear();
        Offers.Clear();
        Seen.Clear();
        ShufflesUsed.Clear();
        AnnouncedAt.Clear();
        Resent.Clear();
        SpreadOffers.Clear();
        _doubleDraftCrewSlotsRemaining = 0;
    }

    private static IEnumerator Run(int session)
    {
        var concurrency = Math.Clamp(Opts.ConcurrentPicks, 1, 2);
        while (Active(session))
        {
            PruneGone();
            var pending = DraftManager.States.Where(s => !s.HasPicked).Select(s => s.SlotNumber).OrderBy(x => x);
            if (!pending.Any()) break;

            _turn++;
            var batch = pending.Take(concurrency).ToList();
            var forcedCrewSlots = pending
                .TakeLast(Math.Min(_doubleDraftCrewSlotsRemaining, pending.Count()))
                .ToHashSet();
            var scheduledSlots = pending.Where(slot => !forcedCrewSlots.Contains(slot)).ToList();
            var spreadOffers = _plan.AssignSpreadOffers(Picked.Values.ToList(), scheduledSlots.Count);
            for (var i = 0; i < batch.Count; i++)
            {
                var slot = batch[i];
                SpreadOffers.Remove(slot);
                if (forcedCrewSlots.Contains(slot))
                {
                    SpreadOffers[slot] = new DraftSpreadOffer(DraftFaction.Crewmate, true);
                }
                else if (spreadOffers[scheduledSlots.IndexOf(slot)] is { } spreadOffer)
                {
                    SpreadOffers[slot] = spreadOffer;
                }

                Seen[slot] = [];
                Offer(slot);
            }

            yield return Wait(batch, session);
            yield return new WaitForSeconds(0.5f);
        }

        if (Active(session))
        {
            Finish();
        }
    }

    private static IEnumerator Wait(List<int> batch, int session)
    {
        var graceUntil = 0f;
        while (Active(session) && batch.Any(IsOpen))
        {
            PruneGone();
            foreach (var slot in batch.Where(IsOpen))
            {
                var state = DraftManager.GetStateForSlot(slot);
                if (Offers[slot].Count == 0)
                {
                    AutoPick(slot);
                }
                else if (!state.IsPickerReady && Time.time - AnnouncedAt[slot] > ReadyResendDelay && Resent.Add(slot))
                {
                    Announce(slot);
                }
            }

            if (DraftManager.TurnTimeLeft > 0f)
            {
                graceUntil = 0f;
            }
            else if (graceUntil == 0f)
            {
                graceUntil = Time.time + SubmissionGrace;
            }

            if (DraftManager.TurnTimeLeft <= 0f)
            {
                foreach (var slot in batch.Where(IsOpen))
                {
                    var state = DraftManager.GetStateForSlot(slot);
                    if (DraftManager.IsBot(state.PlayerId) || Time.time >= graceUntil)
                    {
                        AutoPick(slot);
                    }
                }
            }

            yield return null;
        }
    }

    private static bool IsOpen(int slot) => DraftManager.GetStateForSlot(slot) is { HasPicked: false };

    private static void PruneGone()
    {
        foreach (var state in DraftManager.States.Where(s => DraftManager.IsPlayerGone(s.PlayerId)))
        {
            Drop(state.SlotNumber);
        }
    }

    private static void Drop(int slot)
    {
        if (Picked.Remove(slot, out var pickedCopy) && pickedCopy.Weight > 1)
        {
            var extraSeats = pickedCopy.Weight - 1;
            _plan.RemoveCrewSeats(extraSeats);
            _doubleDraftCrewSlotsRemaining = Math.Max(0, _doubleDraftCrewSlotsRemaining - extraSeats);
        }

        Offers.Remove(slot);
        SpreadOffers.Remove(slot);
        DraftManager.Remove(slot);
        _plan.DropSeat([.. Picked.Values]);
        RebuildPool();
        Revalidate();
    }

    private static void RebuildPool()
    {
        _pool = [.. _basePool];
        foreach (var copy in Picked.Values)
        {
            Take(copy);
        }
    }

    private static void Take(DraftCopy copy)
    {
        var index = _pool.FindIndex(x => x.RoleId == copy.RoleId);
        if (index >= 0)
        {
            _pool.RemoveAt(index);
        }

        _pool.RemoveAll(x => Clashes(copy.RoleId, x.RoleId));
    }

    private static bool Clashes(ushort a, ushort b)
    {
        return a != b && RoleOf(a) is { } roleA && RoleOf(b) is { } roleB && MiscUtils.AreRolesExclusive(roleA, roleB);
    }

    private static IEnumerable<DraftCopy> Without(params DraftCopy[] taken)
    {
        var skipped = new HashSet<ushort>();
        foreach (var x in _pool)
        {
            if (taken.Any(t => t.RoleId == x.RoleId && skipped.Add(t.RoleId)))
            {
                continue;
            }

            if (!taken.Any(t => Clashes(t.RoleId, x.RoleId)))
            {
                yield return x;
            }
        }
    }

    private static bool IsLegal(DraftCopy copy, List<DraftCopy> picks, List<ushort> concurrent)
    {
        if (!_plan.CanComplete([.. picks, copy], Without(copy)))
        {
            return false;
        }

        foreach (var id in concurrent.Where(id => id != copy.RoleId))
        {
            var other = _pool.FirstOrDefault(x => x.RoleId == id);
            if (other == null)
            {
                continue;
            }

            if (Clashes(copy.RoleId, id) || !_plan.CanComplete([.. picks, copy, other], Without(copy, other)))
            {
                return false;
            }
        }

        return true;
    }

    private static List<DraftCopy> Legal(int slot, bool respectReserved)
    {
        var concurrent = Offers.Where(o => o.Key != slot).SelectMany(o => o.Value).ToList();
        var picks = Picked.Values.ToList();
        return _pool.GroupBy(x => x.RoleId)
            .Where(g => !respectReserved || g.Count() > concurrent.Count(id => id == g.Key))
            .Select(g => g.First())
            .Where(c => IsLegal(c, picks, concurrent))
            .ToList();
    }

    private static List<DraftCopy> Offerable(int slot)
    {
        var legal = Legal(slot, true);
        var picks = Picked.Values.ToList();
        var spreadOffer = SpreadOffers.GetValueOrDefault(slot);
        var factionChoices = spreadOffer == null
            ? []
            : legal.Where(copy => DraftPlan.IsFaction(copy.Alignment, spreadOffer.Faction)).ToList();

        if (spreadOffer != null)
        {
            if (factionChoices.Count == 0)
            {
                SpreadOffers.Remove(slot);
                return legal;
            }

            var otherForcedOffers = SpreadOffers
                .Count(pair => pair.Key != slot &&
                    pair.Value.Faction == spreadOffer.Faction && !Offers.ContainsKey(pair.Key) && IsOpen(pair.Key));
            var maxOptions = Math.Max(1, factionChoices.Count - otherForcedOffers);
            return Choose(factionChoices, Math.Min(Opts.OfferedRolesCount, maxOptions));
        }

        var candidates = legal.Where(copy =>
            !DraftPlan.IsFaction(copy.Alignment, DraftFaction.Impostor) &&
            !DraftPlan.IsFaction(copy.Alignment, DraftFaction.Neutral)).ToList();
        if (Random.value < OptionalHostileOfferChance)
        {
            var optionalFactionChoices = legal.Where(copy =>
            {
                var faction = DraftPlan.IsFaction(copy.Alignment, DraftFaction.Impostor)
                    ? DraftFaction.Impostor
                    : DraftFaction.Neutral;
                return DraftPlan.IsFaction(copy.Alignment, faction) &&
                       _plan.RemainingSpreadNeed(picks, faction) == 0;
            }).ToList();
            var nudge = Choose(optionalFactionChoices, 1).FirstOrDefault();
            if (nudge != null)
            {
                candidates.Add(nudge);
            }
        }

        if (candidates.Count > 0)
        {
            return candidates;
        }

        if (legal.Count == 0)
        {
            return legal;
        }

        var neededFaction = new[] { DraftFaction.Impostor, DraftFaction.Neutral }
            .Where(faction => _plan.RemainingSpreadNeed(picks, faction) > 0)
            .OrderByDescending(faction => _plan.RemainingSpreadNeed(picks, faction))
            .FirstOrDefault();
        var neededChoices = legal
            .Where(copy => DraftPlan.IsFaction(copy.Alignment, neededFaction))
            .ToList();
        return neededChoices.Count > 0 ? Choose(neededChoices, 1) : legal;
    }

    private static List<DraftCopy> Choose(List<DraftCopy> candidates, int count)
    {
        var chosen = new List<DraftCopy>();
        while (chosen.Count < count)
        {
            var remaining = candidates.Where(c => chosen.All(x => x.RoleId != c.RoleId)).ToList();
            if (remaining.Count == 0) break;

            var weights = remaining
                .Select(c => c.Chance * Mathf.Pow(DiversityPenalty, chosen.Count(x => x.Alignment == c.Alignment)))
                .ToList();
            var roll = Random.value * weights.Sum();
            var index = 0;
            while (index < remaining.Count - 1 && (roll -= weights[index]) > 0f)
            {
                index++;
            }

            chosen.Add(remaining[index]);
        }

        return chosen;
    }

    private static void TopUp(List<DraftCopy> chosen, List<DraftCopy> source, int count)
    {
        if (chosen.Count >= count) return;

        var rest = source.Where(x => chosen.All(c => c.RoleId != x.RoleId)).ToList();
        chosen.AddRange(Choose(rest, count - chosen.Count));
    }

    private static List<DraftCopy> RandomCrewBucket(int slot, bool respectReserved)
    {
        return Legal(slot, respectReserved).Where(x => DraftRolePool.Crew.Contains(x.Alignment)).ToList();
    }

    private static void Offer(int slot)
    {
        var seen = Seen.TryGetValue(slot, out var known) ? known : Seen[slot] = [];
        var count = Math.Max(1, Opts.OfferedRolesCount);

        var preferred = Offerable(slot);
        var chosen = Choose(preferred.Where(x => !seen.Contains(x.RoleId)).ToList(), count);
        if (chosen.Count < count)
        {
            var fallback = RandomCrewBucket(slot, true);
            TopUp(chosen, fallback.Where(x => !seen.Contains(x.RoleId)).ToList(), count);
            TopUp(chosen, preferred, count);
            TopUp(chosen, fallback, count);
            TopUp(chosen, RandomCrewBucket(slot, false), count);
        }

        if (chosen.Count < count)
        {
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                $"[DraftEngine] Only {chosen.Count}/{count} legal roles exist for slot {slot}");
        }

        seen.UnionWith(chosen.Select(x => x.RoleId));
        Offers[slot] = chosen.Select(x => x.RoleId).ToList();
        Announce(slot);
    }

    private static void Announce(int slot)
    {
        var state = DraftManager.GetStateForSlot(slot);
        state.IsPickerReady = false;
        AnnouncedAt[slot] = Time.time;
        DraftNetworkHelper.SendTurnAnnouncement(slot, state.PlayerId, Offers[slot], _turn);
    }

    private static void Revalidate()
    {
        foreach (var slot in Offers.Keys)
        {
            var legal = Legal(slot, false).Select(x => x.RoleId).ToHashSet();
            if (IsOpen(slot) && Offers[slot].Any(id => !legal.Contains(id)))
            {
                Offer(slot);
            }
        }
    }

    private static void AutoPick(int slot)
    {
        var offer = Offers.GetValueOrDefault(slot) ?? [];
        var roleId = offer.Count > 0 ? offer[Random.RandomRangeInt(0, offer.Count)] : (ushort)0;
        var copy = _pool.FirstOrDefault(x => offer.Count > 0 && x.RoleId == roleId);
        Commit(slot, copy ?? Choose(Offerable(slot), 1).FirstOrDefault(), true);
    }

    private static void Commit(int slot, DraftCopy? copy, bool timedOut)
    {
        var state = DraftManager.GetStateForSlot(slot);
        if (state == null || state.HasPicked) return;

        var fulfillsCrewSeat = SpreadOffers.TryGetValue(slot, out var spreadOffer) &&
            spreadOffer.Faction == DraftFaction.Crewmate;
        state.ChosenRoleId = copy?.RoleId ?? (ushort)RoleTypes.Crewmate;
        state.HasPicked = true;
        Offers.Remove(slot);
        SpreadOffers.Remove(slot);

        if (fulfillsCrewSeat)
        {
            _doubleDraftCrewSlotsRemaining = Math.Max(0, _doubleDraftCrewSlotsRemaining - 1);
        }

        if (copy != null)
        {
            Picked[slot] = copy;
            Take(copy);
            if (copy.Weight > 1)
            {
                var extraSeats = copy.Weight - 1;
                _plan.AddCrewSeats(extraSeats, _basePool);
                _doubleDraftCrewSlotsRemaining += extraSeats;
            }
        }

        DraftNetworkHelper.BroadcastPickConfirmed(slot, state.ChosenRoleId, timedOut);
        Revalidate();
    }

    private static void Finish()
    {
        _running = false;
        var mode = Opts.DraftRecap;
        var entries = DraftManager.States.OrderBy(s => s.SlotNumber).Select(s => Recap(s, mode)).ToList();

        DraftApplier.StorePendingDraftStates(DraftManager.States);
        DraftNetworkHelper.BroadcastRecap(entries, mode);
        Coroutines.Start(StartGame(mode != DraftRecapMode.Nothing ? 6f : 0f));
    }

    private static RecapEntry Recap(DraftSlotState state, DraftRecapMode mode)
    {
        var name = DraftRolePool.GetRoleNameFromId(state.ChosenRoleId) ?? "Unknown";
        var role = DraftUiManager.ResolveRole(state.ChosenRoleId);
        if (!role)
        {
            return new RecapEntry(state.SlotNumber, name, name.ToUpperInvariant(), "FFFFFF");
        }

        var label = mode switch
        {
            DraftRecapMode.Role => name,
            DraftRecapMode.Alignment => MiscUtils.GetParsedRoleAlignment(role),
            _ => DraftUiManager.GetTeamLabel(role)
        };
        var color = mode == DraftRecapMode.Role ? role.TeamColor : MiscUtils.GetRoleFactionColor(role, true);
        return new RecapEntry(state.SlotNumber, name, label.ToUpperInvariant(), ColorUtility.ToHtmlStringRGB(color));
    }

    private static IEnumerator StartGame(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        var client = AmongUsClient.Instance;
        if (!client.AmHost || GameStartManager.Instance == null || client.GameState != InnerNet.InnerNetClient.GameStates.Joined)
        {
            yield break;
        }

        GameStartPatch.SkipIntercept = true;
        GameStartPatch.PostDraftCountdownActive = true;
        var manager = GameStartManager.Instance;
        var minPlayers = manager.MinPlayers;

        manager.ResetStartState();
        manager.MinPlayers = 1;
        manager.startState = GameStartManager.StartingStates.Countdown;

        var remaining = PostDraftCountdown;
        while (client.GameState == InnerNet.InnerNetClient.GameStates.Joined && remaining > 0f)
        {
            remaining -= Time.deltaTime;
            manager.countDownTimer = Mathf.Max(0f, remaining);
            yield return null;
        }

        if (client.GameState == InnerNet.InnerNetClient.GameStates.Joined && GameStartManager.Instance != null)
        {
            GameStartManager.Instance.ReallyBegin(false);
        }

        GameStartManager.Instance?.MinPlayers = minPlayers;

        GameStartPatch.SkipIntercept = false;
        GameStartPatch.PostDraftCountdownActive = false;
    }
}
