using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class FlashModifier : UniversalGameModifier, IWikiDiscoverable, IVisualAppearance
{
    public override string ModifierName => TouLocale.Get(TouNames.Flash, "Flash");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Flash;

    public override ModifierFaction FactionType => ModifierFaction.UniversalVisibility;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = OptionGroupSingleton<FlashOptions>.Instance.FlashSpeed;
        return appearance;
    }

    public string GetAdvancedDescription()
    {
        return
            $"You move {Math.Round(OptionGroupSingleton<FlashOptions>.Instance.FlashSpeed, 2)}x faster than regular players.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return $"You move {Math.Round(OptionGroupSingleton<FlashOptions>.Instance.FlashSpeed, 2)}x faster.";
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.FlashChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.FlashAmount;
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