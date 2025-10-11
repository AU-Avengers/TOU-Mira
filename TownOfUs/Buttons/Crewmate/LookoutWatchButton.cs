using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class WatchButton : TownOfUsRoleButton<LookoutRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleLookoutWatch", "Watch");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Lookout;
    public override float Cooldown => OptionGroupSingleton<LookoutOptions>.Instance.WatchCooldown + MapCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<LookoutOptions>.Instance.MaxWatches;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.WatchSprite;
    public int ExtraUses { get; set; }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return base.IsTargetValid(target) && !target!.HasModifier<LookoutWatchedModifier>(x => x.Lookout.AmOwner);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Watch: Target is null");
            return;
        }

        Target.RpcAddModifier<LookoutWatchedModifier>(PlayerControl.LocalPlayer);

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{TouLocale.GetParsed("TouRoleLookoutWatchNotif").Replace("<player>", Target.Data.PlayerName)}</b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Lookout.LoadAsset());
        notif1.AdjustNotification();
    }
}