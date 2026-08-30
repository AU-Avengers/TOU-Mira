using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class PoliticianEvents
{
    [RegisterEvent]
    public static void EjectionEventEventHandler(EjectionEvent _)
    {
        var maxCampaigns = (int)OptionGroupSingleton<PoliticianOptions>.Instance.MaxCampaigns;
        var button = CustomButtonSingleton<PoliticianCampaignButton>.Instance;
        var prevented = PlayerControl.LocalPlayer.Data.Role is PoliticianRole { CanCampaign: false };

        button.SetUses(prevented ? 0 : maxCampaigns);

        if (!prevented && maxCampaigns == 0)
        {
            button.Button?.usesRemainingText.gameObject.SetActive(false);
            button.Button?.usesRemainingSprite.gameObject.SetActive(false);
        }
        else
        {
            button.Button?.usesRemainingText.gameObject.SetActive(true);
            button.Button?.usesRemainingSprite.gameObject.SetActive(true);
        }
    }
}
