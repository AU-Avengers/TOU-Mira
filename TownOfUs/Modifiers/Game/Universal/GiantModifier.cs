using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class GiantModifier : UniversalGameModifier, IWikiDiscoverable, IVisualAppearance
{
    public override string ModifierName => TouLocale.Get(TouNames.Giant, "Giant");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Giant;

    public override ModifierFaction FactionType => ModifierFaction.UniversalVisibility;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = OptionGroupSingleton<GiantOptions>.Instance.GiantSpeed;
        appearance.Size = new Vector3(1f, 1f, 1f);
        return appearance;
    }

    public string GetAdvancedDescription()
    {
        return
            $"You are bigger than regular players, and you also move {Math.Round(OptionGroupSingleton<GiantOptions>.Instance.GiantSpeed, 2)}x slower than regular players.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return
            $"You are bigger than the average player, moving {Math.Round(1f / OptionGroupSingleton<GiantOptions>.Instance.GiantSpeed, 2)}x slower";
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.GiantChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.GiantAmount;
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