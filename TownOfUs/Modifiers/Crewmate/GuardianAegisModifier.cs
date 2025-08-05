using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class GuardianAegisModifier(PlayerControl guardian) : BaseShieldModifier
{
    public override string ModifierName => "Aegis";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Guardian;
    public override string ShieldDescription => "You are protected by an Aegis!";
    public override bool HideOnUi => OptionGroupSingleton<GuardianOptions>.Instance.TargetSeesAegis;
    public override bool VisibleSymbol => OptionGroupSingleton<GuardianOptions>.Instance.TargetSeesAegis;

    public PlayerControl Guardian { get; } = guardian;

    public override void OnActivate()
    {
        base.OnActivate();
        var touAbilityEvent = new TouAbilityEvent(AbilityType.OracleAegis, Guardian, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }
    
    public override void Update()
    {
        if (Player == null || Guardian == null)
        {
            ModifierComponent?.RemoveModifier(this);
        }
    }
}