using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class SleuthModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get(TouNames.Sleuth, "Sleuth");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Sleuth;

    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);
    public List<byte> Reported { get; set; } = [];

    public string GetAdvancedDescription()
    {
        return
            "You will see the roles of bodies you report.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return "Know the roles of bodies you report.";
    }

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