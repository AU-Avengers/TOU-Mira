using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.CoShow))]
public static class KillOverlayPatch
{
    private static Material material;
    public static void Prefix(OverlayKillAnimation __instance, KillOverlay parent)
    {
        var mode = (KillColor)OptionGroupSingleton<GameMechanicOptions>.Instance.KillAnimationBackgroundColor.Value;
        var flame = parent.transform.FindChild("QuadParent");
        if (flame != null)
        {
            flame.transform.localPosition = new Vector3(0f, 0f);
            if (flame.transform.FindChild("BackgroundFlame").TryGetComponent<SpriteRenderer>(out var flameSprite))
            {
                flameSprite.sprite = TouAssets.KillBG.LoadAsset();
                if (material == null)
                {
                    material = new Material(flameSprite.material);
                }
                flameSprite.material = material;
                var killer = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Data.PlayerName == __instance.initData.killerOutfit.PlayerName);
                Color color = TownOfUsColors.Impostor;

                switch (mode)
                {
                    case KillColor.Faction:
                        if (killer != null)
                        {
                            if (killer.IsNeutral())
                            {
                                flameSprite.sprite = TouAssets.NeutKillBg.LoadAsset();
                            }
                            else if (killer.IsCrewmate())
                            {
                                flameSprite.sprite = TouAssets.CrewKillBg.LoadAsset();
                            }
                        }
                        break;
                    case KillColor.RoleColor:
                        flameSprite.sprite = TouAssets.ColorKillBg.LoadAsset();
                        flameSprite.material = __instance.killerParts.cosmetics.currentBodySprite.BodySprite.material;
                        if (killer != null)
                        {
                            color = killer.Data.Role.TeamColor;
                        }
                        flameSprite.material.SetColor(ShaderID.BodyColor, color);
                        flameSprite.material.SetColor(ShaderID.BackColor, color.LightenColor(.35f));
                        flameSprite.material.SetColor(ShaderID.VisorColor, Color.white);
                        break;
                }
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
                    if (material == null)
                    {
                        material = new Material(flameSprite.material);
                    }
                    flameSprite.material = material;
                }
            }

            __instance.GetComponentsInChildren<SpriteRenderer>(true).ToList()
                .ForEach(x => x.maskInteraction = SpriteMaskInteraction.None);
            __instance.transform.localPosition -= new Vector3(2.4f, 1.5f);
        }
    }
}