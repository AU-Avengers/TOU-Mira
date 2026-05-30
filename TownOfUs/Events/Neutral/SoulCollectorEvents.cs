using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
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

        if (SoulCollectorRole.AutoPlaceFakePlayers && source.IsRole<SoulCollectorRole>() && !MeetingHud.Instance)
            // leave behind standing body
            // Message($"Leaving behind soulless player '{target.Data.PlayerName}'");
        {
            _ = new FakePlayer(target);
        }
    }

    [RegisterEvent]
    public static void ReviveEventHandler(PlayerReviveEvent @event)
    {
        var player = @event.Player;
        
        var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x => x.PlayerId == player.PlayerId);
        if (fakePlayer != null)
        {
            FakePlayer.FakePlayers.Remove(fakePlayer);
            fakePlayer.Destroy();
        }
    }

    // These are semi-frequent but not as costly as constantly updating the fake player names.
    [RegisterEvent]
    public static void RoleChangeEventHandler(SetRoleEvent @event)
    {
        FakePlayer.UpdateFakePlayerText();
    }

    [RegisterEvent]
    public static void ChangeRoleEventHandler(ChangeRoleEvent @event)
    {
        FakePlayer.UpdateFakePlayerText();
    }
}