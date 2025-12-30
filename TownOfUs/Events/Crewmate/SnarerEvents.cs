using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class SnarerEvents
{
    private static int ActiveSnareTaskCount;
    private static uint LastSnareUseTaskId = uint.MaxValue;

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            VentSnareSystem.ClearAll();
            ActiveSnareTaskCount = 0;
            LastSnareUseTaskId = uint.MaxValue;
            return;
        }

        if (!OptionGroupSingleton<SnarerOptions>.Instance.SnaresRemoveOnNewRound)
        {
            return;
        }

        VentSnareSystem.ClearAll();
        ActiveSnareTaskCount = 0;
        LastSnareUseTaskId = uint.MaxValue;

        if (PlayerControl.LocalPlayer?.Data?.Role is SnarerRole)
        {
            var uses = OptionGroupSingleton<SnarerOptions>.Instance.MaxSnares;
            CustomButtonSingleton<SnarerSnareButton>.Instance.SetUses((int)uses);
        }
    }

    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player == null || !@event.Player.AmOwner)
        {
            return;
        }

        if (@event.Player.Data?.Role is not SnarerRole)
        {
            return;
        }

        var options = OptionGroupSingleton<SnarerOptions>.Instance;
        if (options.SnaresRemoveOnNewRound || options.TasksUntilMoreSnares == 0)
        {
            return;
        }

        if (@event.Task != null && @event.Task.Id != LastSnareUseTaskId)
        {
            ++ActiveSnareTaskCount;
            LastSnareUseTaskId = @event.Task.Id;
        }

        var button = CustomButtonSingleton<SnarerSnareButton>.Instance;
        if (button.LimitedUses && options.TasksUntilMoreSnares <= ActiveSnareTaskCount)
        {
            ++button.UsesLeft;
            button.SetUses(button.UsesLeft);
            ActiveSnareTaskCount = 0;
        }
    }
}