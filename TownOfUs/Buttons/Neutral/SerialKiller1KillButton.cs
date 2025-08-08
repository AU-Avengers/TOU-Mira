using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class SerialKiller1KillButton : TownOfUsRoleButton<SerialKillerRole, PlayerControl>, IDiseaseableButton,
    IKillButton
{
    public override string Name => "Kill";
    public override string Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.SerialKiller;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.SerialKillerKillSprite;
    public override float Cooldown => OptionGroupSingleton<SerialKillerOptions>.Instance.KillCooldown + MapCooldown;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("SerialKiller Shoot: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
        PlayerControl.LocalPlayer.AddModifier<SerialKillerBloodlustModifier>();
        if (PlayerControl.LocalPlayer.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.SerialKiller));

            var notif = Helpers.CreateAndShowNotification(
                $"<b>Your bloodlust is ready. You have {OptionGroupSingleton<SerialKillerOptions>.Instance.BloodlustDuration}s to kill another player!</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.SerialKiller.LoadAsset());
            notif.Text.SetOutlineThickness(0.35f);
        }


    }



    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}