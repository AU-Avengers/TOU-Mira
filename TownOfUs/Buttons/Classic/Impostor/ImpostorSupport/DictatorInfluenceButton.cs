using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class DictatorInfluenceButton : TownOfUsRoleButton<DictatorRole, PlayerControl>
{
    public override string Name => TouLocale.GetParsed("TouRoleDictatorInfluence", "Influence");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<DictatorOptions>.Instance.InfluenceCooldown + MapCooldown, 5f, 120f);

    public override LoadableAsset<Sprite> Sprite => TouImpAssets.DictatorInfluenceButtonSprite;

    public override bool ZeroIsInfinite { get; set; } = true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && role is DictatorRole dictator && !dictator.HasCoerced;
    }

    public override bool CanUse()
    {
        return base.CanUse() &&
               !Role.HasCoerced &&
               Role.GetActiveInfluenceCount() < DictatorRole.MaxInfluences;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Target.RpcAddModifier<DictatorInfluencedModifier>(PlayerControl.LocalPlayer.PlayerId);

        var notif = Helpers.CreateAndShowNotification(
            TouLocale.GetParsed("TouRoleDictatorInfluenceNotif")
                .Replace("<player>", $"{TownOfUsColors.Impostor.ToTextColor()}{Target.Data.PlayerName}</color>"),
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Dictator.LoadAsset());
        notif.AdjustNotification();
    }

    public override PlayerControl? GetTarget()
    {
        if (Role.GetActiveInfluenceCount() >= DictatorRole.MaxInfluences)
        {
            return null;
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance, false,
            player => Role.IsValidInfluenceTarget(player) &&
                      !player.HasModifier<DictatorInfluencedModifier>(modifier =>
                          modifier.DictatorId == PlayerControl.LocalPlayer.PlayerId));
    }
}
