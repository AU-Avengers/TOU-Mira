using System.Globalization;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using Reactor;
using Reactor.Localization;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Patches.Misc;
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
    /// <summary>
    ///     Gets the specified Culture for string manipulations.
    /// </summary>
    public static CultureInfo Culture { get; } = new("en-US");

    /// <summary>
    ///     Gets the Harmony instance for patching.
    /// </summary>
    public Harmony Harmony { get; } = new(Id);

    public static ConfigEntry<int> GameSummaryMode { get; set; }

    /// <summary>
    ///     Determines if the current build is a dev build or not. This will change certain visuals as well as always grab news locally to be up to date.
    /// </summary>
    public static bool IsDevBuild => false;
    
    /// <inheritdoc />
    public string OptionsTitleText => "TOU Mira";

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
            ModNewsFetcher.CheckForNews; // Checks for mod announcements after everything is loaded to avoid Epic Games crashing

        var path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(TownOfUsPlugin))!.Location) + "\\touhats.catalog";
        AddressablesLoader.RegisterCatalog(path);
        AddressablesLoader.RegisterHats("touhats");

        GameSummaryMode = Config.Bind("LocalSettings", "GameSummaryMode", 1,
            "How the Game Summary appears in the Win Screen. 0 is to the left, 1 is split, and 2 is hidden.");
        Harmony.PatchAll();
    }
}
