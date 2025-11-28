using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Modifiers.Game;

public class DoubleShotModifier : TouGameModifier, IWikiDiscoverable
{
    public override string LocaleKey => "DoubleShot";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.DoubleShot;
    public override ModifierFaction FactionType => ModifierFaction.AssailantUtility;

    // YES this is scuffed, a better solution will be used at a later time
    public override bool ShowInFreeplay => false;

    public bool Used { get; set; }

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription");
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return 0;
    }

    public override int GetAmountPerGame()
    {
        return 0;
    }

    public override int CustomAmount =>
        (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.DoubleShotAmount +
        (int)OptionGroupSingleton<NeutralModifierOptions>.Instance.DoubleShotAmount;

    public override int CustomChance
    {
        get
        {
            var neutOpt = OptionGroupSingleton<NeutralModifierOptions>.Instance;
            var impOpt = OptionGroupSingleton<ImpostorModifierOptions>.Instance;
            var impChance = (int)impOpt.DoubleShotChance;
            var neutChance = (int)neutOpt.DoubleShotChance;
            if ((int)impOpt.DoubleShotAmount > 0 && (int)neutOpt.DoubleShotAmount > 0)
            {
                return (impChance + neutChance) / 2;
            }

            if ((int)impOpt.DoubleShotAmount > 0)
            {
                return impChance;
            }
            else if ((int)neutOpt.DoubleShotAmount > 0)
            {
                return neutChance;
            }

            return 0;
        }
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return !role.TryCast<SpectatorRole>();
    }
}