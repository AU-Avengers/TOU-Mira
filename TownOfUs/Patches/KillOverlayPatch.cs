using HarmonyLib;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.CoShow))]
public static class KillOverlayPatch
{
    public static void Prefix(OverlayKillAnimation __instance, KillOverlay parent)
    {
        var flame = parent.transform.FindChild("QuadParent");
        if (flame != null)
        {
            flame.transform.localPosition = new Vector3(0f, 0f);
            if (flame.transform.FindChild("BackgroundFlame").TryGetComponent<SpriteRenderer>(out var flameSprite))
            {
                flameSprite.sprite = TouAssets.KillBG.LoadAsset();
            }
        }

        if (ExileController.Instance)
        {
            if (flame != null)
            {
                flame.transform.localPosition = new Vector3(0, -1.5f);
                if (flame.transform.FindChild("BackgroundFlame").TryGetComponent<SpriteRenderer>(out var flameSprite))
                {
                    flameSprite.sprite = TouAssets.RetributionBG.LoadAsset();
                }
            }

            __instance.GetComponentsInChildren<SpriteRenderer>(true).ToList()
                .ForEach(x => x.maskInteraction = SpriteMaskInteraction.None);
            __instance.transform.localPosition -= new Vector3(2.4f, 1.5f);
        }
    }
}