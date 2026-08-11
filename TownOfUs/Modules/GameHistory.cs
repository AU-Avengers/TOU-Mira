using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modules;

public record PlayerEvent(byte PlayerId, float Unix, Vector3 Position);

public record DeadPlayer(byte KillerId, byte VictimId, DateTime KillTime);

public sealed record PlayerStats(string Name, byte PlayerId, NetworkedPlayerInfo PlayerInfo, StoredPlayerState State = StoredPlayerState.Alive)
{
    public string PlayerName { get; private set; } = Name;
    public byte PlayerId { get; private set; } = PlayerId;
    public NetworkedPlayerInfo PlayerInfo { get; private set; } = PlayerInfo;
    public List<RoleBehaviour> TrackedRoles { get; internal set; } = [];
    public List<RoleBehaviour> LivingRoles { get; internal set; } = [];
    public List<BaseModifier> LastKnownModifiers { get; internal set; } = [];
    public StoredPlayerState PlayerState { get; set; } = State;
    public string DeathString { get; set; } = TouLocale.Get("Alive");
    public int RoundOfDeath { get; set; } = -1;

    public int CorrectKills { get; set; }
    public int IncorrectKills { get; set; }
    public int CorrectAssassinKills { get; set; }
    public int IncorrectAssassinKills { get; set; }
}

public enum StoredPlayerState
{
    Alive,
    Revived,
    Dead,
    Disconnected,
    Spectating
}

// body report class for when medic/Forensic reports a body
public sealed record BodyReport
{
    public PlayerControl? Killer { get; init; }
    public PlayerControl? Reporter { get; init; }
    public PlayerControl? Body { get; init; }
    public float KillAge { get; init; }

    public static string ParseMedicReport(BodyReport br)
    {
        var reportColorDuration = OptionGroupSingleton<MedicOptions>.Instance.MedicReportColorDuration;
        var reportNameDuration = OptionGroupSingleton<MedicOptions>.Instance.MedicReportNameDuration;
        var text = TouLocale.GetParsed("TouRoleMedicBodyError");
        if (br.Killer != null)
        {
            if (br.KillAge > reportColorDuration * 1000 && reportColorDuration > 0)
            {
                text = TouLocale.GetParsed("TouRoleMedicBodyOld");
            }
            else if (br.Killer.PlayerId == br.Body?.PlayerId)
            {
                text = TouLocale.GetParsed("TouRoleMedicBodySuicide");
            }
            else if (br.KillAge < reportNameDuration * 1000)
            {
                text = TouLocale.GetParsed("TouRoleMedicBodyKillerName").Replace("<player>", br.Killer.Data.PlayerName);
            }
            else
            {
                var typeOfColor = MedicRole.GetColorTypeForPlayer(br.Killer.Data.DefaultOutfit.ColorId);
                text = TouLocale.GetParsed((typeOfColor == "lighter") ? "TouRoleMedicBodyKillerLightColor" : "TouRoleMedicBodyKillerDarkColor");
            }
        }

        text = text.Replace("<time>", Math.Round(br.KillAge / 1000).ToString(TownOfUsPlugin.Culture));

        return text;
    }

    public static string ParseForensicReport(BodyReport br)
    {
        var text = TouLocale.GetParsed("TouRoleForensicBodyError");
        if (br.Killer != null)
        {
            if (br.KillAge > OptionGroupSingleton<ForensicOptions>.Instance.ForensicFactionDuration * 1000 &&
                OptionGroupSingleton<ForensicOptions>.Instance.ForensicFactionDuration > 0)
            {
                text = TouLocale.GetParsed("TouRoleForensicBodyOld");
            }
            else if (br.Killer!.PlayerId == br.Body!.PlayerId)
            {
                text = TouLocale.GetParsed("TouRoleForensicBodySuicide");
            }
            else if (br.KillAge < OptionGroupSingleton<ForensicOptions>.Instance.ForensicRoleDuration * 1000)
            {
                // if the killer died, they would still appear correctly here
                var role = br.Killer.GetRoleWhenAlive();
                if (br.Killer.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) is ICachedRole cacheMod)
                {
                    role = cacheMod.CachedRole;
                }

                text = TouLocale.GetParsed("TouRoleForensicBodyKillerRole").Replace("<role>",
                    $"#{role.GetRoleName().ToLowerInvariant().Replace(" ", "-")})");
            }

            else if (br.Killer.IsNeutral())
            {
                text = TouLocale.GetParsed("TouRoleForensicBodyKillerNeutral");
            }

            else if (br.Killer.IsCrewmate())
            {
                text = TouLocale.GetParsed("TouRoleForensicBodyKillerCrewmate");
            }
            else
            {
                text = TouLocale.GetParsed("TouRoleForensicBodyKillerImpostor");
            }

        }

        text = text.Replace("<time>", Math.Round(br.KillAge / 1000).ToString(TownOfUsPlugin.Culture));

        return text;
    }
}

public static class GameHistory
{
    public static readonly Dictionary<byte, RoleBehaviour> RoleDictionary = [];
    public static readonly List<KeyValuePair<byte, RoleBehaviour>> RoleHistory = [];
    public static readonly Dictionary<byte, RoleBehaviour> RoleWhenAlive = [];

    // Unused for now
    public static readonly List<PlayerEvent> PlayerEvents = []; //local player events
    public static readonly List<DeadPlayer> KilledPlayers = [];
    public static readonly List<(byte, DeathReason)> DeathHistory = [];
    public static readonly Dictionary<byte, PlayerStats> PlayerStats = [];
    public static string EndGameSummary = string.Empty;
    public static string EndGameSummarySimple = string.Empty;
    public static string EndGameSummaryAdvanced = string.Empty;
    public static string WinningFaction = string.Empty;
    public static IEnumerable<RoleBehaviour> AllRoles => [.. RoleDictionary.Values];

    public static void RegisterRole(PlayerControl player, RoleBehaviour role, bool clean = false)
    {
        //Message($"RegisterRole - player: '{player.Data.PlayerName}', role: '{role.GetRoleName()}'");

        if (!PlayerStats.TryGetValue(player.PlayerId, out var stats))
        {
            PlayerStats.Add(player.PlayerId,
                new PlayerStats(player.Data.PlayerName, player.PlayerId, player.CachedPlayerData,
                    role.IsDead ? StoredPlayerState.Dead : StoredPlayerState.Alive));
            stats = PlayerStats[player.PlayerId];
        }

        if (clean)
        {
            RoleHistory.RemoveAll(x => x.Key == player.PlayerId);
            stats.TrackedRoles = [];
            stats.LivingRoles = [];
        }

        RoleDictionary.Remove(player.PlayerId);
        RoleDictionary.Add(player.PlayerId, role);

        RoleHistory.Add(KeyValuePair.Create(player.PlayerId, role));
        stats.TrackedRoles.Add(role);

        if (!role.IsDead)
        {
            RoleWhenAlive.Remove(player.PlayerId);
            RoleWhenAlive.Add(player.PlayerId, role);
            stats.LivingRoles.Add(role);
        }
    }

    public static void AddMurder(PlayerControl killer, PlayerControl victim)
    {
        var deadBody = new DeadPlayer(killer.PlayerId, victim.PlayerId, DateTime.UtcNow);

        KilledPlayers.Add(deadBody);
    }

    public static void ClearMurder(PlayerControl player)
    {
        var instance = KilledPlayers
            .Where(x => x.VictimId == player.PlayerId)
            .OrderByDescending(x => x.KillTime)
            .FirstOrDefault();

        if (instance == null)
        {
            return;
        }

        KilledPlayers.Remove(instance);
    }

    public static void ClearAll()
    {
        RoleDictionary.Do(x =>
        {
            if (x.Value != null && x.Value.gameObject != null)
            {
                Object.Destroy(x.Value.gameObject);
            }
        });

        RoleDictionary.Clear();

        RoleHistory.Do(x =>
        {
            if (x.Value != null && x.Value.gameObject != null)
            {
                Object.Destroy(x.Value.gameObject);
            }
        });

        RoleHistory.Clear();

        RoleWhenAlive.Do(x =>
        {
            if (x.Value != null && x.Value.gameObject != null)
            {
                Object.Destroy(x.Value.gameObject);
            }
        });

        RoleWhenAlive.Clear();

        KilledPlayers.Clear();
        DeathHistory.Clear();
        PlayerStats.Clear();
        PlayerEvents.Clear();
        EndGamePatches.EndGameData.DisconnectedPlayerRecords.Clear();
        EndGamePatches.ContainedMeetingData.Clear();
    }

    public static RoleBehaviour GetRoleWhenAlive(this PlayerControl player)
    {
        //var role = RoleHistory.LastOrDefault(x => x.Key == player.PlayerId && !x.Value.IsDead);
        //return role.Value != null ? role.Value : null;

        if (RoleWhenAlive.TryGetValue(player.PlayerId, out var role))
        {
            return role;
        }

        if (!player.Data.IsDead)
        {
            return player.Data.Role;
        }

        var role2 = player.Data.RoleWhenAlive;

        if (role2.HasValue)
        {
            return RoleManager.Instance.GetRole(role2.Value);
        }

        return player.Data.Role;
    }

    public static int RoleCount<T>() where T : RoleBehaviour
    {
        return RoleWhenAlive.Count(x => x.Value is T);
    }
}