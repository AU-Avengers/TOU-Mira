using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options.Maps;
using Object = UnityEngine.Object;

// Vent layout taken from Town of Us: Reworked by Az
namespace TownOfUs.Patches.BetterMaps;

[HarmonyPatch(typeof(ShipStatus))]
public static class BetterMiraHq
{
    public static bool IsAdjustmentsDone;
    public static bool IsObjectsFetched;
    public static bool IsRoomsFetched;
    public static bool IsVentsFetched;

    public static Vent SpawnVent;
    public static Vent ReactorVent;
    public static Vent DeconVent;
    public static Vent LockerVent;
    public static Vent LabVent;
    public static Vent LightsVent;
    public static Vent AdminVent;
    public static Vent YRightVent;
    public static Vent O2Vent;
    public static Vent BalcVent;
    public static Vent MedicVent;

    private static void ApplyChanges(ShipStatus instance)
    {
        if (instance.Type == ShipStatus.MapType.Hq)
        {
            FindMiraHqObjects();
            AdjustMiraHq();
        }
    }

    public static void FindMiraHqObjects()
    {
        FindVents();
        // FindRooms();
    }

    public static void AdjustMiraHq()
    {
        var options = OptionGroupSingleton<BetterMiraHqOptions>.Instance;
        if (options.BetterVentNetwork)
        {
            AdjustVents();
        }

        IsAdjustmentsDone = true;
    }

    public static void FindVents()
    {
        var ventsList = Object.FindObjectsOfType<Vent>().ToList();

        if (SpawnVent == null)
        {
            SpawnVent = ventsList.Find(vent => vent.gameObject.name == "LaunchVent")!;
        }

        if (BalcVent == null)
        {
            BalcVent = ventsList.Find(vent => vent.gameObject.name == "BalconyVent")!;
        }

        if (ReactorVent == null)
        {
            ReactorVent = ventsList.Find(vent => vent.gameObject.name == "ReactorVent")!;
        }

        if (LabVent == null)
        {
            LabVent = ventsList.Find(vent => vent.gameObject.name == "LabVent")!;
        }

        if (LockerVent == null)
        {
            LockerVent = ventsList.Find(vent => vent.gameObject.name == "LockerVent")!;
        }

        if (AdminVent == null)
        {
            AdminVent = ventsList.Find(vent => vent.gameObject.name == "AdminVent")!;
        }

        if (LightsVent == null)
        {
            LightsVent = ventsList.Find(vent => vent.gameObject.name == "OfficeVent")!;
        }

        if (O2Vent == null)
        {
            O2Vent = ventsList.Find(vent => vent.gameObject.name == "AgriVent")!;
        }

        if (DeconVent == null)
        {
            DeconVent = ventsList.Find(vent => vent.gameObject.name == "DeconVent")!;
        }

        if (MedicVent == null)
        {
            MedicVent = ventsList.Find(vent => vent.gameObject.name == "MedVent")!;
        }

        if (YRightVent == null)
        {
            YRightVent = ventsList.Find(vent => vent.gameObject.name == "YHallRightVent")!;
        }

        IsVentsFetched = SpawnVent != null && BalcVent != null && ReactorVent != null && LabVent != null &&
                         LockerVent != null && AdminVent != null && O2Vent != null && LightsVent != null &&
                         DeconVent != null && MedicVent != null && YRightVent != null;
    }

    /*
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
    }*/

    public static void AdjustVents()
    {
        if (IsVentsFetched)
        {
            O2Vent.Right = BalcVent;
            O2Vent.Left = MedicVent;
            O2Vent.Center = null;
            MedicVent.Center = O2Vent;
            MedicVent.Right = BalcVent;
            MedicVent.Left = null;
            BalcVent.Left = MedicVent;
            BalcVent.Center = O2Vent;
            BalcVent.Right = null;

            AdminVent.Center = YRightVent;
            AdminVent.Left = null;
            AdminVent.Right = null;
            YRightVent.Center = AdminVent;
            YRightVent.Left = null;
            YRightVent.Right = null;

            LabVent.Right = LightsVent;
            LabVent.Left = null;
            LabVent.Center = null;
            LightsVent.Left = LabVent;
            LightsVent.Right = null;
            LightsVent.Center = null;

            SpawnVent.Center = ReactorVent;
            SpawnVent.Right = null;
            SpawnVent.Left = null;
            ReactorVent.Left = SpawnVent;
            ReactorVent.Right = null;
            ReactorVent.Center = null;

            LockerVent.Right = null;
            LockerVent.Center = DeconVent;
            LockerVent.Left = null;
            DeconVent.Left = LockerVent;
            DeconVent.Right = null;
            DeconVent.Center = null;
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
