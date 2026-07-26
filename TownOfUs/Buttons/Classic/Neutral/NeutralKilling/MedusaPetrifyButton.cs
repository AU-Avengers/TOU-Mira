using MiraAPI.GameOptions;
using MiraAPI.Networking;
using Reactor.Utilities;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Classic.Neutral.NeutralKilling;

public sealed class MedusaPetrifyButton : TownOfUsKillRoleButton<MedusaRole, PlayerControl>, IDiseaseableButton,
    IKillButton
{
    public override string Name => TouLocale.GetParsed("TouRoleMedusaPetrify", "Petrify");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Medusa;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<MedusaOptions>.Instance.KillCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.PetrifySprite;

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

        PlayerControl.LocalPlayer.RpcSpecialMurder(Target, MeetingCheck.OutsideMeeting, createDeadBody: false, causeOfDeath: "Medusa");
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}