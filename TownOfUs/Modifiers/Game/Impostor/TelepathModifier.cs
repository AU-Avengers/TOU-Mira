using MiraAPI.GameOptions;
using TownOfUs.Options;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Impostor;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class TelepathModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Impostor,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Telepath.LoadAsset(),
            "TouMira.Modifier.Impostor.Telepath", 1.45f));
    public override string IdPart => "Telepath";
    public override string ModifierName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Telepath", "Telepath");
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public override string IntroInfo => OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath
        ? MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}IntroBlurbNoDeath")
        : MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.IntroBlurb");

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
        var IdPartfull = $"TownOfUsMira.Modifier.{IdPart}Description";
        return (OptionGroupSingleton<TelepathOptions>.Instance.KnowKillLocation
            ? MiraLocaleManager.Get($"{IdPartfull}IfKnowWhen")
            : MiraLocaleManager.Get($"{IdPartfull}Basic")
              + (OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath &&
                 !OptionGroupSingleton<TelepathOptions>.Instance.KnowDeathLocation
                  ? MiraLocaleManager.Get($"{IdPartfull}AddIfKnowDeath")
                  : string.Empty)
              + (OptionGroupSingleton<TelepathOptions>.Instance.KnowDeath &&
                 OptionGroupSingleton<TelepathOptions>.Instance.KnowDeathLocation
                  ? MiraLocaleManager.Get($"{IdPartfull}AddIfKnowDeathLoc")
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