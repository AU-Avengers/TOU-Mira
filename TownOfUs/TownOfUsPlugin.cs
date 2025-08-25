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

    public static ConfigEntry<bool> DeadSeeGhosts { get; set; }
    public static ConfigEntry<bool> ShowShieldHud { get; set; }
    public static ConfigEntry<bool> ShowSummaryMessage { get; set; }
    public static ConfigEntry<bool> ShowWelcomeMessage { get; set; }
    public static ConfigEntry<bool> ColorPlayerName { get; set; }
    public static ConfigEntry<bool> UseCrewmateTeamColor { get; set; }
    public static ConfigEntry<bool> ShowVents { get; set; }
    public static ConfigEntry<int> GameSummaryMode { get; set; }
    public static ConfigEntry<float> ButtonUIFactor { get; set; }
    public static ConfigEntry<bool> OffsetButtons { get; set; }
    public static ConfigEntry<bool> SortGuessingByAlignment { get; set; }
    public static ConfigEntry<bool> PreciseCooldowns { get; set; }
    public static ConfigEntry<int> ArrowStyle { get; set; }

    /// <summary>
    ///     Determines if the current build is a dev build or not. This will change certain visuals as well as always grab news locally to be up to date.
    /// </summary>
    public static bool IsDevBuild => true;
    
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

        DeadSeeGhosts = Config.Bind("LocalSettings", "DeadSeeGhosts", true, "If you see other ghosts when dead");
        ShowShieldHud = Config.Bind("LocalSettings", "ShowShieldHud", true,
            "If you see shield modifiers with a description, turn this off if it gets in your way.");
        ShowSummaryMessage = Config.Bind("LocalSettings", "ShowSummaryMessage", true,
            "If you see the game summary message when you join the lobby again.");
        ShowWelcomeMessage = Config.Bind("LocalSettings", "ShowWelcomeMessage", true,
            "If you see the welcome message when you first join a game.");
        ColorPlayerName = Config.Bind("LocalSettings", "ColorPlayerName", false,
            "If your name is colored with your role color or if it's left as white.");
        UseCrewmateTeamColor = Config.Bind("LocalSettings", "UseCrewmateTeamColor", false,
            "Changes if all crewmate roles use the vanilla crewmate color instead.");
        ShowVents = Config.Bind("LocalSettings", "ShowVents", true, "If you see the vents on the minimap.");
        ButtonUIFactor = Config.Bind("LocalSettings", "ButtonUIFactor", 0.8f,
            "Scale factor for buttons in-game. Preferably, keep the value between 0.5f and 1.5f.");
        GameSummaryMode = Config.Bind("LocalSettings", "GameSummaryMode", 1,
            "How the Game Summary appears in the Win Screen. 0 is to the left, 1 is split, and 2 is hidden.");
        OffsetButtons = Config.Bind("LocalSettings", "OffsetButtons", false,
            "If venting is disabled (and you're not an impostor), should there be a blank spot where the vent button usually is?");
        SortGuessingByAlignment = Config.Bind("LocalSettings", "SortGuessingByAlignment", false,
            "Sorts the guessing menu by alignment alphabetically or purely alphabetical order.");
        PreciseCooldowns = Config.Bind("LocalSettings", "PreciseCooldowns", false,
            "Whether Button Cooldowns Show To 1 Decimal Place When It is Less Than 10 Seconds Remaining.");
        ArrowStyle = Config.Bind("LocalSettings", "ArrowStyle", 2,
            "How role arrows appear. 0 is the basic arrow, 1 is the arrow with a dark glow, 2 is the arrow with a light glow, and 3 is the legacy arrow / task arrow.");

        Harmony.PatchAll();
    }
}
