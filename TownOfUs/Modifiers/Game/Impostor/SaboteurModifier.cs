using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class SaboteurModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get(TouNames.Saboteur, "Saboteur");
    public override string IntroInfo => "You also have reduced sabotage cooldowns.";
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Saboteur;
    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;

    public float Timer { get; set; }

    public string GetAdvancedDescription()
    {
        return
            "You have a reduced cooldown when sabotaging."
            + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return "You have reduced sabotage cooldowns";
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.SaboteurChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.SaboteurAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor();
    }
}