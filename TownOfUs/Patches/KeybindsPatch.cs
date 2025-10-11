using HarmonyLib;
using MiraAPI.Hud;
using Rewired;
using TownOfUs.Buttons;
using TownOfUs.Utilities;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class Bindings
{
    public static void Postfix(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer == null)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.Data == null)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer.Data.IsDead && !PlayerControl.LocalPlayer.IsImpostor())
        {
            var kill = __instance.KillButton;
            var vent = __instance.ImpostorVentButton;

            if (kill.isActiveAndEnabled)
            {
                var killKey = ReInput.players.GetPlayer(0).GetButtonDown("ActionSecondary");
                var controllerKill = ConsoleJoystick.player.GetButtonDown(8);
                if (killKey || controllerKill)
                {
                    kill.DoClick();
                }
            }

            if (vent.isActiveAndEnabled)
            {
                var ventKey = ReInput.players.GetPlayer(0).GetButtonDown("UseVent");
                var controllerVent = ConsoleJoystick.player.GetButtonDown(50);
                if (ventKey || controllerVent)
                {
                    vent.DoClick();
                }
            }
        }

        if (ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick)
        {
            return;
        }

        var contPlayer = ConsoleJoystick.player;
        var buttonList = CustomButtonManager.Buttons.Where(x =>
            x.Enabled(PlayerControl.LocalPlayer.Data.Role) && x.Button != null && x.Button.isActiveAndEnabled &&
            x.CanUse()).ToList();

        foreach (var button in buttonList.Where(x => x is TownOfUsButton))
        {
            var touButton = button as TownOfUsButton;
            if (touButton == null || touButton.ConsoleBind() == -1)
            {
                continue;
            }

            if (contPlayer.GetButtonDown(touButton.ConsoleBind()))
            {
                touButton.PassiveComp.OnClick.Invoke();
            }
        }

        foreach (var button in buttonList.Where(x => x is TownOfUsTargetButton<DeadBody>))
        {
            var touButton = button as TownOfUsTargetButton<DeadBody>;
            if (touButton == null || touButton.ConsoleBind() == -1)
            {
                continue;
            }

            if (contPlayer.GetButtonDown(touButton.ConsoleBind()))
            {
                touButton.PassiveComp.OnClick.Invoke();
            }
        }

        foreach (var button in buttonList.Where(x => x is TownOfUsTargetButton<Vent>))
        {
            var touButton = button as TownOfUsTargetButton<Vent>;
            if (touButton == null || touButton.ConsoleBind() == -1)
            {
                continue;
            }

            if (contPlayer.GetButtonDown(touButton.ConsoleBind()))
            {
                touButton.PassiveComp.OnClick.Invoke();
            }
        }

        foreach (var button in buttonList.Where(x => x is TownOfUsTargetButton<PlayerControl>))
        {
            var touButton = button as TownOfUsTargetButton<PlayerControl>;
            if (touButton == null || touButton.ConsoleBind() == -1)
            {
                continue;
            }

            if (contPlayer.GetButtonDown(touButton.ConsoleBind()))
            {
                touButton.PassiveComp.OnClick.Invoke();
            }
        }
    }
}