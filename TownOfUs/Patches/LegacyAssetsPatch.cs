using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.Hud;
using TownOfUs.Buttons;
using TownOfUs.GameModes;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class VanillaAssetsPatch
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void PostLoadPatch(HudManager __instance)
    {
        if (!LegacyAssets.IsLegacy)
        {
            return;
        }
        Debug("Applying vanilla assets patch...");
        var killBtn = __instance.KillButton;
        var reportBtn = __instance.ReportButton;
        var saboBtn = __instance.SabotageButton;
        var useBtn = __instance.UseButton;
        var petBtn = __instance.PetButton;
        var ventBtn = __instance.ImpostorVentButton;
        killBtn.defaultKillSprite = LegacyVanillaAssets.KillSprite;
        killBtn.graphic.sprite = LegacyVanillaAssets.KillSprite;
        reportBtn.graphic.sprite = LegacyVanillaAssets.ReportSprite;
        saboBtn.graphic.sprite = LegacyVanillaAssets.SabotageSprite;
        useBtn.graphic.sprite = LegacyVanillaAssets.UseSprite;
        petBtn.graphic.sprite = LegacyVanillaAssets.PetSprite;
        ventBtn.graphic.sprite = LegacyVanillaAssets.VentSprite;
        killBtn.RemoveLabel();
        reportBtn.RemoveLabel();
        saboBtn.RemoveLabel();
        useBtn.RemoveLabel();
        petBtn.RemoveLabel();
        ventBtn.RemoveLabel();
        foreach (var button in CustomButtonManager.Buttons.Where(x => x is ILegacyCapable))
        {
            button.RemoveLabel();
        }
    }

    [HarmonyPatch(typeof(UseButton), nameof(UseButton.SetFromSettings))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPrefix]
    public static bool SetFromSettings(UseButton __instance, UseButtonSettings settings)
    {
        if (!LegacyAssets.IsLegacy && !(!LobbyBehaviour.Instance && CustomGameModeManager.IsActiveGameMode<TownOfPolusMode>()))
        {
            return true;
        }
        var sprite = settings.Image;
        switch (settings.ButtonType)
        {
            case ImageNames.UseButton:
                sprite = LegacyVanillaAssets.UseSprite;
                break;
            case ImageNames.DoorLogsButton:
                sprite = LegacyVanillaAssets.DoorlogSprite;
                break;
            case ImageNames.CamsButton:
                sprite = LegacyVanillaAssets.SecuritySprite;
                break;
            case ImageNames.VitalsButton:
                sprite = LegacyVanillaAssets.VitalsSprite;
                break;
            case ImageNames.OptionsButton:
                sprite = LegacyVanillaAssets.CustomizeSprite;
                break;
            case ImageNames.MIRAAdminButton:
                sprite = LegacyVanillaAssets.AdminMiraSprite;
                break;
            case ImageNames.PolusAdminButton:
                sprite = LegacyVanillaAssets.AdminPolusSprite;
                break;
            case ImageNames.AirshipAdminButton:
                sprite = LegacyVanillaAssets.AdminAirshipSprite;
                break;
            case ImageNames.AdminMapButton:
                sprite = LegacyVanillaAssets.AdminSkeldSprite;
                /*switch (MiscUtils.GetCurrentMap)
                {
                    case ExpandedMapNames.Skeld or ExpandedMapNames.Dleks:
                        sprite = LegacyVanillaAssets.AdminSkeldSprite;
                        break;
                    case ExpandedMapNames.MiraHq:
                        sprite = LegacyVanillaAssets.AdminMiraSprite;
                        break;
                    case ExpandedMapNames.Polus:
                        sprite = LegacyVanillaAssets.AdminPolusSprite;
                        break;
                    case ExpandedMapNames.Airship:
                        sprite = LegacyVanillaAssets.AdminAirshipSprite;
                        break;
                }*/
                break;
            /*case ImageNames.PlayAgainButton:
                sprite = LegacyVanillaAssets.PlayAgainSprite;
                break;
            case ImageNames.ExitRoomButton:
                sprite = LegacyVanillaAssets.QuitSprite;
                break;*/
        }
        __instance.graphic.sprite = sprite;
        __instance.graphic.SetCooldownNormalizedUvs();
        __instance.buttonLabelText.fontSharedMaterial = settings.FontMaterial;
        __instance.buttonLabelText.text = TranslationController.Instance.GetString(settings.Text);
        return false;
    }
    public static void RemoveLabel(this ActionButton? button)
    {
        button!.buttonLabelText.gameObject.SetActive(false);
    }
    public static void RemoveLabel(this CustomActionButton miraButton)
    {
        var button = miraButton.Button!;
        button.buttonLabelText.gameObject.SetActive(false);
        button.usesRemainingText.font = button.cooldownTimerText.font;
        button.usesRemainingText.fontMaterial = button.cooldownTimerText.fontMaterial;
    }
}