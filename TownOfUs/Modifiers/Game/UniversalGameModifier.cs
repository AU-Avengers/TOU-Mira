using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using TownOfUs.Roles.Other;

namespace TownOfUs.Modifiers.Game;

[MiraIgnore]
public abstract class UniversalGameModifier : TouBaseGameModifier
{
    public override float IntroSize => 3f;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return !role.Player.GetModifierComponent().HasModifier<UniversalGameModifier>(true) &&
               role is not SpectatorRole;
    }
}