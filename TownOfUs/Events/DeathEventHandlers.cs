using System.Collections;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Events;

public static class DeathEventHandlers
{
    public static bool IsDeathRecent { get; set; }
    public static IEnumerator CoWaitDeathHandler()
    {
        IsDeathRecent = true;
        yield return new WaitForSeconds(0.15f);
        IsDeathRecent = false;
    }
    public static int CurrentRound { get; set; } = 1;

    [RegisterEvent(-1)]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            CurrentRound = 1;
            Logger<TownOfUsPlugin>.Warning("Game Has Started");
        }
        else
        {
            ++CurrentRound;
            ModifierUtils.GetActiveModifiers<DeathHandlerModifier>().Do(x => x.DiedThisRound = false);
            Logger<TownOfUsPlugin>.Warning($"New Round Started: {CurrentRound}");
        }
    }

    [RegisterEvent(1000)]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        var victim = @event.Player;
        if (!victim.HasModifier<DeathHandlerModifier>())
        {
            var deathHandler = new DeathHandlerModifier();
            victim.AddModifier(deathHandler);
            var cod = "Disconnected";
            deathHandler.DiedThisRound = !MeetingHud.Instance && !ExileController.Instance;
            switch (@event.DeathReason)
            {
                case DeathReason.Exile:
                    cod = "Ejection";
                    deathHandler.DiedThisRound = false;
                    break;
                case DeathReason.Kill:
                    cod = "Killer";
                    break;
            }
            deathHandler.CauseOfDeath = TouLocale.Get($"DiedTo{cod}");
            deathHandler.RoundOfDeath = CurrentRound;
            Coroutines.Start(CoWaitDeathHandler());
        }
    }
    
    [RegisterEvent(500)]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled == null)
        {
            return;
        }
        if (!exiled.HasModifier<DeathHandlerModifier>())
        {
            DeathHandlerModifier.UpdateDeathHandler(exiled, TouLocale.Get("DiedToEjection"), CurrentRound, DeathHandlerOverride.SetFalse);
        }
    }

    [RegisterEvent(500)]
    public static void PlayerReviveEventHandler(PlayerReviveEvent reviveEvent)
    {
        var deathMods = reviveEvent.Player.GetModifiers<DeathHandlerModifier>();

        foreach (var deathMod in deathMods)
        {
            deathMod.ModifierComponent?.RemoveModifier(deathMod);
        }
    }

    [RegisterEvent(500)]
    public static void AfterMurderEventHandler(AfterMurderEvent murderEvent)
    {
        var source = murderEvent.Source;
        var target = murderEvent.Target;
        
        if (target == source && target.TryGetModifier<DeathHandlerModifier>(out var deathHandler) && !deathHandler.LockInfo)
        {
            deathHandler.CauseOfDeath = TouLocale.Get("DiedToSuicide");
            deathHandler.DiedThisRound = !MeetingHud.Instance && !ExileController.Instance;
            deathHandler.RoundOfDeath = CurrentRound;
            deathHandler.LockInfo = true;
        }
        else if (target.TryGetModifier<DeathHandlerModifier>(out var deathHandler2) && !deathHandler2.LockInfo)
        {
            var role = source.GetRoleWhenAlive();
            var cod = "Killer";
            switch (role)
            {
                case MirrorcasterRole mirror:
                    cod = mirror.UnleashString != string.Empty ? mirror.UnleashString : TouLocale.Get("DiedToKiller");
                    mirror.UnleashString = string.Empty;
                    mirror.ContainedRole = null;
                    break;
                default:
                    if (role is not ITownOfUsRole touRole || touRole.LocaleKey == "KEY_MISS")
                    {
                        break;
                    }

                    cod = touRole.LocaleKey;
                    break;
            }

            if (source.Data.Role is PhantomTouRole)
            {
                role = source.Data.Role;
                cod = PhantomTouRole.LocaleKey;
            }
            
            deathHandler2.CauseOfDeath = role is MirrorcasterRole ? cod : TouLocale.Get($"DiedTo{cod}");
            deathHandler2.KilledBy = TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName);
            deathHandler2.DiedThisRound = !MeetingHud.Instance && !ExileController.Instance;
            deathHandler2.RoundOfDeath = CurrentRound;
        }
    }

    [RegisterEvent]
    public static void PlayerLeaveEventHandler(PlayerLeaveEvent @event)
    {
        if (!MeetingHud.Instance)
        {
            return;
        }

        var player = @event.ClientData.Character;

        if (!player)
        {
            return;
        }

        var pva = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == player.PlayerId);

        if (!pva)
        {
            return;
        }

        pva.AmDead = true;
        pva.Overlay.gameObject.SetActive(true);
        pva.Overlay.color = Color.white;
        pva.XMark.gameObject.SetActive(false);
        pva.XMark.transform.localScale = Vector3.one;

        MeetingMenu.Instances.Do(x => x.HideSingle(player.PlayerId));
    }
}