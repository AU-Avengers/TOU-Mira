using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using TownOfUs.Achievements;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class JuggernautEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source is not { AmOwner: true, Data.Role: JuggernautRole juggernaut } || MeetingHud.Instance)
        {
            return;
        }

        juggernaut.KillCount++;
        var button = CustomButtonSingleton<JuggernautKillButton>.Instance;
        button.ResetCooldownAndOrEffect();
        if (button.Cooldown == 0)
        {
            TouAchievements.Ballistic.Unlock();
        }
    }
}