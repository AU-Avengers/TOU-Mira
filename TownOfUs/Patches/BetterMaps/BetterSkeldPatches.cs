using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options.Maps;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.BetterMaps;

[HarmonyPatch(typeof(ShipStatus))]
public static class BetterSkeldPatches
{
    public static bool IsAdjustmentsDone;
    public static bool IsObjectsFetched;
    public static bool IsVentsFetched;
    public static bool ThemesFetched;
    public static GameObject HalloweenTheme;
    public static GameObject BirthdayTheme;

    public static Vent UpperEngineVent;
    public static Vent TopReactorVent;
    public static Vent BottomReactorVent;
    public static Vent LowerEngineVent;

    public static Vent WeaponsVent;
    public static Vent TopNavVent;
    public static Vent BottomNavVent;
    public static Vent ShieldsVent;

    private static void ApplyChanges(ShipStatus instance)
    {
        if (instance.Type == ShipStatus.MapType.Ship)
        {
            FindSkeldObjects();
            AdjustSkeld();
        }
    }

    public static void FindSkeldObjects()
    {
        FindVents();
        FindThemes();
    }

    public static void AdjustSkeld()
    {
        var options = OptionGroupSingleton<BetterSkeldOptions>.Instance;
        var ventMode = (SkeldVentMode)options.BetterVentNetwork.Value;
        var themeMode = (SkeldTheme)options.MapTheme.Value;
        if (ventMode is not SkeldVentMode.Normal)
        {
            AdjustVents(ventMode);
        }

        if (themeMode is not SkeldTheme.Auto)
        {
            AdjustTheme(themeMode);
        }

        IsAdjustmentsDone = true;
    }

    public static void FindThemes()
    {
        var rootObj = GameObject.Find("SkeldShip(Clone)");
        if (rootObj == null)
        {
            rootObj = GameObject.Find("AprilShip(Clone)");
        }
        if (rootObj == null)
        {
            ThemesFetched = false;
            return;
        }
        if (HalloweenTheme == null)
        {
            HalloweenTheme = rootObj.transform.FindChild("HalloweenDecorSkeld").gameObject;
        }
        if (BirthdayTheme == null)
        {
            BirthdayTheme = rootObj.transform.FindChild("BirthdayDecorSkeld").gameObject;
        }
        if (HalloweenTheme == null)
        {
            // Yes, this IS Helloween, not Halloween lol
            HalloweenTheme = rootObj.transform.FindChild("Helloween").gameObject;
        }
        ThemesFetched = HalloweenTheme != null;
    }

    public static void FindVents()
    {
        var ventsList = Object.FindObjectsOfType<Vent>().ToList();
        var suffix = MiscUtils.GetCurrentMap is ExpandedMapNames.Dleks ? " (1)" : "";

        if (UpperEngineVent == null)
        {
            UpperEngineVent = ventsList.Find(vent => vent.gameObject.name == $"LEngineVent{suffix}")!;
        }

        if (LowerEngineVent == null)
        {
            LowerEngineVent = ventsList.Find(vent => vent.gameObject.name == $"REngineVent{suffix}")!;
        }

        if (TopReactorVent == null)
        {
            TopReactorVent = ventsList.Find(vent => vent.gameObject.name == $"UpperReactorVent{suffix}")!;
        }

        if (BottomReactorVent == null)
        {
            BottomReactorVent = ventsList.Find(vent => vent.gameObject.name == $"ReactorVent{suffix}")!;
        }

        if (WeaponsVent == null)
        {
            WeaponsVent = ventsList.Find(vent => vent.gameObject.name == $"WeaponsVent{suffix}")!;
        }

        if (TopNavVent == null)
        {
            TopNavVent = ventsList.Find(vent => vent.gameObject.name == $"NavVentNorth{suffix}")!;
        }

        if (BottomNavVent == null)
        {
            BottomNavVent = ventsList.Find(vent => vent.gameObject.name == $"NavVentSouth{suffix}")!;
        }

        if (ShieldsVent == null)
        {
            ShieldsVent = ventsList.Find(vent => vent.gameObject.name == $"ShieldsVent{suffix}")!;
        }

        IsVentsFetched = UpperEngineVent != null && TopReactorVent != null && BottomReactorVent != null && LowerEngineVent != null &&
                         WeaponsVent != null && TopNavVent != null && BottomNavVent != null && ShieldsVent != null;
    }

    public static void AdjustTheme(SkeldTheme theme)
    {
        if (ThemesFetched)
        {
            var birthdayAvailable = BirthdayTheme != null;
            switch (theme)
            {
                case SkeldTheme.Basic:
                    HalloweenTheme.SetActive(false);
                    if (birthdayAvailable)
                    {
                        BirthdayTheme!.SetActive(false);
                    }
                    break;
                case SkeldTheme.Birthday:
                    HalloweenTheme.SetActive(false);
                    if (birthdayAvailable)
                    {
                        BirthdayTheme!.SetActive(true);
                    }
                    break;
                case SkeldTheme.Halloween:
                    HalloweenTheme.SetActive(true);
                    if (birthdayAvailable)
                    {
                        BirthdayTheme!.SetActive(false);
                    }
                    break;
            }
        }
    }

    public static void AdjustVents(SkeldVentMode ventMode = SkeldVentMode.Normal)
    {
        if (IsVentsFetched)
        {
            switch (ventMode)
            {
                case SkeldVentMode.FourGroups:
                    UpperEngineVent.Right = null;
                    UpperEngineVent.Center = LowerEngineVent;
                    UpperEngineVent.Left = TopReactorVent;
                    TopReactorVent.Right = UpperEngineVent;
                    TopReactorVent.Center = BottomReactorVent;
                    TopReactorVent.Left = null;
                    BottomReactorVent.Left = null;
                    BottomReactorVent.Center = TopReactorVent;
                    BottomReactorVent.Right = LowerEngineVent;
                    LowerEngineVent.Left = BottomReactorVent;
                    LowerEngineVent.Center = UpperEngineVent;
                    LowerEngineVent.Right = null;

                    WeaponsVent.Left = null;
                    WeaponsVent.Center = ShieldsVent;
                    WeaponsVent.Right = TopNavVent;
                    TopNavVent.Left = ShieldsVent;
                    TopNavVent.Center = BottomNavVent;
                    TopNavVent.Right = null;
                    BottomNavVent.Right = null;
                    BottomNavVent.Center = TopNavVent;
                    BottomNavVent.Left = ShieldsVent;
                    ShieldsVent.Right = BottomNavVent;
                    ShieldsVent.Center = WeaponsVent;
                    ShieldsVent.Left = null;
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
