using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class CircumventModifier : TouGameModifier, IWikiDiscoverable
{
    public override string LocaleKey => "Circumvent";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Circumvent;

    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);


    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.CircumventChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.CircumventAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor() && (role is not ICustomRole custom || custom.Configuration.CanUseVent);
    }

    public override bool? CanVent()
    {
        return false;
    }
}