using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Utilities.Assets;
using TownOfUs.Interfaces;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class TestCleanModifier : TouGameModifier, IWikiDiscoverable, IButtonModifier
{
    [HideFromIl2Cpp] public bool IsHiddenFromList => true;
    public override string LocaleKey => "TestClean";
    public override string ModifierName => "Test Clean";
    public override string IntroInfo => "Test modifier for janitor clean button";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Rotting; // Reuse rotting icon for now
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmateUtility;

    public override string GetDescription()
    {
        return "Test modifier that gives you a janitor clean button";
    }

    public string GetAdvancedDescription()
    {
        return "This is a test modifier for testing janitor clean functionality with Time Lord rewind.";
    }

    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new("Clean", "Clean dead bodies (for testing)", TouImpAssets.CleanButtonSprite)
            };
        }
    }

    public override int GetAssignmentChance()
    {
        return 0; // Never assign during game
    }

    public override int GetAmountPerGame()
    {
        return 0; // Never assign during game
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }
}



