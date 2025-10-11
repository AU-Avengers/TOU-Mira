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
    public override string LocaleKey => "Telepath";
    public override string ModifierName => TouLocale.Get("TouModifierTelepath", "Telepath");
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public override string IntroInfo => OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath
        ? TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurbNoDeath")
        : TouLocale.Get($"TouModifier{LocaleKey}IntroBlurb");

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Telepath;
    public override ModifierFaction FactionType => ModifierFaction.ImpostorPostmortem;

    public float Timer { get; set; }

    public string GetAdvancedDescription()
    {
        return
            GetDescription() + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

#pragma warning disable S3358
    public override string GetDescription()
    {
        var localekeyfull = $"TouModifier{LocaleKey}Description";
        return (OptionGroupSingleton<TelepathOptions>.Instance.KnowKillLocation
            ? TouLocale.GetParsed($"{localekeyfull}IfKnowWhen")
            : TouLocale.GetParsed($"{localekeyfull}Basic")
              + (OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath &&
                 !OptionGroupSingleton<TelepathOptions>.Instance.KnowDeathLocation
                  ? TouLocale.GetParsed($"{localekeyfull}AddIfKnowDeath")
                  : string.Empty)
              + (OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath &&
                 OptionGroupSingleton<TelepathOptions>.Instance.KnowDeathLocation
                  ? TouLocale.GetParsed($"{localekeyfull}AddIfKnowDeathLoc")
                  : string.Empty));
#pragma warning restore S3358
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
        return base.IsModifierValidOn(role) && role.IsImpostor() &&
               !OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode &&
               PlayerControl.AllPlayerControls.ToArray().Count(x => x.IsImpostor() && !x.HasDied()) != 1;
    }
}