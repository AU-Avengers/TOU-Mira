using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using TownOfUs.Interfaces;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class ButtonBarryModifier : UniversalGameModifier, IWikiDiscoverable, IButtonModifier
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.ButtonBarry,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.ButtonBarry.LoadAsset(),
            "TouMira.Modifier.Universal.ButtonBarry", 1.45f));
    public override string IdPart => "ButtonBarry";
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.ButtonBarry;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public int Priority { get; set; } = 5;
    public override ModifierFaction FactionType => ModifierFaction.UniversalUtility;

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TouModifier{IdPart}Button"),
                    MiraLocaleManager.Get($"TouModifier{IdPart}Button.WikiDescription").Replace("<barryUses>",
                        $"{Math.Round(OptionGroupSingleton<ButtonBarryOptions>.Instance.MaxNumButtons, 0)}"),
                    TouAssets.BarryButtonSprite)
            ];
        }
    }

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.ButtonBarryChance;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        if (role is SwapperRole && !OptionGroupSingleton<SwapperOptions>.Instance.CanButton)
        {
            return false;
        }

        if (role is JesterRole && !OptionGroupSingleton<JesterOptions>.Instance.CanButton)
        {
            return false;
        }

        if (role is ExecutionerRole && !OptionGroupSingleton<ExecutionerOptions>.Instance.CanButton)
        {
            return false;
        }

        return base.IsModifierValidOn(role) &&
               !role.Player.GetModifierComponent().HasModifier<GameModifier>(true, x => x is IButtonModifier);
    }
}