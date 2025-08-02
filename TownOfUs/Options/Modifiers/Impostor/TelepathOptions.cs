using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Impostor;

public sealed class TelepathOptions : AbstractOptionGroup<TelepathModifier>
{
    public override string GroupName => TouLocale.Get(TouNames.Telepath, "Telepath");
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override uint GroupPriority => 42;

    [ModdedToggleOption("Know Where Teammate Kills")]
    public bool KnowKillLocation { get; set; } = true;

    [ModdedToggleOption("Know When Teammate Dies")]
    public bool KnowDeath { get; set; } = true;

    public ModdedToggleOption KnowDeathLocation { get; } = new("Know Where Teammate Dies", true)
    {
        Visible = () => OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath
    };

    public ModdedNumberOption TelepathArrowDuration { get; } = new("Dead Body Arrow Duration", 2.5f, 0f, 5f, 0.5f,
        MiraNumberSuffixes.Seconds, "0.00")
    {
        Visible = () => OptionGroupSingleton<TelepathOptions>.Instance.KnowKillLocation ||
                        (OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath &&
                         OptionGroupSingleton<TelepathOptions>.Instance.KnowDeathLocation)
    };

    [ModdedToggleOption("Know When Teammate Guesses Successfully")]
    public bool KnowCorrectGuess { get; set; } = true;

    [ModdedToggleOption("Know When Teammate Fails To Guess")]
    public bool KnowFailedGuess { get; set; } = true;
}