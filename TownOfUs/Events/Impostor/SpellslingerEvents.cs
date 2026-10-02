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
        if (@event.TriggeredByIntro || HexBombSabotageSystem.Instance == null || HexBombSabotageSystem.Instance.Stage is HexBombStage.SpellslingerDead or HexBombStage.Finished)
        {
            return;
        }
        HexBombSabotageSystem.Instance.DecreaseTimer(OptionGroupSingleton<SpellslingerOptions>.Instance.HexBombDecreasePerMeeting);
    }
}