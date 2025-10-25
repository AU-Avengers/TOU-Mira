using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class GlitchKillButton : TownOfUsRoleButton<GlitchRole, PlayerControl>, IDiseaseableButton, IKillButton
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.KillLabel, "Kill");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Glitch;
    public override float Cooldown => OptionGroupSingleton<GlitchOptions>.Instance.KillCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.GlitchKillSprite;
    public override bool ShouldPauseInVent => false;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Glitch Shoot: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
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