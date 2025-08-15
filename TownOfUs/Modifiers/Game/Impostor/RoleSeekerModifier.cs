using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using UnityEngine;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class RoleSeekerModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get(TouNames.RoleSeeker, "Role Seeker");
    public override string IntroInfo => "You will reveal a role which is or isn't in this game every kill.";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Telepath;

    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public string GetAdvancedDescription()
    {
        return
            "You will reveal a role which either is or isn't in this game every time you kill.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return "Reveal roles which are or aren't in-game.";
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.RoleSeekerChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.RoleSeekerAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor();
    }
}