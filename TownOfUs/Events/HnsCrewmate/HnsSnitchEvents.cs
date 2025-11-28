using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.HnsCrewmate;
using TownOfUs.Options.Roles.HnsCrewmate;
using TownOfUs.Roles.HnsCrewmate;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Events.HnsCrewmate;

public static class HnsSnitchEvents
{
    [RegisterEvent]
    public static void CompleteHnsTaskEvent(CompleteHnsTaskEvent @event)
    {
        var hnsInstance = @event.HnsManager;
        var player = @event.Player;
        if (player.Data.Role is HnsSnitchRole)
        {
            if (PlayerControl.LocalPlayer.IsImpostor())
            {
                Color color = Palette.PlayerColors[player.GetDefaultAppearance().ColorId];
                player.AddModifier<HnsSnitchArrowModifier>(PlayerControl.LocalPlayer, color, 0f);
            }
            var normalPlayerTask = @event.Task as NormalPlayerTask;

            if (normalPlayerTask != null)
            {
                switch (normalPlayerTask.Length)
                {
                    case NormalPlayerTask.TaskLength.None:
                    case NormalPlayerTask.TaskLength.Common:
                        var commonMult =  Math.Clamp(OptionGroupSingleton<HnsSnitchOptions>.Instance.CommonTaskMultiplier.Value - 1f, 0f, 2f);
                        if (commonMult > 0)
                        {
                            hnsInstance.LogicFlowHnS.OnTaskComplete(hnsInstance.LogicOptionsHnS.GetCommonTaskTimeValue() * commonMult);
                        }
                        break;
                    case NormalPlayerTask.TaskLength.Short:
                        var shortMult =  Math.Clamp(OptionGroupSingleton<HnsSnitchOptions>.Instance.ShortTaskMultiplier.Value - 1f, 0f, 2f);
                        if (shortMult > 0)
                        {
                            hnsInstance.LogicFlowHnS.OnTaskComplete(hnsInstance.LogicOptionsHnS.GetShortTaskTimeValue() * shortMult);
                        }
                        break;
                    case NormalPlayerTask.TaskLength.Long:
                        var longMult =  Math.Clamp(OptionGroupSingleton<HnsSnitchOptions>.Instance.LongTaskMultiplier.Value - 1f, 0f, 2f);
                        if (longMult > 0)
                        {
                            hnsInstance.LogicFlowHnS.OnTaskComplete(hnsInstance.LogicOptionsHnS.GetLongTaskTimeValue() * longMult);
                        }
                        break;
                }
            }
        }
    }
}