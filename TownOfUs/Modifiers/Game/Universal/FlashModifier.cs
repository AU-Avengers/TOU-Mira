using MiraAPI.GameOptions;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class FlashModifier : UniversalGameModifier, IWikiDiscoverable, IVisualAppearance
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Flash,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Flash.LoadAsset(),
            "TouMira.Modifier.Universal.Flash", 1.45f));
    public override string IdPart => "Flash";
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Flash;

    public override ModifierFaction FactionType => ModifierFaction.UniversalVisibility;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = OptionGroupSingleton<FlashOptions>.Instance.FlashSpeed;
        return appearance;
    }

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}TabDescription").Replace("<flashSpeed>",
            $"{Math.Round(OptionGroupSingleton<FlashOptions>.Instance.FlashSpeed, 2)}");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}WikiDescription").Replace("<flashSpeed>",
                   $"{Math.Round(OptionGroupSingleton<FlashOptions>.Instance.FlashSpeed, 2)}") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

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