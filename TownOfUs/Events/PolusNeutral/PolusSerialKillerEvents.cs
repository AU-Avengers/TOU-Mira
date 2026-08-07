using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons.TownofPolus.Neutral;
using TownOfUs.Options.Roles.PolusNeutral;
using TownOfUs.Roles.TownOfPolus.Neutral;

namespace TownOfUs.Events.PolusNeutral;

public static class PolusSerialKillerEveents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        if (source.Data.Role is not PolusSerialKillerRole sk)
        {
            return;
        }

        sk.KillCount++;

        if (!source.AmOwner)
        {
            return;
        }
        var button = CustomButtonSingleton<PolusSerialKillerKillButton>.Instance;
        button.CooldownReduction += OptionGroupSingleton<PolusSerialKillerOptions>.Instance.CooldownKillStreakReduction;
        button.ResetCooldownAndOrEffect();
    }
    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        CustomButtonSingleton<PolusSerialKillerKillButton>.Instance.CooldownReduction = 0;
    }
}