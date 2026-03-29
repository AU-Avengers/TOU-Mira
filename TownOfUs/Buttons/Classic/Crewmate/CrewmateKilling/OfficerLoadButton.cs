using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Events;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class OfficerLoadButton : TownOfUsRoleButton<OfficerRole>
{
    public override string Name => TouLocale.GetParsed("TouRoleOfficerLoad", "Load");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Officer;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<OfficerOptions>.Instance.LoadCooldown.Value + MapCooldown, 5f, 120f);
    public override int MaxUses => (int)OptionGroupSingleton<OfficerOptions>.Instance.MaxBulletsTotal;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.OfficerLoadSprite;
    public static OfficerShootButton ShootButton => CustomButtonSingleton<OfficerShootButton>.Instance;
    public static int MaxLoadedBullets => (int)OptionGroupSingleton<OfficerOptions>.Instance.MaxBulletsAtOnce;
    public bool RecentlyLoadedBullet;

    public static bool Usable =>
        OptionGroupSingleton<OfficerOptions>.Instance.FirstRoundShooting || TutorialManager.InstanceExists || DeathEventHandlers.CurrentRound > 1;

    public override bool CanUse()
    {
        return base.CanUse() && Usable && !ShootButton.FailedShot && ShootButton.LoadedBullets < MaxLoadedBullets;
    }

    protected override void OnClick()
    {
        ShootButton.TotalBullets--;
        ShootButton.LoadedBullets++;
        OfficerRole.RpcOfficerSyncBullets(PlayerControl.LocalPlayer, ShootButton.RoundsBeforeReset, ShootButton.TotalBullets, ShootButton.LoadedBullets);
        Coroutines.Start(CoWaitForFreshBullets());
    }

    public IEnumerator CoWaitForFreshBullets()
    {
        RecentlyLoadedBullet = true;
        yield return new WaitForSeconds(10f);
        RecentlyLoadedBullet = false;
    }
}