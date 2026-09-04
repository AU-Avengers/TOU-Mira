using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(MeetingHud))]
public static class MeetingHudTimerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.UpdateTimerText))]
    public static void TimerUpdatePostfix(MeetingHud __instance)
    {
        var newText = string.Empty;
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data ||
            PlayerControl.LocalPlayer.HasDied())
        {
            return;
        }

        switch (PlayerControl.LocalPlayer.Data.Role)
        {
            case AmbassadorRole ambass:
                var ambassOpt = OptionGroupSingleton<AmbassadorOptions>.Instance;
                newText = $"\n{ambass.RetrainsString()}";
                if (HudManagerHelper.Instance.CurrentRound < (int)ambassOpt.RoundWhenAvailable)
                {
                    newText =
                        $"{newText} | {AmbassadorRole.RetrainWaitString.Replace("<roundToWait>", $"{(int)ambassOpt.RoundWhenAvailable}")}";
                }
                else if (ambass.RoundsCooldown > 0)
                {
                    newText = $"{newText} | {ambass.RetrainCdString()}";
                }

                break;
            case ProsecutorRole pros:
                var total = (int)OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions;
                var prosecutes = total - pros.ProsecutionsCompleted;
                newText =
                    $"\n{MiraLocaleManager.Get("TownOfUsMira.Role.ProsecutorProsecutionsRemaining").Replace("<count>", prosecutes.ToString(TownOfUsPlugin.Culture)).Replace("<total>", total.ToString(TownOfUsPlugin.Culture))}";
                break;
            case DeputyRole dep:
                if (dep.Killer)
                {
                    newText = $"\n{MiraLocaleManager.Get("TownOfUsMira.Role.DeputyShootKiller")}";
                }

                break;
            case PoliticianRole:
                newText = $"\n{MiraLocaleManager.Get("TownOfUsMira.Role.PoliticianRevealRequirement")}";
                break;
            case MayorRole mayor:
                newText = mayor.Revealed ? $"\n{MiraLocaleManager.Get("TownOfUsMira.Role.MayorRevealedVotes")}" : $"\n{MiraLocaleManager.Get("TownOfUsMira.Role.MayorRevealVotes")}";
                break;
            case DoomsayerRole doom:
                var doomOpt = OptionGroupSingleton<DoomsayerOptions>.Instance;
                newText = "\n" + (doomOpt.DoomsayerGuessAllAtOnce
                    ? MiraLocaleManager.Get("TownOfUsMira.Role.DoomsayerGuessAllAtOnce").Replace("<amount>",
                        ((int)doomOpt.DoomsayerGuessesToWin).ToString(TownOfUsPlugin.Culture))
                    : MiraLocaleManager.Get("TownOfUsMira.Role.DoomsayerSuccessfulGuesses")
                        .Replace("<current>", doom.NumberOfGuesses.ToString(TownOfUsPlugin.Culture)).Replace("<total>",
                            ((int)doomOpt.DoomsayerGuessesToWin).ToString(TownOfUsPlugin.Culture)));
                break;
            case VigilanteRole vigi:
                newText =
                    $"\n{vigi.MaxKills} / {(int)OptionGroupSingleton<VigilanteOptions>.Instance.VigilanteKills} {MiraLocaleManager.Get("TownOfUsMira.Role.VigilanteGuessesRemaining")}";
                if ((int)OptionGroupSingleton<VigilanteOptions>.Instance.MultiShots > 0)
                {
                    newText +=
                        $" | {vigi.SafeShotsLeft} / {(int)OptionGroupSingleton<VigilanteOptions>.Instance.MultiShots} {MiraLocaleManager.Get("TownOfUsMira.Role.VigilanteSafeShots")}";
                }

                break;
        }

        if (PlayerControl.LocalPlayer.TryGetModifier<AssassinModifier>(out var assassinMod))
        {
            newText +=
                $"\n{assassinMod.maxKills} / {assassinMod.defaultKills} {MiraLocaleManager.Get("TownOfUsMira.Role.AssassinGuessesRemaining")}";
            if ((PlayerControl.LocalPlayer.TryGetModifier<DoubleShotModifier>(out var doubleShotMod)))
            {
                newText += (doubleShotMod.Used) ? $" | {MiraLocaleManager.Get("TownOfUsMira.Role.AssassinDoubleShotUsed")}" : $" | {MiraLocaleManager.Get("TownOfUsMira.Role.AssassinDoubleShotAvailable")}";
            }
        }

        if (newText != string.Empty)
        {
            __instance.TimerText.text += $"<color=#FFFFFF>{newText}</color>";
        }
    }
}