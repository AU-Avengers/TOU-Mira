using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Utilities.Assets;
using TownOfUs.Interfaces;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class TestTimeLordModifier : TouGameModifier, IWikiDiscoverable, IButtonModifier
{
    public override string LocaleKey => "TestTimeLord";
    public override string ModifierName => "Test Time Lord";
    public override string IntroInfo => "Test modifier for Time Lord rewind ability";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.TimeLord; // Use Time Lord role icon
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);
    [HideFromIl2Cpp] public bool IsHiddenFromList => true;

    public override ModifierFaction FactionType => ModifierFaction.CrewmateUtility;

    public override string GetDescription()
    {
        return "Test modifier that gives you Time Lord rewind ability";
    }

    public string GetAdvancedDescription()
    {
        return "This is a test modifier for testing Time Lord rewind functionality without having the role.";
    }

    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new("Rewind", "Rewind time for everyone (for testing)", TouCrewAssets.RewindSprite)
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

