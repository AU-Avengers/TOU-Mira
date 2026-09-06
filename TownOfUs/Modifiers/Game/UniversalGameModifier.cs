using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using TownOfUs.Modules;
using TownOfUs.Roles.Other;

namespace TownOfUs.Modifiers.Game;

[MiraIgnore]
public abstract class UniversalGameModifier : TouBaseGameModifier
{
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return !role.Player.GetModifierComponent().HasModifier<UniversalGameModifier>(true) &&
               role is not SpectatorRole;
    }

    public override void AddModifierToStats(PlayerStats stats)
    {
        stats.LastKnownModifiers.Add(this);
    }
}