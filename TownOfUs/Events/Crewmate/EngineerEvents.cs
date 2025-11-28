using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class EngineerEvents
{
    public static int ActiveVentTaskCount;
    public static int ActiveFixTaskCount;

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            return; // Only run when game starts.
        }

        ActiveVentTaskCount = 0;
        ActiveFixTaskCount = 0;

        var engiVent = CustomButtonSingleton<EngineerVentButton>.Instance;
        engiVent.ExtraUses = 0;
        engiVent.SetUses((int)OptionGroupSingleton<EngineerOptions>.Instance.MaxVents);
        if (!engiVent.LimitedUses)
        {
            engiVent.Button?.usesRemainingText.gameObject.SetActive(false);
            engiVent.Button?.usesRemainingSprite.gameObject.SetActive(false);
        }
        else
        {
            engiVent.Button?.usesRemainingText.gameObject.SetActive(true);
            engiVent.Button?.usesRemainingSprite.gameObject.SetActive(true);
        }

        var engiFix = CustomButtonSingleton<EngineerFixButton>.Instance;
        engiFix.ExtraUses = 0;
    }

    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        var opt = OptionGroupSingleton<EngineerOptions>.Instance;
        var ventButton = CustomButtonSingleton<EngineerVentButton>.Instance;
        var fixButton = CustomButtonSingleton<EngineerFixButton>.Instance;
        if (@event.Player.AmOwner)
        {
            ++ActiveVentTaskCount;
            ++ActiveFixTaskCount;
            if (@event.Player.Data.Role is not EngineerTouRole)
            {
                return;
            }

            if (ventButton.LimitedUses &&
                opt.VentPerTasks != 0 && opt.VentPerTasks <= ActiveVentTaskCount)
            {
                ++ventButton.UsesLeft;
                ++ventButton.ExtraUses;
                ventButton.SetUses(ventButton.UsesLeft);
                ActiveVentTaskCount = 0;
            }

            if (fixButton.LimitedUses &&
                opt.FixPerTasks != 0 && opt.FixPerTasks <= ActiveFixTaskCount)
            {
                ++fixButton.UsesLeft;
                ++fixButton.ExtraUses;
                fixButton.SetUses(fixButton.UsesLeft);
                ActiveFixTaskCount = 0;
            }
        }
    }
}
