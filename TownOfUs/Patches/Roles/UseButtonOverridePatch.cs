using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons.Neutral.NeutralEvil;
using TownOfUs.Roles.Neutral.NeutralEvil;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class UseButtonOverridePatch
{
    private static readonly Color PortalLabelColor = new Color(0.047f, 0.420f, 0.961f);

    private enum Kind
    {
        None,
        Portal,
        Defuse,
    }

    private static Kind _active = Kind.None;
    private static Sprite? _savedSprite;
    private static bool _savedSpriteValid;
    private static string? _savedLabel;
    private static bool _savedLabelActive;
    private static bool _savedLabelValid;
    private static readonly Dictionary<int, SpriteRenderer[]> RendererCache = new();

    // FixedUpdate runs before Unity polls mouse/collider input, so forcing the use
    // button active here (on an always-active object) guarantees it owns the slot at
    // click time. Postfixing only the Update-phase hooks was too late for the click.
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void PlayerControlFixedUpdatePostfix(PlayerControl __instance)
    {
        if (__instance == null || !__instance.AmOwner)
        {
            return;
        }

        ApplyOverride();
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void HudManagerUpdatePostfix()
    {
        // PortalManager.UpdatePortalOutlines();
        DemolitionistSabotageState.UpdatePlantedConsoleOutline();

        ApplyOverride();
    }

    [HarmonyPatch(typeof(UseButton), nameof(UseButton.SetTarget))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void UseButtonSetTargetPostfix()
    {
        ApplyOverride();
    }

    [HarmonyPatch(typeof(ConsoleJoystick), nameof(ConsoleJoystick.Update))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void ConsoleJoystickUpdatePostfix()
    {
        ApplyOverride();
    }

    [HarmonyPatch(typeof(PetButton), nameof(PetButton.SetTarget))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void PetButtonSetTargetPostfix(PetButton __instance)
    {
        if (__instance == null || ComputeKind() == Kind.None)
        {
            return;
        }

        if (__instance.gameObject.activeSelf)
        {
            __instance.gameObject.SetActive(false);
        }

        ApplyOverride();
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour),
        typeof(bool))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void SetHudActivePostfix()
    {
        ApplyOverride();
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanPet))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool CanPetPrefix(PlayerControl __instance, ref bool __result)
    {
        if (__instance == null || !__instance.AmOwner)
        {
            return true;
        }

        if (ComputeKind() == Kind.None)
        {
            return true;
        }

        __result = false;
        return false;
    }

    [HarmonyPatch(typeof(ActionButton), nameof(ActionButton.SetDisabled))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool ActionButtonSetDisabledPrefix(ActionButton __instance)
    {
        var hud = HudManager.Instance;
        if (hud == null || __instance != hud.UseButton)
        {
            return true;
        }

        return !WantsUseButtonEnabled();
    }

    private static bool WantsUseButtonEnabled()
    {
        switch (ComputeKind())
        {
            case Kind.Defuse:
                return !DemolitionistDefuseButton.IsLocalDefusing;
            default:
                return false;
        }
    }

    [HarmonyPatch(typeof(PetButton), nameof(PetButton.DoClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool PetButtonDoClickPrefix()
    {
        return UseButtonDoClickPrefix();
    }

    [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool UseButtonDoClickPrefix()
    {
        switch (_active)
        {
            case Kind.Defuse:
                CustomButtonSingleton<DemolitionistDefuseButton>.Instance?.TriggerDefuseFromUseButton();
                return false;/*
            case Kind.Portal:
                CustomButtonSingleton<UsePortalButton>.Instance?.TriggerFromUseButton();
                return false;*/
            default:
                return true;
        }
    }

    private static void ApplyOverride()
    {
        var hud = HudManager.Instance;
        if (hud == null)
        {
            return;
        }

        var useButton = hud.UseButton;
        if (useButton == null)
        {
            return;
        }

        var kind = ComputeKind();

        if (kind == Kind.None)
        {
            ClearOverride(useButton);
            return;
        }

        if (_active == Kind.None)
        {
            SaveOriginal(useButton);
        }

        _active = kind;

        DriveSlot(hud, useButton);

        var sprite = GetOverrideSprite(kind);

        var label = kind == Kind.Defuse ? "DEFUSE" : "USE PORTAL";
        var labelColor = kind == Kind.Defuse ? DemolitionistRole.DemolitionistColor : PortalLabelColor;

        var enabled = !DemolitionistDefuseButton.IsLocalDefusing;

        if (enabled)
        {
            useButton.SetEnabled();
            ForceVisualEnabled(useButton);
        }
        else
        {
            useButton.SetDisabled();
        }

        /*if (kind == Kind.Portal && portalCooldown > 0f)
        {
            var maxCooldown = OptionGroupSingleton<PortalmakerOptions>.Instance.UsePortalCooldown.Value;
            useButton.SetCoolDown(portalCooldown, maxCooldown);
        }
        else
        {
        }*/
        useButton.SetCoolDown(0f, 1f);

        if (useButton.graphic != null && sprite != null && useButton.graphic.sprite != sprite)
        {
            useButton.graphic.sprite = sprite;
            useButton.graphic.SetCooldownNormalizedUvs();
        }

        if (useButton.buttonLabelText != null)
        {
            useButton.buttonLabelText.gameObject.SetActive(true);
            useButton.buttonLabelText.text = label;
            useButton.buttonLabelText.color = labelColor;
            useButton.buttonLabelText.SetOutlineColor(labelColor);
        }
    }

    private static void DriveSlot(HudManager hud, UseButton useButton)
    {
        if (!useButton.gameObject.activeSelf)
        {
            useButton.gameObject.SetActive(true);
        }

        var pet = hud.PetButton;
        if (pet != null && pet.gameObject.activeSelf)
        {
            pet.gameObject.SetActive(false);
        }
    }

    private static Sprite? GetOverrideSprite(Kind kind)
    {
        return kind switch
        {
            Kind.Defuse => DivaniAssets.DemolitionistDefuseButton.LoadAsset(),
            // Kind.Portal => _portalSprite ??= DivaniAssets.UsePortalButton.LoadAsset(),
            _ => null,
        };
    }

    private static SpriteRenderer[] GetCachedRenderers(UseButton useButton)
    {
        var key = useButton.GetInstanceID();
        if (!RendererCache.TryGetValue(key, out var renderers))
        {
            renderers = useButton.GetComponentsInChildren<SpriteRenderer>(true);
            RendererCache[key] = renderers;
        }

        return renderers;
    }

    private static void ForceVisualEnabled(UseButton useButton)
    {
        var renderers = GetCachedRenderers(useButton);
        foreach (var sr in renderers)
        {
            if (sr == null)
            {
                continue;
            }

            sr.color = Palette.EnabledColor;
            if (sr.material != null)
            {
                sr.material.SetFloat("_Desat", 0f);
            }
        }
    }

    private static void SaveOriginal(UseButton useButton)
    {
        if (useButton.graphic != null)
        {
            _savedSprite = useButton.graphic.sprite;
            _savedSpriteValid = true;
        }

        if (useButton.buttonLabelText != null)
        {
            _savedLabel = useButton.buttonLabelText.text;
            _savedLabelActive = useButton.buttonLabelText.gameObject.activeSelf;
            _savedLabelValid = true;
        }
    }

    private static void ClearOverride(UseButton useButton)
    {
        if (_active == Kind.None)
        {
            return;
        }

        if (_savedSpriteValid && useButton.graphic != null)
        {
            useButton.graphic.sprite = _savedSprite;
            useButton.graphic.SetCooldownNormalizedUvs();
        }

        RendererCache.Remove(useButton.GetInstanceID());

        if (_savedLabelValid && useButton.buttonLabelText != null)
        {
            useButton.buttonLabelText.text = _savedLabel;
            useButton.buttonLabelText.gameObject.SetActive(_savedLabelActive);
        }

        if (useButton.currentTarget == null)
        {
            useButton.SetDisabled();
        }

        _savedSprite = null;
        _savedSpriteValid = false;
        _savedLabel = null;
        _savedLabelValid = false;
        _active = Kind.None;
    }

    private static Kind ComputeKind()
    {
        if (!ShipStatus.Instance || MeetingHud.Instance || ExileController.Instance)
        {
            return Kind.None;
        }

        if (DemolitionistDefuseButton.ShouldDriveUseButton())
        {
            return Kind.Defuse;
        }

        /*if (UsePortalButton.ShouldDriveUseButton() && !HasVanillaUseTarget())
        {
            return Kind.Portal;
        }*/

        return Kind.None;
    }

    private static bool HasVanillaUseTarget()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.itemsInRange == null)
        {
            return false;
        }

        foreach (var usable in player.itemsInRange)
        {
            if (usable == null)
            {
                continue;
            }

            usable.CanUse(player.Data, out var canUse, out _);
            if (canUse)
            {
                return true;
            }
        }

        return false;
    }
}
