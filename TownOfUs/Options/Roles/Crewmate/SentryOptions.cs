using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SentryOptions : AbstractRoleOptionGroup<SentryRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Sentry", "Sentry");

    [ModdedNumberOption("TouOptionSentryPlacementCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float PlacementCooldown { get; set; } = 30f;

    [ModdedEnumOption("TouOptionSentryDeployedCamerasVisibility", typeof(SentryDeployedCamerasVisibility),
        ["TouOptionSentryDeployedVisibilityEnumImmediately", "TouOptionSentryDeployedVisibilityEnumAfterMeeting"])]
    public SentryDeployedCamerasVisibility DeployedCamerasVisibility { get; set; } = SentryDeployedCamerasVisibility.Immediately;

    public ModdedNumberOption CamerasVisibleAfter { get; } =
        new("TouOptionSentryCamerasVisibleAfter", 3f, 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds, "0.0")
        {
            Visible = () =>
                OptionGroupSingleton<SentryOptions>.Instance.DeployedCamerasVisibility is SentryDeployedCamerasVisibility.Immediately
        };

    public ModdedToggleOption CanMoveWhilePlacingCameras { get; } =
        new("TouOptionSentryCanMoveWhilePlacingCameras", false)
        {
            Visible = () =>
                OptionGroupSingleton<SentryOptions>.Instance.DeployedCamerasVisibility is SentryDeployedCamerasVisibility.AfterMeeting
        };

    [ModdedEnumOption("TouOptionSentryPortableCamerasMode", typeof(SentryPortableCamerasMode),
        ["TouOptionSentryPortableModeEnumAlways", "TouOptionSentryPortableModeEnumAfterTasks", "TouOptionSentryPortableModeEnumCamlessMaps"])]
    public SentryPortableCamerasMode PortableCamerasMode { get; set; } = SentryPortableCamerasMode.OnMapsWithoutCameras;

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


    private static readonly string[] SystemTypeIdParts =
        Enum.GetNames<SystemTypes>().Select(n => $"TouSystemType_{n}").ToArray();

    private static readonly SystemTypes[] BlindspotDefaults = 
        [SystemTypes.Hallway,    SystemTypes.Outside,  SystemTypes.Cafeteria, SystemTypes.Storage, SystemTypes.Admin,
         SystemTypes.Electrical, SystemTypes.Security, SystemTypes.MedBay,    SystemTypes.Weapons, SystemTypes.Comms];

    public ModdedNumberOption BlindspotsCount { get; } =
        new("TouOptionSentryBlindspots", 0f, 0f, 10f, 1f, "Off", "#", MiraNumberSuffixes.None, "0");

    public ModdedOptionList<ModdedEnumOption<SystemTypes>> BlindspotsRooms { get; } =
        new(10, i => new($"TouOptionSentryBlindspot{i + 1}Room", BlindspotDefaults[i], SystemTypeIdParts)
            {
                Visible = () => OptionGroupSingleton<SentryOptions>.Instance.BlindspotsCount.Value >= (i + 1)
            }
        )
        {
            ConfigName = i => $"Blindspot{i + 1}Room"
        };

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>(BlindspotsRooms.Select(room => room.StringName))
        {
            BlindspotsCount.StringName,
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        var count = (int)BlindspotsCount.Value;
        var title = TranslationController.Instance
            ? TranslationController.Instance.GetString(BlindspotsCount.StringName)
            : BlindspotsCount.StringName.ToString();

        if (count <= 0)
        {
            var newArray = new []
                { $"{title}: {BlindspotsCount.ZeroWordValue}" };
            return newArray;
        }

        var selected = new List<SystemTypes>(count);
        for (int i = 0; i < BlindspotsRooms.Count && count >= (i + 1); i++)
        {
            selected.Add(BlindspotsRooms[i]);
        }

        var names = selected
            .Select(s => MiraLocaleManager.Get($"TouSystemType_{s}", $"{s}"))
            .Distinct();

        var newArray2 = new []
            { $"{title}: {count} ({string.Join(", ", names)})" };
        return newArray2;
    }
}

public enum SentryDeployedCamerasVisibility
{
    Immediately,
    AfterMeeting
}

public enum SentryPortableCamerasMode
{
    Immediately,
    AfterTasks,
    OnMapsWithoutCameras
}