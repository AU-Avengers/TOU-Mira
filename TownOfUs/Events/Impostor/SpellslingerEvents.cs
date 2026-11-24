using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class SpellslingerEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro || HexBombSabotageSystem.Instance == null || !HexBombSabotageSystem.Instance.IsActive || HexBombSabotageSystem.Instance.Stage is HexBombStage.SpellslingerDead or HexBombStage.Finished)
        {
            return;
        }

        // This could definitely be better but I decided to do the math for it instead cause a flat 0 won't do.
        var currentTime = HexBombSabotageSystem.Instance.TimeRemaining;
        var decreaseTime = OptionGroupSingleton<SpellslingerOptions>.Instance.HexBombDecreasePerMeeting;
        var measuredDiff = currentTime - decreaseTime;
        HexBombSabotageSystem.Instance.DecreaseTimer(currentTime + 1 > decreaseTime ? decreaseTime : decreaseTime + measuredDiff - 1);
    }
}