using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Events.Crewmate;

public static class PlumberEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            PlumberRole.ClearAll();
        }
    }
    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.AmOwner && @event.Player.Data.Role is PlumberRole &&
            OptionGroupSingleton<PlumberOptions>.Instance.TaskUses)
        {
            var button = CustomButtonSingleton<PlumberBlockButton>.Instance;
            ++button.UsesLeft;
            ++button.ExtraUses;
            button.SetUses(button.UsesLeft);
        }
    }
    
    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (!@event.IsVent)
        {
            return;
        }

        var vent = @event.Usable.TryCast<Vent>();

        if (vent == null)
        {
            return;
        }

        if (PlumberRole.VentsBlocked.Select(x => x.Key).Contains(vent.Id))
        {
            @event.Cancel();
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        if ((int)OptionGroupSingleton<PlumberOptions>.Instance.BarricadeRoundDuration > 0)
        {
            var barricadeList = new List<KeyValuePair<GameObject, int>>();
            if (PlumberRole.Barricades.Count > 0)
            {
                foreach (var barricadePair in PlumberRole.Barricades)
                {
                    if (barricadePair.Value == 1)
                    {
                        UnityEngine.Object.Destroy(barricadePair.Key);
                        continue;
                    }
                    barricadeList.Add(new(barricadePair.Key, barricadePair.Value - 1));
                }
            }
            PlumberRole.Barricades.Clear();
            PlumberRole.Barricades = barricadeList;
        
            var ventList = new List<KeyValuePair<int, int>>();
            if (PlumberRole.VentsBlocked.Count > 0)
            {
                foreach (var ventPair in PlumberRole.VentsBlocked)
                {
                    if (ventPair.Value == 1) continue;
                    ventList.Add(new(ventPair.Key, ventPair.Value - 1));
                }
            }
            PlumberRole.VentsBlocked.Clear();
            PlumberRole.VentsBlocked = ventList;
        }
        
        foreach (var plumber in CustomRoleUtils.GetActiveRolesOfType<PlumberRole>())
        {
            plumber.SetupBarricades();
        }
    }
}