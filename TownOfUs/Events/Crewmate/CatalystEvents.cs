using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class CatalystEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        foreach (var charged in ModifierUtils.GetActiveModifiers<CatalystOverchargedModifier>())
        {
            charged.Player.RemoveModifier(charged);
        }

        if (PlayerControl.LocalPlayer.Data.Role is CatalystRole)
        {
            CustomButtonSingleton<CatalystOverchargeButton>.Instance.SetUses((int)OptionGroupSingleton<CatalystOptions>.Instance.OverchargeUses);
        }
    }
}