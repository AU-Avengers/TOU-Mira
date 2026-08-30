using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Classic.Neutral.NeutralKilling;

public sealed class MedusaStoneGazeButton : TownOfUsRoleButton<MedusaRole>
{
    public override string Name => MiraLocaleManager.Get("TouRoleMedusaStoneGaze", "Stone Gaze");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Medusa;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<MedusaOptions>.Instance.StoneGazeCooldown.Value + MapCooldown, 5f, 120f);
    public override float EffectDuration => OptionGroupSingleton<MedusaOptions>.Instance.StoneGazeDuration.Value;
    public override int MaxUses => (int)OptionGroupSingleton<MedusaOptions>.Instance.StoneGazeUses.Value;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.StoneGazeSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && OptionGroupSingleton<MedusaOptions>.Instance.StoneGazeAvailable.Value;
    }

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcAddModifier<MedusaGazingModifier>();
        OverrideName(MiraLocaleManager.Get("TouRoleMedusaStoneGazing", "Stone Gazing"));
    }

    public override void OnEffectEnd()
    {
        OverrideName(MiraLocaleManager.Get("TouRoleMedusaStoneGaze", "Stone Gaze"));
    }
}