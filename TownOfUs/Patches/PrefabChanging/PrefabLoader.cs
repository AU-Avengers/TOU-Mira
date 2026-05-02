using System.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Reactor.Utilities;
using TownOfUs.Modules;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TownOfUs.Patches.PrefabChanging;

[HarmonyPatch]
public class PrefabLoader
{
    public static ShipStatus Skeld { get; private set; }

    public static PolusShipStatus Polus { get; private set; }

    public static AirshipStatus Airship { get; private set; }

    public static FungleShipStatus Fungle { get; private set; }

    public static ShipStatus? Submerged => SubmarineStatus as ShipStatus;
    private static MonoBehaviour _submarineStatus;
    public static MonoBehaviour SubmarineStatus
    {
        get
        {
            if (!ModCompatibility.SubLoaded) return null!;

            if (_submarineStatus == null || _submarineStatus.WasCollected || !_submarineStatus || _submarineStatus == null)
            {
                if (ShipStatus.Instance is null || ShipStatus.Instance.WasCollected || !ShipStatus.Instance || ShipStatus.Instance == null)
                {
                    return _submarineStatus = null!;
                }
                else
                {
                    if (ShipStatus.Instance.Type == ModCompatibility.SubmergedMapType)
                    {
                        return _submarineStatus =
                            (ShipStatus.Instance.GetComponent(Il2CppType.From(ModCompatibility.SubmarineStatusType))
                                ?.TryOtherCast(ModCompatibility.SubmarineStatusType) as MonoBehaviour)!;
                    }
                    else
                    {
                        return _submarineStatus = null!;
                    }
                }
            }
            else
            {
                return _submarineStatus;
            }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.VeryLow)]
    public static void Postfix()
    {
        Coroutines.Start(LoadMaps());
    }

    public static IEnumerator LoadMaps()
    {
        while (AmongUsClient.Instance == null)
        {
            yield return null;
        }

        if (ModCompatibility.SubLoaded)
        {
            yield return new WaitForSeconds(1.5f);
        }

        if (ModCompatibility.LILoaded)
        {
            yield return new WaitForSeconds(1.5f);
        }

        if (!Skeld)
        {
            Out<ShipStatus> o = new();
            yield return LoadMap(MapNames.Skeld, o);
            Skeld = o.Value;
        }

        if (!Polus)
        {
            Out<PolusShipStatus> o = new();
            yield return LoadMap(MapNames.Polus, o);
            Polus = o.Value;
        }

        if (!Airship)
        {
            Out<AirshipStatus> o = new();
            yield return LoadMap(MapNames.Airship, o);
            Airship = o.Value;
        }

        if (!Fungle)
        {
            Out<FungleShipStatus> o = new();
            yield return LoadMap(MapNames.Fungle, o);
            Fungle = o.Value;
        }
    }

    private static IEnumerator LoadMap<T>(MapNames map, Out<T> shipStatus) where T : ShipStatus
    {
        AssetReference reference = AmongUsClient.Instance.ShipPrefabs._items[(int)map];

        AsyncOperationHandle<GameObject> handle;

        if (reference.OperationHandle.IsValid())
        {
            handle = reference.OperationHandle.Convert<GameObject>();

            if (!handle.IsDone)
                yield return handle;
        }
        else
        {
            handle = reference.LoadAsset<GameObject>();
            yield return handle;
        }

        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            shipStatus.Value = handle.Result.GetComponent<T>();
        }
        else
        {
            UnityEngine.Debug.LogError("Failed to load map asset");
        }
    }

    private sealed class Out<T>
    {
        public T Value { get; set; }
    }
}