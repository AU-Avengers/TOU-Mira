using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Events;
using TownOfUs.Modifiers.Game.Assailant;
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
                var total = (int)OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions;
                var prosecutes = total - pros.ProsecutionsCompleted;
                newText =
                    $"{TouLocale.GetParsed("TouRoleProsecutorProsecutionsRemaining").Replace("<count>", prosecutes.ToString(TownOfUsPlugin.Culture)).Replace("<total>", total.ToString(TownOfUsPlugin.Culture))}";
                break;
            case DeputyRole dep:
                if (dep.Killer)
                {
                    newText = $"\n{TouLocale.GetParsed("TouRoleDeputyShootKiller")}";
                }

                break;
            case PoliticianRole:
                newText = $"\n{TouLocale.GetParsed("TouRolePoliticianRevealRequirement")}";
                break;
            case MayorRole mayor:
                newText = mayor.Revealed ? $"\n{TouLocale.GetParsed("TouRoleMayorRevealedVotes")}" : $"\n{TouLocale.GetParsed("TouRoleMayorRevealVotes")}";
                break;
            case DoomsayerRole doom:
                var doomOpt = OptionGroupSingleton<DoomsayerOptions>.Instance;
                newText = doomOpt.DoomsayerGuessAllAtOnce
                    ? TouLocale.GetParsed("TouRoleDoomsayerGuessAllAtOnce").Replace("<amount>", ((int)doomOpt.DoomsayerGuessesToWin).ToString(TownOfUsPlugin.Culture))
                    : TouLocale.GetParsed("TouRoleDoomsayerSuccessfulGuesses").Replace("<current>", doom.NumberOfGuesses.ToString(TownOfUsPlugin.Culture)).Replace("<total>", ((int)doomOpt.DoomsayerGuessesToWin).ToString(TownOfUsPlugin.Culture));
                break;
            case VigilanteRole vigi:
                newText =
                    $"\n{vigi.MaxKills} / {(int)OptionGroupSingleton<VigilanteOptions>.Instance.VigilanteKills} {TouLocale.GetParsed("TouRoleVigilanteGuessesRemaining")}";
                if ((int)OptionGroupSingleton<VigilanteOptions>.Instance.MultiShots > 0)
                {
                    newText +=
                        $" | {vigi.SafeShotsLeft} / {(int)OptionGroupSingleton<VigilanteOptions>.Instance.MultiShots} {TouLocale.GetParsed("TouRoleVigilanteSafeShots")}";
                }

                break;
        }

        if (PlayerControl.LocalPlayer.TryGetModifier<AssassinModifier>(out var assassinMod))
        {
            newText +=
                $"\n{assassinMod.maxKills} / {assassinMod.defaultKills} {TouLocale.GetParsed("TouRoleAssassinGuessesRemaining")}";
            if ((PlayerControl.LocalPlayer.TryGetModifier<DoubleShotModifier>(out var doubleShotMod)))
            {
                newText += (doubleShotMod.Used) ? $" | {TouLocale.GetParsed("TouRoleAssassinDoubleShotUsed")}" : $" | {TouLocale.GetParsed("TouRoleAssassinDoubleShotAvailable")}";
            }
        }

        if (newText != string.Empty)
        {
            __instance.TimerText.text += $"<color=#FFFFFF>{newText}</color>";
        }
    }
}