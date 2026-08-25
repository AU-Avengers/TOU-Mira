using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Impostor;

public sealed class TelepathOptions : AbstractTouModifierOptionGroup<TelepathModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => TouLocale.Get("TouModifierTelepath", "Telepath");
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override uint GroupPriority => 42;

    [ModdedToggleOption("TouOptionTelepathKnowKillLocation")]
    public bool KnowKillLocation { get; set; } = true;

    [ModdedToggleOption("TouOptionTelepathKnowDeath")]
    public bool KnowDeath { get; set; } = true;

    public ModdedToggleOption KnowDeathLocation { get; } =
        new("TouOptionTelepathKnowDeathLocation", true)
        {
            Visible = () => OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath
        };

    public ModdedNumberOption TelepathArrowDuration { get; } =
        new("TouOptionTelepathArrowDuration", 2.5f, 0f, 5f, 0.5f,
            MiraNumberSuffixes.Seconds, "0.00")
        {
            Visible = () => OptionGroupSingleton<TelepathOptions>.Instance.KnowKillLocation ||
                            (OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath &&
                             OptionGroupSingleton<TelepathOptions>.Instance.KnowDeathLocation)
        };

    [ModdedToggleOption("TouOptionTelepathKnowCorrectGuess")]
    public bool KnowCorrectGuess { get; set; } = true;

    [ModdedToggleOption("TouOptionTelepathKnowFailedGuess")]
    public bool KnowFailedGuess { get; set; } = true;
}