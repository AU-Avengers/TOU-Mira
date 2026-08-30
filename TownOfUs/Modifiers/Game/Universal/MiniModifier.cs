using MiraAPI.GameOptions;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class MiniModifier : UniversalGameModifier, IWikiDiscoverable, IVisualAppearance
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Mini,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Mini.LoadAsset(),
            "TouMira.Modifier.Universal.Mini", 1.45f));
    public override string IdPart => "Mini";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Mini;

    public override ModifierFaction FactionType => ModifierFaction.UniversalVisibility;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = OptionGroupSingleton<MiniOptions>.Instance.MiniSpeed;
        appearance.Size = new Vector3(0.49f, 0.49f, 1f);
        return appearance;
    }

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription").Replace("<miniSpeed>",
            $"{Math.Round(OptionGroupSingleton<MiniOptions>.Instance.MiniSpeed, 2)}");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription").Replace("<miniSpeed>",
            $"{Math.Round(OptionGroupSingleton<MiniOptions>.Instance.MiniSpeed, 2)}");
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.MiniChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.MiniAmount;
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