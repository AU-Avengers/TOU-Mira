using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SentryOptions : AbstractOptionGroup<SentryRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => TouLocale.Get("TouRoleSentry", "Sentry");

    [ModdedToggleOption("TouOptionSentryLegacyMode")]
    public bool LegacyMode { get; set; } = false;

    [ModdedNumberOption("TouOptionSentryPlacementCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float PlacementCooldown { get; set; } = 30f;

    public ModdedNumberOption InitialCameras { get; } = new("TouOptionSentryInitialCameras", 2f, 0f, 15f, 1f, "∞", "#", MiraNumberSuffixes.None, "0", false);

    [ModdedNumberOption("TouOptionSentryCameraRoundsLast", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float CameraRoundsLast { get; set; } = 2f;

    public ModdedNumberOption TasksPerCamera { get; } = new("TouOptionSentryTasksPerCamera", 2f, 0f, 15f, 1f, "Off", "#",
        MiraNumberSuffixes.None, "0")
    {
        Visible = () => OptionGroupSingleton<SentryOptions>.Instance.InitialCameras.Value != 0f
    };

    [ModdedNumberOption("TouOptionSentryMaxCamerasPlaced", 0f, 20f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxCamerasPlaced { get; set; } = 4f;

    [ModdedNumberOption("TouOptionSentryPortableCamsBattery", 0f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PortableCamsBattery { get; set; } = 90f;

    private static readonly string[] SystemTypeLocaleKeys =
        Enum.GetNames<SystemTypes>().Select(n => $"TouSystemType_{n}").ToArray();

    public ModdedNumberOption BlindspotsCount { get; } =
        new("TouOptionSentryBlindspots", 0f, 0f, 10f, 1f, "Off", "#", MiraNumberSuffixes.None, "0");

    public ModdedEnumOption Blindspot1Room { get; } =
        new("TouOptionSentryBlindspot1Room", (int)SystemTypes.Hallway, typeof(SystemTypes), SystemTypeLocaleKeys)
        {
            Visible = () => OptionGroupSingleton<SentryOptions>.Instance.BlindspotsCount.Value >= 1f
        };

    public ModdedEnumOption Blindspot2Room { get; } =
        new("TouOptionSentryBlindspot2Room", (int)SystemTypes.Outside, typeof(SystemTypes), SystemTypeLocaleKeys)
        {
            Visible = () => OptionGroupSingleton<SentryOptions>.Instance.BlindspotsCount.Value >= 2f
        };

    public ModdedEnumOption Blindspot3Room { get; } =
        new("TouOptionSentryBlindspot3Room", (int)SystemTypes.Cafeteria, typeof(SystemTypes), SystemTypeLocaleKeys)
        {
            Visible = () => OptionGroupSingleton<SentryOptions>.Instance.BlindspotsCount.Value >= 3f
        };

    public ModdedEnumOption Blindspot4Room { get; } =
        new("TouOptionSentryBlindspot4Room", (int)SystemTypes.Storage, typeof(SystemTypes), SystemTypeLocaleKeys)
        {
            Visible = () => OptionGroupSingleton<SentryOptions>.Instance.BlindspotsCount.Value >= 4f
        };

    public ModdedEnumOption Blindspot5Room { get; } =
        new("TouOptionSentryBlindspot5Room", (int)SystemTypes.Admin, typeof(SystemTypes), SystemTypeLocaleKeys)
        {
            Visible = () => OptionGroupSingleton<SentryOptions>.Instance.BlindspotsCount.Value >= 5f
        };

    public ModdedEnumOption Blindspot6Room { get; } =
        new("TouOptionSentryBlindspot6Room", (int)SystemTypes.Electrical, typeof(SystemTypes), SystemTypeLocaleKeys)
        {
            Visible = () => OptionGroupSingleton<SentryOptions>.Instance.BlindspotsCount.Value >= 6f
        };

    public ModdedEnumOption Blindspot7Room { get; } =
        new("TouOptionSentryBlindspot7Room", (int)SystemTypes.Security, typeof(SystemTypes), SystemTypeLocaleKeys)
        {
            Visible = () => OptionGroupSingleton<SentryOptions>.Instance.BlindspotsCount.Value >= 7f
        };

    public ModdedEnumOption Blindspot8Room { get; } =
        new("TouOptionSentryBlindspot8Room", (int)SystemTypes.MedBay, typeof(SystemTypes), SystemTypeLocaleKeys)
        {
            Visible = () => OptionGroupSingleton<SentryOptions>.Instance.BlindspotsCount.Value >= 8f
        };

    public ModdedEnumOption Blindspot9Room { get; } =
        new("TouOptionSentryBlindspot9Room", (int)SystemTypes.Weapons, typeof(SystemTypes), SystemTypeLocaleKeys)
        {
            Visible = () => OptionGroupSingleton<SentryOptions>.Instance.BlindspotsCount.Value >= 9f
        };

    public ModdedEnumOption Blindspot10Room { get; } =
        new("TouOptionSentryBlindspot10Room", (int)SystemTypes.Comms, typeof(SystemTypes), SystemTypeLocaleKeys)
        {
            Visible = () => OptionGroupSingleton<SentryOptions>.Instance.BlindspotsCount.Value >= 10f
        };

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>
        {
            BlindspotsCount.StringName,
            Blindspot1Room.StringName,
            Blindspot2Room.StringName,
            Blindspot3Room.StringName,
            Blindspot4Room.StringName,
            Blindspot5Room.StringName,
            Blindspot6Room.StringName,
            Blindspot7Room.StringName,
            Blindspot8Room.StringName,
            Blindspot9Room.StringName,
            Blindspot10Room.StringName,
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        var count = (int)BlindspotsCount.Value;
        var title = TranslationController.Instance != null
            ? TranslationController.Instance.GetString(BlindspotsCount.StringName)
            : BlindspotsCount.StringName.ToString();

        if (count <= 0)
        {
            var newArray = new []
                { $"{title}: {BlindspotsCount.ZeroWordValue}" };
            return newArray;
        }

        var selected = new List<SystemTypes>(count);
        if (count >= 1) selected.Add((SystemTypes)Blindspot1Room.Value);
        if (count >= 2) selected.Add((SystemTypes)Blindspot2Room.Value);
        if (count >= 3) selected.Add((SystemTypes)Blindspot3Room.Value);
        if (count >= 4) selected.Add((SystemTypes)Blindspot4Room.Value);
        if (count >= 5) selected.Add((SystemTypes)Blindspot5Room.Value);
        if (count >= 6) selected.Add((SystemTypes)Blindspot6Room.Value);
        if (count >= 7) selected.Add((SystemTypes)Blindspot7Room.Value);
        if (count >= 8) selected.Add((SystemTypes)Blindspot8Room.Value);
        if (count >= 9) selected.Add((SystemTypes)Blindspot9Room.Value);
        if (count >= 10) selected.Add((SystemTypes)Blindspot10Room.Value);

        var names = selected
            .Select(s => TouLocale.GetParsed($"TouSystemType_{s}", $"{s}"))
            .Distinct()
            .ToList();

        var newArray2 = new []
            { $"{title}: {count} ({string.Join(", ", names)})" };
        return newArray2;
    }
}

