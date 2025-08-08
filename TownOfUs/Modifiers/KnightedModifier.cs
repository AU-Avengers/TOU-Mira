using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers;

public sealed class KnightedModifier : BaseModifier
{
    public override string ModifierName => "Knighted";
    public override bool HideOnUi => true;
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Knighted;
    public override bool Unique => false;

    public override string GetDescription()
    {
        return $"You were knighted by a Monarch. You gained {(int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight} extra vote(s).";
    }

}