using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options;
using TownOfUs.Utilities;

namespace TownOfUs.Events;

public static class VanillaTweakEvents
{
    [RegisterEvent(1000000)]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            return;
        }
        var petMode = (PetVisiblity)OptionGroupSingleton<VanillaTweakOptions>.Instance.ShowPetsMode.Value;
        if (petMode is PetVisiblity.AlwaysVisible)
        {
            return;
        }

        if (petMode is PetVisiblity.ClientSide)
        {
            // Hides pets for everyone except the local player
            foreach (var player in Helpers.GetAlivePlayers().Where(x => !x.AmOwner))
            {
                MiscUtils.RemovePet(player);
            }
        }
    }
    [RegisterEvent(1000000)]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (MiscUtils.CurrentGamemode() is TouGamemode.HideAndSeek)
        {
            return;
        }
        var petMode = (PetVisiblity)OptionGroupSingleton<VanillaTweakOptions>.Instance.ShowPetsMode.Value;
        if (petMode is PetVisiblity.AlwaysVisible)
        {
            return;
        }

        var target = @event.Target;

        if (petMode is PetVisiblity.WhenAlive && !target.AmOwner)
        {
            MiscUtils.RemovePet(target);
        }
    }
}