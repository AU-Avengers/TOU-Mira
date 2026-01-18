using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class ArsonistEvents
{
    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var source = PlayerControl.LocalPlayer;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick() || target.Data.Role is not ArsonistRole ||
            !OptionGroupSingleton<ArsonistOptions>.Instance.DouseInteractions)
        {
            return;
        }

        if (!source.HasModifier<ArsonistDousedModifier>())
        {
            source.RpcAddModifier<ArsonistDousedModifier>(target.PlayerId);
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;
        if (source.Data.Role is ArsonistRole && victim.AmOwner && MeetingHud.Instance == null && ExileController.Instance == null)
        {
            TouAudio.PlaySound(TouAudio.ArsoIgniteSound);
        }
        ArsonistRole.SetDouseUses();
    }

    [RegisterEvent]
    public static void OnRoundStartEventHandler(RoundStartEvent @event)
    {
        ArsonistRole.SetDouseUses();
    }
}