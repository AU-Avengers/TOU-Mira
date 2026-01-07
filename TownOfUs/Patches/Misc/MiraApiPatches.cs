using AmongUs.GameOptions;
using MiraAPI.Patches.Freeplay;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Networking;
using MiraAPI.Patches.Options;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Networking;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch]
public static class MiraApiPatches
{
    [HarmonyPatch(typeof(Helpers), nameof(Helpers.IsRoleBlacklisted))]
    [HarmonyPrefix]
    public static bool IsRoleBlacklisted(RoleBehaviour role, ref bool __result)
    {
        // Since TOU Engineer is just vanilla engineer with the fix mechanic, no need to have two engis around!
        if (role.Role is RoleTypes.Engineer)
        {
            __result = true;
            return false;
        }

        if (MiscUtils.CurrentGamemode() is TouGamemode.HideAndSeek && (role.Role is RoleTypes.Detective ||
                                                                       role.Role is RoleTypes.GuardianAngel ||
                                                                       role.Role is RoleTypes.Noisemaker ||
                                                                       role.Role is RoleTypes.Phantom ||
                                                                       role.Role is RoleTypes.Scientist ||
                                                                       role.Role is RoleTypes.Shapeshifter ||
                                                                       role.Role is RoleTypes.Tracker ||
                                                                       role.Role is RoleTypes.Viper))
        {
            __result = true;
            return false;
        }
        return true;
    }
    [HarmonyPatch(typeof(TeamIntroConfiguration), nameof(TeamIntroConfiguration.Neutral.IntroTeamTitle), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool NeutralTeamPrefix(ref string __result)
    {
        __result = TouLocale.Get("NeutralKeyword").ToUpperInvariant();
        return false;
    }
    [HarmonyPatch(typeof(TaskAdderPatches), nameof(TaskAdderPatches.NeutralName), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool NeutralNamePrefix(ref string __result)
    {
        __result = TouLocale.Get("NeutralKeyword");
        return false;
    }
    [HarmonyPatch(typeof(TaskAdderPatches), nameof(TaskAdderPatches.ModifiersName), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool ModifierNamePrefix(ref string __result)
    {
        __result = TouLocale.Get("Modifiers");
        return false;
    }
    
    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.RpcCustomMurder))]
    [HarmonyPrefix]
    public static bool RpcCustomMurderPatch(
        this PlayerControl source,
        PlayerControl target,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target);
        MiraEventManager.InvokeEvent(beforeMurderEvent);

        if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        // Track kill cooldown before CustomMurder for Time Lord rewind
        float? killCooldownBefore = null;
        if (resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
        {
            killCooldownBefore = source.killTimer;
        }

        source.CustomMurder(
            target,
            murderResultFlags2,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);

        // Force-sync death state after successful murder to prevent desyncs
        if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) && target.HasDied())
        {
            DeathStateSync.ScheduleDeathStateSync(target, true);
            // Request validation after kill to ensure all clients are in sync
            if (source.AmOwner)
            {
                DeathStateSync.RequestValidationAfterKill(source);
            }
        }

        // Record kill cooldown change after CustomMurder if it was reset
        if (killCooldownBefore.HasValue && resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
        {
            Coroutines.Start(CustomTouMurderRpcs.CoRecordKillCooldownAfterCustomMurder(source, killCooldownBefore.Value));
        }
        return false;
    }

    [HarmonyPatch(typeof(RoleSettingMenuPatches), nameof(RoleSettingMenuPatches.ClosePatch))]
    [HarmonyPrefix]
#pragma warning disable S3400
    public static bool MiraClosePatch()
#pragma warning restore S3400
    {
        // Patching this for now
        return false;
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    [HarmonyPostfix]
    public static void OpenPatch()
    {
        HudManager.Instance.PlayerCam.OverrideScreenShakeEnabled = false;
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Close))]
    [HarmonyPostfix]
    public static void ClosePatch()
    {
        HudManager.Instance.PlayerCam.OverrideScreenShakeEnabled = true;
    }
}
