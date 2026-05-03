using System.Collections;
using System.Runtime.InteropServices;
using AmongUs.GameOptions;
using BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using Reactor.Localization.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class LogoPatch
{
    public const string BepInVersionPrefix = "6.0.0-be.";
    public const int BepInVersionMinimum = 738;
#pragma warning disable S1075 // URIs should not be hardcoded
    public const string BepInExDownloadUrl32 = "https://builds.bepinex.dev/projects/bepinex_be/752/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.752%2Bdd0655f.zip";
    public const string BepInExDownloadUrl64 = "https://builds.bepinex.dev/projects/bepinex_be/752/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.752%2Bdd0655f.zip";
#pragma warning restore S1075
    public static string BepInExDownloadUrl => Environment.Is64BitProcess ? BepInExDownloadUrl64 : BepInExDownloadUrl32;
    //public static bool UpdateRequired => !TownOfUsPlugin.IsMobile && Paths.BepInExVersion.ToString().Remove(BepInVersionPrefix.Length);
    public static void Postfix()
    {
        RoleManager.Instance.GetRole(RoleTypes.CrewmateGhost).StringName =
            CustomStringName.CreateAndRegister("Crewmate Ghost");
        RoleManager.Instance.GetRole(RoleTypes.ImpostorGhost).StringName =
            CustomStringName.CreateAndRegister("Impostor Ghost");

        var roles = MiscUtils.AllRoles.Where(x =>
                x is not IWikiDiscoverable or ICustomRole { Configuration.HideSettings: false })
            .ToArray();

        if (roles.Length != 0)
        {
            foreach (var role in roles)
            {
                SoftWikiEntries.RegisterRoleEntry(role);
            }
        }

        Dictionary<RoleBehaviour, RoleTypes> vanillaRoles = new Dictionary<RoleBehaviour, RoleTypes>()
        {
            { RoleManager.Instance.GetRole(RoleTypes.Scientist), RoleTypes.Scientist },
            { RoleManager.Instance.GetRole(RoleTypes.Noisemaker), RoleTypes.Noisemaker },
            { RoleManager.Instance.GetRole(RoleTypes.Tracker), RoleTypes.Tracker },
            { RoleManager.Instance.GetRole(RoleTypes.GuardianAngel), RoleTypes.GuardianAngel },
            { RoleManager.Instance.GetRole(RoleTypes.Detective), RoleTypes.Detective },
            { RoleManager.Instance.GetRole(RoleTypes.Shapeshifter), RoleTypes.Shapeshifter },
            { RoleManager.Instance.GetRole(RoleTypes.Phantom), RoleTypes.Phantom },
            { RoleManager.Instance.GetRole(RoleTypes.Viper), RoleTypes.Viper },
        };
        foreach (var rolePair in vanillaRoles)
        {
            SoftWikiEntries.RegisterVanillaRoleEntry(rolePair.Key, rolePair.Value);
        }

        var newLogo = GameObject.Find("LOGO-AU");
        var sizer = GameObject.Find("Sizer");
        if (newLogo != null)
        {
            newLogo.GetComponent<SpriteRenderer>().sprite = TouAssets.Banner.LoadAsset();
        }

        if (sizer != null)
        {
            sizer.GetComponent<AspectSize>().PercentWidth = 0.3f;
        }

        var menuBg = GameObject.Find("BackgroundTexture");

        if (menuBg != null)
        {
            var render = menuBg.GetComponent<SpriteRenderer>();
            render.flipY = true;
            render.color = new Color(1f, 1f, 1f, 0.65f);
        }

        var tint = GameObject.Find("MainUI").transform.GetChild(0).gameObject;
        if (tint != null)
        {
            tint.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
            tint.transform.localScale = new Vector3(7.5f, 7.5f, 1f);
        }
        var bgFill = GameObject.Find("AccountManager/BackgroundFill");
        var loading = bgFill.transform.GetChild(0).gameObject;
        var loadRend = loading.GetComponent<SpriteRenderer>();
        loadRend.sprite = TouAssets.MayorPet.LoadAsset();
        loadRend.flipX = false;
        loadRend.SetMaterial(newLogo!.GetComponent<SpriteRenderer>().GetMaterial());
        loadRend.color = Color.white;
        var loading2 = GameObject.Find("AccountManager/Loading").transform.GetChild(1).GetChild(0);
        var logo2 = loading2.GetComponent<SpriteRenderer>();
        logo2.sprite = TouAssets.BannerDark.LoadAsset();
        logo2.transform.localScale = new Vector3(0.15f, 0.15f, 1);
        logo2.transform.localPosition = new Vector3(1.21f, 0.7556f, 1);

        if (TownOfUsPlugin.IsMobile)
        {
            return;
        }

        try
        {
            var charCount = BepInVersionPrefix.Length;
            var basicBep = Paths.BepInExVersion.ToString()[charCount..];
            var newBep = basicBep.Split('+')[0];
            var parsedVersion = int.Parse(newBep, TownOfUsPlugin.Culture);
            Error($"Running BepInEx {Paths.BepInExVersion.ToString()}, version is {newBep}");
            if (parsedVersion < BepInVersionMinimum)
            {
                Error($"BepInEx version is too low, minimum required is {BepInVersionMinimum}!");
                Coroutines.Start(CoOpenWarning());
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e);
        }
    }

    [HideFromIl2Cpp]
    public static IEnumerator CoOpenWarning()
    {
        var task = Task.Run(() => MessageBox(GetForegroundWindow(),
            $"Your BepInEx version is out of date! Please update to version {BepInVersionPrefix}{BepInVersionMinimum} or higher. Would you like to download an up-to-date BepInEx?",
            "Town of Us: Mira (ERR-001)", 4));
        while (!task.IsCompleted)
            yield return null;
        Error(task.Result);
        if (task.Result == 6)
        {
            Application.OpenURL(BepInExDownloadUrl);
        }
        Application.Quit();
    }

#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    private static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);
#pragma warning restore CA2101
}