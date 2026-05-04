using System.Globalization;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MiraAPI;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using Reactor;
using Reactor.Localization;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modules.Cosmetics.Unity;
using TownOfUs.Patches;
using TownOfUs.Patches.Misc;
using TownOfUs.Patches.WinConditions;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;
using ModCompatibility = TownOfUs.Modules.ModCompatibility;

namespace TownOfUs;

/// <summary>
///     Plugin class for Town of Us.
/// </summary>
[BepInAutoPlugin("auavengers.tou.mira", "Town of Us Mira")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class TownOfUsPlugin : BasePlugin, IMiraPlugin
{
    public static bool IsMobile => Constants.GetPlatformType() is Platforms.Android or Platforms.IPhone;
    /// <summary>
    ///     Gets the specified Culture for string manipulations.
    /// </summary>
    public static CultureInfo Culture { get; internal set; } = new("en-US");

    /// <summary>
    ///     Gets the Harmony instance for patching.
    /// </summary>
    public Harmony Harmony { get; } = new(Id);

    /// <summary>
    ///     Determines if the current build is a dev build or not. This will change certain visuals as well as always grab news locally to be up to date.
    /// </summary>
    public static bool IsDevBuild => IsBetaBuild || IsWipBuild;

    /// <summary>
    ///     Determines if the current build is a beta build. Beta builds are dev builds but should have restricted features like /up command.
    /// </summary>
    public static bool IsBetaBuild => Version.Contains("beta", StringComparison.OrdinalIgnoreCase) || Version.Contains("prerelease", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     Determines if the current build is a beta build. Beta builds are dev builds but should have restricted features like /up command.
    /// </summary>
    public static bool IsWipBuild => Version.Contains("dev", StringComparison.OrdinalIgnoreCase) || Version.Contains("ci", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string OptionsTitleText => "TOU Mira";

    /// <inheritdoc />
    public string CustomOptionMenuNameOne => TouLocale.Get("TouTabOptionBetterMaps");
    public string CustomOptionMenuOneDescription => TouLocale.Get("TouTabOptionBetterMapsDesc");
    public string ModifierMenuDescription => TouLocale.Get("TouTabOptionModifiersDesc");

    /// <inheritdoc />
    public ConfigFile GetConfigFile()
    {
        return Config;
    }

    public TownOfUsPlugin()
    {
        TouLocale.Initialize();
    }

    /// <summary>
    ///     The Load method for the plugin.
    /// </summary>
    public override void Load()
    {
        ReactorCredits.Register("Town Of Us: Mira", Version, IsDevBuild, ReactorCredits.AlwaysShow);
        LocalizationManager.Register(new TaskProvider());

        TouAssets.Initialize();

        IL2CPPChainloader.Instance.Finished +=
            ModCompatibility
                .Initialize; // Initialise AFTER the mods are loaded to ensure maximum parity (no need for the soft dependency either then)

        IL2CPPChainloader.Instance.Finished +=
            ModNewsFetcher
                .CheckForNews; // Checks for mod announcements after everything is loaded to avoid Epic Games crashing

        if (!IsMobile)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "touhats.catalog");
            if (!File.Exists(path))
            {
                Error("touhats.catalog not found!");
            }
            else
            {
                AddressablesLoader.RegisterCatalog(path);
                AddressablesLoader.RegisterHats("touhats");
                Error("touhats.catalog was loaded!");
            }
        }

        ClassInjector.RegisterTypeInIl2Cpp<HatLocator>(new RegisterTypeOptions
        {
            Interfaces = new Il2CppInterfaceCollection([typeof(IResourceLocator)])
        });

        ClassInjector.RegisterTypeInIl2Cpp<HatProvider>(new RegisterTypeOptions
        {
            Interfaces = new Il2CppInterfaceCollection([typeof(IResourceProvider)])
        });

        Info("Initializing HatProvider...");
        HatProvider.Initialize();
        Info("HatProvider initialized!");
        
        Info("Initializing HatLocator...");
        HatLocator.Initialize();
        Info("HatLocator initialized!");

        Harmony.PatchAll();
        RegisterWinConditions();
    }

    /// <summary>
    ///     Registers all built-in win conditions.
    ///     Extension mods can register their own win conditions by calling WinConditionRegistry.Register().
    /// </summary>
    public static void RegisterWinConditions()
    {
        WinConditionRegistry.Register(new NeutralRoleWinCondition());
        WinConditionRegistry.Register(new LoversWinCondition());
    }
}