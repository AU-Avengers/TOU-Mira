using MiraAPI.GameOptions;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class ImpostorAssassinModifier : AssassinModifier
{
    public override string ModifierName => TouLocale.Get(TouNames.Assassin, "Assassin");
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<AssassinOptions>.Instance.NumberOfImpostorAssassins;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<AssassinOptions>.Instance.ImpAssassinChance;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.TeamType == RoleTeamTypes.Impostor;
    }
}