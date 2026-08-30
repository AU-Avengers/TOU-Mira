using AmongUs.GameOptions;
using MiraAPI.Patches.Freeplay;
using HarmonyLib;
using MiraAPI;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Networking;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch]
public static class MiraApiPatches
{
    [HarmonyPatch(typeof(Helpers), nameof(Helpers.IsRoleBlacklisted))]
    [HarmonyPrefix]
    public static bool IsRoleBlacklisted(RoleBehaviour role, ref bool __result)
    {
        // Since TOU Engineer is just vanilla engineer with the fix mechanic, no need to have two engis around!
        // Also Judge being a near carbon copy of Prosecutor is lame.
        if (role.Role is RoleTypes.Engineer or RoleTypes.Judge)
        {
            __result = true;
            return false;
        }
        if (MiscUtils.CurrentGamemode() is TouGamemode.HideAndSeek && (role.Role is not RoleTypes.Engineer and not RoleTypes.Impostor))
        {
            __result = true;
            return false;
        }
        if (MiscUtils.CurrentGamemode() is not TouGamemode.Normal)
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
        __result = MiraLocaleManager.Get("NeutralKeyword").ToUpperInvariant();
        return false;
    }
    [HarmonyPatch(typeof(TaskAdderPatches), nameof(TaskAdderPatches.NeutralName), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool NeutralNamePrefix(ref string __result)
    {
        __result = MiraLocaleManager.Get("NeutralKeyword");
        return false;
    }
    [HarmonyPatch(typeof(TaskAdderPatches), nameof(TaskAdderPatches.ModifiersName), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool ModifierNamePrefix(ref string __result)
    {
        __result = MiraLocaleManager.Get("Modifiers");
        return false;
    }

    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.CustomMurder), typeof(PlayerControl), typeof(PlayerControl), typeof(MurderResultFlags), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.CustomMurder), typeof(PlayerControl), typeof(PlayerControl), typeof(PlayerControl), typeof(bool), typeof(bool), typeof(MurderResultFlags), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
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
        var defenseFlag = beforeMurderEvent.IgnoreDefense;
        var indirectFlag = beforeMurderEvent.IsIndirectAttack;
        var isMeetingActive = MeetingHud.Instance || ExileController.Instance;
        if ((inMeeting is MeetingCheck.ForMeeting && !isMeetingActive) || (inMeeting is MeetingCheck.OutsideMeeting && isMeetingActive))
        {
            beforeMurderEvent.Cancel();
        }

        if (!defenseFlag && target.ProtectedByGa())
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
            indirectFlag,
            defenseFlag,
            murderResultFlags,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);
        return false;
    }

    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.RpcConfirmAdvancedCustomMurder), typeof(PlayerControl), typeof(PlayerControl), typeof(PlayerControl), typeof(bool), typeof(bool), typeof(MurderResultFlags), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
    [HarmonyPrefix]
    public static bool RpcConfirmAdvancedCustomMurderPatch(
        this PlayerControl host,
        PlayerControl source,
        PlayerControl target,
        bool isIndirect,
        bool ignoreDefense,
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
            null,
            isIndirect,
            ignoreDefense,
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

    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.RpcConfirmFramedCustomMurder), typeof(PlayerControl), typeof(PlayerControl), typeof(PlayerControl), typeof(PlayerControl), typeof(bool), typeof(bool), typeof(MurderResultFlags), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
    [HarmonyPrefix]
    public static bool RpcConfirmFramedCustomMurderPatch(
        this PlayerControl host,
        PlayerControl source,
        PlayerControl target,
        PlayerControl framed,
        bool isIndirect,
        bool ignoreDefense,
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
            framed,
            isIndirect,
            ignoreDefense,
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

    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.RpcConfirmCustomMurder), typeof(PlayerControl), typeof(PlayerControl), typeof(PlayerControl), typeof(bool), typeof(bool), typeof(MurderResultFlags), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
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

    [HarmonyPatch(typeof(HowToPlayScene), nameof(HowToPlayScene.OpenRolesSelectionMenu))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool HowToPlayPrefix(HowToPlayScene __instance)
    {
        if (MiraApiPlugin.IsDevBuild)
        {
            // This is simply a fix for public release, no api update needed.
            return true;
        }

        __instance.sceneIndex = 0;
        __instance.category = HowToPlayScene.HowToPlayCategory.RolesSelection;
        __instance.startPage.SetActive(false);
        if (__instance.roleButtonsParent.childCount == 0)
        {
            foreach (var role in RoleManager.Instance.AllRoles.ToArray().Where(x => !x.IsCustomRole()))
            {
                if (!role.IsSimpleRole && role.Role != RoleTypes.CrewmateGhost && role.Role != RoleTypes.ImpostorGhost)
                {
                    HowToPlayRoleButton component = UnityEngine.Object
                        .Instantiate(__instance.roleButtonPrefab, __instance.roleButtonsParent)
                        .GetComponent<HowToPlayRoleButton>();
                    Sprite roleIcon = __instance.rolesScenes.ToArray().First(r => r.role == role.Role).roleIcon;
                    component.SetRoleInfo(role, roleIcon);
                    component.SetButtonAction((Il2CppSystem.Action)(() => { OpenRolePage(__instance, role.Role); }));
                    __instance.controllerSelectables.Add(component.GetComponent<PassiveButton>());
                }
            }

            foreach (UiElement uiElement in __instance.controllerSelectables)
            {
                uiElement.ReceiveMouseOut();
            }

            ControllerManager.Instance.NewScene(__instance.name, __instance.closeButton,
                __instance.defaultButtonSelected, __instance.controllerSelectables, false);
        }

        __instance.DisableAllScenes();
        __instance.roleSelectionScene.SetActive(true);
        ControllerManager.Instance.SetDefaultSelection(__instance.defaultButtonSelected, null);
        return false;
    }

    public static void OpenRolePage(HowToPlayScene instance, RoleTypes roleType)
    {
        instance.category = HowToPlayScene.HowToPlayCategory.Roles;
        var newList = instance.rolesScenes.ToArray().ToList();
        var buttonList = instance.roleButtons;
        instance.sceneIndex = newList.FindIndex(r => r.role == roleType);
        if (roleType != RoleTypes.Crewmate)
        {
            foreach (var button in buttonList)
            {
                if (button.GetRole().Role == roleType)
                {
                    instance.previouslySelectedRoleButton = button.GetComponent<PassiveButton>();
                }
            }
        }
        instance.SetupDots(instance.rolesScenes[instance.sceneIndex].rolePages.Count);
        instance.ChangeScene(0);
    }
}
