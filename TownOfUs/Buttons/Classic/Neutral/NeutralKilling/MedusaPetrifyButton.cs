using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class MedusaPetrifyButton : TownOfUsKillRoleButton<MedusaRole, PlayerControl>, IDiseaseableButton,
    IKillButton, ILegacyCapable
{
    public override string Name => TouLocale.GetParsed("TouRoleMedusaPetrify", "Reap");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Medusa;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<MedusaOptions>.Instance.KillCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyNeutAssets.ReapSprite : TouNeutAssets.ReapSprite;

    public override bool UsableFirstRound => OptionGroupSingleton<MedusaOptions>.Instance.FirstRound;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(this, false));
    }

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting, createDeadBody:false);

        if (Target.Data.IsDead)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                TouLocale.GetParsed("TouRoleMedusaPetrifyNotif").Replace("<player>", $"{TownOfUsColors.Medusa.ToTextColor()}{Target.Data.PlayerName}</color>"),
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Medusa.LoadAsset());

            notif1.AdjustNotification();
        }
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}