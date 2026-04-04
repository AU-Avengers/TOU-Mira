using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options;

namespace TownOfUs.Events;

public static class VanillaTweakEvents
{
    [RegisterEvent(1000000)]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        var ventSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation].TryCast<VentilationSystem>();
        if (ventSystem != null)
        {
            Warning($"Remedied vent bugs!");
            // This fixes an issue within vanilla where any players who were removed out of vents via a meeting can be "kicked" out of the vent they were previously in, even if they aren't in there.
            ventSystem.PlayersInsideVents.Clear();
        }
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