using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class MorphlingSampleButton : TownOfUsRoleButton<MorphlingRole, PlayerControl>, IAftermathablePlayerButton
{
    public override string Name => TouLocale.Get("TouRoleMorphlingSample", "Sample");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override int MaxUses => (int)OptionGroupSingleton<MorphlingOptions>.Instance.MaxSamples;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.SampleSprite;

    public void AftermathHandler()
    {
        var body = PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
        if (body == null)
        {
            return;
        }
        var player = MiscUtils.PlayerById(body.ParentId);

        if (player == null)
        {
            return;
        }

        Role.Sampled = player;

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}You have sampled {player.Data.PlayerName}. The sample will be reset after this round.</b></color>",
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Morphling.LoadAsset());
        notif1.AdjustNotification();

        CustomButtonSingleton<MorphlingMorphButton>.Instance.SetActive(true, Role);
        CustomButtonSingleton<MorphlingMorphButton>.Instance.ResetCooldownAndOrEffect();
        SetActive(false, Role);
    }
    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role is { Sampled: null };
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Role.Sampled = Target;

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}You have sampled {Target.Data.PlayerName}. The sample will be reset after this round.</b></color>",
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Morphling.LoadAsset());
        notif1.AdjustNotification();

        CustomButtonSingleton<MorphlingMorphButton>.Instance.SetActive(true, Role);
        CustomButtonSingleton<MorphlingMorphButton>.Instance.ResetCooldownAndOrEffect();
        SetActive(false, Role);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}