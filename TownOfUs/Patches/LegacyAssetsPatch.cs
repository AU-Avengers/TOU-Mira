using HarmonyLib;
using MiraAPI.Hud;
using TownOfUs.Buttons;

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
        killBtn.defaultKillSprite = LegacyVanillaAssets.KillSprite.LoadAsset();
        killBtn.graphic.sprite = LegacyVanillaAssets.KillSprite.LoadAsset();
        reportBtn.graphic.sprite = LegacyVanillaAssets.ReportSprite.LoadAsset();
        saboBtn.graphic.sprite = LegacyVanillaAssets.SabotageSprite.LoadAsset();
        useBtn.graphic.sprite = LegacyVanillaAssets.UseSprite.LoadAsset();
        petBtn.graphic.sprite = LegacyVanillaAssets.PetSprite.LoadAsset();
        ventBtn.graphic.sprite = LegacyVanillaAssets.VentSprite.LoadAsset();
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
        if (!LegacyAssets.IsLegacy)
        {
            return true;
        }
        var sprite = settings.Image;
        switch (settings.ButtonType)
        {
            case ImageNames.UseButton:
                sprite = LegacyVanillaAssets.UseSprite.LoadAsset();
                break;
            case ImageNames.DoorLogsButton:
                sprite = LegacyVanillaAssets.DoorlogSprite.LoadAsset();
                break;
            case ImageNames.CamsButton:
                sprite = LegacyVanillaAssets.SecuritySprite.LoadAsset();
                break;
            case ImageNames.VitalsButton:
                sprite = LegacyVanillaAssets.VitalsSprite.LoadAsset();
                break;
            case ImageNames.OptionsButton:
                sprite = LegacyVanillaAssets.CustomizeSprite.LoadAsset();
                break;
            case ImageNames.MIRAAdminButton:
                sprite = LegacyVanillaAssets.AdminMiraSprite.LoadAsset();
                break;
            case ImageNames.PolusAdminButton:
                sprite = LegacyVanillaAssets.AdminPolusSprite.LoadAsset();
                break;
            case ImageNames.AirshipAdminButton:
                sprite = LegacyVanillaAssets.AdminAirshipSprite.LoadAsset();
                break;
            case ImageNames.AdminMapButton:
                sprite = LegacyVanillaAssets.AdminSkeldSprite.LoadAsset();
                /*switch (MiscUtils.GetCurrentMap)
                {
                    case ExpandedMapNames.Skeld or ExpandedMapNames.Dleks:
                        sprite = LegacyVanillaAssets.AdminSkeldSprite.LoadAsset();
                        break;
                    case ExpandedMapNames.MiraHq:
                        sprite = LegacyVanillaAssets.AdminMiraSprite.LoadAsset();
                        break;
                    case ExpandedMapNames.Polus:
                        sprite = LegacyVanillaAssets.AdminPolusSprite.LoadAsset();
                        break;
                    case ExpandedMapNames.Airship:
                        sprite = LegacyVanillaAssets.AdminAirshipSprite.LoadAsset();
                        break;
                }*/
                break;
            /*case ImageNames.PlayAgainButton:
                sprite = LegacyVanillaAssets.PlayAgainSprite.LoadAsset();
                break;
            case ImageNames.ExitRoomButton:
                sprite = LegacyVanillaAssets.QuitSprite.LoadAsset();
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