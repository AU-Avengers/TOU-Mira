using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers;

public sealed class KnightedModifier : BaseModifier
{
    public override string ModifierName => "Knighted";
    public override bool HideOnUi => false;
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Monarch;
    public override bool Unique => false;

public override string GetDescription()
{
    return TouLocale.GetParsed("TouModifierKnightedDescription")
        .Replace(
            "<votes>",
            ((int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight)
            .ToString(TownOfUsPlugin.Culture));
}

}