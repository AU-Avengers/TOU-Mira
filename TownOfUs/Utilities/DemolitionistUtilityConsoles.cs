using System.Reflection;
using TownOfUs.Patches;
using Il2CppInterop.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Utilities;

public enum DemolitionistUtilityKind : byte
{
    None = 0,
    Admin = 1,
    Cameras = 2,
    Vitals = 3,
    DoorLog = 4,
}

public static class DemolitionistUtilityConsoles
{
    private const float ConsoleSearchRadius = 2f;

    private static SystemConsole[]? _cachedSystemConsoles;
    private static MapConsole[]? _cachedMapConsoles;
    private static SystemConsole? _cachedCameraConsole;
    private static SystemConsole? _cachedDoorLogConsole;
    private static int _cachedConsoleFrame = -1;

    public static string GetDisplayName(DemolitionistUtilityKind kind) =>
        kind switch
        {
             DemolitionistUtilityKind.Admin =>
            MiraLocaleManager.Get("TownOfUsMira.Demolitionist.Utility.Admin"),

            DemolitionistUtilityKind.Cameras =>
                MiraLocaleManager.Get(
                    MiscUtils.GetCurrentMap == ExpandedMapNames.Fungle
                        ? "TownOfUsMira.Demolitionist.Utility.Lookout"
                        : "TownOfUsMira.Demolitionist.Utility.Security"),

            DemolitionistUtilityKind.Vitals =>
                MiraLocaleManager.Get("TownOfUsMira.Demolitionist.Utility.Vitals"),

            DemolitionistUtilityKind.DoorLog =>
                MiraLocaleManager.Get("TownOfUsMira.Demolitionist.Utility.DoorLog"),

            _ =>
                MiraLocaleManager.Get("TownOfUsMira.Demolitionist.Utility.Unknown"),
        };

    public static bool TryGetClosest(
        PlayerControl player,
        out Vector2 position,
        out DemolitionistUtilityKind kind,
        bool forDemolitionistPlant = false)
    {
        position = Vector2.zero;
        kind = DemolitionistUtilityKind.None;

        if (player == null || player.Data == null || !ShipStatus.Instance)
        {
            return false;
        }

        if (TryGetFromVanillaUseButton(player, forDemolitionistPlant, out position, out kind))
        {
            return true;
        }

        var playerPos = player.GetTruePosition();
        var range = ConsoleSearchRadius;
        DemolitionistUtilityKind bestKind = DemolitionistUtilityKind.None;
        var bestDistance = float.MaxValue;
        var bestPosition = Vector2.zero;

        void ConsiderMap(DemolitionistUtilityKind candidateKind, MapConsole? candidateConsole)
        {
            if (candidateConsole == null
                || !IsInConsoleUseRange(candidateConsole, player, candidateKind, forDemolitionistPlant))
            {
                return;
            }

            var candidatePos = (Vector2)candidateConsole.transform.position;
            var distance = Vector2.Distance(playerPos, candidatePos);
            if (distance >= bestDistance)
            {
                return;
            }

            bestDistance = distance;
            bestKind = candidateKind;
            bestPosition = candidatePos;
        }

        void ConsiderSystem(DemolitionistUtilityKind candidateKind, SystemConsole? candidateConsole)
        {
            if (candidateConsole == null
                || !IsInConsoleUseRange(candidateConsole, player, candidateKind, forDemolitionistPlant))
            {
                return;
            }

            var candidatePos = (Vector2)candidateConsole.transform.position;
            var distance = Vector2.Distance(playerPos, candidatePos);
            if (distance >= bestDistance)
            {
                return;
            }

            bestDistance = distance;
            bestKind = candidateKind;
            bestPosition = candidatePos;
        }

        if (TryGetAdminConsole(playerPos, range, out var admin) && admin != null)
        {
            ConsiderMap(DemolitionistUtilityKind.Admin, admin);
        }

        if (TryGetCameraConsole(out var cam) && cam != null)
        {
            ConsiderSystem(DemolitionistUtilityKind.Cameras, cam);
        }

        if (TryGetVitalsConsole(out var vitals) && vitals != null)
        {
            ConsiderSystem(DemolitionistUtilityKind.Vitals, vitals);
        }

        if (TryGetDoorLogConsole(out var doorLog) && doorLog != null)
        {
            ConsiderSystem(DemolitionistUtilityKind.DoorLog, doorLog);
        }

        if (bestKind == DemolitionistUtilityKind.None)
        {
            return false;
        }

        kind = bestKind;
        position = bestPosition;
        return true;
    }

    public static int GetStableId(DemolitionistUtilityKind kind, Vector2 position)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + (int)kind;
            hash = hash * 31 + Mathf.RoundToInt(position.x * 100f);
            hash = hash * 31 + Mathf.RoundToInt(position.y * 100f);
            return hash;
        }
    }

    public static bool TryGetWorldPosition(DemolitionistUtilityKind kind, Vector2 fallbackPosition, out Vector3 worldPos)
    {
        worldPos = Vector3.zero;
        switch (kind)
        {
            case DemolitionistUtilityKind.Admin:
                if (TryGetAdminConsole(fallbackPosition, 2f, out var admin) && admin != null)
                {
                    worldPos = admin.transform.position;
                    return true;
                }
                break;
            case DemolitionistUtilityKind.Cameras:
                if (TryGetCameraConsole(out var cam) && cam != null)
                {
                    worldPos = cam.transform.position;
                    return true;
                }
                break;
            case DemolitionistUtilityKind.Vitals:
                if (TryGetVitalsConsole(out var vitals) && vitals != null)
                {
                    worldPos = vitals.transform.position;
                    return true;
                }
                break;
            case DemolitionistUtilityKind.DoorLog:
                if (TryGetDoorLogConsole(out var door) && door != null)
                {
                    worldPos = door.transform.position;
                    return true;
                }
                break;
        }

        worldPos = new Vector3(fallbackPosition.x, fallbackPosition.y, 0f);
        return kind != DemolitionistUtilityKind.None;
    }

    private static bool TryGetPlantedConsole(
        DemolitionistUtilityKind kind, Vector2 plantedPosition, out Component? console)
    {
        console = null;

        switch (kind)
        {
            case DemolitionistUtilityKind.Admin:
                if (TryGetAdminConsole(plantedPosition, ConsoleSearchRadius, out var admin)) console = admin;
                break;
            case DemolitionistUtilityKind.Cameras:
                if (TryGetCameraConsole(out var cam)) console = cam;
                break;
            case DemolitionistUtilityKind.Vitals:
                if (TryGetVitalsConsole(out var vitals)) console = vitals;
                break;
            case DemolitionistUtilityKind.DoorLog:
                if (TryGetDoorLogConsole(out var door)) console = door;
                break;
        }

        return console != null;
    }

    public static bool TryGetPlantedConsoleRenderer(
        DemolitionistUtilityKind kind, Vector2 plantedPosition, out SpriteRenderer? rend)
    {
        rend = null;
        if (!TryGetPlantedConsole(kind, plantedPosition, out var console) || console == null)
        {
            return false;
        }

        rend = console.GetComponentInChildren<SpriteRenderer>();
        return rend != null;
    }
    public static bool IsLocalPlayerInPlantedConsoleUseRange(
        PlayerControl player, DemolitionistUtilityKind kind, Vector2 plantedPosition)
    {
        if (player?.Data == null)
        {
            return false;
        }

        if (!TryGetPlantedConsole(kind, plantedPosition, out var console) || console == null)
        {
            return false;
        }

        if (console.TryCast<MapConsole>() is MapConsole map)
        {
            map.CanUse(player.Data, out var canUse, out _);
            return canUse;
        }

        if (console.TryCast<SystemConsole>() is SystemConsole sys)
        {
            sys.CanUse(player.Data, out var canUse, out _);
            return canUse;
        }

        return false;
    }

    public static bool IsAtPlantedUtility(
        PlayerControl player,
        DemolitionistUtilityKind plantedKind,
        Vector2 plantedPosition,
        int plantedConsoleKey)
    {
        if (player == null || plantedKind == DemolitionistUtilityKind.None)
        {
            return false;
        }

        return plantedKind switch
        {
            DemolitionistUtilityKind.Admin => TryGetAdminConsole(plantedPosition, ConsoleSearchRadius, out var admin)
                && admin != null
                && MatchesPlantedConsole(plantedKind, plantedPosition, plantedConsoleKey, admin.transform.position)
                && IsInConsoleUseRange(admin, player, DemolitionistUtilityKind.Admin, forDemolitionistPlant: true),
            DemolitionistUtilityKind.Cameras => TryGetCameraConsole(out var cam)
                && cam != null
                && MatchesPlantedConsole(plantedKind, plantedPosition, plantedConsoleKey, cam.transform.position)
                && IsInConsoleUseRange(cam, player, DemolitionistUtilityKind.Cameras, forDemolitionistPlant: true),
            DemolitionistUtilityKind.Vitals => TryGetVitalsConsole(out var vitals)
                && vitals != null
                && MatchesPlantedConsole(plantedKind, plantedPosition, plantedConsoleKey, vitals.transform.position)
                && IsInConsoleUseRange(vitals, player, DemolitionistUtilityKind.Vitals, forDemolitionistPlant: true),
            DemolitionistUtilityKind.DoorLog => TryGetDoorLogConsole(out var door)
                && door != null
                && MatchesPlantedConsole(plantedKind, plantedPosition, plantedConsoleKey, door.transform.position)
                && IsInConsoleUseRange(door, player, DemolitionistUtilityKind.DoorLog, forDemolitionistPlant: true),
            _ => false,
        };
    }

    private static bool MatchesPlantedConsole(
        DemolitionistUtilityKind kind,
        Vector2 plantedPosition,
        int plantedConsoleKey,
        Vector3 consolePosition)
    {
        var pos = (Vector2)consolePosition;
        return GetStableId(kind, pos) == plantedConsoleKey
            || Vector2.Distance(pos, plantedPosition) <= 0.25f;
    }

    public static bool IsInConsoleUseRange(
        MapConsole console,
        PlayerControl player,
        DemolitionistUtilityKind kind,
        bool forDemolitionistPlant)
    {
        return IsInConsoleUseRangeInternal(console, player, kind, forDemolitionistPlant);
    }

    /// <inheritdoc cref="IsInConsoleUseRange(MapConsole, PlayerControl, DemolitionistUtilityKind, bool)"/>
    public static bool IsInConsoleUseRange(
        SystemConsole console,
        PlayerControl player,
        DemolitionistUtilityKind kind,
        bool forDemolitionistPlant)
    {
        return IsInConsoleUseRangeInternal(console, player, kind, forDemolitionistPlant);
    }

    private static bool TryGetFromVanillaUseButton(
        PlayerControl player,
        bool forDemolitionistPlant,
        out Vector2 position,
        out DemolitionistUtilityKind kind)
    {
        position = Vector2.zero;
        kind = DemolitionistUtilityKind.None;

        var hud = HudManager.Instance;
        if (hud?.UseButton is not { isActiveAndEnabled: true })
        {
            return false;
        }

        var target = hud.UseButton.currentTarget;
        if (target == null)
        {
            return false;
        }

        if (target.TryCast<MapConsole>() is MapConsole map
            && TryGetAdminConsole((Vector2)map.transform.position, 0.25f, out var admin)
            && admin == map
            && IsInConsoleUseRange(map, player, DemolitionistUtilityKind.Admin, forDemolitionistPlant))
        {
            kind = DemolitionistUtilityKind.Admin;
            position = map.transform.position;
            return true;
        }

        if (target.TryCast<SystemConsole>() is not SystemConsole sys)
        {
            return false;
        }

        if (TryGetCameraConsole(out var cam) && cam == sys
            && IsInConsoleUseRange(sys, player, DemolitionistUtilityKind.Cameras, forDemolitionistPlant))
        {
            kind = DemolitionistUtilityKind.Cameras;
            position = sys.transform.position;
            return true;
        }

        if (TryGetVitalsConsole(out var vitals) && vitals == sys
            && IsInConsoleUseRange(vitals, player, DemolitionistUtilityKind.Vitals, forDemolitionistPlant))
        {
            kind = DemolitionistUtilityKind.Vitals;
            position = sys.transform.position;
            return true;
        }

        if (TryGetDoorLogConsole(out var door) && door == sys
            && IsInConsoleUseRange(door, player, DemolitionistUtilityKind.DoorLog, forDemolitionistPlant))
        {
            kind = DemolitionistUtilityKind.DoorLog;
            position = sys.transform.position;
            return true;
        }

        return false;
    }

    private static bool IsInConsoleUseRangeInternal(
        Component console,
        PlayerControl player,
        DemolitionistUtilityKind kind,
        bool forDemolitionistPlant)
    {
        if (console == null || player?.Data == null || kind == DemolitionistUtilityKind.None)
        {
            return false;
        }

        var consolePos = (Vector2)console.transform.position;
        var playerPos = player.GetTruePosition();
        var dist = Vector2.Distance(playerPos, consolePos);
        var maxReach = GetUsableDistance(console) + 0.1f;
        if (dist > maxReach)
        {
            return false;
        }

        var usableD = GetUsableDistance(console);
        var canUseDistance = QueryCanUseDistance(console, player, out var couldUse);

        if (couldUse && canUseDistance <= usableD + 0.15f)
        {
            return true;
        }

        if (!forDemolitionistPlant)
        {
            return false;
        }

        var key = GetStableId(kind, consolePos);
        if (!DemolitionistSabotageState.IsUtilityDisabled(key, kind))
        {
            return false;
        }

        return true;
    }

    private static float QueryCanUseDistance(Component console, PlayerControl player, out bool couldUse)
    {
        couldUse = false;
        if (console.TryCast<MapConsole>() is MapConsole map)
        {
            return map.CanUse(player.Data, out _, out couldUse);
        }

        if (console.TryCast<SystemConsole>() is SystemConsole sys)
        {
            return sys.CanUse(player.Data, out _, out couldUse);
        }

        if (console.TryCast<global::Console>() is global::Console plain)
        {
            return plain.CanUse(player.Data, out _, out couldUse);
        }

        return float.MaxValue;
    }

    public static float GetUsableDistance(Component console)
    {
        if (console == null)
        {
            return 1f;
        }

        if (console.TryCast<global::Console>() is global::Console c)
        {
            return c.UsableDistance;
        }

        foreach (var name in new[] { "UsableDistance", "usableDistance" })
        {
            var prop = console.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (prop?.PropertyType == typeof(float))
            {
                return (float)prop.GetValue(console)!;
            }

            var field = console.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field?.FieldType == typeof(float))
            {
                return (float)field.GetValue(console)!;
            }
        }

        return 1f;
    }

    /// <inheritdoc cref="GetUsableDistance"/>
    public static float GetConsoleUseDistance(object console) =>
        console is Component component ? GetUsableDistance(component) : 1f;

    public static bool TryClassifySystemConsole(SystemConsole sys, out DemolitionistUtilityKind kind)
    {
        kind = DemolitionistUtilityKind.None;
        if (sys == null)
        {
            return false;
        }

        if (TryGetCameraConsole(out var cam) && cam == sys)
        {
            kind = DemolitionistUtilityKind.Cameras;
            return true;
        }

        if (TryGetVitalsConsole(out var vitals) && vitals == sys)
        {
            kind = DemolitionistUtilityKind.Vitals;
            return true;
        }

        if (TryGetDoorLogConsole(out var door) && door == sys)
        {
            kind = DemolitionistUtilityKind.DoorLog;
            return true;
        }

        return false;
    }

    private static bool TryGetAdminDistance(Vector2 from, float range, out float dist)
    {
        dist = float.MaxValue;
        RefreshConsoleCache();
        var consoles = _cachedMapConsoles;
        if (consoles == null || consoles.Length == 0)
        {
            return false;
        }

        foreach (var mc in consoles)
        {
            if (mc == null)
            {
                continue;
            }

            var d = Vector2.Distance(from, (Vector2)mc.transform.position);
            if (d <= range && d < dist)
            {
                dist = d;
            }
        }

        return dist < float.MaxValue;
    }

    private static bool TryGetAdminConsole(Vector2 from, float range, out MapConsole? console)
    {
        console = null;
        RefreshConsoleCache();
        var consoles = _cachedMapConsoles;
        if (consoles == null || consoles.Length == 0)
        {
            return false;
        }

        MapConsole? closest = null;
        var best = float.MaxValue;

        foreach (var mc in consoles)
        {
            if (mc == null)
            {
                continue;
            }

            var d = Vector2.Distance(from, (Vector2)mc.transform.position);
            if (d > range || d >= best)
            {
                continue;
            }

            closest = mc;
            best = d;
        }

        console = closest;
        return console != null;
    }

    private static bool TryGetVitalsDistance(Vector2 from, float range, out float dist)
    {
        dist = float.MaxValue;
        if (!TryGetVitalsConsole(out var vitals) || vitals == null)
        {
            return false;
        }

        var d = Vector2.Distance(from, (Vector2)vitals.transform.position);
        if (d <= range)
        {
            dist = d;
            return true;
        }

        return false;
    }

    private static bool TryGetVitalsConsole(out SystemConsole? console)
    {
        console = null;
        foreach (var sc in GetCachedSystemConsoles())
        {
            if (sc == null || sc.MinigamePrefab == null)
            {
                continue;
            }

            if (sc.MinigamePrefab.TryCast<VitalsMinigame>() == null
                && !sc.name.Contains("vitals", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            console = sc;
            return true;
        }

        return false;
    }

    private static bool TryGetCameraDistance(Vector2 from, float range, out float dist)
    {
        dist = float.MaxValue;
        if (!TryGetCameraConsole(out var cam) || cam == null)
        {
            return false;
        }

        var d = Vector2.Distance(from, (Vector2)cam.transform.position);
        if (d <= range)
        {
            dist = d;
            return true;
        }

        return false;
    }

    private static Minigame? _cachedKeypadAssetPrefab;

    public static bool TryGetO2KeypadPrefab(out Minigame? prefab)
    {
        prefab = null;

        // Prefer a non-scene asset so an imp O2 sabo cannot leave animating/done stuck on the prefab we clone.
        if (_cachedKeypadAssetPrefab?.gameObject != null)
        {
            prefab = _cachedKeypadAssetPrefab;
            return true;
        }

        _cachedKeypadAssetPrefab = null;

        if (TryGetKeypadPrefabFromResources(out prefab) && prefab != null)
        {
            _cachedKeypadAssetPrefab = prefab;
            return true;
        }

        Minigame? fallback = null;

        // O2 fix uses Console (not always SystemConsole) on Skeld; FindObjectsOfType can miss some loads.
        foreach (var console in EnumerateGameplayConsoles())
        {
            var minigame = TryGetMinigamePrefabFromTaskConsole(console);
            if (minigame == null)
            {
                continue;
            }

            if (minigame.TryCast<KeypadGame>() == null)
            {
                continue;
            }

            fallback ??= minigame;
            if (ConsoleHasTaskType(console, TaskTypes.RestoreOxy))
            {
                prefab = minigame;
                return true;
            }
        }

        foreach (var console in GetCachedSystemConsoles())
        {
            if (console == null || console.MinigamePrefab == null)
            {
                continue;
            }

            if (console.MinigamePrefab.TryCast<KeypadGame>() == null)
            {
                continue;
            }

            fallback ??= console.MinigamePrefab;
            var name = console.gameObject.name;
            if (name.Contains("o2", System.StringComparison.OrdinalIgnoreCase)
                || name.Contains("oxygen", System.StringComparison.OrdinalIgnoreCase)
                || name.Contains("LifeSupp", System.StringComparison.OrdinalIgnoreCase))
            {
                prefab = console.MinigamePrefab;
                return true;
            }
        }

        prefab = fallback;
        if (prefab != null)
        {
            _cachedKeypadAssetPrefab = prefab;
            return true;
        }

        return false;
    }

    public static void InvalidateKeypadPrefabCache()
    {
        _cachedKeypadAssetPrefab = null;
    }

    private static bool TryGetKeypadPrefabFromResources(out Minigame? prefab)
    {
        prefab = null;
        try
        {
            var arr = Resources.FindObjectsOfTypeAll(Il2CppType.From(typeof(KeypadGame)));
            if (arr == null)
            {
                return false;
            }

            foreach (var obj in arr)
            {
                if (obj == null)
                {
                    continue;
                }

                var kg = obj.TryCast<KeypadGame>();
                if (kg == null || kg.gameObject == null)
                {
                    continue;
                }

                // Never clone an in-scene keypad (e.g. after imp O2 sabo) — it can keep animating/done stuck.
                var scene = kg.gameObject.scene;
                if (scene.IsValid() && scene.isLoaded)
                {
                    continue;
                }

                prefab = kg;
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static Minigame? TryGetMinigamePrefabFromTaskConsole(global::Console console)
    {
        if (console == null)
        {
            return null;
        }

        try
        {
            var prop = console.GetType().GetProperty(
                "MinigamePrefab",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return prop?.GetValue(console) as Minigame;
        }
        catch
        {
            return null;
        }
    }

    private static bool ConsoleHasTaskType(global::Console console, TaskTypes taskType)
    {
        try
        {
            var tasks = console.TaskTypes;
            if (tasks == null)
            {
                return false;
            }

            var len = tasks.Length;
            for (var i = 0; i < len; i++)
            {
                if (tasks[i] == taskType)
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static IEnumerable<global::Console> EnumerateGameplayConsoles()
    {
        var seen = new HashSet<int>();
        foreach (var console in Object.FindObjectsOfType<global::Console>())
        {
            if (console == null || !console || console.gameObject == null)
            {
                continue;
            }

            var id = console.gameObject.GetInstanceID();
            if (!seen.Add(id))
            {
                continue;
            }

            yield return console;
        }

        var allObjects = Resources.FindObjectsOfTypeAll(Il2CppType.From(typeof(global::Console)));
        if (allObjects == null)
        {
            yield break;
        }

        foreach (var obj in allObjects)
        {
            if (obj == null)
            {
                continue;
            }

            var console = obj.TryCast<global::Console>();
            if (console == null || console.gameObject == null || !console.gameObject.scene.isLoaded)
            {
                continue;
            }

            var id = console.gameObject.GetInstanceID();
            if (!seen.Add(id))
            {
                continue;
            }

            yield return console;
        }
    }

    private static bool TryGetCameraConsole(out SystemConsole? console)
    {
        console = null;
        if (_cachedCameraConsole != null
            && _cachedCameraConsole.gameObject != null
            && _cachedCameraConsole.gameObject.activeInHierarchy)
        {
            console = _cachedCameraConsole;
            return true;
        }

        _cachedCameraConsole = null;
        var mapId = GetMapId();
        var consoles = GetCachedSystemConsoles();
        if (consoles.Length == 0)
        {
            return false;
        }

        SystemConsole? result = null;
        if (mapId == MapNames.Airship)
        {
            result = FirstMatching(consoles, x => x.gameObject.name.Contains("task_cams"));
        }
        else if (mapId is MapNames.Skeld or MapNames.Dleks)
        {
            result = FirstMatching(consoles, x => x.gameObject.name.Contains("SurvConsole"));
        }
        else if (mapId == MapNames.MiraHQ)
        {
            result = FirstMatching(consoles, IsDoorLogSystemConsole)
                     ?? FirstMatching(consoles, x => x.gameObject.name.Contains("SurvLogConsole"));
        }
        else
        {
            result = FirstMatching(consoles, x =>
                x.gameObject.name.Contains("Surv_Panel", System.StringComparison.OrdinalIgnoreCase)
                || x.name.Contains("Cam", System.StringComparison.OrdinalIgnoreCase)
                || x.name.Contains("BinocularsSecurityConsole", System.StringComparison.OrdinalIgnoreCase)
                || x.gameObject.name.Contains("SecurityConsole", System.StringComparison.OrdinalIgnoreCase));
        }

        if (result != null)
        {
            _cachedCameraConsole = result;
            console = result;
            return true;
        }

        return false;
    }

    private static bool TryGetDoorLogDistance(Vector2 from, float range, out float dist)
    {
        dist = float.MaxValue;
        var mapId = GetMapId();

        if (mapId == MapNames.MiraHQ)
        {
            var miraDoorLogPos = new Vector2(15.9f, 4.8f);
            var d = Vector2.Distance(from, miraDoorLogPos);
            if (d <= range)
            {
                dist = d;
                return true;
            }

            return false;
        }

        if (!TryGetDoorLogConsole(out var door) || door == null)
        {
            return false;
        }

        var d2 = Vector2.Distance(from, (Vector2)door.transform.position);
        if (d2 <= range)
        {
            dist = d2;
            return true;
        }

        return false;
    }

    private static bool TryGetDoorLogConsole(out SystemConsole? console)
    {
        console = null;
        if (_cachedDoorLogConsole != null
            && _cachedDoorLogConsole.gameObject != null
            && _cachedDoorLogConsole.gameObject.activeInHierarchy)
        {
            console = _cachedDoorLogConsole;
            return true;
        }

        _cachedDoorLogConsole = null;
        var consoles = GetCachedSystemConsoles();
        var result = FirstMatching(consoles, IsDoorLogSystemConsole)
                     ?? FirstMatching(consoles, x =>
                         x.gameObject.name.Contains("DoorLog", System.StringComparison.OrdinalIgnoreCase)
                         || x.gameObject.name.Contains("SurvLogConsole", System.StringComparison.OrdinalIgnoreCase)
                         || x.gameObject.name.Contains("SurvLog", System.StringComparison.OrdinalIgnoreCase));

        if (result != null)
        {
            _cachedDoorLogConsole = result;
            console = result;
            return true;
        }

        return false;
    }

    private static bool IsDoorLogSystemConsole(SystemConsole console)
    {
        if (console == null || console.MinigamePrefab == null)
        {
            return false;
        }

        if (console.MinigamePrefab.TryCast<SecurityLogGame>() != null)
        {
            return true;
        }

        return console.gameObject.name.Contains("SurvLogConsole", System.StringComparison.OrdinalIgnoreCase)
               || console.gameObject.name.Contains("DoorLog", System.StringComparison.OrdinalIgnoreCase);
    }

    private static SystemConsole? FirstMatching(SystemConsole[] consoles, System.Func<SystemConsole, bool> predicate)
    {
        foreach (var sc in consoles)
        {
            if (sc != null && predicate(sc))
            {
                return sc;
            }
        }

        return null;
    }

    private static MapNames GetMapId()
    {
        if (TutorialManager.InstanceExists)
        {
            return (MapNames)AmongUsClient.Instance.TutorialMapId;
        }

        return (MapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId;
    }

    private static void RefreshConsoleCache()
    {
        if (_cachedMapConsoles != null && Time.frameCount == _cachedConsoleFrame)
        {
            return;
        }

        _cachedMapConsoles = Object.FindObjectsOfType<MapConsole>();
        _cachedSystemConsoles = FindAllSystemConsoles();
        _cachedCameraConsole = null;
        _cachedDoorLogConsole = null;
        _cachedConsoleFrame = Time.frameCount;
    }

    private static SystemConsole[] GetCachedSystemConsoles()
    {
        RefreshConsoleCache();
        return _cachedSystemConsoles ?? System.Array.Empty<SystemConsole>();
    }

    private static SystemConsole[] FindAllSystemConsoles()
    {
        var consoles = Object.FindObjectsOfType<SystemConsole>();
        if (consoles != null && consoles.Length > 0)
        {
            return consoles;
        }

        try
        {
            var allObjects = Resources.FindObjectsOfTypeAll(Il2CppType.From(typeof(SystemConsole)));
            if (allObjects == null)
            {
                return System.Array.Empty<SystemConsole>();
            }

            var result = new List<SystemConsole>();
            foreach (var obj in allObjects)
            {
                if (obj == null)
                {
                    continue;
                }

                var sc = obj.TryCast<SystemConsole>();
                if (sc == null || sc.gameObject == null || !sc.gameObject.scene.isLoaded)
                {
                    continue;
                }

                result.Add(sc);
            }

            return result.ToArray();
        }
        catch
        {
            return System.Array.Empty<SystemConsole>();
        }
    }

    public static void InvalidateCache()
    {
        _cachedSystemConsoles = null;
        _cachedMapConsoles = null;
        _cachedCameraConsole = null;
        _cachedDoorLogConsole = null;
        _cachedConsoleFrame = -1;
        InvalidateKeypadPrefabCache();
    }
}
