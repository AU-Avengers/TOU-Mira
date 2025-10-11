using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class HunterStalkButton : TownOfUsRoleButton<HunterRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleHunterStalk", "Stalk");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Hunter;
    public override float Cooldown => OptionGroupSingleton<HunterOptions>.Instance.HunterStalkCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<HunterOptions>.Instance.HunterStalkDuration;
    public override int MaxUses => (int)OptionGroupSingleton<HunterOptions>.Instance.StalkUses;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.StalkButtonSprite;
    public int ExtraUses { get; set; }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Stalk: Target is null");
            return;
        }

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{TouLocale.GetParsed("TouRoleHunterStalkNotif").Replace("<player>", Target.Data.PlayerName)}</b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Hunter.LoadAsset());
        notif1.AdjustNotification();

        Target.RpcAddModifier<HunterStalkedModifier>(PlayerControl.LocalPlayer);
        OverrideName(TouLocale.Get("TouRoleHunterStalking", "Stalking"));
    }

    public override void OnEffectEnd()
    {
        OverrideName(TouLocale.Get("TouRoleHunterStalk", "Stalk"));
    }

    public override PlayerControl? GetTarget()
    {
        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsLover());
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}