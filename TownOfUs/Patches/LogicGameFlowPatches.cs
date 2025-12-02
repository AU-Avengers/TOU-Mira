using HarmonyLib;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Events;
using TownOfUs.GameOver;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class LogicGameFlowPatches
{
    public static bool CheckEndGameViaTasks(LogicGameFlowNormal instance)
    {
        GameData.Instance.RecomputeTaskCounts();

        if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
        {
            instance.Manager.RpcEndGame(GameOverReason.CrewmatesByTask, false);

            return true;
        }

        return false;
    }

    public static bool CheckEndGameViaTimeLimit(LogicGameFlowNormal instance)
    {
        if (OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled && GameTimerPatch.TriggerEndGame)
        {
            var timeType = (GameTimerType)OptionGroupSingleton<GameTimerOptions>.Instance.TimerEndOption.Value;
            if (timeType is GameTimerType.Impostors)
            {
                instance.Manager.RpcEndGame(GameOverReason.ImpostorsBySabotage, false);
            }
            else
            {
                var randomPlayer = PlayerControl.AllPlayerControls.ToArray().Where(x =>
                    !x.Data.Role.DidWin(CustomGameOver.GameOverReason<DrawGameOver>()) && !x
                        .GetModifiers<GameModifier>()
                        .Any(x => x.DidWin(CustomGameOver.GameOverReason<DrawGameOver>()) == true)).Random();
                CustomGameOver.Trigger<DrawGameOver>([
                    randomPlayer != null ? randomPlayer.Data : PlayerControl.LocalPlayer.Data
                ]);
            }

            GameTimerPatch.TriggerEndGame = false;
            return true;
        }

        return false;
    }

    public static bool CheckEndGameViaHexBomb(LogicGameFlowNormal instance)
    {
        if (HexBombSabotageSystem.BombFinished && SpellslingerRole.EveryoneHexed() && CustomRoleUtils.GetActiveRolesOfType<SpellslingerRole>().Any())
        {
            instance.Manager.RpcEndGame(GameOverReason.ImpostorsBySabotage, false);
            return true;
        }
        return false;
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
    [HarmonyPrefix]
    private static bool RecomputeTasksPatch(GameData __instance)
    {
        if (MiscUtils.CurrentGamemode() is TouGamemode.HideAndSeek)
        {
            return true;
        }

        if (__instance == null || GameOptionsManager.Instance == null || GameManager.Instance == null)
        {
            return false;
        }

        __instance.TotalTasks = 0;
        __instance.CompletedTasks = 0;
        for (var i = 0; i < __instance.AllPlayers.Count; i++)
        {
            var playerInfo = __instance.AllPlayers.ToArray()[i];
            if (playerInfo == null || playerInfo.Disconnected || !playerInfo.Object || playerInfo.Tasks == null || playerInfo.Object == null || !playerInfo.Object.TryGetComponent<ModifierComponent>(out _))
            {
                continue;
            }

            if ((!playerInfo.IsDead || GameOptionsManager.Instance.currentNormalGameOptions.GhostsDoTasks) &&
                !playerInfo._object.IsImpostor() &&
                !(
                    (playerInfo._object.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.DoesTasks)
                    || !playerInfo._object.Data.Role.TasksCountTowardProgress
                ))
            {
                for (var j = 0; j < playerInfo.Tasks.Count; j++)
                {
                    __instance.TotalTasks++;
                    if (playerInfo.Tasks.ToArray()[j].Complete)
                    {
                        __instance.CompletedTasks++;
                    }
                }
            }
        }

        if (__instance.TotalTasks == 0)
        {
            __instance.TotalTasks =
                1; // This results in avoiding unfair task wins by essentially defaulting to 0/1 which can never lead to a win
        }

        return false;
    }

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    [HarmonyPrefix]
    public static bool CheckEndCriteriaPatch(LogicGameFlowNormal __instance)
    {
        if (TutorialManager.InstanceExists)
        {
            return true;
        }

        if (!AmongUsClient.Instance.AmHost)
        {
            return false;
        }

        if (!GameData.Instance)
        {
            return false;
        }

        // Prevents game end on exile screen
        if (ExileController.Instance)
        {
            return false;
        }

        if (ShipStatus.Instance.Systems.ContainsKey(SystemTypes.LifeSupp))
        {
            var lifeSuppSystemType = ShipStatus.Instance.Systems[SystemTypes.LifeSupp].Cast<LifeSuppSystemType>();
            if (lifeSuppSystemType is { Countdown: < 0f })
            {
                __instance.EndGameForSabotage();
                lifeSuppSystemType.Countdown = 10000f;

                return false;
            }
        }

        foreach (var systemType2 in ShipStatus.Instance.Systems.Values)
        {
            var sabo = systemType2.TryCast<ICriticalSabotage>();
            if (sabo == null)
            {
                continue;
            }

            if (sabo.Countdown < 0f)
            {
                __instance.EndGameForSabotage();
                sabo.ClearSabotage();
            }
        }

        if (CheckEndGameViaHexBomb(__instance))
        {
            return false;
        }

        if (CheckEndGameViaTasks(__instance))
        {
            return false;
        }

        if (CheckEndGameViaTimeLimit(__instance))
        {
            return false;
        }

        if (DeathHandlerModifier.IsCoroutineRunning || DeathHandlerModifier.IsAltCoroutineRunning || DeathEventHandlers.IsDeathRecent)
        {
            return false;
        }

        // End game if there are 3 players alive and 2 are lovers.
        var activeLovers = ModifierUtils.GetActiveModifiers<LoverModifier>().ToArray();
        if (!ExileController.Instance && LoverModifier.WinConditionMet(activeLovers))
        {
            CustomGameOver.Trigger<LoverGameOver>(activeLovers.Select(x => x.Player.Data).ToArray());
            return false;
        }

        // If any neutral win condition is met -> game over
        // Using RoleAlignment as a quick and basic way to prioritise NeutralEvil wins over NeutralKiller wins
        if (CustomRoleUtils.GetActiveRolesOfTeam(ModdedRoleTeams.Custom)
                .OrderBy(x => x.GetRoleAlignment())
                .FirstOrDefault(x => x is ITownOfUsRole role && role.WinConditionMet()) is { } winner)
        {
            Message($"Game Over");
            CustomGameOver.Trigger<NeutralGameOver>([winner.Player.Data]);

            return false;
        }

        // Prevents game end when all impostors are dead but there are neutral killers alive, or when roles like doom are present.
        if (MiscUtils.NKillersAliveCount > 0 ||
            (MiscUtils.ImpAliveCount > 0 && MiscUtils.CrewKillersAliveCount > 0) ||
            (MiscUtils.GameHaltersAliveCount > 0 && Helpers.GetAlivePlayers().Count > 1))
        {
            return false;
        }

        // Causes the game to draw in extreme scenarios
        if (Helpers.GetAlivePlayers().Count <= 0)
        {
            var randomPlayer = PlayerControl.AllPlayerControls.ToArray().Where(x =>
                !x.Data.Role.DidWin(CustomGameOver.GameOverReason<DrawGameOver>()) && !x.GetModifiers<GameModifier>()
                    .Any(x => x.DidWin(CustomGameOver.GameOverReason<DrawGameOver>()) == true)).Random();
            CustomGameOver.Trigger<DrawGameOver>([
                randomPlayer != null ? randomPlayer.Data : PlayerControl.LocalPlayer.Data
            ]);
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
    public static void IsGameOverDueToDeathPatch(LogicGameFlowNormal __instance, ref bool __result)
    {
        __result = false;
    }
}