using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Impostor;

namespace TownOfUs.Modifiers.Impostor;

public sealed class BootleggerPoisonModifier(PlayerControl bootlegger) : TimedModifier
{
    public override float Duration => OptionGroupSingleton<BootleggerOptions>.Instance.ForcedPoisonDelay.Value;
    public override bool AutoStart => false;
    public override string ModifierName => "Poison";
    public override bool HideOnUi => true;
    public PoisonProgress Poison = PoisonProgress.Begun;
    public bool HasReceivedSickMsg;
    public PlayerControl Bootlegger = bootlegger;

    public override void OnMeetingStart()
    {
        if (Player.HasDied())
        {
            return;
        }

        if (Poison == PoisonProgress.Sick && !HasReceivedSickMsg)
        {
            HasReceivedSickMsg = true;
            var title = $"<color=#{TownOfUsColors.ImpSoft.ToHtmlStringRGBA()}>{TouLocale.GetParsed("TouRoleBootleggerMessageTitle")}</color>";
            if (Player.AmOwner)
            {
                var msg = TouLocale.GetParsed("TouRoleBootleggerSickenFeedbackAffected");
                MiscUtils.AddFakeChat(Player.Data, title, msg, false, true);
            }
            else if (Bootlegger && Bootlegger.AmOwner)
            {
                var msg = TouLocale.GetParsed("TouRoleBootleggerSickenFeedbackBootlegger").Replace("<player>", Player.Data.PlayerName);
                MiscUtils.AddFakeChat(Player.Data, title, msg, false, true);
            }
        }
    }

    public override void OnDeactivate()
    {
        if (!Player.HasDied() && PlayerControl.LocalPlayer.IsHost())
        {
            Error($"{Player.CachedPlayerData.PlayerName} is dying to poison after custom duration completed.");
            if (MeetingHud.Instance || ExileController.Instance)
            {
                Bootlegger.RpcMeetingMurder(Player, MeetingAnimation.PlayerNameplateAnimation, CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate),
                    didSucceed: !Player.HasModifier<InvulnerabilityModifier>(), causeOfDeath: "Poison");
            }
            else
            {
                Bootlegger.RpcSpecialMurder(Player, MeetingCheck.OutsideMeeting, true, true, resetKillTimer: false, teleportMurderer: false, causeOfDeath: "Poison");
            }
        }
        base.OnDeactivate();
    }
}

public enum PoisonProgress
{
    Begun,
    Sick,
    Poison,
    Immune
}