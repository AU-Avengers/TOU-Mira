using HarmonyLib;
using TownOfUs.Modules.Cosmetics;

namespace TownOfUs.Patches.Cosmetics;

[HarmonyPatch(typeof(HatManager))]
public static class HatManagerGetCosmeticsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetPetById))]
    public static bool GetHatPrefix(string petId, ref PetData __result)
    {
        if (!CosmeticsLoader.Instance.TryGetPet(petId, out var customPet))
        {
            return true;
        }

        __result = customPet.PetData;
        return false;
    }
}