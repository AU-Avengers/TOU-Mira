using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Utilities;
using TownOfUs.Modules.Components;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class DetectiveEvents
{
    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        if (@event.Target == null)
        {
            return;
        }

        if (@event.Reporter.Data.Role is DetectiveRole detective && @event.Reporter.AmOwner)
        {
            detective.Report(@event.Target.PlayerId);
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        if (CrimeSceneComponent._crimeScenes.Count == 0)
        {
            return;
        }

        if (!Helpers.GetAlivePlayers().Any(x => x.Data.Role is DetectiveRole))
        {
            return;
        }

        foreach (var scene in CrimeSceneComponent._crimeScenes)
        {
            if (scene == null || scene.gameObject == null || !scene.gameObject)
            {
                continue;
            }
            scene.gameObject.SetActive(false);
        }

        if (PlayerControl.LocalPlayer.Data.Role is DetectiveRole)
        {
            foreach (var scene in CrimeSceneComponent._crimeScenes)
            {
                if (scene == null || scene.gameObject == null || !scene.gameObject)
                {
                    continue;
                }
                scene.gameObject.SetActive(true);
            }
        }
    }

    /* [RegisterEvent]
    public static void EjectionEventEventHandler(EjectionEvent @event)
    {
        CrimeSceneComponent.Clear();
    } */

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source.IsRole<SoulCollectorRole>())
        {
            return;
        }

        if (MeetingHud.Instance)
        {
            return;
        }

        var victim = @event.Target;
        var bodyPos = victim.transform.position;
        bodyPos.y -= 0.3f;
        bodyPos.x -= 0.11f;

        CrimeSceneComponent.CreateCrimeScene(victim, bodyPos);
    }
}