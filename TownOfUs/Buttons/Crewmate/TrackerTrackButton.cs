using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class TrackerTrackButton : TownOfUsRoleButton<TrackerTouRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleTrackerTrack", "Track");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Tracker;
    public override float Cooldown => OptionGroupSingleton<TrackerOptions>.Instance.TrackCooldown + MapCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<TrackerOptions>.Instance.MaxTracks;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.TrackSprite;
    public int ExtraUses { get; set; }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return base.IsTargetValid(target) && !target!.HasModifier<TrackerArrowTargetModifier>();
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Track: Target is null");
            return;
        }

        Color color = Palette.PlayerColors[Target.GetDefaultAppearance().ColorId];
        var update = OptionGroupSingleton<TrackerOptions>.Instance.UpdateInterval;

        Target.AddModifier<TrackerArrowTargetModifier>(PlayerControl.LocalPlayer, color, update);

        TouAudio.PlaySound(TouAudio.TrackerActivateSound);
    }
}