using MiraAPI.GameOptions;
using TownOfUs.Options;
using TownOfUs.Roles;
using Random = UnityEngine.Random;
namespace TownOfUs.Modules.DraftMode;

public sealed record DraftSpreadOffer(DraftFaction Faction, bool Forced);

public sealed class DraftPlan
{
    private readonly List<RoleAlignment[]> _seats;
    private readonly Dictionary<RoleAlignment, int> _caps;
    private readonly Dictionary<RoleAlignment, int> _extraCrewCaps = [];

    private DraftPlan(List<RoleAlignment[]> seats, Dictionary<RoleAlignment, int> caps)
    {
        _seats = seats;
        _caps = caps;
    }

    public int SeatCount => _seats.Count;

    public static DraftPlan Create(int players, List<DraftCopy> pool)
    {
        return OptionGroupSingleton<RoleOptions>.Instance.UseRoleListForPool
            ? FromRoleList(players)
            : FromManualAmounts(players, pool);
    }

    private static DraftPlan FromRoleList(int players)
    {
        var slots = OptionGroupSingleton<RoleDraftRoleListOptions>.Instance.Slot;
        var seats = new List<RoleAlignment[]>();
        for (var i = 0; i < Math.Min(players, slots.Count); i++)
        {
            seats.Add(Alignments(slots[i].Value));
        }

        while (seats.Count < players)
        {
            var bucket = Random.RandomRangeInt(0, 4) == 0
                ? RoleListOption.CrewRandom
                : RoleListOption.NonImp;
            seats.Add(Alignments(bucket));
        }

        Shuffle(seats);
        return new DraftPlan(seats, []);
    }

    private static DraftPlan FromManualAmounts(int players, List<DraftCopy> pool)
    {
        var crew = OptionGroupSingleton<RoleDraftCrewOptions>.Instance;
        var imp = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
        var neut = OptionGroupSingleton<RoleDraftNeutOptions>.Instance;

        var caps = new Dictionary<RoleAlignment, int>
        {
            [RoleAlignment.CrewmateInvestigative] = (int)crew.MaxCrewInvestigative.Value,
            [RoleAlignment.CrewmateKilling] = (int)crew.MaxCrewKilling.Value,
            [RoleAlignment.CrewmatePower] = (int)crew.MaxCrewPower.Value,
            [RoleAlignment.CrewmateProtective] = (int)crew.MaxCrewProtective.Value,
            [RoleAlignment.CrewmateSupport] = (int)crew.MaxCrewSupport.Value,
            [RoleAlignment.ImpostorConcealing] = (int)imp.MaxImpConcealing.Value,
            [RoleAlignment.ImpostorKilling] = (int)imp.MaxImpKilling.Value,
            [RoleAlignment.ImpostorPower] = (int)imp.MaxImpPower.Value,
            [RoleAlignment.ImpostorSupport] = (int)imp.MaxImpSupport.Value,
            [RoleAlignment.NeutralBenign] = (int)neut.MaxNeutBenign.Value,
            [RoleAlignment.NeutralEvil] = (int)neut.MaxNeutEvil.Value,
            [RoleAlignment.NeutralKilling] = (int)neut.MaxNeutKilling.Value,
            [RoleAlignment.NeutralOutlier] = (int)neut.MaxNeutOutlier.Value
        };

        var impSeats = Math.Min((int)imp.MaxImpostors.Value, Capacity(DraftRolePool.Impostor, caps, pool));
        var neutSeats = Math.Min((int)neut.MaxNeutrals.Value, Capacity(DraftRolePool.Neutral, caps, pool));
        impSeats = Math.Clamp(impSeats, 0, Math.Max(0, players - 1));
        neutSeats = Math.Clamp(neutSeats, 0, Math.Max(0, players - impSeats - 1));
        var crewSeats = players - impSeats - neutSeats;

        if (Capacity(DraftRolePool.Crew, caps, pool) < crewSeats)
        {
            foreach (var alignment in DraftRolePool.Crew)
            {
                caps.Remove(alignment);
            }
        }

        var seats = new List<RoleAlignment[]>();
        seats.AddRange(Enumerable.Repeat(DraftRolePool.Impostor, impSeats));
        seats.AddRange(Enumerable.Repeat(DraftRolePool.Neutral, neutSeats));
        seats.AddRange(Enumerable.Repeat(DraftRolePool.Crew, crewSeats));

        Shuffle(seats);
        return new DraftPlan(seats, caps);
    }

    public List<DraftSpreadOffer?> AssignSpreadOffers(
        IReadOnlyList<DraftCopy> picks,
        int activePicks)
    {
        var deficits = new Dictionary<DraftFaction, int>
        {
            [DraftFaction.Impostor] = RequiredSeats(DraftFaction.Impostor) -
                picks.Where(x => IsFaction(x.Alignment, DraftFaction.Impostor)).Sum(x => x.Weight),
            [DraftFaction.Neutral] = RequiredSeats(DraftFaction.Neutral) -
                picks.Where(x => IsFaction(x.Alignment, DraftFaction.Neutral)).Sum(x => x.Weight)
        };
        foreach (var faction in deficits.Keys.ToList())
        {
            deficits[faction] = Math.Max(0, deficits[faction]);
        }

        var offers = Enumerable.Repeat<DraftSpreadOffer?>(null, activePicks).ToList();
        var slots = Enumerable.Range(0, activePicks).ToList();
        Shuffle(slots);
        var offersToSchedule = Math.Min(deficits.Values.Sum(), activePicks);
        for (var i = 0; i < offersToSchedule; i++)
        {
            var deficit = deficits.Values.Sum();
            var roll = Random.RandomRangeInt(0, deficit);
            var factionToLock = roll < deficits[DraftFaction.Impostor]
                ? DraftFaction.Impostor
                : DraftFaction.Neutral;
            offers[slots[i]] = new DraftSpreadOffer(factionToLock, true);
            deficits[factionToLock]--;
        }

        return offers;
    }

    public static bool IsFaction(RoleAlignment alignment, DraftFaction faction) => faction switch
    {
        DraftFaction.Impostor => DraftRolePool.Impostor.Contains(alignment),
        DraftFaction.Neutral => DraftRolePool.Neutral.Contains(alignment),
        DraftFaction.Crewmate => DraftRolePool.Crew.Contains(alignment),
        _ => false
    };

    public int RemainingSpreadNeed(IReadOnlyList<DraftCopy> picks, DraftFaction faction)
    {
        var pickedWeight = picks
            .Where(copy => IsFaction(copy.Alignment, faction))
            .Sum(copy => copy.Weight);
        return Math.Max(0, RequiredSeats(faction) - pickedWeight);
    }

    private int RequiredSeats(DraftFaction faction)
    {
        return _seats.Count(seat =>
            seat.Length > 0 && seat.All(alignment => IsFaction(alignment, faction)));
    }

    public void AddCrewSeats(int count, IReadOnlyList<DraftCopy> pool)
    {
        if (_caps.Count > 0)
        {
            var alignment = DraftRolePool.Crew
                .OrderByDescending(candidate => pool.Count(copy => copy.Alignment == candidate) - _caps.GetValueOrDefault(candidate))
                .ThenByDescending(candidate => pool.Count(copy => copy.Alignment == candidate))
                .FirstOrDefault();
            _caps[alignment] = _caps.GetValueOrDefault(alignment) + count;
            _extraCrewCaps[alignment] = _extraCrewCaps.GetValueOrDefault(alignment) + count;
        }

        for (var i = 0; i < count; i++)
        {
            _seats.Add(DraftRolePool.Crew);
        }
    }

    public void RemoveCrewSeats(int count)
    {
        while (count-- > 0)
        {
            var capAlignment = _extraCrewCaps.FirstOrDefault(pair => pair.Value > 0).Key;
            if (_extraCrewCaps.GetValueOrDefault(capAlignment) > 0)
            {
                _extraCrewCaps[capAlignment]--;
                _caps[capAlignment]--;
            }

            var index = _seats.FindLastIndex(seat => seat.SequenceEqual(DraftRolePool.Crew));
            if (index < 0) return;
            _seats.RemoveAt(index);
        }
    }

    private static void Shuffle<T>(IList<T> items)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = Random.RandomRangeInt(0, i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }

    private static int Capacity(RoleAlignment[] alignments, Dictionary<RoleAlignment, int> caps, List<DraftCopy> pool)
    {
        return alignments.Sum(a => Math.Min(caps[a], pool.Count(x => x.Alignment == a)));
    }

    public bool CanComplete(IReadOnlyList<DraftCopy> picks, IEnumerable<DraftCopy> supply)
    {
        var candidates = new List<RoleAlignment>();
        foreach (var group in picks.GroupBy(x => x.Alignment))
        {
            var weight = group.Sum(x => x.Weight);
            if (weight > CapFor(group.Key))
            {
                return false;
            }

            candidates.AddRange(Enumerable.Repeat(group.Key, weight));
        }

        var mandatory = candidates.Count;
        foreach (var group in supply.GroupBy(x => x.Alignment))
        {
            var room = CapFor(group.Key) - picks.Where(x => x.Alignment == group.Key).Sum(x => x.Weight);
            candidates.AddRange(Enumerable.Repeat(group.Key, Math.Max(0, Math.Min(room, group.Count()))));
        }

        var owner = Enumerable.Repeat(-1, _seats.Count).ToArray();
        var matched = 0;
        for (var i = 0; i < candidates.Count; i++)
        {
            if (i >= mandatory && matched >= _seats.Count)
            {
                break;
            }

            var assigned = TryAssign(i, candidates, owner, new bool[_seats.Count]);
            if (i < mandatory && !assigned)
            {
                return false;
            }

            if (assigned)
            {
                matched++;
            }
        }

        return matched >= _seats.Count;
    }

    public bool DropSeat(IReadOnlyList<DraftCopy> picks)
    {
        var picked = picks.SelectMany(x => Enumerable.Repeat(x.Alignment, x.Weight)).ToList();
        foreach (var seat in _seats.OrderByDescending(x => x.Length).ToList())
        {
            var index = _seats.IndexOf(seat);
            var rest = _seats.Where((_, i) => i != index).ToList();
            var owner = Enumerable.Repeat(-1, rest.Count).ToArray();
            if (picked.Select((_, i) => i).All(i => TryAssign(i, picked, owner, new bool[rest.Count], rest)))
            {
                _seats.RemoveAt(index);
                return true;
            }
        }

        return false;
    }

    private int CapFor(RoleAlignment alignment) => _caps.GetValueOrDefault(alignment, int.MaxValue);

    private bool TryAssign(int candidate, List<RoleAlignment> candidates, int[] owner, bool[] seen, List<RoleAlignment[]>? seats = null)
    {
        seats ??= _seats;
        for (var s = 0; s < seats.Count; s++)
        {
            if (seen[s] || !seats[s].Contains(candidates[candidate]))
            {
                continue;
            }

            seen[s] = true;
            if (owner[s] == -1 || TryAssign(owner[s], candidates, owner, seen, seats))
            {
                owner[s] = candidate;
                return true;
            }
        }

        return false;
    }

    private static RoleAlignment[] Alignments(RoleListOption bucket)
    {
        RoleAlignment[] crew = DraftRolePool.Crew;
        RoleAlignment[] imp = DraftRolePool.Impostor;
        RoleAlignment[] neut = DraftRolePool.Neutral;

        return bucket switch
        {
            RoleListOption.CrewInvest => [RoleAlignment.CrewmateInvestigative],
            RoleListOption.CrewKilling => [RoleAlignment.CrewmateKilling],
            RoleListOption.CrewProtective => [RoleAlignment.CrewmateProtective],
            RoleListOption.CrewPower => [RoleAlignment.CrewmatePower],
            RoleListOption.CrewSupport => [RoleAlignment.CrewmateSupport],
            RoleListOption.CrewCommon => [RoleAlignment.CrewmateInvestigative, RoleAlignment.CrewmateProtective, RoleAlignment.CrewmateSupport],
            RoleListOption.CrewSpecial => [RoleAlignment.CrewmateKilling, RoleAlignment.CrewmatePower],
            RoleListOption.CrewRandom => crew,
            RoleListOption.NeutBenign => [RoleAlignment.NeutralBenign],
            RoleListOption.NeutEvil => [RoleAlignment.NeutralEvil],
            RoleListOption.NeutKilling => [RoleAlignment.NeutralKilling],
            RoleListOption.NeutOutlier => [RoleAlignment.NeutralOutlier],
            RoleListOption.NeutCommon => [RoleAlignment.NeutralBenign, RoleAlignment.NeutralEvil],
            RoleListOption.NeutSpecial => [RoleAlignment.NeutralKilling, RoleAlignment.NeutralOutlier],
            RoleListOption.NeutWildcard => [RoleAlignment.NeutralBenign, RoleAlignment.NeutralEvil, RoleAlignment.NeutralOutlier],
            RoleListOption.NeutRandom => neut,
            RoleListOption.ImpConceal => [RoleAlignment.ImpostorConcealing],
            RoleListOption.ImpKilling => [RoleAlignment.ImpostorKilling],
            RoleListOption.ImpPower => [RoleAlignment.ImpostorPower],
            RoleListOption.ImpSupport => [RoleAlignment.ImpostorSupport],
            RoleListOption.ImpCommon => [RoleAlignment.ImpostorConcealing, RoleAlignment.ImpostorSupport],
            RoleListOption.ImpSpecial => [RoleAlignment.ImpostorKilling, RoleAlignment.ImpostorPower],
            RoleListOption.ImpRandom => imp,
            RoleListOption.NonImp => [.. crew, .. neut],
            _ => [.. crew, .. imp, .. neut]
        };
    }
}
