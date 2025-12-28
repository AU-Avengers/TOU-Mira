using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events;

public static class ControlStateEvents
{
    public static int ActiveControlKillCount;
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            return; // Only run when game starts.
        }

        ActiveControlKillCount = 0;

        var hunterStalk = CustomButtonSingleton<PuppeteerControlButton>.Instance;
        hunterStalk.ExtraUses = 0;
        hunterStalk.SetUses((int)OptionGroupSingleton<PuppeteerOptions>.Instance.ControlUses);
        if (!hunterStalk.LimitedUses)
        {
            hunterStalk.Button?.usesRemainingText.gameObject.SetActive(false);
            hunterStalk.Button?.usesRemainingSprite.gameObject.SetActive(false);
        }
        else
        {
            hunterStalk.Button?.usesRemainingText.gameObject.SetActive(true);
            hunterStalk.Button?.usesRemainingSprite.gameObject.SetActive(true);
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var opt = OptionGroupSingleton<PuppeteerOptions>.Instance;
        var controlButton = CustomButtonSingleton<PuppeteerControlButton>.Instance;
        if (@event.Source.AmOwner)
        {
            ++ActiveControlKillCount;
            if (@event.Source.Data.Role is not PuppeteerRole)
            {
                return;
            }

            if (!controlButton.EffectActive)
            {
                controlButton.ResetCooldownAndOrEffect();
            }

            if (controlButton.LimitedUses &&
                opt.ControlPerKills != 0 && opt.ControlPerKills <= ActiveControlKillCount)
            {
                ++controlButton.UsesLeft;
                ++controlButton.ExtraUses;
                controlButton.SetUses(controlButton.UsesLeft);
                ActiveControlKillCount = 0;
            }
        }
    }
    
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        ParasiteControlState.ClearAll();
        PuppeteerControlState.ClearAll();

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.Role is ParasiteRole parasiteRole)
            {
                parasiteRole.ClearControlLocal();
            }

            if (player.TryGetModifier<ParasiteInfectedModifier>(out var mod))
            {
                player.RemoveModifier(mod);
            }
            
            if (player.Data.Role is PuppeteerRole puppeteerRole)
            {
                puppeteerRole.ClearControlLocal();
            }

            if (player.TryGetModifier<PuppeteerControlModifier>(out var mod2))
            {
                player.RemoveModifier(mod2);
            }
        }
    }

    [RegisterEvent]
    public static void ClientGameEndEventHandler(ClientGameEndEvent @event)
    {
        ParasiteControlState.ClearAll();
        PuppeteerControlState.ClearAll();

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.Role is ParasiteRole parasiteRole)
            {
                parasiteRole.ClearControlLocal();
            }

            if (player.TryGetModifier<ParasiteInfectedModifier>(out var mod))
            {
                player.RemoveModifier(mod);
            }
            
            if (player.Data.Role is PuppeteerRole puppeteerRole)
            {
                puppeteerRole.ClearControlLocal();
            }

            if (player.TryGetModifier<PuppeteerControlModifier>(out var mod2))
            {
                player.RemoveModifier(mod2);
            }
        }
    }
}