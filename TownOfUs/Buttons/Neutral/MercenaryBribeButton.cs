using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class MercenaryBribeButton : TownOfUsRoleButton<MercenaryRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleMercenaryBribe", "Bribe");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Mercenary;
    public override float Cooldown => 0.001f + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.BribeSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && role is MercenaryRole && !PlayerControl.LocalPlayer.Data.IsDead && Role.CanBribe;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Mercenary Bribed: Target is null");
            return;
        }

        Target.RpcAddModifier<MercenaryBribedModifier>(PlayerControl.LocalPlayer);
        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>If {Target.Data.PlayerName} wins, you will win as well.</b>", Color.white, new Vector3(0f, 1f, -20f),
            spr: TouRoleIcons.Mercenary.LoadAsset());
        notif1.AdjustNotification();

        Role.Gold -= MercenaryRole.BrideCost;

        SetActive(false, Role);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: x => !x.HasModifier<MercenaryBribedModifier>());
    }
}