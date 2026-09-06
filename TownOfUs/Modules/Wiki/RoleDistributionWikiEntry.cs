using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;

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


}