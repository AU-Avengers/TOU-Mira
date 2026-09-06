using MiraAPI.GameOptions;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.HnsGame.Crewmate;

public sealed class HnsFrostyModifier : HnsGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Frosty,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Frosty.LoadAsset(),
            "TouMira.Modifier.HnS.Hider.Frosty", 1.45f));
    public override string IdPart => "Frosty";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Frosty;
    public override ModifierFaction FactionType => ModifierFaction.HiderPostmortem;

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.FrostyChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.FrostyAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }
}