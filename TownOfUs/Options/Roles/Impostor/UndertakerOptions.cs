using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class UndertakerOptions : AbstractOptionGroup<UndertakerRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Undertaker, "Undertaker");

    [ModdedNumberOption("Drag Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DragCooldown { get; set; } = 25f;

    [ModdedNumberOption("Drag Speed", 0.25f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float DragSpeedMultiplier { get; set; } = 0.75f;

    [ModdedToggleOption("Dragging Speed Is Affected by Body Size")]
    public bool AffectedSpeed { get; set; } = true;

    [ModdedToggleOption("Undertaker Can Vent")]
    public bool CanVent { get; set; } = true;

    public ModdedToggleOption CanVentWithBody { get; } = new("Can Vent With Body", false)
    {
        Visible = () => OptionGroupSingleton<UndertakerOptions>.Instance.CanVent
    };

    [ModdedToggleOption("Undertaker Can Kill With Teammate")]
    public bool UndertakerKill { get; set; } = true;
}