using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Events;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(MeetingHud))]
public static class MeetingHudTimerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.UpdateTimerText))]
    public static void TimerUpdatePostfix(MeetingHud __instance)
    {
        var newText = string.Empty;
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null ||
            PlayerControl.LocalPlayer.HasDied())
        {
            return;
        }

        switch (PlayerControl.LocalPlayer.Data.Role)
        {
            case AmbassadorRole ambass:
                var ambassOpt = OptionGroupSingleton<AmbassadorOptions>.Instance;
                newText = $"\n{ambass.RetrainsString()}";
                if (DeathEventHandlers.CurrentRound < (int)ambassOpt.RoundWhenAvailable)
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
                var prosecutes = OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions -
                                 pros.ProsecutionsCompleted;
                newText =
                    $"\n{prosecutes} / {OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions} Prosecutions Remaining";
                break;
            case DeputyRole dep:
                if (dep.Killer)
                {
                    newText = "\nShoot a player successfully if they were the killer!";
                }

                break;
            case PoliticianRole:
                newText = "\nReveal successfully if half the crewmates are campaigned!";
                break;
            case MayorRole mayor:
                newText = mayor.Revealed ? "\nYou unleash 3 votes at once!" : "\nReveal yourself to get 3 total votes!";
                break;
            case DoomsayerRole doom:
                var doomOpt = OptionGroupSingleton<DoomsayerOptions>.Instance;
                newText = doomOpt.DoomsayerGuessAllAtOnce
                    ? $"\nGuess the roles of {(int)doomOpt.DoomsayerGuessesToWin} players at once to win!"
                    : $"\n{doom.NumberOfGuesses} / {(int)doomOpt.DoomsayerGuessesToWin} Successful Role Guesses to win!";
                break;
            case VigilanteRole vigi:
                newText =
                    $"\n{vigi.MaxKills} / {(int)OptionGroupSingleton<VigilanteOptions>.Instance.VigilanteKills} Guesses Remaining";
                if ((int)OptionGroupSingleton<VigilanteOptions>.Instance.MultiShots > 0)
                {
                    newText +=
                        $" | {vigi.SafeShotsLeft} / {(int)OptionGroupSingleton<VigilanteOptions>.Instance.MultiShots} Safe Shots";
                }

                break;
        }

        if (PlayerControl.LocalPlayer.TryGetModifier<AssassinModifier>(out var assassinMod))
        {
            newText +=
                $"\n{assassinMod.maxKills} / {(int)OptionGroupSingleton<AssassinOptions>.Instance.AssassinKills} Guesses Remaining";
            if ((PlayerControl.LocalPlayer.TryGetModifier<DoubleShotModifier>(out var doubleShotMod)))
            {
                newText += (doubleShotMod.Used) ? " | Double Shot Used" : " | Double Shot Available";
            }
        }

        if (newText != string.Empty)
        {
            __instance.TimerText.text += $"<color=#FFFFFF>{newText}</color>";
        }
    }
}