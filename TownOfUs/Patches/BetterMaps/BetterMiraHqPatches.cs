using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options.Maps;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.BetterMaps;

[HarmonyPatch(typeof(ShipStatus))]
public static class BetterMiraHqPatches
{
    public static bool IsAdjustmentsDone;
    public static bool IsObjectsFetched;
    public static bool IsVentsFetched;
    public static bool ThemesFetched;
    public static GameObject HalloweenTheme;

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
        FindThemes();
    }

    public static void AdjustMiraHq()
    {
        var options = OptionGroupSingleton<BetterMiraHqOptions>.Instance;
        var ventMode = (MiraVentMode)options.BetterVentNetwork.Value;
        var themeMode = (PolusTheme)options.MapTheme.Value;
        if (ventMode is not MiraVentMode.Normal)
        {
            AdjustVents(ventMode);
        }

        if (themeMode is not PolusTheme.Auto)
        {
            AdjustTheme(themeMode);
        }

        IsAdjustmentsDone = true;
    }

    public static void FindThemes()
    {
        var rootObj = GameObject.Find("MiraShip(Clone)");
        if (rootObj == null)
        {
            ThemesFetched = false;
            return;
        }
        if (HalloweenTheme == null)
        {
            HalloweenTheme = rootObj.transform.FindChild("HalloweenDecorMira").gameObject;
        }
        ThemesFetched = HalloweenTheme != null;
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

    public static void AdjustTheme(PolusTheme theme)
    {
        if (ThemesFetched)
        {
            switch (theme)
            {
                case PolusTheme.Basic:
                    HalloweenTheme.SetActive(false);
                    break;
                case PolusTheme.Halloween:
                    HalloweenTheme.SetActive(true);
                    break;
            }
        }
    }

    public static void AdjustVents(MiraVentMode ventMode = MiraVentMode.Normal)
    {
        if (IsVentsFetched)
        {
            switch (ventMode)
            {
                case MiraVentMode.ThreeGroups:
                    LabVent.Left = null;
                    LabVent.Center = null;
                    LabVent.Right = LightsVent;
                    LightsVent.Left = LabVent;
                    LightsVent.Center = null;
                    LightsVent.Right = O2Vent;
                    O2Vent.Left = LightsVent;
                    O2Vent.Center = null;
                    O2Vent.Right = null;

                    MedicVent.Left = AdminVent;
                    MedicVent.Center = null;
                    MedicVent.Right = BalcVent;
                    BalcVent.Left = MedicVent;
                    BalcVent.Center = null;
                    BalcVent.Right = YRightVent;
                    YRightVent.Left = BalcVent;
                    YRightVent.Center = null;
                    YRightVent.Right = AdminVent;
                    AdminVent.Left = YRightVent;
                    AdminVent.Center = null;
                    AdminVent.Right = MedicVent;

                    SpawnVent.Left = ReactorVent;
                    SpawnVent.Center = null;
                    SpawnVent.Right = LockerVent;
                    LockerVent.Left = SpawnVent;
                    LockerVent.Center = null;
                    LockerVent.Right = DeconVent;
                    DeconVent.Left = LockerVent;
                    DeconVent.Center = null;
                    DeconVent.Right = ReactorVent;
                    ReactorVent.Left = DeconVent;
                    ReactorVent.Center = null;
                    ReactorVent.Right = SpawnVent;
                    break;
                case MiraVentMode.FourGroups:
                    O2Vent.Left = null;
                    O2Vent.Center = null;
                    O2Vent.Right = AdminVent;
                    AdminVent.Left = O2Vent;
                    AdminVent.Center = null;
                    AdminVent.Right = YRightVent;
                    YRightVent.Left = AdminVent;
                    YRightVent.Center = null;
                    YRightVent.Right = null;

                    DeconVent.Left = null;
                    DeconVent.Center = null;
                    DeconVent.Right = LabVent;
                    LabVent.Left = DeconVent;
                    LabVent.Center = null;
                    LabVent.Right = LightsVent;
                    LightsVent.Left = LabVent;
                    LightsVent.Center = null;
                    LightsVent.Right = null;

                    SpawnVent.Left = null;
                    SpawnVent.Center = ReactorVent;
                    SpawnVent.Right = null;
                    ReactorVent.Left = SpawnVent;
                    ReactorVent.Center = null;
                    ReactorVent.Right = null;

                    LockerVent.Left = null;
                    LockerVent.Center = null;
                    LockerVent.Right = MedicVent;
                    MedicVent.Left = LockerVent;
                    MedicVent.Center = null;
                    MedicVent.Right = BalcVent;
                    BalcVent.Left = MedicVent;
                    BalcVent.Center = null;
                    BalcVent.Right = null;
                    break;
            }
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
