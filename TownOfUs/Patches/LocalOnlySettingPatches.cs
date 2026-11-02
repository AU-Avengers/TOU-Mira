using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class LocalSettings
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    public static void HideGhosts()
    {
        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer.Data.IsDead)
        {
            return;
        }

        if (MeetingHud.Instance)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.AmOwner)
            {
                continue;
            }

            if (!player.Data.IsDead)
            {
                continue;
            }

            switch (player.Data.Role)
            {
                case IGhostRole { Caught: false }:
                    continue;
            }

            var show = LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.DeadSeeGhostsToggle.Value;
            var bodyForms = player.gameObject.transform.GetChild(2).gameObject;

            foreach (var form in bodyForms.GetAllChildren())
            {
                if (form.activeSelf)
                {
                    form.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, show ? 1f : 0f);
                }
            }

            if (player.cosmetics.HasPetEquipped())
            {
                player.cosmetics.CurrentPet.Visible = show;
            }

            player.cosmetics.gameObject.SetActive(show);
            player.gameObject.transform.GetChild(4).gameObject.SetActive(show);
        }

        if (OptionGroupSingleton<SentryOptions>.Instance.DeployedCamerasVisibility is SentryDeployedCamerasVisibility.AfterMeeting)
        {
            foreach (var cameraPair in SentryRole.Cameras)
            {
                if (cameraPair.Key == null || cameraPair.Key.gameObject == null)
                {
                    continue;
                }

                cameraPair.Key.gameObject.SetActive(true);
                var spriteRenderer = cameraPair.Key.gameObject.GetComponent<SpriteRenderer>();
                spriteRenderer?.color = Color.white;
            }
        }
    }
}