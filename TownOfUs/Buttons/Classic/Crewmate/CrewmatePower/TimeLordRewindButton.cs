using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class TimeLordRewindButton : TownOfUsRoleButton<TimeLordRole>, ILegacyCapable
{
    public override string Name => MiraLocaleManager.Get("TouRoleTimeLordRewind", "Rewind");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.TimeLord;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<TimeLordOptions>.Instance.RewindCooldown + MapCooldown, 5f, 120f);

    public override float EffectDuration => OptionGroupSingleton<TimeLordOptions>.Instance.RewindDuration;

    public override int MaxUses => (int)OptionGroupSingleton<TimeLordOptions>.Instance.MaxUses;

    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyCrewAssets.RewindSprite : TouCrewAssets.RewindSprite;

    protected override void OnClick()
    {
        TimeLordRole.RpcStartRewind(PlayerControl.LocalPlayer, EffectDuration);
        OverrideName(MiraLocaleManager.Get("TouRoleTimeLordRewinding", "Rewinding"));
    }

    public override void OnEffectEnd()
    {
        OverrideName(MiraLocaleManager.Get("TouRoleTimeLordRewind", "Rewind"));
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);

        if (Button == null)
        {
            return;
        }

        var spr = LegacyCrewAssets.RewindSprite.LoadAsset();
        if (!LegacyAssets.IsLegacy)
        {
            spr = EffectActive ? TouCrewAssets.RewindingSprite.LoadAsset() : TouCrewAssets.RewindSprite.LoadAsset();
        }
        if (Button.graphic != null && Button.graphic.sprite != spr)
        {
            Button.graphic.sprite = spr;
        }
    }
}


