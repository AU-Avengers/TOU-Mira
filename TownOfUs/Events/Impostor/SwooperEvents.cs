using AchievementsAPI.API;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Achievements;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class SwooperEvents
{
    [RegisterEvent]
    public static void EjectionEventEventHandler(EjectionEvent _)
    {
        var button = CustomButtonSingleton<SwooperSwoopButton>.Instance;
        button.SetUses((int)OptionGroupSingleton<SwooperOptions>.Instance.MaxSwoops);

        if ((int)OptionGroupSingleton<SwooperOptions>.Instance.MaxSwoops == 0)
        {
            button.Button?.usesRemainingText.gameObject.SetActive(false);
            button.Button?.usesRemainingSprite.gameObject.SetActive(false);
        }
        else
        {
            button.Button?.usesRemainingText.gameObject.SetActive(true);
            button.Button?.usesRemainingSprite.gameObject.SetActive(true);
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;
        if (!source.AmOwner || source == victim || !source.TryGetModifier<SwoopModifier>(out var swoopModifier))
        {
            return;
        }

        if (swoopModifier.AchProgress is SwoopProgress.Nothing)
        {
            swoopModifier.AchProgress = SwoopProgress.Killed;
        }
        var ach = AchievementsTabSingleton<TouImpRoleAchievementsTab>.Instance.Untraceable;
        if (!ach.Unlocked)
        {
            ach.Increment(1, ach.CurrentValue == 15);
        }
    }

    [RegisterEvent]
    public static void EnterVentEventHandler(EnterVentEvent @event)
    {
        var player = @event.Player;
        if (!player.AmOwner || !player.TryGetModifier<SwoopModifier>(out var swoopModifier))
        {
            return;
        }
        if (swoopModifier.AchProgress is SwoopProgress.Killed)
        {
            swoopModifier.AchProgress = SwoopProgress.KilledAndVented;
            AchievementsTabSingleton<TouImpRoleAchievementsTab>.Instance.Sneakman.Unlock();
        }
    }
}