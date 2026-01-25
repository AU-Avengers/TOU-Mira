using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class BenefactorAegisModifier(PlayerControl benefactor) : BaseShieldModifier
{
    public override string ModifierName => "Aegis";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Benefactor;
    public override string ShieldDescription => "You are protected by an Aegis!";
    public override bool HideOnUi => OptionGroupSingleton<BenefactorOptions>.Instance.TargetSeesAegis;
    public override bool VisibleSymbol => OptionGroupSingleton<BenefactorOptions>.Instance.TargetSeesAegis;

    public PlayerControl Benefactor { get; } = benefactor;

    public override void OnActivate()
    {
        base.OnActivate();
        var touAbilityEvent = new TouAbilityEvent(AbilityType.BenefactorAegis, Benefactor, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }
    
    public override void Update()
    {
        if (Player == null || Benefactor == null)
        {
            ModifierComponent?.RemoveModifier(this);
        }
    }
}