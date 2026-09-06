using HarmonyLib;
using TownOfUs.Modifiers.Game.Crewmate;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class NoisemakerKillPatch
{
    // we run this code elsewhere!
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NoisemakerRole))]
    [HarmonyPatch(nameof(NoisemakerRole.NotifyOfDeath))]
    public static bool Prefix(NoisemakerRole __instance)
    {
        return false;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ArrowBehaviour))]
    [HarmonyPatch(nameof(ArrowBehaviour.Awake))]
    public static void Postfix(ArrowBehaviour __instance)
    {
        if (__instance is not NoisemakerArrow nmArrow)
        {
            return;
        }
        var toggleAction = new Action(() =>
        {
            if (NoisemakerModifier.ActiveNoisemakerTriggers.ContainsValue(nmArrow))
            {
                var associatedPair = NoisemakerModifier.ActiveNoisemakerTriggers.FirstOrDefault(x => x.Value == nmArrow).Key;
                NoisemakerModifier.ActiveNoisemakerTriggers.Remove(associatedPair);
            }
        });
        nmArrow.onFadeTrigger += toggleAction;
    }
}