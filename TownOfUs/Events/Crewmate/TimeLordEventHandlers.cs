using MiraAPI.Events;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.TimeLord;
using UnityEngine;

namespace TownOfUs.Events.Crewmate;

/// <summary>
/// Event handlers for Time Lord events. These handlers register actions during normal gameplay
/// and trigger undo events during rewind.
/// </summary>
public static class TimeLordEventHandlers
{
    private static readonly TimeLordEventRegistry EventQueue = new();

    static TimeLordEventHandlers()
    {
        // Register undo handlers for each event type
        EventQueue.RegisterUndoHandler<TimeLordVentEnterEvent>(evt =>
        {
            var undoEvent = new TimeLordVentEnterUndoEvent(evt);
            MiraEventManager.InvokeEvent(undoEvent);
        });

        EventQueue.RegisterUndoHandler<TimeLordVentExitEvent>(evt =>
        {
            var undoEvent = new TimeLordVentExitUndoEvent(evt);
            MiraEventManager.InvokeEvent(undoEvent);
        });

        EventQueue.RegisterUndoHandler<TimeLordTaskCompleteEvent>(evt =>
        {
            var undoEvent = new TimeLordTaskCompleteUndoEvent(evt);
            MiraEventManager.InvokeEvent(undoEvent);
        });

        EventQueue.RegisterUndoHandler<TimeLordBodyCleanedEvent>(evt =>
        {
            var undoEvent = new TimeLordBodyCleanedUndoEvent(evt);
            MiraEventManager.InvokeEvent(undoEvent);
        });

        EventQueue.RegisterUndoHandler<TimeLordKillEvent>(evt =>
        {
            var undoEvent = new TimeLordKillUndoEvent(evt);
            MiraEventManager.InvokeEvent(undoEvent);
        });

        EventQueue.RegisterUndoHandler<TimeLordChefCookEvent>(evt =>
        {
            var undoEvent = new TimeLordChefCookUndoEvent(evt);
            MiraEventManager.InvokeEvent(undoEvent);
        });

        EventQueue.RegisterUndoHandler<TimeLordChefServeEvent>(evt =>
        {
            var undoEvent = new TimeLordChefServeUndoEvent(evt);
            MiraEventManager.InvokeEvent(undoEvent);
        });

        EventQueue.RegisterUndoHandler<TimeLordKillCooldownEvent>(evt =>
        {
            var undoEvent = new TimeLordKillCooldownUndoEvent(evt);
            MiraEventManager.InvokeEvent(undoEvent);
        });
    }

    /// <summary>
    /// Gets the event queue for rewind processing.
    /// </summary>
    public static TimeLordEventRegistry GetEventQueue() => EventQueue;

    /// <summary>
    /// Records a vent enter action and fires the appropriate event.
    /// </summary>
    public static void RecordVentEnter(PlayerControl player, Vent vent)
    {
        if (player == null || vent == null || !TimeLordRewindSystem.MatchHasTimeLord())
        {
            return;
        }

        var evt = new TimeLordVentEnterEvent(player, vent, UnityEngine.Time.time);
        MiraEventManager.InvokeEvent(evt);
        EventQueue.RecordEvent(evt);
    }

    /// <summary>
    /// Records a vent exit action and fires the appropriate event.
    /// </summary>
    public static void RecordVentExit(PlayerControl player, Vent vent)
    {
        if (player == null || vent == null || !TimeLordRewindSystem.MatchHasTimeLord())
        {
            return;
        }

        var evt = new TimeLordVentExitEvent(player, vent, UnityEngine.Time.time);
        MiraEventManager.InvokeEvent(evt);
        EventQueue.RecordEvent(evt);
    }

    /// <summary>
    /// Records a task completion and fires the appropriate event.
    /// </summary>
    public static void RecordTaskComplete(PlayerControl player, PlayerTask task)
    {
        if (player == null || task == null || !TimeLordRewindSystem.MatchHasTimeLord())
        {
            return;
        }

        var evt = new TimeLordTaskCompleteEvent(player, task, UnityEngine.Time.time);
        MiraEventManager.InvokeEvent(evt);
        EventQueue.RecordEvent(evt);
    }

    /// <summary>
    /// Records a body cleaning and fires the appropriate event.
    /// </summary>
    public static void RecordBodyCleaned(PlayerControl player, DeadBody body, Vector3 position, 
        TimeLordBodyManager.CleanedBodySource source)
    {
        if (player == null || body == null || !TimeLordRewindSystem.MatchHasTimeLord())
        {
            return;
        }

        var evt = new TimeLordBodyCleanedEvent(player, body, position, source, UnityEngine.Time.time);
        MiraEventManager.InvokeEvent(evt);
        EventQueue.RecordEvent(evt);
        
        TimeLordBodyManager.RecordBodyCleaned(body, source);
    }

    /// <summary>
    /// Records a kill and fires the appropriate event.
    /// </summary>
    public static void RecordKill(PlayerControl killer, PlayerControl victim)
    {
        if (killer == null || victim == null || !TimeLordRewindSystem.MatchHasTimeLord())
        {
            return;
        }

        var evt = new TimeLordKillEvent(killer, victim, UnityEngine.Time.time);
        MiraEventManager.InvokeEvent(evt);
        EventQueue.RecordEvent(evt);
    }

    /// <summary>
    /// Records a Chef cook action and fires the appropriate event.
    /// </summary>
    public static void RecordChefCook(PlayerControl chef, DeadBody body, TownOfUs.Roles.Neutral.PlatterType platterType)
    {
        if (chef == null || body == null || !TimeLordRewindSystem.MatchHasTimeLord())
        {
            return;
        }

        var evt = new TimeLordChefCookEvent(chef, body, platterType, UnityEngine.Time.time);
        MiraEventManager.InvokeEvent(evt);
        EventQueue.RecordEvent(evt);
    }

    /// <summary>
    /// Records a Chef serve action and fires the appropriate event.
    /// </summary>
    public static void RecordChefServe(PlayerControl chef, PlayerControl target, byte bodyId, TownOfUs.Roles.Neutral.PlatterType platterType)
    {
        if (chef == null || target == null || !TimeLordRewindSystem.MatchHasTimeLord())
        {
            return;
        }

        var evt = new TimeLordChefServeEvent(chef, target, bodyId, platterType, UnityEngine.Time.time);
        MiraEventManager.InvokeEvent(evt);
        EventQueue.RecordEvent(evt);
    }

    /// <summary>
    /// Records a kill cooldown change and fires the appropriate event.
    /// </summary>
    public static void RecordKillCooldown(PlayerControl player, float cooldownBefore, float cooldownAfter)
    {
        if (player == null || !TimeLordRewindSystem.MatchHasTimeLord())
        {
            return;
        }

        var evt = new TimeLordKillCooldownEvent(player, cooldownBefore, cooldownAfter, UnityEngine.Time.time);
        MiraEventManager.InvokeEvent(evt);
        EventQueue.RecordEvent(evt);
    }

    /// <summary>
    /// Handles vent enter events during rewind (undoes the enter = exits vent).
    /// </summary>
    [RegisterEvent]
    public static void HandleVentEnterUndo(TimeLordVentEnterUndoEvent @event)
    {
        if (@event.OriginalEvent is not TimeLordVentEnterEvent originalEvent)
        {
            return;
        }
        var player = originalEvent.Player;
        var vent = originalEvent.Vent;

        if (player == null || vent == null || !player.AmOwner)
        {
            return;
        }

        if (player.inVent && Vent.currentVent == vent)
        {
            try
            {
                vent.SetButtons(false);
                player.MyPhysics?.RpcExitVent(vent.Id);
                player.MyPhysics?.ExitAllVents();
            }
            catch
            {
                // ignored
            }
        }

        player.inVent = false;
        if (Vent.currentVent == vent)
        {
            Vent.currentVent = null;
        }
    }

    /// <summary>
    /// Handles vent exit events during rewind (undoes the exit = enters vent).
    /// </summary>
    [RegisterEvent]
    public static void HandleVentExitUndo(TimeLordVentExitUndoEvent @event)
    {
        if (@event.OriginalEvent is not TimeLordVentExitEvent originalEvent)
        {
            return;
        }
        var player = originalEvent.Player;
        var vent = originalEvent.Vent;

        if (player == null || vent == null || !player.AmOwner)
        {
            return;
        }

        if (!player.inVent)
        {
            Vent.currentVent = vent;
            try
            {
                player.MyPhysics?.RpcEnterVent(vent.Id);
            }
            catch
            {
                // ignored
            }
            player.inVent = true;
            player.walkingToVent = false;
        }
    }

    /// <summary>
    /// Handles task completion undo events during rewind.
    /// </summary>
    [RegisterEvent]
    public static void HandleTaskCompleteUndo(TimeLordTaskCompleteUndoEvent @event)
    {
        if (@event.OriginalEvent is not TimeLordTaskCompleteEvent originalEvent)
        {
            return;
        }
        var player = originalEvent.Player;
        var taskId = originalEvent.TaskId;

        if (player == null || !player.AmOwner)
        {
            return;
        }

        TimeLordRewindSystem.UndoTask(player.PlayerId, taskId);
    }

    /// <summary>
    /// Handles body cleaned undo events during rewind (restores the body).
    /// </summary>
    [RegisterEvent]
    public static void HandleBodyCleanedUndo(TimeLordBodyCleanedUndoEvent @event)
    {
        if (@event.OriginalEvent is not TimeLordBodyCleanedEvent originalEvent)
        {
            return;
        }
        var bodyId = originalEvent.BodyId;

        TimeLordBodyManager.RestoreCleanedBody(bodyId);
    }

    /// <summary>
    /// Handles kill undo events during rewind (revives the player).
    /// </summary>
    [RegisterEvent]
    public static void HandleKillUndo(TimeLordKillUndoEvent @event)
    {
        if (@event.OriginalEvent is not TimeLordKillEvent originalEvent)
        {
            return;
        }
        var victim = originalEvent.Victim;

        if (victim == null || !victim.Data.IsDead)
        {
            return;
        }

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            TownOfUs.Roles.Crewmate.TimeLordRole.RpcRewindRevive(victim);
        }
    }

    /// <summary>
    /// Handles Chef cook undo events during rewind (restores the body and removes from Chef's stored bodies).
    /// </summary>
    [RegisterEvent]
    public static void HandleChefCookUndo(TimeLordChefCookUndoEvent @event)
    {
        if (@event.OriginalEvent is not TimeLordChefCookEvent originalEvent)
        {
            return;
        }
        var bodyId = originalEvent.BodyId;

        TimeLordBodyManager.RestoreCleanedBody(bodyId);

        var chef = originalEvent.Player;
        if (chef != null && chef.Data?.Role is TownOfUs.Roles.Neutral.ChefRole chefRole)
        {
            var idx = chefRole.StoredBodies.FindIndex(x => x.Key == bodyId);
            if (idx >= 0)
            {
                chefRole.StoredBodies.RemoveAt(idx);
            }

            if (chef.AmOwner)
            {
                var cookBtn = CustomButtonSingleton<ChefCookButton>.Instance;
                if (cookBtn.LimitedUses)
                {
                    cookBtn.UsesLeft = Math.Min(cookBtn.UsesLeft + 1, cookBtn.MaxUses);
                    cookBtn.SetUses(cookBtn.UsesLeft);
                }

                CustomButtonSingleton<ChefServeButton>.Instance.UpdateServingType();
            }
        }
    }

    /// <summary>
    /// Handles Chef serve undo events during rewind (removes the served modifier).
    /// </summary>
    [RegisterEvent]
    public static void HandleChefServeUndo(TimeLordChefServeUndoEvent @event)
    {
        if (@event.OriginalEvent is not TimeLordChefServeEvent originalEvent)
        {
            return;
        }
        var target = originalEvent.Target;

        if (target == null)
        {
            return;
        }

        // Remove the ChefServedModifier
        var servedMod = target.GetModifier<TownOfUs.Modifiers.Neutral.ChefServedModifier>();
        if (servedMod != null)
        {
            target.RemoveModifier(servedMod);
        }

        // Add the body back to Chef's stored bodies
        var chef = originalEvent.Player;
        if (chef != null && chef.Data?.Role is TownOfUs.Roles.Neutral.ChefRole chefRole)
        {
            chefRole.StoredBodies.Insert(0, new KeyValuePair<int, TownOfUs.Roles.Neutral.PlatterType>(
                originalEvent.BodyId, originalEvent.PlatterType));
            chefRole.BodiesServed = Math.Max(0, chefRole.BodiesServed - 1);
        }
    }

    /// <summary>
    /// Handles kill cooldown undo events during rewind (restores the previous cooldown).
    /// </summary>
    [RegisterEvent]
    public static void HandleKillCooldownUndo(TimeLordKillCooldownUndoEvent @event)
    {
        if (@event.OriginalEvent is not TimeLordKillCooldownEvent originalEvent)
        {
            return;
        }

        var player = originalEvent.Player;
        if (player == null || !player.AmOwner)
        {
            return;
        }

        var cooldownBefore = originalEvent.CooldownBefore;
        
        if (player.Data.Role.CanUseKillButton && GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown > 0f)
        {
            var maxKillCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
            
            if (TimeLordRewindSystem.IsRewinding &&
                !TimeLordRewindSystem.LocalKillCooldownMaxClampedThisRewind &&
                (cooldownBefore <= 0f || cooldownBefore < 0.1f) &&
                originalEvent.CooldownAfter > 0.1f)
            {
                TimeLordRewindSystem.LocalKillCooldownMaxClampedThisRewind = true;
            }

            if (TimeLordRewindSystem.IsRewinding && TimeLordRewindSystem.LocalKillCooldownMaxClampedThisRewind)
            {
                cooldownBefore = maxKillCooldown;
            }
            
            var maxvalue = cooldownBefore > maxKillCooldown
                ? cooldownBefore + 1f
                : maxKillCooldown;
            player.killTimer = Mathf.Clamp(cooldownBefore, 0, maxvalue);
            if (HudManager.Instance != null && HudManager.Instance.KillButton != null)
            {
                HudManager.Instance.KillButton.SetCoolDown(player.killTimer, maxvalue);
            }
        }
    }
}