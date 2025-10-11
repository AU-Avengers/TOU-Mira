using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class VeteranAlertButton : TownOfUsRoleButton<VeteranRole>
{
    public override string Name => TouLocale.Get("TouRoleVeteranAlert", "Alert");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Veteran;
    public override float Cooldown => OptionGroupSingleton<VeteranOptions>.Instance.AlertCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<VeteranOptions>.Instance.AlertDuration;
    public override int MaxUses => (int)OptionGroupSingleton<VeteranOptions>.Instance.MaxNumAlerts;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.AlertSprite;
    public int ExtraUses { get; set; }

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcAddModifier<VeteranAlertModifier>();
        OverrideName(TouLocale.Get("TouRoleVeteranAlerting", "Alerting"));
    }

    public override void OnEffectEnd()
    {
        OverrideName(TouLocale.Get("TouRoleVeteranAlert", "Alert"));
    }
}