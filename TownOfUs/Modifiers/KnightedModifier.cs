using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers;

public sealed class KnightedModifier : BaseModifier
{
    public override string ModifierName => "Knighted";
    public override bool HideOnUi => OptionGroupSingleton<MonarchOptions>.Instance.RevealAtMeeting && !Announced;
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Monarch;
    public override bool Unique => false;

    public bool Announced { get; set; }

    public override string GetDescription()
    {
        return $"You were knighted by the Monarch. You gained {(int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight} extra vote(s).";
    }

}