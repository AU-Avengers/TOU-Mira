using System.Collections;
using Il2CppInterop.Runtime;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class OfficerShootButton : TownOfUsKillRoleButton<OfficerRole, PlayerControl>, IKillButton
{
    public override string Name => TouLocale.GetParsed("TouRoleOfficerShoot", "Shoot");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Officer;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<OfficerOptions>.Instance.ShootCooldown.Value + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.OfficerShootSprite;

    public override bool ZeroIsInfinite { get; set; } = true;

    public int RoundsBeforeReset { get; set; }
    public bool FailedShot => RoundsBeforeReset > 0;
    public int TotalBullets { get; set; } = -1;
    public int LoadedBullets { get; set; }

    public static bool Usable =>
        OptionGroupSingleton<OfficerOptions>.Instance.FirstRoundShooting || TutorialManager.InstanceExists || DeathEventHandlers.CurrentRound > 1;
    public static int MaxLoadedBullets => (int)OptionGroupSingleton<OfficerOptions>.Instance.MaxBulletsAtOnce;

    public override bool CanUse()
    {
        return base.CanUse() && Usable && !FailedShot && LoadedBullets > 0;
    }

    public void UpdateUses()
    {
        Button!.usesRemainingText.gameObject.SetActive(true);
        Button.usesRemainingSprite.gameObject.SetActive(true);
        Button.usesRemainingText.text = $"{LoadedBullets}/{MaxLoadedBullets}";
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        UpdateUses();
    }

    private void Misfire()
    {
        if (Target == null)
        {
            Error("Misfire: Target is null");
            return;
        }

        OfficerRole.RpcOfficerMisfire(PlayerControl.LocalPlayer);
        PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);

        var notif1 = Helpers.CreateAndShowNotification($"<b>{TouLocale.GetParsed("TouRoleOfficerBadKillFeedback")}</b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Officer.LoadAsset());

        notif1.AdjustNotification();

        Coroutines.Start(MiscUtils.CoFlash(Color.red));
    }

    private void Shoot()
    {
        if (Target == null)
        {
            Error("Shoot: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
        LoadedBullets--;
        OfficerRole.RpcOfficerSyncBullets(PlayerControl.LocalPlayer, RoundsBeforeReset, TotalBullets, LoadedBullets);
    }

    private static IEnumerator CoSetBodyReportable(byte bodyId)
    {
        var waitDelegate =
            DelegateSupport.ConvertDelegate<Il2CppSystem.Func<bool>>(() => Helpers.GetBodyById(bodyId) != null);
        yield return new WaitUntil(waitDelegate);
        var body = Helpers.GetBodyById(bodyId);

        if (body != null)
        {
            body.gameObject.layer = LayerMask.NameToLayer("Ship");
            body.Reported = true;
        }
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Sheriff Shoot: Target is null");
            return;
        }

        if (Target.HasModifier<FirstDeadShield>())
        {
            return;
        }

        if (Target.HasModifier<BaseShieldModifier>())
        {
            return;
        }

        var options = OptionGroupSingleton<OfficerOptions>.Instance;
        var alignment = Target.Data.Role.GetRoleAlignment();
        var hasKilled = GameHistory.PlayerStats.TryGetValue(Target.PlayerId, out var stats) &&
                        (stats.CorrectAssassinKills > 0 || stats.CorrectKills > 0 || stats.IncorrectKills > 0) ||
                        GameHistory.KilledPlayers.Any(x =>
                            x.KillerId == Target.PlayerId && x.VictimId != Target.PlayerId);
        var evilOfficer = (PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) &&
                            !allyMod.GetsPunished);

        if (options.CanOnlyShootActiveKillers.Value)
        {
            if (!evilOfficer && Target.IsCrewmate() && options.CrewKillingAreInnocent.Value || !hasKilled)
            {
                Misfire();
            }
            else
            {
                Shoot();
            }
        }
        else if (!(Target.TryGetModifier<AllianceGameModifier>(out var allyMod2) && !allyMod2.GetsPunished))
        {
            var safeNeutral = options.NonKillingNeutralsAreInnocent.Value &&
                              alignment is RoleAlignment.NeutralBenign
                                  or RoleAlignment.NeutralEvil or RoleAlignment.NeutralOutlier;
            if (safeNeutral || !evilOfficer && Target.IsCrewmate())
            {
                Misfire();
            }
            else
            {
                Shoot();
            }
        }
        else
        {
            Shoot();
        }

        if (!OptionGroupSingleton<OfficerOptions>.Instance.CanSelfReport.Value)
        {
            Coroutines.Start(CoSetBodyReportable(Target.PlayerId));
        }
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