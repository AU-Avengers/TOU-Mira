using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events;

public static class HnsGamemodeEvents
{
    public static IEnumerator CoChangeImpostorRole(IntroCutscene cutscene)
    {
        yield return new WaitForSeconds(0.01f);
        
        var seeker = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.IsImpostor());
        if (seeker != null)
        {
            cutscene.ImpostorTitle.text = seeker.Data.Role.GetRoleName();
        }
    }

    [RegisterEvent]
    public static void IntroBeginEventHandler(IntroBeginEvent @event)
    {
        if (MiscUtils.CurrentGamemode() is not TouGamemode.HideAndSeek)
        {
            return;
        }
        var cutscene = @event.IntroCutscene;
        Coroutines.Start(CoChangeImpostorRole(cutscene));
    }

    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (MiscUtils.CurrentGamemode() is not TouGamemode.HideAndSeek)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data ||
            !PlayerControl.LocalPlayer.Data.Role)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            @event.Cancel();
        }

        // Prevent last 2 players from venting (or however many are set up)
        if (@event.IsVent)
        {
            if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>())
            {
                if (PlayerControl.LocalPlayer.inVent)
                {
                    PlayerControl.LocalPlayer.GetModifier<GlitchHackedModifier>()!.ShowHacked();
                    PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                    PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
                }

                @event.Cancel();
            }
            else if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
            {
                @event.Cancel();
            }

            var aliveCount = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied());
            var minimum = (int)OptionGroupSingleton<VanillaTweakOptions>.Instance.PlayerCountWhenVentsDisable.Value;

            if (PlayerControl.LocalPlayer.inVent && (aliveCount <= minimum
                                                     || PlayerControl.LocalPlayer.IsImpostor()))
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
            }

            if (aliveCount <= minimum || PlayerControl.LocalPlayer.IsImpostor())
            {
                @event.Cancel();
            }
        }
    }
}