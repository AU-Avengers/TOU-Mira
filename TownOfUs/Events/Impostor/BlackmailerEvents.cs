using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class BlackmailerEvents
{
    [RegisterEvent]
    public static void EjectionEvent(EjectionEvent _)
    {
        var sparedPlayers = ModifierUtils.GetPlayersWithModifier<BlackmailSparedModifier>().ToList();
        sparedPlayers.Do(x => x.RemoveModifier<BlackmailSparedModifier>());

        var players = ModifierUtils.GetPlayersWithModifier<BlackmailedModifier>().ToList();
        if (!OptionGroupSingleton<BlackmailerOptions>.Instance.BlackmailInARow)
        {
            players.Do(x =>
                x.AddModifier<BlackmailSparedModifier>(x.GetModifier<BlackmailedModifier>()!.BlackMailerId));
        }

        players.Do(x => x.RemoveModifier<BlackmailedModifier>());
    }
}