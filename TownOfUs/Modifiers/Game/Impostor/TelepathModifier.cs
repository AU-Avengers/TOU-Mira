using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class TelepathModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get(TouNames.Telepath, "Telepath");
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public override string IntroInfo => "You also know information about teammates' kills" +
                                        (OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath
                                            ? " and deaths."
                                            : ".");

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Telepath;
    public override ModifierFaction FactionType => ModifierFaction.ImpostorPostmortem;

    public float Timer { get; set; }

    public string GetAdvancedDescription()
    {
        var options = OptionGroupSingleton<TelepathOptions>.Instance;
        return
            (options.KnowKillLocation ? "Know when & where your teammate kills" : "Know when your teammate kills")
            + (options.KnowDeath && !options.KnowDeathLocation ? ", know when they die." : string.Empty)
            + (options.KnowDeath && options.KnowDeathLocation ? ", know when & where they die." : string.Empty)
            + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return (OptionGroupSingleton<TelepathOptions>.Instance.KnowKillLocation
                   ? "Know when & where your teammate kills"
                   : "Know when your teammate kills")
               + (OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath &&
                  !OptionGroupSingleton<TelepathOptions>.Instance.KnowDeathLocation
                   ? ", know when they die"
                   : string.Empty)
               + (OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath &&
                  OptionGroupSingleton<TelepathOptions>.Instance.KnowDeathLocation
                   ? ", know when & where they die."
                   : string.Empty);
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.TelepathChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.TelepathAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor() && !OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode &&
               PlayerControl.AllPlayerControls.ToArray().Count(x => x.IsImpostor() && !x.HasDied()) != 1;
    }
}