using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Utilities;

namespace TownOfUs.Networking;

public static class CustomTouMurderRpcs
{
    /// <summary>
    /// Networked Custom Murder method. Use this if changing from a dictionary is needed.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="targets">The players to murder.</param>
    /// <param name="isIndirect">Determines if the attack is indirect.</param>
    /// <param name="ignoreShields">If indirect, determines if shields are ignored.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    /// <param name="causeOfDeath">The appended cause of death from the XML, so if you write "Guess", it will look for "DiedToGuess".</param>
    public static void RpcSpecialMultiMurder(
        this PlayerControl source,
        List<PlayerControl> targets,
        bool isIndirect = false,
        bool ignoreShields = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        var newTargets = targets.Select(x => new KeyValuePair<byte, string>(x.PlayerId, x.Data.PlayerName)).ToDictionary(x => x.Key, x => x.Value);
        RpcSpecialMultiMurder(source, newTargets, isIndirect, ignoreShields, didSucceed, resetKillTimer, createDeadBody,
            teleportMurderer, showKillAnim, playKillSound, causeOfDeath);
    }

    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="targets">The players to murder.</param>
    /// <param name="isIndirect">Determines if the attack is indirect.</param>
    /// <param name="ignoreShields">If indirect, determines if shields are ignored.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    /// <param name="causeOfDeath">The appended cause of death from the XML, so if you write "Guess", it will look for "DiedToGuess".</param>
    [MethodRpc((uint)TownOfUsRpc.SpecialMultiMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcSpecialMultiMurder(
        this PlayerControl source,
        Dictionary<byte, string> targets,
        bool isIndirect = false,
        bool ignoreShields = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        var role = source.GetRoleWhenAlive();
        IndirectAttackerModifier? attackerMod = null;
        if (isIndirect)
        {
            attackerMod = source.AddModifier<IndirectAttackerModifier>(ignoreShields);
        }

        var cod = "Killer";
        if (causeOfDeath != "null")
        {
            cod = causeOfDeath;
        }
        else if (role is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        foreach (var target in targets)
        {
            var newPlayer = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == target.Key || x.Data.PlayerName == target.Value);
            if (newPlayer == null)
            {
                continue;
            }
            var beforeMurderEvent = new BeforeMurderEvent(source, newPlayer);
            MiraEventManager.InvokeEvent(beforeMurderEvent);

            if (beforeMurderEvent.IsCancelled)
            {
                murderResultFlags = MurderResultFlags.FailedError;
            }

            var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

            if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
                murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
            {
                DeathHandlerModifier.UpdateDeathHandler(newPlayer, TouLocale.Get($"DiedTo{cod}"), DeathEventHandlers.CurrentRound,
                    (MeetingHud.Instance == null && ExileController.Instance == null) ? DeathHandlerOverride.SetTrue : DeathHandlerOverride.SetFalse,
                    TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
                    lockInfo: DeathHandlerOverride.SetTrue);
            }

            source.CustomMurder(
                newPlayer,
                murderResultFlags2,
                resetKillTimer,
                createDeadBody,
                teleportMurderer,
                showKillAnim,
                playKillSound);
        }
        if (attackerMod != null)
        {
            source.RemoveModifier(attackerMod);
        }
    }
    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="isIndirect">Determines if the attack is indirect.</param>
    /// <param name="ignoreShield">If indirect, determines if shields are ignored.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    /// <param name="causeOfDeath">The appended cause of death from the XML, so if you write "Guess", it will look for "DiedToGuess".</param>
    [MethodRpc((uint)TownOfUsRpc.SpecialMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcSpecialMurder(
        this PlayerControl source,
        PlayerControl target,
        bool isIndirect = false,
        bool ignoreShield = false,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true,
        string causeOfDeath = "null")
    {
        var role = source.GetRoleWhenAlive();
        IndirectAttackerModifier? attackerMod = null;
        if (isIndirect)
        {
            attackerMod = source.AddModifier<IndirectAttackerModifier>(ignoreShield);
        }

        var cod = "Killer";
        if (causeOfDeath != "null")
        {
            cod = causeOfDeath;
        }
        else if (role is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target);
        MiraEventManager.InvokeEvent(beforeMurderEvent);

        if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
            murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
        {
            DeathHandlerModifier.UpdateDeathHandler(target, TouLocale.Get($"DiedTo{cod}"), DeathEventHandlers.CurrentRound,
                (MeetingHud.Instance == null && ExileController.Instance == null) ? DeathHandlerOverride.SetTrue : DeathHandlerOverride.SetFalse,
                TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
                lockInfo: DeathHandlerOverride.SetTrue);
        }

        source.CustomMurder(
            target,
            murderResultFlags2,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);
        if (attackerMod != null)
        {
            source.RemoveModifier(attackerMod);
        }
    }
    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    [MethodRpc((uint)TownOfUsRpc.GhostRoleMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcGhostRoleMurder(
        this PlayerControl source,
        PlayerControl target)
    {
        if (!source.HasDied() || target.HasDied())
        {
            return;
        }

        var role = source.GetRoleWhenAlive();
        if (source.Data.Role is IGhostRole)
        {
            role = source.Data.Role;
        }

        var touRole = role as ITownOfUsRole;
        if (touRole == null || touRole.RoleAlignment is not RoleAlignment.NeutralEvil)
        {
            return;
        }

        var indirectMod = source.AddModifier<IndirectAttackerModifier>(true);

        var cod = "Killer";
        if (touRole.LocaleKey != "KEY_MISS")
        {
            cod = touRole.LocaleKey;
        }

        DeathHandlerModifier.UpdateDeathHandler(target, TouLocale.Get($"DiedTo{cod}"), DeathEventHandlers.CurrentRound,
            DeathHandlerOverride.SetTrue,
            TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
            lockInfo: DeathHandlerOverride.SetTrue);
        DeathHandlerModifier.UpdateDeathHandler(source, "null", -1, DeathHandlerOverride.SetFalse,
            lockInfo: DeathHandlerOverride.SetTrue);
        source.CustomMurder(
            target,
            MurderResultFlags.Succeeded);

        source.RemoveModifier(indirectMod!);
    }
}