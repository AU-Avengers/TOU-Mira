using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class AmbusherPursueButton : TownOfUsRoleButton<AmbusherRole, PlayerControl>
{
    public override string Name => "Pursue";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.PursueSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role is { Pursued: null };
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Role.Pursued = Target;
        
        Color color = Palette.PlayerColors[Target.GetDefaultAppearance().ColorId];
        var update = OptionGroupSingleton<AmbusherOptions>.Instance.UpdateInterval;

        Target.AddModifier<AmbusherArrowTargetModifier>(PlayerControl.LocalPlayer, color, update);

        TouAudio.PlaySound(TouAudio.TrackerActivateSound);

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}You are now pursuing {Target.Data.PlayerName}. Ambush anyone near them at any time you wish.</b></color>",
            Color.white, spr: TouRoleIcons.Ambusher.LoadAsset());
        notif1.Text.SetOutlineThickness(0.35f);
        notif1.transform.localPosition = new Vector3(0f, 1f, -20f);

        CustomButtonSingleton<AmbusherAmbushButton>.Instance.SetActive(true, Role);
        CustomButtonSingleton<AmbusherAmbushButton>.Instance.ResetCooldownAndOrEffect();
        SetActive(false, Role);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}