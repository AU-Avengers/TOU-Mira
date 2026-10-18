using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Achievements;

/// <summary>
/// Event handlers driving the bundle achievements defined in the role tabs.
/// </summary>
public static class TouAchievementEvents
{
    private static readonly HashSet<byte> KnightedIds = [];
    private static readonly HashSet<byte> PendingBombVictims = [];
    private static readonly HashSet<byte> HysteriaTargets = [];
    private static readonly HashSet<Type> ImitatedRoles = [];

    private static bool _snitchRevealed;
    private static byte _meetingReporterId = byte.MaxValue;
    private static byte _reportedKillerId = byte.MaxValue;
    private static byte _mysticSensedId = byte.MaxValue;
    private static float _mysticSenseTime;
    private static int _vetKillsThisAlert;
    private static int _vigilanteGuesses;
    private static int _doomsayerGuesses;
    private static int _assassinGuesses;
    private static int _rewindRevives;
    private static float _lastHysteriaTime = -10f;
    private static int _bombKills;
    private static int _meetingsSeen;

    /// <summary>
    /// Called by the Snitch role when it finishes tasks and reveals the evils.
    /// </summary>
    public static void OnSnitchReveal(PlayerControl snitch)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        if (snitch.AmOwner)
        {
            _snitchRevealed = true;
        }
    }

    /// <summary>
    /// Called by the Engineer role right before a critical sabotage is fixed.
    /// </summary>
    public static void OnEngineerCriticalFix(PlayerControl engineer)
    {
        if (!AchievementsAPIHelper.IsLoaded || !engineer.AmOwner ||
            AchievementsAPIHelper.IsUnlocked(TouAchievements.HeroHour))
        {
            return;
        }

        foreach (var system in ShipStatus.Instance.Systems.Values)
        {
            if (system.TryCast<ICriticalSabotage>() is { IsActive: true, Countdown: <= 5f })
            {
                AchievementsAPIHelper.Unlock(TouAchievements.HeroHour);
                break;
            }
        }
    }

    /// <summary>
    /// Called by the Altruist revive button with the number of players being revived.
    /// </summary>
    public static void OnAltruistRevive(PlayerControl altruist, int reviveCount)
    {
        if (!AchievementsAPIHelper.IsLoaded || !altruist.AmOwner || reviveCount < 2)
        {
            return;
        }

        AchievementsAPIHelper.Unlock(TouAchievements.MassResurrection);
    }

    /// <summary>
    /// Called by the Ambusher when they die mid-ambush.
    /// </summary>
    public static void OnAmbusherDeathMidAmbush()
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        AchievementsAPIHelper.Unlock(TouAchievements.FailedAmbush);
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        // Failed Ambush: die while mid-ambush (has AmbusherConcealedModifier)
        if (@event.Player.AmOwner && @event.Player.HasModifier<AmbusherConcealedModifier>())
        {
            AchievementsAPIHelper.Unlock(TouAchievements.FailedAmbush);
        }
    }

    /// <summary>
    /// Called when the Spellslinger's hex bomb detonates.
    /// </summary>
    public static void OnHexBombComplete(PlayerControl spellslinger)
    {
        if (!AchievementsAPIHelper.IsLoaded || !spellslinger.AmOwner)
        {
            return;
        }

        AchievementsAPIHelper.Unlock(TouAchievements.Detonation);
    }

    /// <summary>
    /// Called by the Bomber's bomb when it detonates with its intended victim list.
    /// </summary>
    public static void OnBombDetonate(PlayerControl bomber, IEnumerable<PlayerControl> victims)
    {
        if (!AchievementsAPIHelper.IsLoaded || !bomber.AmOwner)
        {
            return;
        }

        _bombKills = 0;
        PendingBombVictims.Clear();
        foreach (var victim in victims)
        {
            PendingBombVictims.Add(victim.PlayerId);
        }
    }

    [RegisterEvent]
    public static void TouAbilityEventHandler(TouAbilityEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        switch (@event.AbilityType)
        {
            case AbilityType.ClericCleanse
                when @event.Player.AmOwner && @event.Target is PlayerControl cleansed &&
                     cleansed.HasModifier<SpellslingerHexedModifier>():
                AchievementsAPIHelper.Unlock(TouAchievements.Exorcist);
                break;

            case AbilityType.VeteranAlert when @event.Player.AmOwner:
                _vetKillsThisAlert = 0;
                break;

            case AbilityType.PlumberFlush
                when @event.Player.AmOwner && PlayerControl.AllPlayerControls.ToArray().Any(x => x.inVent):
                AchievementsAPIHelper.Unlock(TouAchievements.FlushedOut);
                break;

            case AbilityType.BlackmailerBlackmail
                when @event.Player.AmOwner && @event.Target is PlayerControl blackmailed &&
                     blackmailed.PlayerId == _meetingReporterId:
                AchievementsAPIHelper.Unlock(TouAchievements.HushMoney);
                break;

            case AbilityType.HypnotistHysteria when @event.Player.AmOwner && @event.Target is PlayerControl hypnotised:
                if (Time.time - _lastHysteriaTime > 1f)
                {
                    HysteriaTargets.Clear();
                }

                _lastHysteriaTime = Time.time;
                HysteriaTargets.Add(hypnotised.PlayerId);
                if (HysteriaTargets.Count >= 4)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.MassHysteria);
                }

                break;

            case AbilityType.JanitorClean when @event.Player.AmOwner:
                AchievementsAPIHelper.Increment(TouAchievements.Spotless, 1);
                break;

            case AbilityType.MinerPlaceVent when @event.Player.AmOwner:
                AchievementsAPIHelper.Increment(TouAchievements.SubwaySystem, 1);
                break;

            case AbilityType.VampireBite when @event.Player.AmOwner:
                AchievementsAPIHelper.Increment(TouAchievements.Nosferatu, 1);
                break;

            case AbilityType.PlaguebearerInfect when @event.Player.AmOwner:
                AchievementsAPIHelper.Increment(TouAchievements.PatientZero, 1);
                break;

            case AbilityType.MonarchKnight when @event.Player.AmOwner && @event.Target is PlayerControl knight:
                KnightedIds.Add(knight.PlayerId);
                break;

            case AbilityType.MedicShieldProtect when @event.Player.AmOwner && @event.Result == "New Attack":
                var medicNotified = OptionGroupSingleton<MedicOptions>.Instance.WhoGetsNotification
                    is MedicOption.Medic or MedicOption.ShieldedAndMedic or MedicOption.Everyone;
                AchievementsAPIHelper.SetToastOnGameEnd(TouAchievements.IronWall, !medicNotified);
                AchievementsAPIHelper.Increment(TouAchievements.IronWall, 1);
                break;

            case AbilityType.TimeLordRewind:
                _rewindRevives = 0;
                break;

            case AbilityType.TimeLordRevive
                when TimeLordRewindSystem.IsRewinding && PlayerControl.LocalPlayer.Data?.Role is TimeLordRole:
                _rewindRevives++;
                if (_rewindRevives >= 2)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.Undo);
                }

                break;
        }
    }

    [RegisterEvent(2)]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        var source = @event.Source;
        var target = @event.Target;
        var local = PlayerControl.LocalPlayer;

        if (!source || !target || local == null)
        {
            return;
        }

        // Justice Served: shoot the killer of a body the sheriff reported.
        if (source.AmOwner && source.Data.Role is SheriffRole && target.PlayerId == _reportedKillerId)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.JusticeServed);
        }

        // Knight in Shining Armor: a knighted crewmate kills an evil.
        if (local.Data?.Role is MonarchRole && KnightedIds.Contains(source.PlayerId) && source.IsCrewmate() &&
            (target.IsImpostor() || target.Is(RoleAlignment.NeutralKilling)))
        {
            AchievementsAPIHelper.UnlockTentative(TouAchievements.KnightInShiningArmor);
        }

        // Too Close for Comfort: two kills during a single Alert.
        if (source.AmOwner && source.HasModifier<VeteranAlertModifier>())
        {
            _vetKillsThisAlert++;
            if (_vetKillsThisAlert >= 2)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.TooCloseForComfort);
            }
        }

        // Meeting guesses: a successful guess murders the victim, a misguess kills the guesser.
        if (MeetingHud.Instance && source.AmOwner && target.PlayerId != source.PlayerId)
        {
            if (source.Data.Role is VigilanteRole && ++_vigilanteGuesses >= 2)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.DoubleTap);
            }

            if (source.Data.Role is DoomsayerRole && ++_doomsayerGuesses >= 3)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.Nostradamus);
            }

            if (source.HasModifier<AssassinModifier>() && ++_assassinGuesses >= 2)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.Hitman);
            }
        }

        // Sixth Sense: record the death the Mystic sensed.
        if (local.Data?.Role is MysticRole && target.PlayerId != local.PlayerId)
        {
            _mysticSensedId = target.PlayerId;
            _mysticSenseTime = Time.time;
        }

        // Full Moon: kills made by the local Werewolf.
        if (source.AmOwner && source.Data.Role is WerewolfRole && target.PlayerId != source.PlayerId)
        {
            AchievementsAPIHelper.Increment(TouAchievements.FullMoon, 1);
        }

        // Are you stupid?: the ambushed player is the one being pursued.
        if (source.AmOwner && source.Data.Role is AmbusherRole { Pursued: not null } ambusher &&
            ambusher.Pursued == target)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.AreYouStupid);
        }

        // Burst Fire: WarlockEvents (priority 0) already incremented Kills for this kill.
        if (source.AmOwner && source.Data.Role is WarlockRole)
        {
            var warlockButton = CustomButtonSingleton<WarlockKillButton>.Instance;
            if (warlockButton is { BurstActive: true, Kills: >= 2 })
            {
                AchievementsAPIHelper.Unlock(TouAchievements.BurstFire);
            }
        }

        // Fireworks: victims from the bomb's target list that actually died.
        if (source.AmOwner && PendingBombVictims.Remove(target.PlayerId) && ++_bombKills >= 3)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Fireworks);
        }
    }

    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        if (@event.Reporter != null)
        {
            _meetingReporterId = @event.Reporter.PlayerId;
        }

        if (@event.Reporter == null || !@event.Reporter.AmOwner)
        {
            return;
        }

        // Justice Served: record who killed the reported body.
        if (@event.Target != null)
        {
            var killer = GameHistory.KilledPlayers.LastOrDefault(x => x.VictimId == @event.Target.PlayerId);
            _reportedKillerId = killer?.KillerId ?? byte.MaxValue;
        }

        // Sixth Sense: the reported body is the one the Mystic sensed recently.
        if (@event.Body != null && @event.Body.ParentId == _mysticSensedId &&
            Time.time - _mysticSenseTime <= 15f)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.SixthSense);
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;
        var local = PlayerControl.LocalPlayer;
        if (exiled == null || local == null)
        {
            return;
        }

        // Switched Fate: the swapper redirected a crew vote onto an impostor.
        if (local.Data?.Role is SwapperRole { Swap1: not null, Swap2: not null } swapper)
        {
            var first = MiscUtils.PlayerById(swapper.Swap1.PlayerId);
            var second = MiscUtils.PlayerById(swapper.Swap2.PlayerId);
            var other = exiled.PlayerId == first?.PlayerId ? second : first;

            if ((exiled.PlayerId == first?.PlayerId || exiled.PlayerId == second?.PlayerId) &&
                exiled.IsImpostor() && other != null && other.IsCrewmate())
            {
                AchievementsAPIHelper.Unlock(TouAchievements.SwitchedFate);
            }
        }

        // Spotlight: an impostor ejected in the meeting right after the Snitch reveal.
        if (_snitchRevealed && exiled.IsImpostor())
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Spotlight);
        }

        // Open and Shut: the prosecuted player was an impostor and got ejected.
        if (local.Data?.Role is ProsecutorRole { HasProsecuted: true } prosecutor &&
            prosecutor.ProsecuteVictim == exiled.PlayerId && exiled.IsImpostor())
        {
            AchievementsAPIHelper.Unlock(TouAchievements.OpenAndShut);
        }

        // Hangman: the executioner's target got ejected within 3 meetings.
        if (local.Data?.Role is ExecutionerRole && _meetingsSeen <= 3 &&
            exiled.GetModifiers<ExecutionerTargetModifier>().Any(mod => mod.OwnerId == local.PlayerId))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Hangman);
        }

        if (exiled.PlayerId == local.PlayerId && local.Data?.Role is JesterRole)
        {
            if (_meetingsSeen <= 3)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.TrickyClown);
            }

            var remaining = PlayerControl.AllPlayerControls.ToArray()
                .Count(x => !x.Data.IsDead && x.PlayerId != exiled.PlayerId);
            if (remaining <= 3)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.FinalAct);
            }
        }
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent _)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        _meetingsSeen++;
        _vigilanteGuesses = 0;
        _doomsayerGuesses = 0;
        _assassinGuesses = 0;

        // Poltergeist: the Haunter's reveal happens when the meeting starts.
        if (PlayerControl.LocalPlayer.Data?.Role is HaunterRole { CompletedAllTasks: true, Caught: false })
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Poltergeist);
        }
    }

    [RegisterEvent]
    public static void EndMeetingEventHandler(EndMeetingEvent _)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        _snitchRevealed = false;
        _meetingReporterId = byte.MaxValue;
        _reportedKillerId = byte.MaxValue;
        _mysticSensedId = byte.MaxValue;
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        if (!@event.TriggeredByIntro)
        {
            return;
        }

        KnightedIds.Clear();
        ImitatedRoles.Clear();
        PendingBombVictims.Clear();
        _bombKills = 0;
        _meetingsSeen = 0;
    }

    [RegisterEvent]
    public static void ChangeRoleEventHandler(ChangeRoleEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        var player = @event.Player;
        if (!player.AmOwner || @event.NewRole is ImitatorRole ||
            AchievementsAPIHelper.IsUnlocked(TouAchievements.MethodActor))
        {
            return;
        }

        if (@event.OldRole is not ImitatorRole)
        {
            return;
        }

        ImitatedRoles.Add(@event.NewRole.GetType());
        if (ImitatedRoles.Count >= 3)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.MethodActor);
        }
    }
}