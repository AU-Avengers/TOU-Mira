using AchievementsAPI.API;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Achievements;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modifiers.Impostor;

public sealed class MorphlingMorphModifier(PlayerControl target) : DisguisedModifier(target)
{
    public override float Duration => OptionGroupSingleton<MorphlingOptions>.Instance.MorphlingDuration;
    public bool CanMorphlingVent = true;

    protected override TownOfUsAppearances Appearance => TownOfUsAppearances.Morph;

    public override void OnActivate()
    {
        CanMorphlingVent =
            (MorphlingVent)OptionGroupSingleton<MorphlingOptions>.Instance.CanVent.Value is MorphlingVent.Always;

        base.OnActivate();

        if (Player.AmOwner && (Target.HasModifier<MiniModifier>() && Player.HasModifier<GiantModifier>()) ||
            (Target.HasModifier<GiantModifier>() && Player.HasModifier<MiniModifier>()))
        {
            AchievementsTabSingleton<TouImpRoleAchievementsTab>.Instance.SizeDoesntMatter.Unlock();
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.MorphlingMorph, Player, Target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.MorphlingUnmorph, Player, Target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override bool? CanVent()
    {
        if (!CanMorphlingVent)
        {
            return false;
        }

        return null;
    }
}