using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class UndertakerOptions : AbstractRoleOptionGroup<UndertakerRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Undertaker", "Undertaker");

    [ModdedNumberOption("TouOptionUndertakerDragCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DragCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionUndertakerDragSpeed", 0.25f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float DragSpeedMultiplier { get; set; } = 0.75f;

    [ModdedToggleOption("TouOptionUndertakerDraggingSpeedIsAffectedbyBodySize")]
    public bool AffectedSpeed { get; set; } = true;

    [ModdedToggleOption("TouOptionUndertakerCanVent")]
    public bool CanVent { get; set; } = true;

    public ModdedToggleOption CanVentWithBody { get; } = new("TouOptionUndertakerCanVentWithBody", false)
    {
        Visible = () => OptionGroupSingleton<UndertakerOptions>.Instance.CanVent
    };

    [ModdedToggleOption("TouOptionUndertakerUndertakerCanKillWithTeammate")]
    public bool UndertakerKill { get; set; } = true;
}