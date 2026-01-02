using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class TimeLordRewindButton : TownOfUsRoleButton<TimeLordRole>
{
    public override string Name => TouLocale.GetParsed("TouRoleTimeLordRewind", "Rewind");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.TimeLord;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<TimeLordOptions>.Instance.RewindCooldown + MapCooldown, 5f, 120f);

    public override float EffectDuration => 3.5f;

    public override int MaxUses => (int)OptionGroupSingleton<TimeLordOptions>.Instance.MaxUses;

    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.RewindSprite;

    public override bool ZeroIsInfinite { get; set; } = true;

    protected override void OnClick()
    {
        TimeLordRole.RpcStartRewind(PlayerControl.LocalPlayer);
        OverrideName(TouLocale.GetParsed("TouRoleTimeLordRewinding", "Rewinding"));
    }

    public override void OnEffectEnd()
    {
        OverrideName(TouLocale.GetParsed("TouRoleTimeLordRewind", "Rewind"));
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


