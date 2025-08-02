using MiraAPI.GameOptions;
using TownOfUs.Options;
using TownOfUs.Roles;

namespace TownOfUs.Modifiers.Game.Neutral;

public sealed class NeutralKillerAssassinModifier : AssassinModifier
{
    public override string ModifierName => TouLocale.Get(TouNames.Assassin, "Assassin");

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<AssassinOptions>.Instance.NumberOfNeutralAssassins;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<AssassinOptions>.Instance.NeutAssassinChance;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role is ITownOfUsRole { RoleAlignment: RoleAlignment.NeutralKilling };
    }
}