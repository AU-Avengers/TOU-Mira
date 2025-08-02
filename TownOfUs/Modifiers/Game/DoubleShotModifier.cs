using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game;

public class DoubleShotModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Double Shot";
    public override string IntroInfo => "You also get a second chance when guessing.";

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.DoubleShot;
    public override ModifierFaction FactionType => ModifierFaction.AssailantUtility;
    
    // YES this is scuffed, a better solution will be used at a later time
    public override bool ShowInFreeplay => false;

    public bool Used { get; set; }

    public string GetAdvancedDescription()
    {
        return
            "You get a second chance when you fail to guess a player in a meeting.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return "You have an extra chance when assassinating";
    }

    public override int GetAssignmentChance()
    {
        return 0;
    }

    public override int GetAmountPerGame()
    {
        return 0;
    }
    public override int CustomAmount =>
        (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.DoubleShotAmount + (int)OptionGroupSingleton<NeutralModifierOptions>.Instance.DoubleShotAmount;

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
        return false;
    }
}