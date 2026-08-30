using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class GeneralOptions : AbstractOptionGroup
{
    public override string GroupName => MiraLocaleManager.Get("TouOptionTitleGeneral");
    public override uint GroupPriority => 1;

    // Legacy Compatibility, this allows mods like ChaosTokens to still use this value as normal.
    
#pragma warning disable S2325 // Make a static property.
    
#pragma warning disable CA1822 // Member does not access instance data and can be marked as static
    public bool TheDeadKnow => OptionGroupSingleton<PostmortemOptions>.Instance.TheDeadKnow.Value;
    public float TempSaveCdReset => OptionGroupSingleton<GameMechanicOptions>.Instance.TempSaveCdReset;
    
#pragma warning restore CA1822 // Member does not access instance data and can be marked as static
    
#pragma warning restore S2325 // Make a static property.

    [ModdedToggleOption("TouOptionFFAImpostorMode")]
    public bool FFAImpostorMode { get; set; } = false;

    public ModdedToggleOption ImpsKnowRoles { get; set; } =
        new("TouOptionImpsKnowRoles", true)
        {
            Visible = () => !OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode
        };

    public ModdedToggleOption ImpostorChat { get; set; } =
        new("TouOptionImpostorChat", true)
        {
            Visible = () => !OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode
        };

    [ModdedToggleOption("TouOptionVampireChat")]
    public bool VampireChat { get; set; } = true;

    [ModdedNumberOption("TouOptionAddedMeetingDeathTimer", 0f, 15f, 1f,
        MiraNumberSuffixes.Seconds, "0.#")]
    public float AddedMeetingDeathTimer { get; set; } = 5f;
}