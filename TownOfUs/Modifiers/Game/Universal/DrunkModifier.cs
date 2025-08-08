using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class DrunkModifier : UniversalGameModifier, IWikiDiscoverable, IVisualAppearance
{
    public override string ModifierName => TouLocale.Get(TouNames.Drunk, "Drunk");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Drunk;

    public override ModifierFaction FactionType => ModifierFaction.UniversalHarmful;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = -1;
        appearance.PetId = string.Empty;
        return appearance;
    }

    public override string GetDescription()
    {
        return
            $"Youuuuuuu haaaaaaaave inverrrrrrted contrrrrrrrrrols";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public string GetAdvancedDescription()
    {
        return $"Your controls are inverted.\n\nYou are also immune to the effects of Barkeeper and Bootlegger roleblocking you.";
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.DrunkChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.DrunkAmount;
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
    }

    public override void OnDeactivate()
    {
        Player?.ResetAppearance(fullReset: true);
    }
}