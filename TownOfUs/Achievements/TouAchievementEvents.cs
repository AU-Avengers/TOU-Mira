using System.Collections;
using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Events.TouEvents;
using TownOfUs.GameOver;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Impostor.Herbalist;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
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

    private static readonly Dictionary<byte, byte> KnightToMonarch = [];
    private static readonly HashSet<byte> BetrayedMonarchIds = [];
    private static readonly HashSet<byte> AltruistRevivedIds = [];
    private static readonly HashSet<byte> PendingAltruistRevives = [];
    private static readonly Dictionary<byte, float> _reviveTimes = [];
    private static readonly HashSet<byte> TimeLordRevivedIds = [];
    private static readonly HashSet<byte> LocalKillerIds = [];
    private static readonly HashSet<byte> NearbyKillerIds = [];
    private static readonly HashSet<byte> HunterStalkedIds = [];
    private static readonly HashSet<int> SeerEnemyPairs = [];
    private static readonly List<(byte SwapperId, byte OtherId)> SwapSelfPairs = [];
    private static readonly List<(Type Role, bool IsCrewmate)> Imitations = [];
    private static int _prosecutions;
    private static bool _prosecutionEjected;
    private static bool _medicShieldAttacked;
    private static bool _snitchCompletedTasks;
    private static bool _snitchRevealedLastMeeting;
    private static float _lastMeetingStartTime = -999f;
    private static float _sabotageStartTime = -999f;

    private static readonly HashSet<byte> FramedIds = [];
    private static readonly HashSet<byte> ExposedThisRoundIds = [];
    private static readonly HashSet<byte> RetrainedIds = [];
    private static readonly Dictionary<byte, int> PuppeteerControlCounts = [];
    private static readonly Dictionary<byte, Vector2> DragPickupPositions = [];
    private static readonly HashSet<byte> DraggedBodies = [];
    private static readonly HashSet<byte> SwoopNearbyIds = [];
    private static bool _becameTraitor;
    private static int _meetingsWhenTraitor;
    private static bool _hexBombCompleted;
    private static int _scavengerStreak;
    private static int _swoopsThisRound;
    private static byte _bootlegServeTarget = byte.MaxValue;
    private static int _bootlegServeCount;
    private static int _hysteriaRounds;
    private static bool _swoopScanActive;

    private static readonly HashSet<byte> MercenaryBribedIds = [];
    private static readonly HashSet<byte> ServedLocalIds = [];
    private static readonly Dictionary<byte, HashSet<RoleTypes>> DoomsayerObservedRoleLists = [];
    private static bool _inquisitorUsedAbility;
    private static bool _rampageActive;
    private static bool _rampageEnded;
    private static int _rampageKills;
    private static int _rampageAliveOthers;

    /// <summary>
    /// True until the first meeting of the match has been called.
    /// </summary>
    public static bool IsFirstRound => _meetingsSeen == 0;

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

        var victim = @event.Player;
        var local = PlayerControl.LocalPlayer;

        // Endgame: a death may have left only a Juggernaut and a Pestilence alive.
        Coroutines.Start(CoCheckHorsemanDuel());

        // Lost Cause: a player the local Altruist revived dies again.
        if (AltruistRevivedIds.Remove(victim.PlayerId))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.LostCause);
        }

        // Nullified Protection: the Warden's fortified player died anyway.
        if (victim.TryGetModifier<WardenFortifiedModifier>(out var fortifiedMod) && fortifiedMod.Warden.AmOwner)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.NullifiedProtection);
        }

        if (!victim.AmOwner || local == null)
        {
            return;
        }

        // Too Slow: the Snitch died after being revealed to the evils.
        if (@event.DeathReason != DeathReason.Exile && victim.GetRoleWhenAlive() is SnitchRole { OnLastTask: true })
        {
            AchievementsAPIHelper.Unlock(TouAchievements.TooSlow);
        }

        // Cultural Discharge: the Spellslinger died while their Hex Bomb is running.
        if (victim.GetRoleWhenAlive() is SpellslingerRole &&
            ShipStatus.Instance.Systems.TryGetValue((SystemTypes)HexBombSabotageSystem.SabotageId,
                out var hexBombSystem) &&
            hexBombSystem.TryCast<HexBombSabotageSystem>() is { IsActive: true })
        {
            AchievementsAPIHelper.Unlock(TouAchievements.CulturalDischarge);
        }

        // Off-The-Glock: the Spy died while using the Admin Table.
        if (local.Data?.Role is SpyRole &&
            ((MapBehaviour.Instance && MapBehaviour.Instance.countOverlay &&
              MapBehaviour.Instance.countOverlay.gameObject.activeSelf) ||
             UnityEngine.Object.FindObjectOfType<MapCountOverlay>()?.gameObject.activeSelf == true))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.OffTheGlock);
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

        _hexBombCompleted = true;
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

    /// <summary>
    /// Called by the Snitch role when it completes all of its tasks.
    /// </summary>
    public static void OnSnitchCompletedTasks(PlayerControl snitch)
    {
        if (!AchievementsAPIHelper.IsLoaded || !snitch.AmOwner)
        {
            return;
        }

        _snitchCompletedTasks = true;

        // Dead Tell No Tales: every evil the snitch can reveal is already dead.
        var evils = PlayerControl.AllPlayerControls.ToArray().Where(SnitchRole.IsTargetOfSnitch);
        if (evils.Any() && evils.All(x => x.HasDied()))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.DeadTellNoTales);
        }
    }

    /// <summary>
    /// Called by the Trapper role when their trap feedback is reported.
    /// </summary>
    public static void OnTrapperFeedback(PlayerControl trapper, int roleCount)
    {
        if (!AchievementsAPIHelper.IsLoaded || !trapper.AmOwner)
        {
            return;
        }

        if (roleCount >= 5)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.RoleCollector);
        }
    }

    /// <summary>
    /// Called by the Mirrorcaster when they unleash an attack.
    /// </summary>
    public static void OnMirrorcasterUnleash(PlayerControl mirrorcaster, PlayerControl target, byte attackerId)
    {
        if (!AchievementsAPIHelper.IsLoaded || !mirrorcaster.AmOwner)
        {
            return;
        }

        if (target.PlayerId == attackerId)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.ReturnToSender);
        }
    }

    /// <summary>
    /// Called when an Oracle's blessing saves a player from ejection.
    /// </summary>
    public static void OnOracleBlessSaved(PlayerControl oracle, PlayerControl exiled)
    {
        if (!AchievementsAPIHelper.IsLoaded || !oracle.AmOwner)
        {
            return;
        }

        AchievementsAPIHelper.Unlock(TouAchievements.HolyBlessing);
    }

    /// <summary>
    /// Called when an Oracle confession returns no results due to too few players.
    /// </summary>
    public static void OnOracleConfessEmpty(PlayerControl oracle)
    {
        if (!AchievementsAPIHelper.IsLoaded || !oracle.AmOwner)
        {
            return;
        }

        AchievementsAPIHelper.Unlock(TouAchievements.NoConfessions);
    }

    /// <summary>
    /// Called whenever a sabotage is activated.
    /// </summary>
    public static void OnSabotageStart()
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        _sabotageStartTime = Time.time;
    }

    /// <summary>
    /// Called when a Barkeeper or Bootlegger serves a drink to a player.
    /// </summary>
    public static void OnDrinkServed(PlayerControl server, PlayerControl target)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        if (target.AmOwner)
        {
            ServedLocalIds.Add(server.PlayerId);
        }

        if (!server.AmOwner)
        {
            return;
        }

        // No You: the local player served a drink back to someone who served them this round.
        if (ServedLocalIds.Contains(target.PlayerId))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.PartyOfTwo);
        }

        if (server.Data.Role is not BootleggerRole)
        {
            return;
        }

        // Blackout: three drinks served to the same player in a row in one round.
        if (target.PlayerId == _bootlegServeTarget)
        {
            _bootlegServeCount++;
        }
        else
        {
            _bootlegServeTarget = target.PlayerId;
            _bootlegServeCount = 1;
        }

        if (_bootlegServeCount >= 3)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Blackout);
        }
    }

    /// <summary>
    /// Called when a player dies to the Bootlegger's poison.
    /// </summary>
    public static void OnBootleggerPoisonKill(PlayerControl bootlegger, PlayerControl victim)
    {
        if (!AchievementsAPIHelper.IsLoaded || !bootlegger.AmOwner || bootlegger == victim)
        {
            return;
        }

        AchievementsAPIHelper.Unlock(TouAchievements.Poisoner);
    }

    /// <summary>
    /// Called when an Arsonist's ignite fails to kill a protected player.
    /// </summary>
    public static void OnArsonistIgniteBlocked(PlayerControl arsonist)
    {
        if (!AchievementsAPIHelper.IsLoaded || !arsonist.AmOwner)
        {
            return;
        }

        AchievementsAPIHelper.Unlock(TouAchievements.Fireproof);
    }

    /// <summary>
    /// Called when the local Werewolf's rampage effect ends.
    /// </summary>
    public static void OnWerewolfRampageEnd(PlayerControl werewolf)
    {
        if (!AchievementsAPIHelper.IsLoaded || !werewolf.AmOwner || !_rampageActive || _rampageEnded)
        {
            return;
        }

        // Blood Moon still needs _rampageActive at game end, so only mark the natural end here.
        _rampageEnded = true;

        // All Bark No Bite: the rampage ran out naturally without any kills.
        if (_rampageKills == 0 && !MeetingHud.Instance && !ExileController.Instance)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.AllBarkNoBite);
        }
    }

    /// <summary>
    /// Called when the local player sends a message through the Jailor chat.
    /// </summary>
    public static void OnJailorChat(PlayerControl sender)
    {
        if (!AchievementsAPIHelper.IsLoaded || !sender.AmOwner)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;

        // Stolen Valor: an Imitator who became the Jailor uses the jailor chat while someone is jailed.
        if (local.Data?.Role is JailorRole && local.HasModifier<ImitatorCacheModifier>() &&
            PlayerControl.AllPlayerControls.ToArray().Any(x => !x.HasDied() && x.IsJailed()))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.StolenValor);
        }
    }

    /// <summary>
    /// Called by the Doomsayer report with the roles listed for an observed player.
    /// </summary>
    public static void OnDoomsayerObserveReport(PlayerControl doomsayer, byte observedId,
        IEnumerable<RoleTypes> listedRoles)
    {
        if (!AchievementsAPIHelper.IsLoaded || !doomsayer.AmOwner)
        {
            return;
        }

        DoomsayerObservedRoleLists[observedId] = listedRoles.ToHashSet();
    }

    /// <summary>
    /// Called when the Doomsayer picks a role guess for a target.
    /// </summary>
    public static void OnDoomsayerGuessAttempt(PlayerControl doomsayer, byte targetId, RoleTypes guessedRole)
    {
        if (!AchievementsAPIHelper.IsLoaded || !doomsayer.AmOwner)
        {
            return;
        }

        // Blind Faith: the guessed role was never listed for an observed target.
        if (DoomsayerObservedRoleLists.TryGetValue(targetId, out var listedRoles) &&
            !listedRoles.Contains(guessedRole))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.BlindFaith);
        }
    }

    /// <summary>
    /// Called when the Venerer unlocks the Freeze ability.
    /// </summary>
    public static void OnVenererFreezeUnlocked(PlayerControl venerer)
    {
        if (!AchievementsAPIHelper.IsLoaded || !venerer.AmOwner)
        {
            return;
        }

        AchievementsAPIHelper.Unlock(TouAchievements.Evolution);
    }

    /// <summary>
    /// Called by the Time Lord rewind system when a player is revived through a rewind.
    /// </summary>
    public static void OnTimeLordRevive(PlayerControl revived)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        TimeLordRevivedIds.Add(revived.PlayerId);
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
            case AbilityType.ClericCleanse when @event.Player.AmOwner:
                if (@event.Target is PlayerControl cleansed && cleansed.HasModifier<SpellslingerHexedModifier>())
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.Exorcist);
                }

                if (int.TryParse(@event.Result, out var cleansedEffects) && cleansedEffects >= 3)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.HowDidWeGetHere);
                }

                break;

            case AbilityType.ClericBarrier when @event.Player.AmOwner && @event.Result == "Blocked" &&
                                               IsLoverOf(@event.Target, PlayerControl.LocalPlayer):
                AchievementsAPIHelper.Unlock(TouAchievements.YoureSafeWithMe);
                break;

            case AbilityType.WardenProtect when @event.Player.AmOwner &&
                                               IsLoverOf(@event.Target, PlayerControl.LocalPlayer):
                AchievementsAPIHelper.Unlock(TouAchievements.YoureSafeWithMe);
                break;

            // Divine Protector: the Fairy's protection fended off an attacker.
            case AbilityType.FairyProtected when @event.Player.AmOwner:
                AchievementsAPIHelper.Unlock(TouAchievements.DivineProtector);
                if (IsLoverOf(@event.Target, PlayerControl.LocalPlayer))
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.YoureSafeWithMe);
                }

                break;

            case AbilityType.SeerComparisonResult when @event.Player.AmOwner &&
                                                      @event.Target is PlayerControl seerA &&
                                                      @event.Target2 is PlayerControl seerB:
                if (@event.Result == "Enemies")
                {
                    var pairKey = Math.Min(seerA.PlayerId, seerB.PlayerId) * 256 + Math.Max(seerA.PlayerId, seerB.PlayerId);
                    if (SeerEnemyPairs.Add(pairKey))
                    {
                        AchievementsAPIHelper.Increment(TouAchievements.TownOfHatred, 1);
                    }
                }
                else
                {
                    var roleA = seerA.GetRoleWhenAlive();
                    var roleB = seerB.GetRoleWhenAlive();
                    if (!seerA.IsCrewmate() && !seerB.IsCrewmate() && roleA != null && roleB != null &&
                        (roleA.Role == roleB.Role ||
                         roleA is ITownOfUsRole touA && roleB is ITownOfUsRole touB &&
                         touA.RoleAlignment == touB.RoleAlignment))
                    {
                        AchievementsAPIHelper.Unlock(TouAchievements.RoleTwins);
                    }
                }

                break;

            case AbilityType.SonarTrack when @event.Player.AmOwner:
                {
                    var tracked = PlayerControl.AllPlayerControls.ToArray().Count(x =>
                    x.TryGetModifier<SonarArrowTargetModifier>(out var arrowMod) && arrowMod.Owner.AmOwner ||
                    x.TryGetModifier<SonarHeartbeatTargetModifier>(out var pingMod) && pingMod.Owner.AmOwner);
                    if (tracked > 5)
                    {
                        AchievementsAPIHelper.Unlock(TouAchievements.Hyperfocused);
                    }

                    break;
                }

            case AbilityType.HunterStalk when @event.Player.AmOwner && @event.Target is PlayerControl stalked:
                HunterStalkedIds.Add(stalked.PlayerId);
                break;

            case AbilityType.HunterRetribution when @event.Player.AmOwner && @event.Target is PlayerControl retTarget:
                if (IsEvil(retTarget))
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.Punisher);
                }

                if (HunterStalkedIds.Contains(retTarget.PlayerId))
                {
                    AchievementsAPIHelper.Increment(TouAchievements.HuntingSeason, 1);
                }

                break;

            case AbilityType.ProsecutorProsecutes when @event.Player.AmOwner:
                _prosecutions++;
                if (_prosecutions >= 2)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.TheFinalSay);
                }

                break;

            case AbilityType.EngineerFix when @event.Player.AmOwner && @event.Result != "Critical":
                if (Time.time - _sabotageStartTime <= 5f)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.Mechanic);
                }

                break;

            case AbilityType.AltruistRevive when @event.Player.AmOwner && @event.Target is PlayerControl revived:
                PendingAltruistRevives.Add(revived.PlayerId);
                break;

            case AbilityType.SentryPlaceCam when @event.Player.AmOwner && @event.Target is SurvCamera camera:
                {
                    var camPos = camera.transform.position;
                    var inSecurity = false;
                    foreach (var room in ShipStatus.Instance.FastRooms.Values)
                    {
                        if (room.RoomId == SystemTypes.Security && room.roomArea && room.roomArea.OverlapPoint(camPos))
                        {
                            inSecurity = true;
                            break;
                        }
                    }
                    if (inSecurity)
                    {
                        AchievementsAPIHelper.Unlock(TouAchievements.ExtraSecure);
                    }

                    break;
                }

            case AbilityType.TransporterTransport when @event.Player.AmOwner:
                if (@event.Target is DeadBody && @event.Target2 is DeadBody)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.PotatoTomato);
                }

                break;

            case AbilityType.BlackmailerBlackmail
                when @event.Target is PlayerControl bmTarget && bmTarget.AmOwner && _snitchRevealedLastMeeting:
                AchievementsAPIHelper.Unlock(TouAchievements.SeeSomethingSayNothing);
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

                // Horsing Around: hysteria triggered while every living non-Impostor is hypnotized.
                var livingNonImpOthers = PlayerControl.AllPlayerControls.ToArray().Count(x =>
                    !x.AmOwner && !x.HasDied() && !x.Data.Disconnected && !x.IsImpostor());
                if (livingNonImpOthers >= 3 && PlayerControl.AllPlayerControls.ToArray().All(x =>
                        x.AmOwner || x.HasDied() || x.Data.Disconnected || x.IsImpostor() ||
                        HysteriaTargets.Contains(x.PlayerId)))
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.HorsingAround);
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

                // Change of Heart: a conversion with fewer than 5 players alive.
                if (AliveOthers().Count + 1 < 5)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.ChangeOfHeart);
                }

                break;

            case AbilityType.PlaguebearerInfect when @event.Player.AmOwner:
                AchievementsAPIHelper.Increment(TouAchievements.PatientZero, 1);
                break;

            case AbilityType.MonarchKnight when @event.Target is PlayerControl knight:
                if (@event.Player.AmOwner)
                {
                    KnightedIds.Add(knight.PlayerId);
                }

                KnightToMonarch[knight.PlayerId] = @event.Player.PlayerId;

                // Your Majesty: another Monarch knighted the local player.
                if (knight.AmOwner && !@event.Player.AmOwner)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.YourMajesty);
                }

                break;

            case AbilityType.MedicShieldProtect when @event.Player.AmOwner:
                _medicShieldAttacked = true;
                if (@event.Result == "New Attack")
                {
                    AchievementsAPIHelper.Increment(TouAchievements.IronWall, 1);
                }

                break;

            // True Darkness: an eclipse triggered while the lights are out.
            case AbilityType.EclipsalBlind when @event.Player.AmOwner:
                if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Electrical, out var electrical) &&
                    electrical.TryCast<SwitchSystem>() is { IsActive: true })
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.TrueDarkness);
                }

                break;

            // Oh, Henry!: recall into a room another player is standing in.
            case AbilityType.EscapistRecall when @event.Player.AmOwner:
                {
                    var recallPos = @event.Player.GetTruePosition();
                    PlainShipRoom? destination = null;
                    foreach (var room in ShipStatus.Instance.FastRooms.Values)
                    {
                        if (room.roomArea && room.roomArea.OverlapPoint(recallPos))
                        {
                            destination = room;
                            break;
                        }
                    }

                    if (destination != null && PlayerControl.AllPlayerControls.ToArray().Any(x =>
                            !x.AmOwner && !x.HasDied() && !x.inVent &&
                            destination.roomArea.OverlapPoint(x.GetTruePosition())))
                    {
                        AchievementsAPIHelper.Unlock(TouAchievements.OhHenry);
                    }

                    break;
                }

            // Introvert / Under the Radar: swoop tracking for the local Swooper.
            case AbilityType.SwooperSwoop when @event.Player.AmOwner && @event.Player.Data.Role is SwooperRole:
                _swoopsThisRound++;
                if (_swoopsThisRound >= 3)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.Introvert);
                }

                if (!_swoopScanActive)
                {
                    SwoopNearbyIds.Clear();
                    Coroutines.Start(CoSwoopScan());
                }

                break;

            case AbilityType.ScavengerCorrectKill when @event.Player.AmOwner:
                _scavengerStreak++;
                if (_scavengerStreak >= 3)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.UavInbound);
                }

                break;

            case AbilityType.ScavengerIncorrectKill when @event.Player.AmOwner:
                _scavengerStreak = 0;
                break;

            case AbilityType.AmbassadorRetrainAccepted
                when @event.Player.AmOwner && @event.Target is PlayerControl retrained:
                RetrainedIds.Add(retrained.PlayerId);
                if (RetrainedIds.Count >= 2 && RetrainedIds.Contains(@event.Player.PlayerId))
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.DoubleTrouble);
                }

                break;

            case AbilityType.HerbalistExpose when @event.Player.AmOwner:
                {
                    AchievementsAPIHelper.Increment(TouAchievements.Strategist, 1);
                    if (@event.Target is PlayerControl exposed)
                    {
                        ExposedThisRoundIds.Add(exposed.PlayerId);
                    }

                    // All-Knowing: every living non-impostor other than the local player is exposed.
                    var alive = PlayerControl.AllPlayerControls.ToArray()
                    .Where(x => !x.AmOwner && !x.HasDied() && !x.Data.Disconnected);
                    var nonImpOthers = alive.Count(x => !x.IsImpostorAligned());
                    var unexposedNonImpOthers = alive.Count(x =>
                    !x.IsImpostorAligned() && !x.HasModifier<HerbalistExposedModifier>());
                    if (unexposedNonImpOthers == 0 && nonImpOthers >= 2)
                    {
                        AchievementsAPIHelper.Unlock(TouAchievements.AllKnowing);
                    }

                    break;
                }

            case AbilityType.HerbalistConfuse when @event.Player.AmOwner:
            case AbilityType.HerbalistProtect when @event.Player.AmOwner:
                AchievementsAPIHelper.Increment(TouAchievements.Strategist, 1);
                break;

            case AbilityType.PuppeteerControl when @event.Player.AmOwner && @event.Target is PlayerControl puppet:
                PuppeteerControlCounts[puppet.PlayerId] = PuppeteerControlCounts.GetValueOrDefault(puppet.PlayerId) + 1;
                if (PuppeteerControlCounts[puppet.PlayerId] >= 2)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.DoubleDipping);
                }

                // Fratricide: the Puppeteer controlled a fellow Impostor.
                if (puppet.IsImpostorAligned())
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.Fratricide);
                }

                break;

            // Public Execution: the Parasite's Overtaken victim was killed with a witness nearby.
            case AbilityType.ParasiteKill when @event.Player.AmOwner && @event.Target is PlayerControl overtaken:
                if (PlayerControl.AllPlayerControls.ToArray().Any(x =>
                        x.PlayerId != @event.Player.PlayerId && x.PlayerId != overtaken.PlayerId &&
                        !x.HasDied() && !x.Data.Disconnected && !x.inVent &&
                        Vector2.Distance(x.GetTruePosition(), overtaken.GetTruePosition()) <= 3f))
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.PublicExecution);
                }

                break;

            case AbilityType.TraitorChangeRole when @event.Player.AmOwner:
                _becameTraitor = true;
                _meetingsWhenTraitor = _meetingsSeen;
                break;

            case AbilityType.BlackmailerBlackmail
                when @event.Player.AmOwner && @event.Target is PlayerControl jailed &&
                     jailed.HasModifier<JailedModifier>():
                AchievementsAPIHelper.Unlock(TouAchievements.UselessJail);
                break;

            case AbilityType.UndertakerDrag when @event.Player.AmOwner && @event.Target is DeadBody draggedBody:
                DragPickupPositions[draggedBody.ParentId] = draggedBody.transform.position;
                break;

            // Do a Barrelroll!: two or more bribes by the local Mercenary.
            case AbilityType.MercenaryBribe when @event.Player.AmOwner && @event.Target is PlayerControl bribed:
                MercenaryBribedIds.Add(bribed.PlayerId);
                if (MercenaryBribedIds.Count >= 2)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.DoABarrelroll);
                }

                break;

            case AbilityType.InquisitorInquire when @event.Player.AmOwner:
                _inquisitorUsedAbility = true;
                break;

            case AbilityType.WerewolfRampage when @event.Player.AmOwner:
                _rampageActive = true;
                _rampageEnded = false;
                _rampageKills = 0;
                _rampageAliveOthers = AliveOthers().Count;
                break;

            case AbilityType.ChefWin when @event.Player.AmOwner:
                AchievementsAPIHelper.Unlock(TouAchievements.HappyCustomers);
                break;

            case AbilityType.UndertakerDrop when @event.Player.AmOwner && @event.Target is DeadBody droppedBody:
                if (DragPickupPositions.TryGetValue(droppedBody.ParentId, out var pickupPos) &&
                    Vector2.Distance(droppedBody.transform.position, pickupPos) > 3f)
                {
                    DraggedBodies.Add(droppedBody.ParentId);
                }

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

            // Bloodshed: four or more evils killed while on Alert.
            if (IsEvil(target))
            {
                AchievementsAPIHelper.Increment(TouAchievements.Bloodshed, 1);
            }

            // Go Fetch: a Werewolf killed while on Alert.
            if (target.GetRoleWhenAlive() is WerewolfRole)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.GoFetch);
            }
        }

        // Track killers for Time Cop / Avenger / Lost Cause style checks.
        if (target.AmOwner)
        {
            LocalKillerIds.Add(source.PlayerId);
        }

        if (!target.AmOwner && !local.HasDied() && IsNearLocal(target))
        {
            NearbyKillerIds.Add(source.PlayerId);
        }

        // Injustice: an Egotist Officer or Sheriff shooting a non-killing crewmate.
        if (source.AmOwner && source.HasModifier<EgotistModifier>() &&
            (local.Data?.Role is OfficerRole || local.Data?.Role is SheriffRole) &&
            target.IsCrewmate() && !target.Is(RoleAlignment.CrewmateKilling))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Injustice);
        }

        // Caught Ghostly-Handed: the Medium saw a player die while Mediating.
        if (!target.AmOwner && local.Data?.Role is MediumRole && IsNearLocal(target) &&
            ModifierUtils.GetPlayersWithModifier<MediatedModifier>(x => x.MediumId == local.PlayerId).HasAny())
        {
            AchievementsAPIHelper.Unlock(TouAchievements.CaughtGhostlyHanded);
        }

        // Become the Hunted: the Hunter is killed by their stalked player.
        if (target.AmOwner && target.TryGetModifier<HunterStalkedModifier>(out var stalkedMod) &&
            stalkedMod.Hunter == source)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.BecomeTheHunted);
        }

        // Hunting Season: the Hunter kills a player they stalked.
        if (source.AmOwner && source.Data.Role is HunterRole && target.PlayerId != source.PlayerId &&
            HunterStalkedIds.Contains(target.PlayerId))
        {
            AchievementsAPIHelper.Increment(TouAchievements.HuntingSeason, 1);
        }

        // Avenger: the Sheriff shoots the killer of someone who died nearby.
        if (source.AmOwner && source.Data.Role is SheriffRole && NearbyKillerIds.Contains(target.PlayerId))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Avenger);
        }

        // Time Cop: the Officer shoots their killer after a Time Lord revive.
        if (source.AmOwner && source.Data.Role is OfficerRole && LocalKillerIds.Contains(target.PlayerId) &&
            TimeLordRevivedIds.Contains(source.PlayerId))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.TimeCop);
        }

        // Meeting guesses: a successful guess murders the victim, a misguess kills the guesser.
        if (MeetingHud.Instance && source.AmOwner && target.PlayerId != source.PlayerId)
        {
            if (source.Data.Role is VigilanteRole)
            {
                AchievementsAPIHelper.Increment(TouAchievements.TheDarkKnight, 1);

                // Shot in the Dark: a correct guess after all safe shots are gone.
                if (source.Data.Role is VigilanteRole vig && vig.SafeShotsLeft == 0 &&
                    OptionGroupSingleton<VigilanteOptions>.Instance.MultiShots > 0)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.ShotInTheDark);
                }

                if (++_vigilanteGuesses >= 2)
                {
                    AchievementsAPIHelper.Unlock(TouAchievements.DoubleTap);
                }
            }

            // Sharpshooter: a Deputy blast within 30 seconds of the meeting starting.
            if (source.Data.Role is DeputyRole && Time.time - _lastMeetingStartTime <= 30f)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.Sharpshooter);
            }

            // This Town Ain't Big Enough: a Deputy blasts an Assassin.
            if (source.Data.Role is DeputyRole && target.HasModifier<AssassinModifier>())
            {
                AchievementsAPIHelper.Unlock(TouAchievements.ThisTownAintBigEnough);
            }

            if (source.Data.Role is DoomsayerRole && ++_doomsayerGuesses >= 3)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.Nostradamus);
            }

            // Role-Eater: the Doomsayer met their win condition within 3 meetings.
            if (source.Data.Role is DoomsayerRole doomsayer && doomsayer.WinConditionMet() && _meetingsSeen <= 3)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.RoleEater);
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

        // Boogeyman: kills made while concealed as an ambushed body.
        if (source.AmOwner && source.HasModifier<AmbusherConcealedModifier>() && target.PlayerId != source.PlayerId)
        {
            AchievementsAPIHelper.Increment(TouAchievements.Boogeyman, 1);
        }

        // Framer: remember who the local Morphling was disguised as during a kill.
        if (source.AmOwner && source.TryGetModifier<MorphlingMorphModifier>(out var morphMod) &&
            morphMod.Target is { } morphed && morphed.PlayerId != target.PlayerId)
        {
            FramedIds.Add(morphed.PlayerId);
        }

        // Threat Eliminated: the Herbalist killed a killer they exposed this round.
        if (source.AmOwner && source.Data.Role is HerbalistRole && ExposedThisRoundIds.Contains(target.PlayerId) &&
            (target.Is(RoleAlignment.NeutralKilling) || target.Is(RoleAlignment.CrewmateKilling)))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.ThreatEliminated);
        }

        // Homewrecker: the puppet killed their own lover.
        if (source.TryGetModifier<PuppeteerControlModifier>(out var controlMod) && controlMod.Controller.AmOwner &&
            target.TryGetModifier<LoverModifier>(out var victimLover) && victimLover.OtherLover == source)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Homewrecker);
        }

        // Blood Moon: kills made while the local Werewolf is rampaging.
        if (source.AmOwner && source.Data.Role is WerewolfRole { Rampaging: true } &&
            target.PlayerId != source.PlayerId)
        {
            _rampageKills++;
        }

        // Bad Staring Contest: the local player was killed by a gazing Medusa.
        if (target.AmOwner && source.Data.Role is MedusaRole && source.HasModifier<MedusaGazingModifier>())
        {
            AchievementsAPIHelper.Unlock(TouAchievements.BadStaringContest);
        }

        // Lazybones: the local Inquisitor used Vanquish.
        if (source.AmOwner && source.Data.Role is InquisitorRole)
        {
            _inquisitorUsedAbility = true;
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

        // False Evidence: someone else reported a body the local Undertaker dragged away.
        if (@event.Reporter != null && !@event.Reporter.AmOwner && @event.Body != null &&
            DraggedBodies.Contains(@event.Body.ParentId))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.FalseEvidence);
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

        // Judgement Day: the meeting ended with the local Prosecutor's prosecution.
        _prosecutionEjected = local.Data?.Role is ProsecutorRole { HasProsecuted: true } localProsecutor &&
                              localProsecutor.ProsecuteVictim == exiled.PlayerId;

        // Lost Cause: a player the local Altruist revived dies again.
        if (AltruistRevivedIds.Remove(exiled.PlayerId))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.LostCause);
        }

        // Boston Tea Party: the Monarch was voted out by their own Knight.
        if (exiled.AmOwner && BetrayedMonarchIds.Contains(exiled.PlayerId))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.BostonTeaParty);
        }

        // Sike: the other swap target got ejected after the Swapper swapped votes on themself.
        foreach (var (swapperId, otherId) in SwapSelfPairs)
        {
            var selfSwapper = MiscUtils.PlayerById(swapperId);
            if (selfSwapper != null && selfSwapper.AmOwner && exiled.PlayerId == otherId)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.Sike);
            }
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

        // Framer: a player the local Morphling killed while disguised as got voted out.
        if (FramedIds.Contains(exiled.PlayerId))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Framer);
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
            exiled.HasModifier<ExecutionerTargetModifier>(mod => mod.OwnerId == local.PlayerId))
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

        // Endgame: an ejection may have left only a Juggernaut and a Pestilence alive.
        Coroutines.Start(CoCheckHorsemanDuel());
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent _)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        _meetingsSeen++;
        _hexBombCompleted = false;
        _rampageActive = false;
        _vigilanteGuesses = 0;
        _doomsayerGuesses = 0;
        _assassinGuesses = 0;
        _lastMeetingStartTime = Time.time;
        _prosecutionEjected = false;
        _snitchRevealedLastMeeting = _snitchCompletedTasks;
        _snitchCompletedTasks = false;

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
        DoomsayerObservedRoleLists.Clear();
    }

    [RegisterEvent]
    public static void VotingCompleteEventHandler(VotingCompleteEvent _)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        // Boston Tea Party: record monarchs that their own knights voted for.
        BetrayedMonarchIds.Clear();
        foreach (var knight in ModifierUtils.GetPlayersWithModifier<KnightedModifier>())
        {
            if (KnightToMonarch.TryGetValue(knight.PlayerId, out var monarchId) &&
                knight.GetVoteData().Votes.Any(vote => vote.Suspect == monarchId))
            {
                BetrayedMonarchIds.Add(monarchId);
            }
        }

        // Sike: record swappers whose swap targets included themselves.
        SwapSelfPairs.Clear();
        foreach (var swapper in CustomRoleUtils.GetActiveRolesOfType<SwapperRole>())
        {
            if (!swapper.Swap1 || !swapper.Swap2)
            {
                continue;
            }

            var swapperId = swapper.Player.PlayerId;
            if (swapper.Swap1.PlayerId == swapperId)
            {
                SwapSelfPairs.Add((swapperId, swapper.Swap2.PlayerId));
            }
            else if (swapper.Swap2.PlayerId == swapperId)
            {
                SwapSelfPairs.Add((swapperId, swapper.Swap1.PlayerId));
            }
        }
    }

    [RegisterEvent]
    public static void PlayerReviveEventHandler(PlayerReviveEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        var revived = @event.Player;
        if (revived == null)
        {
            return;
        }

        _reviveTimes[revived.PlayerId] = Time.time;
        if (PendingAltruistRevives.Remove(revived.PlayerId))
        {
            AltruistRevivedIds.Add(revived.PlayerId);
        }

        // But it refused.: the local player and their lover were revived at the same time.
        if (revived.TryGetModifier<LoverModifier>(out var loverMod) && loverMod.OtherLover is { } otherLover &&
            !otherLover.HasDied() && _reviveTimes.TryGetValue(otherLover.PlayerId, out var loverReviveTime) &&
            Mathf.Abs(Time.time - loverReviveTime) <= 3f && (revived.AmOwner || otherLover.AmOwner))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.ButItRefused);
        }
    }

    [RegisterEvent]
    public static void GameEndEventHandler(GameEndEvent _)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null)
        {
            return;
        }

        // Last Crew Standing: the game ended right after the Mystic sensed a death.
        if (local.GetRoleWhenAlive() is MysticRole && !local.HasDied() && Time.time - _mysticSenseTime <= 5f)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.LastCrewStanding);
        }

        // Fallen Knights: the Monarch wins and survives while all of their knights are dead.
        if (local.Data.Role is MonarchRole && LocalWon() && !local.HasDied())
        {
            var ownKnights = KnightToMonarch.Where(kv => kv.Value == local.PlayerId).Select(kv => kv.Key);
            if (ownKnights.Any() && ownKnights.All(id => MiscUtils.PlayerById(id)?.HasDied() != false))
            {
                AchievementsAPIHelper.Unlock(TouAchievements.FallenKnights);
            }
        }

        // Circus Performer: the Monarch knighted a Jester who went on to win.
        if (local.GetRoleWhenAlive() is MonarchRole && KnightToMonarch.Any(kv =>
                kv.Value == local.PlayerId && IsWinner(MiscUtils.PlayerById(kv.Key)) &&
                MiscUtils.PlayerById(kv.Key)?.GetRoleWhenAlive() is JesterRole))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.CircusPerformer);
        }

        // Judgement Day: the game ended with the local Prosecutor's prosecution.
        if (_prosecutionEjected && LocalWon())
        {
            AchievementsAPIHelper.Unlock(TouAchievements.JudgementDay);
        }

        // Intact: the Medic's shielded player survived to the end without ever being attacked.
        if (local.GetRoleWhenAlive() is MedicRole && !_medicShieldAttacked && _meetingsSeen >= 1 &&
            ModifierUtils.GetPlayersWithModifier<MedicShieldModifier>(x => x.Medic.AmOwner && !x.Player.HasDied()).HasAny())
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Intact);
        }

        // Mimicry: the Imitator only imitated non-Crewmates, imitating at least twice.
        if (Imitations.Count >= 2 && Imitations.All(x => !x.IsCrewmate))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Mimicry);
        }

        // Undercover: a living Crewpostor who never became the Traitor won.
        if (local.HasModifier<CrewpostorModifier>() && !local.HasDied() && LocalWon() && !_becameTraitor)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Undercover);
        }

        // World-Ender: the Spellslinger's Hex Bomb finished and they won the game.
        if (_hexBombCompleted && local.GetRoleWhenAlive() is SpellslingerRole && LocalWon())
        {
            AchievementsAPIHelper.Unlock(TouAchievements.WorldEnder);
        }

        // Deadly Delusion: the Traitor won within two rounds of becoming the Traitor.
        if (_becameTraitor && LocalWon() && _meetingsSeen - _meetingsWhenTraitor <= 1)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.DeadlyDelusion);
        }

        // Win-Win Situation: the Mercenary won with at least two-thirds of the living bribed.
        if (local.GetRoleWhenAlive() is MercenaryRole && LocalWon())
        {
            var others = AliveOthers();
            var bribed = others.Count(x =>
                x.HasModifier<MercenaryBribedModifier>(mod => mod.Mercenary.AmOwner));
            if (others.Count >= 2 && bribed * 3 >= others.Count * 2)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.WinWinSituation);
            }
        }

        // Survival of the Fittest: the Survivor was the sole winner.
        if (local.GetRoleWhenAlive() is SurvivorRole && LocalWon() &&
            EndGameResult.CachedWinners.Count == 1)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.SurvivalOfTheFittest);
        }

        // Third-Wheel: the Survivor won alongside the Lovers.
        if (local.GetRoleWhenAlive() is SurvivorRole && LocalWon() &&
            EndGameResult.CachedGameOverReason == CustomGameOver.GameOverReason<LoverGameOver>())
        {
            AchievementsAPIHelper.Unlock(TouAchievements.ThirdWheel);
        }

        // Carrier: the Plaguebearer won without ever transforming into Pestilence.
        if (local.GetRoleWhenAlive() is PlaguebearerRole && LocalWon())
        {
            AchievementsAPIHelper.Unlock(TouAchievements.CommonCold);
        }

        // Blood Moon: the Werewolf killed every other living player in a single Rampage.
        if (local.GetRoleWhenAlive() is WerewolfRole && LocalWon() && _rampageActive &&
            _rampageKills >= 2 && _rampageKills >= _rampageAliveOthers && _rampageAliveOthers > 0)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.BloodMoon);
        }

        // Lazybones: the Inquisitor won without inquiring or vanquishing.
        if (local.GetRoleWhenAlive() is InquisitorRole && LocalWon() && !_inquisitorUsedAbility)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Lazybones);
        }
    }

    private static IEnumerator CoCheckHorsemanDuel()
    {
        // Defer a frame so the dying/ejected player's death state has been applied.
        yield return null;

        if (!AchievementsAPIHelper.IsLoaded || AchievementsAPIHelper.IsUnlocked(TouAchievements.Endgame))
        {
            yield break;
        }

        var alive = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => !x.HasDied() && !x.Data.Disconnected && x.Data.Role is not SpectatorRole);
        if (alive.Count() == 2 && alive.Any(x => x.Data.Role is JuggernautRole) &&
            alive.Any(x => x.Data.Role is PestilenceRole))
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Endgame);
        }
    }

    private static IEnumerator CoSwoopScan()
    {
        _swoopScanActive = true;
        var local = PlayerControl.LocalPlayer;

        while (local != null && local.HasModifier<SwoopModifier>() && !local.HasDied() && !MeetingHud.Instance)
        {
            foreach (var player in PlayerControl.AllPlayerControls.ToArray())
            {
                if (player.AmOwner || player.HasDied() || player.Data.Disconnected || player.IsImpostor())
                {
                    continue;
                }

                if (Vector2.Distance(local.GetTruePosition(), player.GetTruePosition()) <= 1f)
                {
                    SwoopNearbyIds.Add(player.PlayerId);
                }
            }

            if (SwoopNearbyIds.Count >= 3)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.UnderTheRadar);
                break;
            }

            yield return new WaitForSeconds(0.2f);
        }

        _swoopScanActive = false;
    }

    private static bool LocalWon()
    {
        var local = PlayerControl.LocalPlayer;
        if (local?.Data == null)
        {
            return false;
        }

        return EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == local.Data.PlayerName);
    }

    private static bool IsWinner(PlayerControl? player)
    {
        if (player?.Data == null)
        {
            return false;
        }

        return EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == player.Data.PlayerName);
    }

    private static bool IsEvil(PlayerControl player)
    {
        return player.IsImpostor() || player.Is(RoleAlignment.NeutralKilling) || player.Is(RoleAlignment.NeutralEvil);
    }

    private static bool IsNearLocal(PlayerControl player)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || player == null)
        {
            return false;
        }

        var radius = 3f;
        if (ShipStatus.Instance != null && local.Data != null)
        {
            radius = ShipStatus.Instance.CalculateLightRadius(local.Data);
        }

        return Vector2.Distance(local.transform.position, player.transform.position) <= radius;
    }

    private static List<PlayerControl> AliveOthers()
    {
        return PlayerControl.AllPlayerControls.ToArray()
            .Where(x => !x.HasDied() && !x.AmOwner && !x.Data.Disconnected && x.Data.Role is not SpectatorRole)
            .ToList();
    }

    private static bool IsLoverOf(MonoBehaviour? target, PlayerControl lover)
    {
        return target is PlayerControl player && player.TryGetModifier<LoverModifier>(out var loverMod) &&
               loverMod.OtherLover == lover;
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        _sabotageStartTime = -999f;
        _hexBombCompleted = false;
        _rampageActive = false;
        FramedIds.Clear();
        ExposedThisRoundIds.Clear();
        _swoopsThisRound = 0;
        _bootlegServeTarget = byte.MaxValue;
        _bootlegServeCount = 0;
        ServedLocalIds.Clear();

        // Delusional: the Hypnotist's hysteria is still going into another round.
        var roundLocal = PlayerControl.LocalPlayer;
        if (!@event.TriggeredByIntro && roundLocal?.Data?.Role is HypnotistRole { HysteriaActive: true })
        {
            _hysteriaRounds++;
            if (_hysteriaRounds >= 3)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.Delusional);
            }
        }

        if (!@event.TriggeredByIntro)
        {
            // Endgame: a disconnect may have left only a Juggernaut and a Pestilence alive.
            Coroutines.Start(CoCheckHorsemanDuel());
            return;
        }

        KnightedIds.Clear();
        ImitatedRoles.Clear();
        PendingBombVictims.Clear();
        _bombKills = 0;
        _meetingsSeen = 0;

        KnightToMonarch.Clear();
        BetrayedMonarchIds.Clear();
        AltruistRevivedIds.Clear();
        PendingAltruistRevives.Clear();
        _reviveTimes.Clear();
        TimeLordRevivedIds.Clear();
        LocalKillerIds.Clear();
        NearbyKillerIds.Clear();
        HunterStalkedIds.Clear();
        SeerEnemyPairs.Clear();
        SwapSelfPairs.Clear();
        Imitations.Clear();
        _prosecutions = 0;
        _prosecutionEjected = false;
        _medicShieldAttacked = false;
        _snitchCompletedTasks = false;
        _snitchRevealedLastMeeting = false;
        _lastMeetingStartTime = -999f;

        RetrainedIds.Clear();
        PuppeteerControlCounts.Clear();
        DragPickupPositions.Clear();
        DraggedBodies.Clear();
        SwoopNearbyIds.Clear();
        _becameTraitor = false;
        _meetingsWhenTraitor = 0;
        _scavengerStreak = 0;
        _hysteriaRounds = 0;
        _swoopScanActive = false;
        MercenaryBribedIds.Clear();
        ServedLocalIds.Clear();
        DoomsayerObservedRoleLists.Clear();
        _inquisitorUsedAbility = false;
        _rampageKills = 0;
        _rampageAliveOthers = 0;
    }

    [RegisterEvent]
    public static void ChangeRoleEventHandler(ChangeRoleEvent @event)
    {
        if (!AchievementsAPIHelper.IsLoaded)
        {
            return;
        }

        var player = @event.Player;
        if (!player.AmOwner || @event.NewRole is ImitatorRole || @event.OldRole is not ImitatorRole)
        {
            return;
        }

        Imitations.Add((@event.NewRole.GetType(), @event.NewRole.IsCrewmate()));

        // Powerless: the Imitator copied a Crewmate Power role.
        if (@event.NewRole is ITownOfUsRole imitatedRole && imitatedRole.RoleAlignment is RoleAlignment.CrewmatePower)
        {
            AchievementsAPIHelper.Unlock(TouAchievements.Powerless);
        }

        if (!AchievementsAPIHelper.IsUnlocked(TouAchievements.MethodActor))
        {
            ImitatedRoles.Add(@event.NewRole.GetType());
            if (ImitatedRoles.Count >= 3)
            {
                AchievementsAPIHelper.Unlock(TouAchievements.MethodActor);
            }
        }
    }
}