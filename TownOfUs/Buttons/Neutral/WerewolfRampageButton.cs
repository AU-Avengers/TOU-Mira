using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class WerewolfRampageButton : TownOfUsRoleButton<WerewolfRole>
{
    public override string Name => TouLocale.Get("TouRoleWerewolfRampage", "Rampage");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Werewolf;
    public override float Cooldown => OptionGroupSingleton<WerewolfOptions>.Instance.RampageCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<WerewolfOptions>.Instance.RampageDuration;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.RampageSprite;

    public override bool CanClick()
    {
        return base.CanClick() && Role?.Rampaging == false;
    }

    protected override void OnClick()
    {
        if (Role == null)
        {
            return;
        }

        Role.Rampaging = true;

        CustomButtonSingleton<WerewolfKillButton>.Instance.SetActive(true, Role);
        CustomButtonSingleton<WerewolfKillButton>.Instance.SetTimer(0.01f);
        TouAudio.PlaySound(TouAudio.WerewolfRampageSound);
    }

    public override void OnEffectEnd()
    {
        if (Role == null)
        {
            return;
        }

        Role.Rampaging = false;

        CustomButtonSingleton<WerewolfKillButton>.Instance.SetActive(false, Role);
    }
}