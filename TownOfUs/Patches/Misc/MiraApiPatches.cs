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

    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.CustomMurder))]
    [HarmonyPrefix]
    public static bool CustomMurderPatch(PlayerControl source)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.RpcCustomMurder), typeof(PlayerControl), typeof(PlayerControl), typeof(MeetingCheck), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
    [HarmonyPrefix]
    public static bool RpcAltCustomMurderPatch(
        this PlayerControl source,
        PlayerControl target,
        MeetingCheck inMeeting,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return false;
        }
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target, inMeeting);
        MiraEventManager.InvokeEvent(beforeMurderEvent);
        var isMeetingActive = MeetingHud.Instance || ExileController.Instance;
        if ((inMeeting is MeetingCheck.ForMeeting && !isMeetingActive) || (inMeeting is MeetingCheck.OutsideMeeting && isMeetingActive))
        {
            beforeMurderEvent.Cancel();
        }

        if (target.ProtectedByGa())
        {
            beforeMurderEvent.Cancel();
            murderResultFlags = MurderResultFlags.FailedProtected;
        }
        else if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        if (beforeMurderEvent.IsCancelled && source.AmOwner)
        {
            source.isKilling = true;
        }

        // Track kill cooldown before CustomMurder for Time Lord rewind
        CustomTouMurderRpcs.RecordedKillCooldown = -1f;
        if (resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
        {
            CustomTouMurderRpcs.RecordedKillCooldown = source.killTimer;
        }

        if (!PlayerControl.LocalPlayer.IsHost())
        {
            return false;
        }

        CustomMurderRpc.RpcConfirmCustomMurder(
            PlayerControl.LocalPlayer,
            source,
            target,
            murderResultFlags,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);
        return false;
    }

    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.RpcConfirmCustomMurder), typeof(PlayerControl), typeof(PlayerControl), typeof(PlayerControl), typeof(MurderResultFlags), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
    [HarmonyPrefix]
    public static bool RpcConfirmCustomMurderPatch(
        this PlayerControl host,
        PlayerControl source,
        PlayerControl target,
        MurderResultFlags murderResultFlags,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        if (LobbyBehaviour.Instance)
        {
            source.isKilling = false;
            MiscUtils.RunAnticheatWarning(source);
            return false;
        }
        if (!host.IsHost() || target.HasDied())
        {
            return false;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        source.CustomMurder(
            target,
            murderResultFlags2,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);

        // Record kill cooldown change after CustomMurder if it was reset
        if (CustomTouMurderRpcs.RecordedKillCooldown > -1f && resetKillTimer && source.AmOwner && source.Data?.Role?.CanUseKillButton == true)
        {
            Coroutines.Start(CustomTouMurderRpcs.CoRecordKillCooldownAfterCustomMurder(source, CustomTouMurderRpcs.RecordedKillCooldown));
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
