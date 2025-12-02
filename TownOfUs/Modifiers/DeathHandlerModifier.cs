using System.Collections;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers;

public sealed class DeathHandlerModifier : BaseModifier
{
    public static bool IsFullyDead(PlayerControl player)
    {
        if (!player.HasDied())
        {
            return false;
        }

        if (player.TryGetModifier<DeathHandlerModifier>(out var deathHandler))
        {
            return !deathHandler.DiedThisRound;
        }

        return false;
    }
    public override string ModifierName => "Death Handler";
    public override bool HideOnUi => true;

    public override bool ShowInFreeplay => false;

    // This will determine if another mira event should be able to modify the information
    public bool LockInfo { get; set; }

    // This will determine if symbols or anything are shown
    public bool DiedThisRound { get; set; } = !MeetingHud.Instance;

    // This will specify how the player died such as; Suicide, Prosecuted, Ejected, Rampaged, Reaped, etc.
    public string CauseOfDeath { get; set; } = "Suicide";

    // This is set up by the game itself and will display in the lobby
    public int RoundOfDeath { get; set; } = -1;

    // This will specify who killed the player, if any, such as; By Innersloth
    public string KilledBy { get; set; } = string.Empty;

    [MethodRpc((uint)TownOfUsRpc.UpdateDeathHandler)]
    public static void RpcUpdateDeathHandler(PlayerControl player, string causeOfDeath = "null", int roundOfDeath = -1,
        DeathHandlerOverride diedThisRound = DeathHandlerOverride.Ignore, string killedBy = "null",
        DeathHandlerOverride lockInfo = DeathHandlerOverride.Ignore)
    {
        UpdateDeathHandler(player, causeOfDeath, roundOfDeath, diedThisRound, killedBy, lockInfo);
    }

    [MethodRpc((uint)TownOfUsRpc.UpdateLocalDeathHandler)]
    public static void RpcUpdateLocalDeathHandler(PlayerControl player, string causeOfDeath = "null", int roundOfDeath = -1,
        DeathHandlerOverride diedThisRound = DeathHandlerOverride.Ignore, string killedByString = "null", PlayerControl? killedBy = null,
        DeathHandlerOverride lockInfo = DeathHandlerOverride.Ignore)
    {
        var localizedCod = TouLocale.Get(causeOfDeath).Contains("STRMISS") ? "null" : TouLocale.Get(causeOfDeath);
        var localizedKilledBy = (TouLocale.GetParsed(killedByString).Contains("STRMISS") || killedBy == null) ? "null" : TouLocale.GetParsed(killedByString).Replace("<player>", killedBy.Data.PlayerName);
        UpdateDeathHandler(player, localizedCod, roundOfDeath, diedThisRound, localizedKilledBy, lockInfo);
    }

    public static void UpdateDeathHandler(PlayerControl player, string causeOfDeath = "null", int roundOfDeath = -1,
        DeathHandlerOverride diedThisRound = DeathHandlerOverride.Ignore, string killedBy = "null",
        DeathHandlerOverride lockInfo = DeathHandlerOverride.Ignore)
    {
        Coroutines.Start(CoWriteDeathHandler(player, causeOfDeath, roundOfDeath, diedThisRound, killedBy, lockInfo));
    }

    public static void UpdateDeathHandlerImmediate(PlayerControl player, string causeOfDeath = "null", int roundOfDeath = -1,
        DeathHandlerOverride diedThisRound = DeathHandlerOverride.Ignore, string killedBy = "null",
        DeathHandlerOverride lockInfo = DeathHandlerOverride.Ignore)
    {
        if (!player.HasModifier<DeathHandlerModifier>())
        {
            Error("UpdateDeathHandlerImmediate - Player had no DeathHandlerModifier");
            player.AddModifier<DeathHandlerModifier>();
        }

        Coroutines.Start(CoWriteDeathHandlerImmediate(player, causeOfDeath, roundOfDeath, diedThisRound, killedBy, lockInfo));
    }

    public static bool IsCoroutineRunning { get; set; }
    public static bool IsAltCoroutineRunning { get; set; }

    public static IEnumerator CoWriteDeathHandler(PlayerControl player, string causeOfDeath, int roundOfDeath,
        DeathHandlerOverride diedThisRound, string killedBy, DeathHandlerOverride lockInfo)
    {
        IsCoroutineRunning = true;
        yield return new WaitForSeconds(0.05f);
        DeathHandlerModifier deathHandler;
        if (!player.HasModifier<DeathHandlerModifier>())
        {
            Error("UpdateDeathHandler - Player had no DeathHandlerModifier");
            deathHandler = player.AddModifier<DeathHandlerModifier>()!;
        }
        else
        {
            #pragma warning disable S1854 // Unused assignments should be removed
            deathHandler = player.GetModifier<DeathHandlerModifier>()!; // This is actually a used assignment. Surprise Surprise.
            #pragma warning restore S1854 // Unused assignments should be removed

            IsCoroutineRunning = false;
            yield break;
        }
        yield return new WaitForEndOfFrame();

        // For future refence, do note that AddModifier only begins on the next unity physics update, as of MIRA CI 820, and so
        //   WaitForEndOfFrame may not necessarily hit it. The solution to this is being a bit more 'c' like in code style, or ensuring you wait.
        //   I have chosen the former here.
        // Consider also introducing a GetOrAddModifier method to trend developers away from similar misakes :)
        // Yes there are nicer ways to fix this/write "cleaner" code. However, in the effort of making the issue clear for future, I have chosen the
        //   explicit and slightly long winded way, which imo gets the point across more.
        if (deathHandler == null) {
            Error("There has been a significant issue with MiraApi modifier application.\n  TownOfUs/Modifiers/DeathHandlerModifier.cs:line 106\n"
                + "Consider the timings of this Coroutine and the physics update event in Mira.");
            IsCoroutineRunning = false;
            yield break;
        }

        if (causeOfDeath != "null")
        {
            deathHandler.CauseOfDeath = causeOfDeath;
        }

        if (roundOfDeath != -1)
        {
            deathHandler.RoundOfDeath = roundOfDeath;
        }

        if (diedThisRound != DeathHandlerOverride.Ignore)
        {
            deathHandler.DiedThisRound = diedThisRound is DeathHandlerOverride.SetTrue;
        }

        if (killedBy != "null")
        {
            deathHandler.KilledBy = killedBy;
        }

        if (lockInfo != DeathHandlerOverride.Ignore)
        {
            deathHandler.LockInfo = lockInfo is DeathHandlerOverride.SetTrue;
        }

        IsCoroutineRunning = false;
    }

    public static IEnumerator CoWriteDeathHandlerImmediate(PlayerControl player, string causeOfDeath, int roundOfDeath,
        DeathHandlerOverride diedThisRound, string killedBy, DeathHandlerOverride lockInfo)
    {
        IsAltCoroutineRunning = true;
        while (!player.HasModifier<DeathHandlerModifier>())
        {
            yield return null;
        }
        var deathHandler = player.GetModifier<DeathHandlerModifier>()!;
        if (causeOfDeath != "null")
        {
            deathHandler.CauseOfDeath = causeOfDeath;
        }

        if (roundOfDeath != -1)
        {
            deathHandler.RoundOfDeath = roundOfDeath;
        }

        if (diedThisRound != DeathHandlerOverride.Ignore)
        {
            deathHandler.DiedThisRound = diedThisRound is DeathHandlerOverride.SetTrue;
        }

        if (killedBy != "null")
        {
            deathHandler.KilledBy = killedBy;
        }

        if (lockInfo != DeathHandlerOverride.Ignore)
        {
            deathHandler.LockInfo = lockInfo is DeathHandlerOverride.SetTrue;
        }

        IsAltCoroutineRunning = false;
    }
}

public enum DeathHandlerOverride
{
    SetTrue,
    SetFalse,
    Ignore
}