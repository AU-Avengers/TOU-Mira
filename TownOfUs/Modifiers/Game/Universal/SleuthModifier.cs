using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class SleuthModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string LocaleKey => "Sleuth";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Sleuth;

    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);
    public List<byte> Reported { get; set; } = [];

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.SleuthChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.SleuthAmount;
    }

    public static bool SleuthVisibilityFlag(PlayerControl player)
    {
        if (PlayerControl.LocalPlayer.HasModifier<SleuthModifier>())
        {
            var mod = PlayerControl.LocalPlayer.GetModifier<SleuthModifier>()!;
            return mod.Reported.Contains(player.PlayerId);
        }

        return false;
    }
}