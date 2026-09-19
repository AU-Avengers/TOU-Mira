using AmongUs.Data;
using HarmonyLib;
using Innersloth.Assets;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class StabilityPatches
{
    [HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
    [HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickUp))]
    [HarmonyPrefix]
    public static bool PrefixClick(PassiveButton __instance)
    {
        if (__instance == null || __instance.Pointer == IntPtr.Zero || __instance.WasCollected)
        {
            return false;
        }

        return true;
    }
    /*[HarmonyPatch(typeof(HudManager), nameof(HudManager.OpenMeetingRoom))]
    [HarmonyPrefix]
    public static bool PrefixClick(HudManager __instance, PlayerControl reporter)
    {
        if (MeetingHud.Instance)
        {
            return false;
        }
        Info("Opening meeting room: " + ((reporter != null) ? reporter.ToString() : null));
        ShipStatus.Instance.RepairCriticalSabotages();
        MeetingHud.Instance = UnityEngine.Object.Instantiate(__instance.MeetingPrefab);
        if (reporter == null)
        {
            Error($"Meeting has a null reporter, resorting to displaying the local player!");
            MeetingHud.Instance.ServerStart(PlayerControl.LocalPlayer.PlayerId);
        }
        else
        {
            Info($"{reporter.CachedPlayerData.PlayerName} is starting a meeting!");
            MeetingHud.Instance.ServerStart(reporter.PlayerId);
        }
        AmongUsClient.Instance.Spawn(MeetingHud.Instance);
        try
        {
            GameData.OnMeetingStart();
            __instance.Chat.OnMeetingStart();
        }
        catch (Exception e)
        {
            Error(e);
        }
        return false;
    }*/

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Toggle))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPrefix]
    public static bool Toggle(ChatController __instance)
    {
        CustomNetworkTransform customNetworkTransform = PlayerControl.LocalPlayer ? PlayerControl.LocalPlayer.NetTransform : null!;
        // TODO: Uncomment and test this to make sure Parasite Chat doesn't break once match info guide is actually used.
        if (!customNetworkTransform /*|| MatchInfoGuide.Instance && MatchInfoGuide.Instance.IsActive*/)
        {
            return false;
        }

        try
        {
            if (FriendsListManager.InstanceExists && FriendsListManager.Instance.Ui.gameObject &&
                FriendsListManager.Instance.Ui.gameObject.activeSelf)
            {
                return false;
            }
        }
        catch
        {
            // ignored cause of innerslop
        }
        if (PlayerCustomizationMenu.Instance && PlayerCustomizationMenu.Instance.cosmicubeMenu.activeSelf)
        {
            return false;
        }
        __instance.StopAllCoroutines();
        if (__instance.IsOpenOrOpening)
        {
            __instance.StartCoroutine(__instance.CoClose());
            if (FriendsListManager.InstanceExists)
            {
                FriendsListManager.Instance.SetFriendButtonColor(false);
            }
        }
        else
        {
            __instance.chatScreen.SetActive(true);
            customNetworkTransform.Halt();
            __instance.StartCoroutine(__instance.CoOpen());
            if (FriendsListManager.InstanceExists)
            {
                FriendsListManager.Instance.SetFriendButtonColor(true);
            }

            try
            {
                if (__instance.chatNotification.gameObject.activeSelf)
                {
                    __instance.chatNotification.Close();
                }
            }
            catch
            {
                Error("CHat notification failed to be checked.");
            }
        }

        return false;
    }
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
    [HarmonyPrefix]
    public static bool PrefixSetCosmetics(PlayerVoteArea __instance, NetworkedPlayerInfo playerInfo)
    {
        try
        {
            __instance.Background.sprite = ShipStatus.Instance.CosmeticsCache
                .GetNameplate(playerInfo.DefaultOutfit.NamePlateId).Image;
        }
        catch
        {
            Error($"Running fallback nameplate checks for {playerInfo.PlayerName}");
            try
            {
                NamePlateData namePlateById =
                    HatManager.Instance.GetNamePlateById(playerInfo.DefaultOutfit.NamePlateId);
                var x = (NamePlateViewData viewData) => { __instance.Background.sprite = viewData.Image; };
                __instance.StartCoroutine(
                    AddressableAssetExtensions.CoLoadAssetAsync<NamePlateViewData>(
                        __instance,
                        namePlateById.GetAssetReference(),
                        x));
            }
            catch
            {
                Error($"Fallback nameplate checks failed for {playerInfo.PlayerName}. No nameplate applied.");
            }
        }

        try
        {
            __instance.PlayerIcon.UpdateFromEitherPlayerDataOrCache(playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, false, null);
            __instance.PlayerIcon.ToggleName(false);
            __instance.NameText.text = playerInfo.PlayerName;
            __instance.LevelNumberText.text = ProgressionManager.FormatVisualLevel(playerInfo.PlayerLevel);
            PlayerMaterial.SetColors(DataManager.Player.Customization.Color, __instance.ThumbsDown);
            DataManager.Settings.Accessibility.OnColorBlindModeChanged += new Action(__instance.SetColorblindText);
            __instance.SetColorblindText();
        }
        catch (Exception e)
        {
            Error($"Something critically failed when fetching cosmetics or player data from {playerInfo.PlayerName}");
            Error(e);
        }
        return false;
    }
}
