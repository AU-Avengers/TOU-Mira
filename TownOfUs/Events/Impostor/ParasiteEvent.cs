using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using TownOfUs.AnimationSystem.BasicKills;
using TownOfUs.AnimationSystem.Events;
using TownOfUs.Buttons.Impostor;

namespace TownOfUs.Events.Impostor;

public static class ParasiteEvents
{
    public static bool NextKillFromParasite;
    [RegisterEvent]
    public static void BeforeKillAnim(BeforeKillAnimationEvent @event)
    {
        if (NextKillFromParasite)
        {
            @event.FinalAnimation = SuffocateKillAnimation.CreateKillAnimation(@event.Killer, @event.Victim);
            @event.ShouldAlterAnimationAfter = false;
        }

        NextKillFromParasite = false;
    }
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        var button = CustomButtonSingleton<ParasiteOvertakeButton>.Instance;
        button.ResetCooldownAndOrEffect();
    }
}