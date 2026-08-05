using System.Text;
using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using TownOfUs.Patches;

namespace TownOfUs.Modules.Wiki
{
    [HarmonyPatch(typeof(IngameWikiMinigame), nameof(IngameWikiMinigame.AddNewSettings))]
    public static class RoleDistributionWikiSettingsPatch
    {
        internal const string PageTitle = "WikiSettingsRoleDistOptions";

        [HarmonyPostfix]
        public static void Postfix(IngameWikiMinigame instance)
        {
            instance._activeSettings.Add(new OptionWikiInfo(
                PageTitle,
                new List<AbstractOptionGroup>
                {
                    OptionGroupSingleton<RoleOptions>.Instance
                },
                TouAssets.IconDraftMode
            ));
        }
    }

    [HarmonyPatch(typeof(IngameWikiMinigame), "SelectSettingsPage")]
    public static class RoleDistWikiContentPatch
    {
        [HarmonyPostfix]
        public static void Postfix(IngameWikiMinigame __instance)
        {
            var sectionName = __instance.SettingsScreenSectionName.Value;
            if (sectionName == null) return;

            string currentTitle = sectionName.text ?? string.Empty;
            bool isRoleDistPage = currentTitle.Contains("Role Dist");

            if (!isRoleDistPage) return;

            var description = __instance.SettingsDescription.Value;
            if (description == null) return;

            string baseOptionText = description.text ?? string.Empty;

            string roleListText = HudManagerPatches.RoleListTextComp != null 
                ? HudManagerPatches.RoleListTextComp.text 
                : "No Role List Available";

            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(baseOptionText))
            {
                sb.AppendLine(baseOptionText);
            }

            sb.AppendLine(roleListText);

            description.text = sb.ToString();
            description.ForceMeshUpdate();
        }
    }
}