using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using PowerTools;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class WardenFortifiedModifier(PlayerControl warden) : BaseShieldModifier
{
    public override string ModifierName => "Fortified";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Warden;
    public override string ShieldDescription => "You are fortified by a Warden!\nNo one can interact with you.";
    public GameObject? WardenFort { get; set; }
    public bool ShowFort { get; set; }

    public override bool HideOnUi
    {
        get
        {
            var showFort = OptionGroupSingleton<WardenOptions>.Instance.ShowFortified;
            return !LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ShowShieldHudToggle.Value ||
                   showFort is FortifyOptions.Warden;
        }
    }

    public override bool VisibleSymbol
    {
        get
        {
            var show = OptionGroupSingleton<WardenOptions>.Instance.ShowFortified;
            var showShieldedEveryone = show == FortifyOptions.Everyone;
            var showShieldedSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                                   show is FortifyOptions.Self or FortifyOptions.SelfAndWarden;
            return showShieldedSelf || showShieldedEveryone;
        }
    }

    public PlayerControl Warden { get; } = warden;

    public override void OnActivate()
    {
        base.OnActivate();
        var touAbilityEvent = new TouAbilityEvent(AbilityType.WardenFortify, Warden, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var show = OptionGroupSingleton<WardenOptions>.Instance.ShowFortified;

        var showShieldedEveryone = show == FortifyOptions.Everyone;
        var showShieldedSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                               show is FortifyOptions.Self or FortifyOptions.SelfAndWarden;
        var showShieldedWarden = PlayerControl.LocalPlayer.PlayerId == Warden.PlayerId &&
                                 show is FortifyOptions.Warden or FortifyOptions.SelfAndWarden;

        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x =>
            x.ParentId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);
        var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x =>
            x.PlayerId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);

        ShowFort = showShieldedEveryone || showShieldedSelf || showShieldedWarden ||
                   (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body);

        WardenFort = AnimStore.SpawnAnimBody(Player, TouAssets.WardenFort.LoadAsset(), false, -1.1f, -0.35f, 1.5f)!;
        WardenFort.GetComponent<SpriteAnim>().SetSpeed(0.75f);
        WardenFort.GetComponentsInChildren<SpriteAnim>().FirstOrDefault()?.SetSpeed(0.75f);
    }

    public override void OnDeactivate()
    {
        if (WardenFort?.gameObject != null)
        {
            WardenFort.gameObject.Destroy();
        }
    }

    public override void Update()
    {
        if (Player == null || Warden == null)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (!MeetingHud.Instance && WardenFort?.gameObject != null)
        {
            WardenFort?.SetActive(!Player.IsConcealed() && IsVisible && ShowFort);
        }
    }
}