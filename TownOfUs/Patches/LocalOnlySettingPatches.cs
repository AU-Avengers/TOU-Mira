using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
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

        if (!OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow)
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
                case PhantomTouRole { Caught: false }:
                case HaunterRole { Caught: false }:
                    continue;
            }

            var show = LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.DeadSeeGhostsToggle.Value;
            var bodyForms = player.gameObject.transform.GetChild(1).gameObject;

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
            player.gameObject.transform.GetChild(3).gameObject.SetActive(show);
        }
    }

    public static IEnumerable<GameObject> GetAllChildren(this GameObject go)
    {
        for (var i = 0; i < go.transform.childCount; i++)
        {
            yield return go.transform.GetChild(i).gameObject;
        }
    }

    public sealed class SelectionBehaviour
    {
        public bool DefaultValue;
        public string ObjName;

        public Func<bool> OnClick;
        public string Title;
        public Color Enabled { get; set; } = Color.green;
        public Color Disabled { get; set; } = Palette.ImpostorRed;
        public Color Hover { get; set; } = new Color32(34, 139, 34, 255);
    }
}