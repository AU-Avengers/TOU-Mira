using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;

namespace TownOfUs.Events.Modifiers;

public static class CelebrityEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (target.HasModifier<CelebrityModifier>())
        {
            if (!MeetingHud.Instance)
            {
                CelebrityModifier.CelebrityKilled(source, target);
            }
            else
            {
                var celeb = target.GetModifier<CelebrityModifier>()!;
                celeb.Announced = true;
            }
        }
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.DeathReason != DeathReason.Exile)
        {
            return;
        }

        if (@event.Player.TryGetModifier<CelebrityModifier>(out var celeb))
        {
            celeb.Announced = true;
        }
    }

    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        if (@event.Reporter.AmOwner)
        {
            var celebrity = ModifierUtils.GetActiveModifiers<CelebrityModifier>(x => x.Player.HasDied() && !x.Announced)
                .FirstOrDefault();
            if (celebrity != null)
            {
                var milliSeconds = (float)(DateTime.UtcNow - celebrity.DeathTime).TotalMilliseconds;

                CelebrityModifier.RpcUpdateCelebrityKilled(celebrity.Player, milliSeconds);
            }
        }
    }

    [RegisterEvent]
    public static void WrapUpEvent(EjectionEvent @event)
    {
        var player = @event.ExileController.initData.networkedPlayer?.Object;
        if (player == null)
        {
            return;
        }

        if (player.TryGetModifier<CelebrityModifier>(out var celeb))
        {
            celeb.Announced = true;
        }
    }
}