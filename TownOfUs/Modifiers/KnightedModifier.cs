using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers;

public sealed class KnightedModifier : BaseModifier
{
    public override string ModifierName => MiraLocaleManager.Get("TouModifierKnighted");
    public override bool HideOnUi => OptionGroupSingleton<MonarchOptions>.Instance.RevealAtMeeting && !Announced;
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Monarch;
    public override bool Unique => false;

    public bool Announced { get; set; }

    public override string GetDescription()
    {
        return MiraLocaleManager.Get("TouModifierKnightedDescription")
            .Replace(
                "<votes>",
                ((int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight)
                .ToString(TownOfUsPlugin.Culture));
    }
}
