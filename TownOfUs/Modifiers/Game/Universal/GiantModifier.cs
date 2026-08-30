using MiraAPI.GameOptions;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class GiantModifier : UniversalGameModifier, IWikiDiscoverable, IVisualAppearance
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Giant,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Giant.LoadAsset(),
            "TouMira.Modifier.Universal.Giant", 1.45f));
    public override string IdPart => "Giant";
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
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

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.TabDescription").Replace("<giantSpeed>",
            $"{Math.Round(OptionGroupSingleton<GiantOptions>.Instance.GiantSpeed, 2)}");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.WikiDescription").Replace("<giantSpeed>",
                   $"{Math.Round(OptionGroupSingleton<GiantOptions>.Instance.GiantSpeed, 2)}") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

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