using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Modifiers;

public sealed class TestTimeLordRewindButton : TownOfUsButton
{
    public override string Name => TouLocale.GetParsed("TouRoleTimeLordRewind", "Rewind");
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => TownOfUsColors.TimeLord;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<TimeLordOptions>.Instance.RewindCooldown + MapCooldown, 5f, 120f);

    public override float EffectDuration => 3.5f;

    public override int MaxUses => (int)OptionGroupSingleton<TimeLordOptions>.Instance.MaxUses;

    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.RewindSprite;

    public override bool ZeroIsInfinite { get; set; } = true;

    protected override void OnClick()
    {
        // Use the same RPC as Time Lord role, but check for modifier instead
        TimeLordRole.RpcStartRewind(PlayerControl.LocalPlayer);
        OverrideName(TouLocale.GetParsed("TouRoleTimeLordRewinding", "Rewinding"));
    }

    public override void OnEffectEnd()
    {
        OverrideName(TouLocale.GetParsed("TouRoleTimeLordRewind", "Rewind"));
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               PlayerControl.LocalPlayer.HasModifier<TestTimeLordModifier>() &&
               !PlayerControl.LocalPlayer.Data.IsDead;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);

        if (Button == null)
        {
            return;
        }

        var spr = EffectActive ? TouCrewAssets.RewindingSprite.LoadAsset() : TouCrewAssets.RewindSprite.LoadAsset();
        if (Button.graphic != null && Button.graphic.sprite != spr)
        {
            Button.graphic.sprite = spr;
        }
    }
}


