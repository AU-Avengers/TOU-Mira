using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class SleuthModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Sleuth,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Sleuth.LoadAsset(),
            "TouMira.Modifier.Universal.Sleuth", 1.45f));
    public override string IdPart => "Sleuth";
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Sleuth;

    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);
    public List<byte> Reported { get; set; } = [];

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
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

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role is not AltruistRole;
    }

    public static bool SleuthVisibilityFlag(PlayerControl player)
    {
        if (PlayerControl.LocalPlayer.TryGetModifier<SleuthModifier>(out var sleuth))
        {
            return sleuth.Reported.Contains(player.PlayerId);
        }

        return false;
    }
}