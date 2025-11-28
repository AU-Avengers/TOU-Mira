using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class GlitchHackButton : TownOfUsRoleButton<GlitchRole, PlayerControl>, IAftermathablePlayerButton
{
    public override string Name => TouLocale.GetParsed("TouRoleGlitchHack", "Hack");
    public override BaseKeybind Keybind => Keybinds.TertiaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Glitch;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<GlitchOptions>.Instance.HackCooldown + MapCooldown, 5f, 120f);
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
            Error("Glitch Hack: Target is null");
            return;
        }

        var notif1 = Helpers.CreateAndShowNotification(
            TouLocale.GetParsed("TouRoleGlitchHackNotif").Replace("<player>", $"{TownOfUsColors.Glitch.ToTextColor()}{Target.Data.PlayerName}</color>"),
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Glitch.LoadAsset());
        notif1.AdjustNotification();

        TouAudio.PlaySound(TouAudio.HackedSound);
        Target.RpcAddModifier<GlitchHackedModifier>(PlayerControl.LocalPlayer.PlayerId);
    }
}