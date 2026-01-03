using HarmonyLib;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace TownOfUs.Patches.ControlSystem;

/// <summary>
/// Patches to allow puppeteer/parasite to trigger interactions for controlled players
/// </summary>
[HarmonyPatch]
public static class ControlledPlayerInteractionPatches
{
    private static List<IUsable>? _cachedInteractables;
    private static float _lastCacheRefresh;
    private const float CacheRefreshInterval = 10f;
    private const float UpdateThrottle = 0.1f;
    private static float _lastUpdateTime;

    private static void RefreshInteractablesCache()
    {
        _cachedInteractables = new List<IUsable>();
        var allUsables = UnityObject.FindObjectsOfType<MonoBehaviour>();
        foreach (var obj in allUsables)
        {
            if (obj.TryCast<IUsable>() is { } usable && usable.TryCast<Vent>() == null)
            {
                _cachedInteractables.Add(usable);
            }
        }
        _lastCacheRefresh = Time.time;
    }
    /// <summary>
    /// Allow UseButton to work for puppeteer/parasite when controlling someone
    /// </summary>
    [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
    [HarmonyPrefix]
    public static bool UseButtonDoClickPrefix(UseButton __instance)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null)
        {
            return true;
        }

        if (localPlayer.Data?.Role is PuppeteerRole puppeteerRole && puppeteerRole.Controlled != null)
        {
            var controlled = puppeteerRole.Controlled;
            if (controlled != null && !controlled.HasDied() && 
                PuppeteerControlState.IsControlled(controlled.PlayerId, out _))
            {
                var (interactable, interactablePos) = FindClosestInteractable(controlled);
                if (interactable != null)
                {
                    PuppeteerRole.RpcPuppeteerTriggerInteraction(PlayerControl.LocalPlayer, controlled, interactablePos);
                    return false;
                }
            }
        }

        if (localPlayer.Data?.Role is ParasiteRole parasiteRole && parasiteRole.Controlled != null)
        {
            var controlled = parasiteRole.Controlled;
            if (controlled != null && !controlled.HasDied() && 
                ParasiteControlState.IsControlled(controlled.PlayerId, out _))
            {
                var (interactable, interactablePos) = FindClosestInteractable(controlled);
                if (interactable != null)
                {
                    ParasiteRole.RpcParasiteTriggerInteraction(PlayerControl.LocalPlayer, controlled, interactablePos);
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Allow UseButton to show as usable when puppeteer/parasite can interact with something
    /// This runs after SetTarget to override the target with the controlled player's interactables
    /// </summary>
    [HarmonyPatch(typeof(UseButton), nameof(UseButton.SetTarget))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void UseButtonSetTargetPostfix(UseButton __instance)
    {
        UpdateUseButtonTarget(__instance);
    }

    /// <summary>
    /// Refresh cache when ShipStatus loads (new map/game start)
    /// </summary>
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    [HarmonyPostfix]
    public static void ShipStatusAwakePostfix()
    {
        _cachedInteractables = null;
        _lastCacheRefresh = 0f;
    }

    /// <summary>
    /// Also patch HudManager Update to continuously check for interactables near controlled player
    /// Throttled to avoid performance issues
    /// </summary>
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void HudManagerUpdatePostfix(HudManager __instance)
    {
        // Throttle updates to avoid stuttering
        var now = Time.time;
        if (now - _lastUpdateTime < UpdateThrottle)
        {
            return;
        }
        _lastUpdateTime = now;

        if (__instance?.UseButton != null)
        {
            UpdateUseButtonTarget(__instance.UseButton);
        }
    }

    private static void UpdateUseButtonTarget(UseButton useButton)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || useButton == null)
        {
            return;
        }

        var isControlling = false;
        PlayerControl? controlledPlayer = null;

        if (localPlayer.Data?.Role is PuppeteerRole puppeteerRole && puppeteerRole.Controlled != null)
        {
            var controlled = puppeteerRole.Controlled;
            if (controlled != null && !controlled.HasDied() && 
                PuppeteerControlState.IsControlled(controlled.PlayerId, out _))
            {
                isControlling = true;
                controlledPlayer = controlled;
            }
        }

        if (localPlayer.Data?.Role is ParasiteRole parasiteRole && parasiteRole.Controlled != null)
        {
            var controlled = parasiteRole.Controlled;
            if (controlled != null && !controlled.HasDied() && 
                ParasiteControlState.IsControlled(controlled.PlayerId, out _))
            {
                isControlling = true;
                controlledPlayer = controlled;
            }
        }

        if (!isControlling || controlledPlayer == null)
        {
            return;
        }

        var (usable, _) = FindClosestInteractable(controlledPlayer);
        if (usable != null)
        {
            useButton.currentTarget = usable;
            useButton.SetEnabled();
            ForceUseButtonVisualEnabled(useButton);
        }
        else
        {
            useButton.currentTarget = null;
            useButton.SetDisabled();
        }
    }


    private static void ForceUseButtonVisualEnabled(UseButton useButton)
    {
        try
        {
            var renderers = useButton.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in renderers)
            {
                if (sr == null) continue;
                sr.color = Palette.EnabledColor;
                if (sr.material != null)
                {
                    sr.material.SetFloat("_Desat", 0f);
                }
            }

            var tmps = useButton.GetComponentsInChildren<TMPro.TMP_Text>(true);
            foreach (var tmp in tmps)
            {
                if (tmp == null) continue;
                tmp.color = Palette.EnabledColor;
            }
        }
        catch
        {
            // Ignore
        }
    }

    /// <summary>
    /// Find the closest interactable object near a player
    /// Uses cached interactables list to avoid expensive FindObjectsOfType every call
    /// </summary>
    private static (IUsable? interactable, Vector2 position) FindClosestInteractable(PlayerControl player)
    {
        if (player == null || player.Collider == null)
        {
            return (null, Vector2.zero);
        }

        // Refresh cache periodically or if it's null
        if (_cachedInteractables == null || Time.time - _lastCacheRefresh > CacheRefreshInterval)
        {
            RefreshInteractablesCache();
        }

        if (_cachedInteractables == null || _cachedInteractables.Count == 0)
        {
            return (null, Vector2.zero);
        }

        var closestDistance = float.MaxValue;
        IUsable? closestInteractable = null;
        Vector2 closestPosition = Vector2.zero;
        var playerPos = (Vector2)player.transform.position;

        const float maxCheckDistance = 5f;

        foreach (var usable in _cachedInteractables)
        {
            if (usable == null)
            {
                continue;
            }

            var obj = usable.TryCast<MonoBehaviour>();
            if (obj == null)
            {
                continue;
            }

            var objPos = (Vector2)obj.transform.position;
            var distance = Vector2.Distance(playerPos, objPos);
            if (distance > maxCheckDistance || distance > usable.UsableDistance)
            {
                continue;
            }

            // Check if player can use this
            bool canUse;
            usable.CanUse(player.Data, out canUse, out _);
            if (!canUse)
            {
                continue;
            }

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = usable;
                closestPosition = objPos;
            }
        }

        return (closestInteractable, closestPosition);
    }

    /// <summary>
    /// Public accessor for cached interactables (used by PuppeteerRole/ParasiteRole RPC handlers)
    /// </summary>
    public static List<IUsable>? GetCachedInteractables()
    {
        if (_cachedInteractables == null || Time.time - _lastCacheRefresh > CacheRefreshInterval)
        {
            RefreshInteractablesCache();
        }
        return _cachedInteractables;
    }
}