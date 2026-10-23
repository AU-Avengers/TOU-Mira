using HarmonyLib;
using TownOfUs.Roles.Impostor;
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
                PuppeteerRole.ControlState.IsControlled(controlled.PlayerId, out _) &&
                !__instance.isCoolingDown)
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
                ParasiteRole.ControlState.IsControlled(controlled.PlayerId, out _) &&
                !__instance.isCoolingDown)
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

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanPet))]
    [HarmonyPrefix]
    public static bool PetButtonCanPetPrefix(PlayerControl __instance, ref bool __result)
    {
        if (LobbyBehaviour.Instance)
        {
            return true;
        }
        if (__instance.Data?.Role is PuppeteerRole puppeteerRole && puppeteerRole.Controlled != null)
        {
            var controlled = puppeteerRole.Controlled;
            if (controlled != null && !controlled.HasDied() &&
                PuppeteerRole.ControlState.IsControlled(controlled.PlayerId, out _))
            {
                __result = false;
                return false;
            }
        }

        if (__instance.Data?.Role is ParasiteRole parasiteRole && parasiteRole.Controlled != null)
        {
            var controlled = parasiteRole.Controlled;
            if (controlled != null && !controlled.HasDied() &&
                ParasiteRole.ControlState.IsControlled(controlled.PlayerId, out _))
            {
                __result = false;
                return false;
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

    private static void UpdateUseButtonTarget(UseButton useButton)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (!useButton || !localPlayer || !localPlayer.Data || !localPlayer.Data.Role)
        {
            return;
        }

        var isControlling = false;
        PlayerControl? controlledPlayer = null;

        if (localPlayer.Data.Role is PuppeteerRole puppeteerRole && puppeteerRole.Controlled != null)
        {
            var controlled = puppeteerRole.Controlled;
            if (controlled != null && !controlled.HasDied() && 
                PuppeteerRole.ControlState.IsControlled(controlled.PlayerId, out _))
            {
                isControlling = true;
                controlledPlayer = controlled;
            }
        }

        if (localPlayer.Data.Role is ParasiteRole parasiteRole && parasiteRole.Controlled != null)
        {
            var controlled = parasiteRole.Controlled;
            if (controlled != null && !controlled.HasDied() && 
                ParasiteRole.ControlState.IsControlled(controlled.PlayerId, out _))
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
        useButton.currentTarget = usable;
        if (usable != null)
        {
            useButton.SetEnabled();
            
            if (usable.TryCast<IUsableCoolDown>() is { } usableCoolDown)
            {
                useButton.SetCoolDown(usableCoolDown.CoolDown, usableCoolDown.MaxCoolDown);
            }
            else
            {
                useButton.ResetCoolDown();
            }
            useButton.SetCooldownFill(usable.PercentCool);
        }
        else
        {
            useButton.SetDisabled();
            useButton.ResetCoolDown();
        }
    }

    /// <summary>
	/// Find the closest interactable object near a player
	/// Uses cached interactables list to avoid expensive FindObjectsOfType every call
	/// </summary>
	public static IUsable? FindClosestInteractable(PlayerControl player, Vector2 position)
    {
        return FindClosestInteractable(player, position, false).interactable;
    }

    /// <summary>
    /// Find the closest interactable object near a player
    /// Uses cached interactables list to avoid expensive FindObjectsOfType every call
    /// </summary>
    private static (IUsable? interactable, Vector2 position) FindClosestInteractable(
        PlayerControl player,
        Vector2? position = null,
        bool setOutlines = false
        )
    {
        if (player == null || player.Collider == null)
        {
            return (null, Vector2.zero);
        }

        var cachedInteractables = GetCachedInteractables();

        if (cachedInteractables == null || cachedInteractables.Count == 0)
        {
            return (null, Vector2.zero);
        }

        var closestDistance = float.MaxValue;
        IUsable? closestInteractable = null;
        Vector2 closestPosition = Vector2.zero;
        var playerPos = position ?? (Vector2)player.transform.position;

        foreach (var usable in cachedInteractables)
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

            usable.CanUse(player.Data, out bool canUse, out bool couldUse);

            if (setOutlines)
            {
                usable.SetOutline(couldUse && distance <= player.MaxReportDistance, false);
            }

            if (!canUse || distance > player.MaxReportDistance || distance > usable.UsableDistance)
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

        if (setOutlines)
        {
            closestInteractable?.SetOutline(true, true);
        }

        return (closestInteractable, closestPosition);
    }

    /// <summary>
    /// Public accessor for cached interactables (used by PuppeteerRole/ParasiteRole RPC handlers)
	/// </summary>
	public static List<IUsable> GetCachedInteractables()
    {
        if (_cachedInteractables == null || Time.time - _lastCacheRefresh > CacheRefreshInterval)
        {
            _cachedInteractables = GetInteractablesList();
            _lastCacheRefresh = Time.time;
        }
        return _cachedInteractables!;
    }

    public static List<IUsable> GetInteractablesList()
    {
        List<IUsable> interactables = [];
        var allUsables = UnityObject.FindObjectsOfType<MonoBehaviour>();
        foreach (var obj in allUsables)
        {
            if (obj.TryCast<IUsable>() is { } usable &&
                usable.TryCast<Vent>() == null &&
                !obj.name.Contains("Vent"))
            {
                interactables.Add(usable);
            }
        }
        return interactables;
    }
}