using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
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
    public RoleBehaviour DisplayedRole { get; internal set; }
    public List<BaseModifier> LastKnownModifiers { get; internal set; } = [];
    public StoredPlayerState PlayerState { get; set; } = State;
    public bool LockDeathInfo { get; set; }
    public string DeathString { get; set; } = TouLocale.Get("Alive");
    public int RoundOfDeath { get; set; } = -1;
    public bool DiedThisRound { get; set; }
    public string KilledBy { get; set; } = string.Empty;
    public string ExtendedCauseOfDeath { get; set; } = string.Empty;
    public bool IsSpectator { get; set; }

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
    Disconnected
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

    [MethodRpc((uint)TownOfUsRpc.UpdateDeathHandler)]
    public static void RpcUpdateDeathHandler(PlayerControl player, string causeOfDeath = "null", int roundOfDeath = -1,
        DeathHandlerOverride diedThisRound = DeathHandlerOverride.Ignore, string killedBy = "null",
        DeathHandlerOverride lockInfo = DeathHandlerOverride.Ignore)
    {
        UpdatePlayerDeathData(player.PlayerId, causeOfDeath, 0, roundOfDeath, diedThisRound, killedBy, lockInfo: lockInfo);
    }

    [MethodRpc((uint)TownOfUsRpc.UpdateLocalDeathHandler)]
    public static void RpcUpdateLocalDeathHandler(PlayerControl player, PlayerControl killedBy,
        string causeOfDeath = "null", int roundOfDeath = -1,
        DeathHandlerOverride diedThisRound = DeathHandlerOverride.Ignore, string killedByString = "null",
        DeathHandlerOverride lockInfo = DeathHandlerOverride.Ignore)
    {
        var localizedCod = TouLocale.Get(causeOfDeath).Contains("STRMISS") ? "null" : TouLocale.Get(causeOfDeath);
        var localizedKilledBy = (TouLocale.GetParsed(killedByString).Contains("STRMISS") || killedBy == player)
            ? "null"
            : TouLocale.GetParsed(killedByString).Replace("<player>", killedBy.Data.PlayerName);
        UpdatePlayerDeathData(player.PlayerId, localizedCod, 0, roundOfDeath, diedThisRound, localizedKilledBy, lockInfo: lockInfo);
    }

    [MethodRpc((uint)TownOfUsRpc.MisguessSummary, LocalHandling = RpcLocalHandling.After)]
    public static void RpcSetMisguessSummary(PlayerControl player, byte victimId, uint guessId, bool isRole)
    {
        var text = string.Empty;
        if (isRole)
        {
            var role = RoleManager.Instance.GetRole((RoleTypes)guessId);
            text = $"{role.TeamColor.ToTextColor()}{role.GetRoleName()}</color>";
        }
        else
        {
            var modifier = ModifierManager.Modifiers.FirstOrDefault(x => x.TypeId == guessId)!;
            text =
                $"{MiscUtils.GetModifierColour(modifier).ToTextColor()}{modifier.ModifierName}</color>";
        }

        var name = GameData.Instance?.GetPlayerById(victimId)?.Object?.Data?.PlayerName ?? "?";
        var summary = TouLocale.GetParsed("MisguessSummary")
            .Replace("<player>", name)
            .Replace("<role>", text);

        UpdatePlayerDeathData(player.PlayerId, "null", 0, extendedDeathInfo: summary);
    }

    public static void UpdatePlayerDeathData(PlayerControl player, string causeOfDeath = "null",
        float additionalDelayTime = 1f, int roundOfDeath = -1,
        DeathHandlerOverride diedThisRound = DeathHandlerOverride.Ignore, string killedBy = "null",
        string extendedDeathInfo = "null",
        DeathHandlerOverride lockInfo = DeathHandlerOverride.Ignore, StoredPlayerState playerState = (StoredPlayerState)4)
    {
        UpdatePlayerDeathData(player.PlayerId, causeOfDeath, additionalDelayTime, roundOfDeath, diedThisRound, killedBy,
            extendedDeathInfo, lockInfo, playerState);
    }

    public static void UpdatePlayerDeathData(byte playerId, string causeOfDeath = "null", float additionalDelayTime = 1f, int roundOfDeath = -1,
        DeathHandlerOverride diedThisRound = DeathHandlerOverride.Ignore, string killedBy = "null",
        string extendedDeathInfo = "null",
        DeathHandlerOverride lockInfo = DeathHandlerOverride.Ignore, StoredPlayerState playerState = (StoredPlayerState)4)
    {
        HudManagerHelper.Instance.DeathTimer = Math.Max(HudManagerHelper.Instance.DeathTimer + additionalDelayTime, 0);
        var stats = PlayerStats[playerId];
        if (causeOfDeath != "null")
        {
            stats.DeathString = causeOfDeath;
        }

        if (roundOfDeath != -1)
        {
            stats.RoundOfDeath = roundOfDeath;
        }

        if (diedThisRound != DeathHandlerOverride.Ignore)
        {
            stats.DiedThisRound = diedThisRound is DeathHandlerOverride.SetTrue;
        }

        if (killedBy != "null")
        {
            stats.KilledBy = killedBy;
        }

        if (extendedDeathInfo != "null")
        {
            stats.ExtendedCauseOfDeath = extendedDeathInfo;
        }

        if (playerState != (StoredPlayerState)4)
        {
            stats.PlayerState = playerState;
        }

        if (lockInfo != DeathHandlerOverride.Ignore)
        {
            stats.LockDeathInfo = lockInfo is DeathHandlerOverride.SetTrue;
        }
    }
    public static bool IsFullyDead(PlayerControl player)
    {
        if (!player.HasDied() || !PlayerStats.TryGetValue(player.PlayerId, out var stats))
        {
            return false;
        }
        return stats.DiedThisRound;
    }
    public static void RegisterRole(PlayerControl player, RoleBehaviour role, bool clean = false)
    {
        //Message($"RegisterRole - player: '{player.Data.PlayerName}', role: '{role.GetRoleName()}'");

        if (!PlayerStats.TryGetValue(player.PlayerId, out var stats))
        {
            PlayerStats.Add(player.PlayerId,
                new PlayerStats(player.Data.PlayerName, player.PlayerId, player.CachedPlayerData,
                    role.IsDead ? StoredPlayerState.Dead : StoredPlayerState.Alive));
            stats = PlayerStats[player.PlayerId];
            Info($"Added stats for {player.Data.PlayerName}");
        }

        if (clean)
        {
            RoleHistory.RemoveAll(x => x.Key == player.PlayerId);
            stats.TrackedRoles = [];
        }

        RoleDictionary.Remove(player.PlayerId);
        RoleDictionary.Add(player.PlayerId, role);

        RoleHistory.Add(KeyValuePair.Create(player.PlayerId, role));
        stats.DisplayedRole = role;

        var trackRole = true;
        if (!role.IsDead)
        {
            RoleWhenAlive.Remove(player.PlayerId);
            RoleWhenAlive.Add(player.PlayerId, role);
        }
        else if (MiscUtils.IsBasicGhost(role))
        {
            trackRole = false;
        }

        if (role is SpectatorRole)
        {
            stats.PlayerState = StoredPlayerState.Dead;
            stats.IsSpectator = true;
            stats.TrackedRoles.Add(role);
        }
        else if (trackRole)
        {
            stats.TrackedRoles.Add(role);
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

public enum DeathHandlerOverride
{
    SetTrue,
    SetFalse,
    Ignore
}