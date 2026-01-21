using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class SoulCollectorReapButton : TownOfUsKillRoleButton<SoulCollectorRole, PlayerControl>, IDiseaseableButton,
    IKillButton
{
    public override string Name => TouLocale.GetParsed("TouRoleSoulCollectorReap", "Reap");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.SoulCollector;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<SoulCollectorOptions>.Instance.KillCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.ReapSprite;

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
            Error("Soul Collector Reap: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting, createDeadBody:false);

        if (Target.Data.IsDead)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                TouLocale.GetParsed("TouRoleSoulCollectorReapNotif").Replace("<player>", $"{TownOfUsColors.SoulCollector.ToTextColor()}{Target.Data.PlayerName}</color>"),
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.SoulCollector.LoadAsset());

            notif1.AdjustNotification();
        }
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}