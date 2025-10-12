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

    public static void UpdateDeathHandler(PlayerControl player, string causeOfDeath = "null", int roundOfDeath = -1,
        DeathHandlerOverride diedThisRound = DeathHandlerOverride.Ignore, string killedBy = "null",
        DeathHandlerOverride lockInfo = DeathHandlerOverride.Ignore)
    {
        if (!player.HasModifier<DeathHandlerModifier>())
        {
            Logger<TownOfUsPlugin>.Error("RpcUpdateDeathHandler - Player had no DeathHandlerModifier");
            player.AddModifier<DeathHandlerModifier>();
        }

        Coroutines.Start(CoWriteDeathHandler(player, causeOfDeath, roundOfDeath, diedThisRound, killedBy, lockInfo));
    }

    public static bool IsCoroutineRunning { get; set; }

    public static IEnumerator CoWriteDeathHandler(PlayerControl player, string causeOfDeath, int roundOfDeath,
        DeathHandlerOverride diedThisRound, string killedBy, DeathHandlerOverride lockInfo)
    {
        IsCoroutineRunning = true;
        yield return new WaitForSeconds(0.1f);
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

        IsCoroutineRunning = false;
    }
}

public enum DeathHandlerOverride
{
    SetTrue,
    SetFalse,
    Ignore
}