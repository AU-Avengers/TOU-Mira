using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch]
public static class ShowVentsPatch
{
    public static readonly List<List<Vent>> VentNetworks = [];

    public static readonly Dictionary<int, GameObject> VentIcons = [];
    public static readonly Dictionary<int, GameObject> BodyIcons = [];

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowCountOverlay))]
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    [HarmonyPostfix]
    public static void Postfix(MapBehaviour __instance)
    {
        if (PlayerControl.LocalPlayer.HasModifier<SatelliteModifier>())
        {
            foreach (var deadBody in ModifierUtils.GetActiveModifiers<SatelliteArrowModifier>()
                         .Select(bodyMod => bodyMod.DeadBody))
            {
                var location = deadBody.transform.position / ShipStatus.Instance.MapScale;
                location.z = -1.99f;

                if (!BodyIcons.TryGetValue(deadBody.ParentId, out var Icon) || Icon == null)
                {
                    Icon = Object.Instantiate(__instance.HerePoint.gameObject, __instance.HerePoint.transform.parent);
                    var renderer = Icon.GetComponent<SpriteRenderer>();
                    renderer.sprite = TouAssets.MapBodySprite.LoadAsset();
                    Icon.name = $"Satellite Body {deadBody.ParentId} Map Icon";
                    Icon.transform.localPosition = location;
                    BodyIcons[deadBody.ParentId] = Icon;
                }

                Icon.transform.localScale = Vector3.one;
            }
        }

        if (!ModifierUtils.GetActiveModifiers<SatelliteArrowModifier>().Any())
        {
            foreach (var icon in BodyIcons.Values.Where(x => x))
            {
                Object.Destroy(icon);
            }

            BodyIcons.Clear();
        }

        if (!LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ShowVentsToggle.Value)
        {
            foreach (var icon in VentIcons.Values.Where(x => x))
            {
                Object.Destroy(icon);
            }

            VentIcons.Clear();
            VentNetworks.Clear();
            return;
        }

        var task = PlayerControl.LocalPlayer.myTasks.ToArray()
            .FirstOrDefault(x => x.TaskType == TaskTypes.VentCleaning);

        foreach (var vent in ShipStatus.Instance.AllVents)
        {
            if (vent.name.StartsWith("MinerVent-", StringComparison.Ordinal))
            {
                continue;
            }

            var location = vent.transform.position / ShipStatus.Instance.MapScale;
            location.z = -0.99f;

            if (!VentIcons.TryGetValue(vent.Id, out var Icon) || Icon == null)
            {
                Icon = Object.Instantiate(__instance.HerePoint.gameObject, __instance.HerePoint.transform.parent);
                var renderer = Icon.GetComponent<SpriteRenderer>();
                renderer.sprite = TouAssets.MapVentSprite.LoadAsset();
                Icon.name = $"Vent {vent.Id} Map Icon";
                Icon.transform.localPosition = location;
                VentIcons[vent.Id] = Icon;
            }

            if (task?.IsComplete == false && task.FindConsoles()[0].ConsoleId == vent.Id)
            {
                Icon.transform.localScale *= 0.6f;
            }
            else
            {
                Icon.transform.localScale = Vector3.one;
            }

            HandleMiraOrSub();

            var network = GetNetworkFor(vent);
            if (network == null)
            {
                VentNetworks.Add(new List<Vent>(vent.NearbyVents.Where(x => x != null)) { vent });
            }
            else
            {
                if (!network.Any(x => x == vent))
                {
                    network.Add(vent);
                }
            }
        }

        if (AllVentsRegistered())
        {
            var array = VentNetworks.ToArray();
            foreach (var connectedgroup in VentNetworks)
            {
                var index = Array.IndexOf(array, connectedgroup);
                connectedgroup.Do(x =>
                    VentIcons[x.Id].GetComponent<SpriteRenderer>().color = Palette.PlayerColors[index]);
            }
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
    [HarmonyPostfix]
    public static void Postfix()
    {
        BodyIcons.Clear();
        VentIcons.Clear();
        VentNetworks.Clear();
    }

    public static List<Vent>? GetNetworkFor(Vent vent)
    {
        return VentNetworks.FirstOrDefault(x =>
            x.Any(y => y == vent || y == vent.Left || y == vent.Center || y == vent.Right));
    }

    public static bool AllVentsRegistered()
    {
        foreach (var vent in ShipStatus.Instance.AllVents)
        {
            if (!vent.isActiveAndEnabled)
            {
                continue;
            }

            if (vent.name.StartsWith("MinerVent-", StringComparison.Ordinal))
            {
                continue;
            }

            var network = GetNetworkFor(vent);
            if (network == null || !network.Any(x => x == vent))
            {
                return false;
            }
        }

        return true;
    }

    public static void HandleMiraOrSub()
    {
        if (VentNetworks.Count != 0)
        {
            return;
        }

        if (MiscUtils.IsMap(1))
        {
            var vents = ShipStatus.Instance.AllVents.Where(x => !x.name.Contains("MinerVent"));
            VentNetworks.Add(vents.ToList());
            return;
        }

        if (ShipStatus.Instance.Type == ModCompatibility.SubmergedMapType)
        {
            var vents = ShipStatus.Instance.AllVents.Where(x => x.Id is 12 or 13 or 15 or 16);
            VentNetworks.Add(vents.ToList());
        }
    }
}