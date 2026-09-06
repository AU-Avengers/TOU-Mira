using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using TownOfUs.Modules;
using TownOfUs.Roles.Other;

namespace TownOfUs.Modifiers.Game;

[MiraIgnore]
public abstract class TouGameModifier : TouBaseGameModifier
{
    public override float IntroSize => 2.8f;
    public virtual bool PreventsOtherModifiers => true;
    public virtual bool AppearsInSummary => true;
    public virtual bool AppearsInIntro => true;
    public virtual bool HideFromGuessing => false;
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return !role.Player.GetModifierComponent().HasModifier<TouGameModifier>(true, x => x.PreventsOtherModifiers) && role is not SpectatorRole;
    }

    public override void AddModifierToStats(PlayerStats stats)
    {
        if (AppearsInSummary)
        {
            stats.LastKnownModifiers.Add(this);
        }
    }
}