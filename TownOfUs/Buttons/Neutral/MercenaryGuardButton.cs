using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class MercenaryGuardButton : TownOfUsRoleButton<MercenaryRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleMercenaryGuard", "Guard");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Mercenary;
    public override float Cooldown => OptionGroupSingleton<MercenaryOptions>.Instance.GuardCooldown + MapCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<MercenaryOptions>.Instance.MaxUses;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.GuardSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && !PlayerControl.LocalPlayer.Data.IsDead;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Mercenary Guard: Target is null");
            return;
        }

        Target.RpcAddModifier<MercenaryGuardModifier>(PlayerControl.LocalPlayer);
        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>Once {Target.Data.PlayerName} is interacted with, you will get one gold.</b>", Color.white,
            new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Mercenary.LoadAsset());
        notif1.AdjustNotification();
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: x => !x.HasModifier<MercenaryGuardModifier>());
    }
}