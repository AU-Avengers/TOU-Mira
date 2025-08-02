using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.CustomPolus;

[HarmonyPatch(typeof(ShipStatus))]
public static class BetterPolusPatches
{
    public const float DvdScreenNewScale = 0.75f;
    public static readonly Vector3 DvdScreenNewPos = new(26.635f, -15.92f, 1f);
    public static readonly Vector3 VitalsNewPos = new(31.275f, -6.45f, 1f);

    public static readonly Vector3 WifiNewPos = new(15.975f, 0.084f, 1f);
    public static readonly Vector3 NavNewPos = new(11.07f, -15.298f, -0.015f);

    public static readonly Vector3 TempColdNewPos = new(25.4f, -6.4f, 1f);
    public static readonly Vector3 TempColdNewPosDV = new(7.772f, -17.103f, -0.017f);

    public static bool IsAdjustmentsDone;
    public static bool IsObjectsFetched;
    public static bool IsRoomsFetched;
    public static bool IsVentsFetched;

    public static Console WifiConsole;
    public static Console NavConsole;
    public static Console TempCold;

    public static SystemConsole Vitals;
    public static GameObject DvdScreenOffice;

    public static Vent ElectricBuildingVent;
    public static Vent ElectricalVent;
    public static Vent ScienceBuildingVent;
    public static Vent StorageVent;
    public static Vent LightCageVent;

    public static GameObject Comms;
    public static GameObject DropShip;
    public static GameObject Outside;
    public static GameObject Science;

    private static void ApplyChanges(ShipStatus instance)
    {
        if (instance.Type == ShipStatus.MapType.Pb)
        {
            FindPolusObjects();
            AdjustPolus();
        }
    }

    public static void FindPolusObjects()
    {
        FindVents();
        FindRooms();
        FindObjects();
    }

    public static void AdjustPolus()
    {
        var options = OptionGroupSingleton<BetterMapOptions>.Instance;
        if (IsObjectsFetched && IsRoomsFetched)
        {
            if (options.BPVitalsInLab)
            {
                MoveVitals();
            }

            if (!options.BPTempInDeathValley && options.BPVitalsInLab)
            {
                MoveTempCold();
            }

            if (options.BPTempInDeathValley)
            {
                MoveTempColdDV();
            }

            if (options.BPSwapWifiAndChart)
            {
                SwitchNavWifi();
            }
        }

        if (options.BPVentNetwork)
        {
            AdjustVents();
        }

        IsAdjustmentsDone = true;
    }

    public static void FindVents()
    {
        var ventsList = Object.FindObjectsOfType<Vent>().ToList();

        if (ElectricBuildingVent == null)
        {
            ElectricBuildingVent = ventsList.Find(vent => vent.gameObject.name == "ElectricBuildingVent")!;
        }

        if (ElectricalVent == null)
        {
            ElectricalVent = ventsList.Find(vent => vent.gameObject.name == "ElectricalVent")!;
        }

        if (ScienceBuildingVent == null)
        {
            ScienceBuildingVent = ventsList.Find(vent => vent.gameObject.name == "ScienceBuildingVent")!;
        }

        if (StorageVent == null)
        {
            StorageVent = ventsList.Find(vent => vent.gameObject.name == "StorageVent")!;
        }

        if (LightCageVent == null)
        {
            LightCageVent = ventsList.Find(vent => vent.gameObject.name == "ElecFenceVent")!;
        }

        IsVentsFetched = ElectricBuildingVent != null && ElectricalVent != null && ScienceBuildingVent != null &&
                         StorageVent != null && LightCageVent != null;
    }

    public static void FindRooms()
    {
        if (Comms == null)
        {
            Comms = Object.FindObjectsOfType<GameObject>().ToList().Find(o => o.name == "Comms")!;
        }

        if (DropShip == null)
        {
            DropShip = Object.FindObjectsOfType<GameObject>().ToList().FindLast(o => o.name == "Dropship")!;
        }

        if (Outside == null)
        {
            Outside = Object.FindObjectsOfType<GameObject>().ToList().Find(o => o.name == "Outside")!;
        }

        if (Science == null)
        {
            Science = Object.FindObjectsOfType<GameObject>().ToList().Find(o => o.name == "Science")!;
        }

        IsRoomsFetched = Comms != null && DropShip != null && Outside != null && Science != null;
    }

    public static void FindObjects()
    {
        if (WifiConsole == null)
        {
            WifiConsole = Object.FindObjectsOfType<Console>().ToList()
                .Find(console => console.name == "panel_wifi")!;
        }

        if (NavConsole == null)
        {
            NavConsole = Object.FindObjectsOfType<Console>().ToList()
                .Find(console => console.name == "panel_nav")!;
        }

        if (Vitals == null)
        {
            Vitals = Object.FindObjectsOfType<SystemConsole>().ToList()
                .Find(console => console.name == "panel_vitals")!;
        }

        if (DvdScreenOffice == null)
        {
            var DvdScreenAdmin = Object.FindObjectsOfType<GameObject>().ToList()
                .Find(o => o.name == "dvdscreen")!;

            if (DvdScreenAdmin != null)
            {
                DvdScreenOffice = Object.Instantiate(DvdScreenAdmin);
            }
        }

        if (TempCold == null)
        {
            TempCold = Object.FindObjectsOfType<Console>().ToList()
                .Find(console => console.name == "panel_tempcold")!;
        }

        IsObjectsFetched = WifiConsole != null && NavConsole != null && Vitals != null &&
                           DvdScreenOffice != null && TempCold != null;
    }

    public static void AdjustVents()
    {
        if (IsVentsFetched)
        {
            ElectricBuildingVent.Left = ElectricalVent;
            ElectricalVent.Center = ElectricBuildingVent;
            ElectricBuildingVent.Center = LightCageVent;
            LightCageVent.Center = ElectricBuildingVent;

            ScienceBuildingVent.Left = StorageVent;
            StorageVent.Center = ScienceBuildingVent;
        }
    }

    public static void MoveTempCold()
    {
        if (TempCold.transform.position != TempColdNewPos)
        {
            var tempColdTransform = TempCold.transform;
            tempColdTransform.parent = Outside.transform;
            tempColdTransform.position = TempColdNewPos;

            var collider = TempCold.GetComponent<BoxCollider2D>();
            collider.isTrigger = false;
            collider.size += new Vector2(0f, -0.3f);
        }
    }

    public static void MoveTempColdDV()
    {
        if (TempCold.transform.position != TempColdNewPosDV)
        {
            var tempColdTransform = TempCold.transform;
            tempColdTransform.parent = Outside.transform;
            tempColdTransform.position = TempColdNewPosDV;

            var collider = TempCold.GetComponent<BoxCollider2D>();
            collider.isTrigger = false;
            collider.size += new Vector2(0f, -0.3f);
        }
    }

    public static void SwitchNavWifi()
    {
        if (WifiConsole.transform.position != WifiNewPos)
        {
            var wifiTransform = WifiConsole.transform;
            wifiTransform.parent = DropShip.transform;
            wifiTransform.position = WifiNewPos;
        }

        if (NavConsole.transform.position != NavNewPos)
        {
            var navTransform = NavConsole.transform;
            navTransform.parent = Comms.transform;
            navTransform.position = NavNewPos;

            NavConsole.checkWalls = true;
        }
    }

    public static void MoveVitals()
    {
        if (Vitals.transform.position != VitalsNewPos)
        {
            var vitalsTransform = Vitals.gameObject.transform;
            vitalsTransform.parent = Science.transform;
            vitalsTransform.position = VitalsNewPos;
        }

        if (DvdScreenOffice.transform.position != DvdScreenNewPos)
        {
            var dvdScreenTransform = DvdScreenOffice.transform;
            dvdScreenTransform.position = DvdScreenNewPos;

            var localScale = dvdScreenTransform.localScale;
            localScale =
                new Vector3(DvdScreenNewScale, localScale.y,
                    localScale.z);
            dvdScreenTransform.localScale = localScale;
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
    public static class ShipStatusBeginPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch]
        public static void Prefix(ShipStatus __instance)
        {
            ApplyChanges(__instance);
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatusAwakePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch]
        public static void Prefix(ShipStatus __instance)
        {
            ApplyChanges(__instance);
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
    public static class ShipStatusFixedUpdatePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch]
        public static void Prefix(ShipStatus __instance)
        {
            if (!IsObjectsFetched || !IsAdjustmentsDone)
            {
                ApplyChanges(__instance);
            }
        }
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class TaskTextUpdates
{
    public static void Prefix(HudManager __instance)
    {
        if (!MiscUtils.IsMap(2))
        {
            return;
        }

        if (BetterPolusPatches.IsObjectsFetched && BetterPolusPatches.IsAdjustmentsDone)
        {
            var opts = OptionGroupSingleton<BetterMapOptions>.Instance;

            if (!PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.myTasks == null ||
                PlayerControl.LocalPlayer.myTasks.Count == 0)
            {
                return;
            }

            foreach (var task in PlayerControl.LocalPlayer.myTasks)
            {
                if (task.TaskType == TaskTypes.RecordTemperature && task.StartAt == SystemTypes.Laboratory &&
                    opts.BPTempInDeathValley)
                {
                    task.StartAt = TaskProvider.DeathValleySystemType;
                    BetterPolusPatches.TempCold.Room = TaskProvider.DeathValleySystemType;
                }

                if (opts.BPSwapWifiAndChart)
                {
                    if (task.TaskType == TaskTypes.RebootWifi && task.StartAt != SystemTypes.Dropship)
                    {
                        task.StartAt = SystemTypes.Dropship;
                    }
                    else if (task.TaskType == TaskTypes.ChartCourse && task.StartAt != SystemTypes.Comms)
                    {
                        task.StartAt = SystemTypes.Comms;
                    }
                }
            }
        }
    }
}