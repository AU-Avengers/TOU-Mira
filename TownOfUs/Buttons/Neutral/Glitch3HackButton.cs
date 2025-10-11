using MiraAPI.GameOptions;
using MiraAPI.Hud;
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

public sealed class GlitchHackButton : TownOfUsRoleButton<GlitchRole, PlayerControl>, IAftermathablePlayerButton
{
    public override string Name => TouLocale.Get("TouRoleGlitchHack", "Hack");
    public override BaseKeybind Keybind => Keybinds.TertiaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Glitch;
    public override float Cooldown => OptionGroupSingleton<GlitchOptions>.Instance.HackCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.HackSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool ShouldPauseInVent => false;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    public void AftermathHandler()
    {
        TouAudio.PlaySound(TouAudio.HackedSound);
        PlayerControl.LocalPlayer.RpcAddModifier<GlitchHackedModifier>(PlayerControl.LocalPlayer.PlayerId);
    }
    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Glitch Hack: Target is null");
            return;
        }

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>Once {Target.Data.PlayerName} attempts to use an ability, all their abilities will get disabled.</b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Glitch.LoadAsset());
        notif1.AdjustNotification();

        TouAudio.PlaySound(TouAudio.HackedSound);
        Target.RpcAddModifier<GlitchHackedModifier>(PlayerControl.LocalPlayer.PlayerId);
    }
}