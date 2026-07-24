using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class SoulCollectorEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (MedusaRole.AutoPlaceFakePlayers && source.IsRole<MedusaRole>() && !MeetingHud.Instance)
            // leave behind standing body
            // Message($"Leaving behind soulless player '{target.Data.PlayerName}'");
        {
            _ = new StonedPlayer(target);
        }
    }

    [RegisterEvent]
    public static void ReviveEventHandler(PlayerReviveEvent @event)
    {
        var player = @event.Player;
        
        var stonedPlayer = StonedPlayer.FakePlayers.FirstOrDefault(x => x.PlayerId == player.PlayerId && x.ProgressStage is not StoneStage.Permanent and not StoneStage.Petrified);
        if (stonedPlayer != null)
        {
            StonedPlayer.FakePlayers.Remove(stonedPlayer);
            if (stonedPlayer.CurrentCoroutine != null)
            {
                Coroutines.Stop(stonedPlayer.CurrentCoroutine);
            }
            stonedPlayer.Destroy();
        }
        
        var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x => x.PlayerId == player.PlayerId);
        if (fakePlayer != null)
        {
            FakePlayer.FakePlayers.Remove(fakePlayer);
            fakePlayer.Destroy();
        }
    }

    // These are semi-frequent but not as costly as constantly updating the fake player names.
    [RegisterEvent]
    public static void RoleChangeEventHandler(SetRoleEvent _)
    {
        StonedPlayer.UpdateFakePlayerText(true);
        FakePlayer.UpdateFakePlayerText(true);
    }

    [RegisterEvent]
    public static void ChangeRoleEventHandler(ChangeRoleEvent _)
    {
        StonedPlayer.UpdateFakePlayerText(true);
        FakePlayer.UpdateFakePlayerText(true);
    }
}