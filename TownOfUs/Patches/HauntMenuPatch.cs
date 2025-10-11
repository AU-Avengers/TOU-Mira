using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetHauntTarget))]
public static class HauntMenuMinigamePatch
{
    public static void Postfix(HauntMenuMinigame __instance)
    {
        if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek)
        {
            return;
        }

        if (!TutorialManager.InstanceExists && !PlayerControl.LocalPlayer.DiedOtherRound() &&
            PlayerControl.LocalPlayer.Data.Role is not SpectatorRole)
        {
            __instance.Close();
            __instance.NameText.text = string.Empty;
            __instance.FilterText.text = string.Empty;

            var text = "You must wait until next round to haunt!";
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{text}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Spectator.LoadAsset());

            notif1.AdjustNotification();
            return;
        }

        var target = __instance.HauntTarget;
        __instance.FilterText.text = string.Empty;

        var modifiers = target.GetModifiers<GameModifier>().Where(x => x is not ExcludedGameModifier)
            .OrderBy(x => x.ModifierName).ToList();
        __instance.FilterText.text =
            $"<color=#FFFFFF><size=100%>({TouLocale.Get("PlayerHasNoModifiers")})</size></color>";
        if (modifiers.Count != 0)
        {
            var modifierTextBuilder = new StringBuilder("<color=#FFFFFF><size=100%>(");
            var first = true;
            foreach (var modifier in modifiers)
            {
                var color = MiscUtils.GetModifierColour(modifier);

                if (!first)
                {
                    modifierTextBuilder.Append(", ");
                }

                modifierTextBuilder.Append(CultureInfo.InvariantCulture,
                    $"{color.ToTextColor()}{modifier.ModifierName}</color>");
                first = false;
            }

            modifierTextBuilder.Append(")</size></color>");
            __instance.FilterText.text = modifierTextBuilder.ToString();
        }

        var role = target.Data.Role;
        if (target.Data.IsDead && role is not PhantomTouRole or GuardianAngelRole or HaunterRole)
        {
            role = target.GetRoleWhenAlive();
        }

        var name = role.GetRoleName();

        var rColor = role is ICustomRole custom ? custom.RoleColor : role.TeamColor;

        if (!OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow && !TutorialManager.InstanceExists)
        {
            if (role.IsNeutral())
            {
                name = TouLocale.Get("NeutralKeyword");
                rColor = Color.gray;
            }
            else if (role.IsCrewmate())
            {
                name = TranslationController.Instance.GetString(StringNames.Crewmate);
                rColor = Palette.CrewmateBlue;
            }
            else
            {
                name = TranslationController.Instance.GetString(StringNames.Impostor);
                rColor = Palette.ImpostorRed;
            }
        }

        __instance.NameText.text =
            $"<size=90%>{__instance.NameText.text} - {rColor.ToTextColor()}{name}</color></size>";
    }
}