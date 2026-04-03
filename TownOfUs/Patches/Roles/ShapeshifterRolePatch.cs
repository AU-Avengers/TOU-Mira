using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(PlayerControl))]
public static class ShapeshifterRolePatch
{
    [HarmonyPatch(nameof(PlayerControl.Shapeshift))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool CrewmateColorPrefix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        __instance.waitingForShapeshiftResponse = false;
        var cancelAbility = false;
        if (__instance.GetAppearanceType() == TownOfUsAppearances.MushroomMixUp)
        {
            __instance.logger.Info(
                "Ignoring shapeshift message for " +
                ((targetPlayer == null) ? "null player" : targetPlayer.PlayerId.ToString(TownOfUsPlugin.Culture)) +
                " because of mushroom mixup", null);
            cancelAbility = true;
        }

        if (__instance.GetAppearanceType() == TownOfUsAppearances.Camouflage)
        {
            __instance.logger.Info(
                "Ignoring shapeshift message for " +
                ((targetPlayer == null) ? "null player" : targetPlayer.PlayerId.ToString(TownOfUsPlugin.Culture)) +
                " because of camouflage", null);
            cancelAbility = true;
        }

        if (cancelAbility)
        {
            if (__instance.AmOwner)
            {
                HudManager.Instance.AbilityButton.SetFromSettings(__instance.Data.Role.Ability);
                __instance.Data.Role.SetCooldown();
            }

            return false;
        }

        var targetPlayerInfo = targetPlayer == null ? __instance.Data : targetPlayer.Data;

        Action changeOutfit = delegate()
        {
            if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
            {
                __instance.RemoveModifier<ShapeshifterShiftModifier>();
                __instance.logger.Info(
                    string.Format(TownOfUsPlugin.Culture, "Player {0} Shapeshift is reverting", __instance.PlayerId),
                    null);
                __instance.shapeshiftTargetPlayerId = -1;
                if (__instance.AmOwner)
                {
                    HudManager.Instance.AbilityButton.SetFromSettings(
                        __instance.Data.Role.Ability);
                }
            }
            else
            {
                __instance.AddModifier<ShapeshifterShiftModifier>(targetPlayer);
                __instance.logger.Info(
                    string.Format(TownOfUsPlugin.Culture, "Player {0} is shapeshifting into {1}", __instance.PlayerId,
                        targetPlayer.PlayerId),
                    null);
                __instance.shapeshiftTargetPlayerId = (int)targetPlayer.PlayerId;
                if (__instance.AmOwner)
                {
                    HudManager.Instance.AbilityButton.OverrideText(
                        TranslationController.Instance.GetString(
                            StringNames.ShapeshiftAbilityUndo));
                }
            }
        };
        Action animationDelegate = delegate()
        {
            changeOutfit();
            __instance.cosmetics.SetScale(__instance.MyPhysics.Animations.DefaultPlayerScale,
                __instance.defaultCosmeticsScale);
            (__instance.Data.Role.TryCast<ShapeshifterRole>()!).SetEvidence();
        };
        Action shiftDelegate = delegate()
        {
            __instance.shapeshifting = false;
            if (AprilFoolsMode.ShouldLongAround())
            {
                __instance.cosmetics.ShowLongModeParts(true);
                __instance.cosmetics.SetHatVisorVisible(true);
            }
        };
        if (animate)
        {
            __instance.shapeshifting = true;
            __instance.MyPhysics.SetNormalizedVelocity(Vector2.zero);
            if (__instance.AmOwner && !Minigame.Instance)
            {
                PlayerControl.HideCursorTemporarily();
            }

            RoleEffectAnimation roleEffectAnimation =
                Object.Instantiate(RoleManager.Instance.shapeshiftAnim,
                    __instance.gameObject.transform);
            roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(__instance.AmOwner);
            roleEffectAnimation.SetMaterialColor(__instance.Data.Outfits[PlayerOutfitType.Default].ColorId);
            if (__instance.cosmetics.FlipX)
            {
                roleEffectAnimation.transform.position -= new Vector3(0.14f, 0f, 0f);
            }

            roleEffectAnimation.MidAnimCB = animationDelegate;
            float shapeshiftScale = __instance.MyPhysics.Animations.ShapeshiftScale;
            if (AprilFoolsMode.ShouldLongAround())
            {
                __instance.cosmetics.ShowLongModeParts(false);
                __instance.cosmetics.SetHatVisorVisible(false);
            }

            __instance.StartCoroutine(__instance.ScalePlayer(shapeshiftScale, 0.25f));
            roleEffectAnimation.Play(__instance, shiftDelegate, PlayerControl.LocalPlayer.cosmetics.FlipX,
                RoleEffectAnimation.SoundType.Local, 0f, true, 0f);
            return false;
        }

        changeOutfit();
        return false;
    }
}
