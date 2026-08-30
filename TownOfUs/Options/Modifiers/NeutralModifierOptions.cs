/*using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class NeutralModifierOptions : AbstractOptionGroup
{
    public override string GroupName => TouLocale.Get("TouOptionTitleNeutralModifiers");
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => TownOfUsColors.Neutral;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 4;
}*/