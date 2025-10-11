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

public sealed class ClericBarrierModifier(PlayerControl cleric) : BaseShieldModifier
{
    public override string ModifierName => "Barrier";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Cleric;
    public override string ShieldDescription => "You are shielded by a Cleric!\nNo one can interact with you.";
    public override float Duration => OptionGroupSingleton<ClericOptions>.Instance.BarrierCooldown;
    public override bool AutoStart => true;
    public bool ShowBarrier { get; set; }

    public override bool HideOnUi
    {
        get
        {
            var showBarrier = OptionGroupSingleton<ClericOptions>.Instance.ShowBarriered;
            return !LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ShowShieldHudToggle.Value ||
                   (showBarrier is BarrierOptions.Cleric);
        }
    }

    public override bool VisibleSymbol
    {
        get
        {
            var showBarrier = OptionGroupSingleton<ClericOptions>.Instance.ShowBarriered;
            var showBarrierSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                                  (showBarrier == BarrierOptions.Self || showBarrier == BarrierOptions.SelfAndCleric);
            return showBarrierSelf;
        }
    }

    public PlayerControl Cleric { get; } = cleric;
    public GameObject? ClericBarrier { get; set; }


    public override void OnActivate()
    {
        var touAbilityEvent = new TouAbilityEvent(AbilityType.ClericBarrier, Cleric, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var showBarrier = OptionGroupSingleton<ClericOptions>.Instance.ShowBarriered;

        var showBarrierSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                              (showBarrier == BarrierOptions.Self || showBarrier == BarrierOptions.SelfAndCleric);
        var showBarrierCleric = PlayerControl.LocalPlayer.PlayerId == Cleric.PlayerId &&
                                (showBarrier == BarrierOptions.Cleric || showBarrier == BarrierOptions.SelfAndCleric);

        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x =>
            x.ParentId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);
        var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x =>
            x.PlayerId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);

        ShowBarrier = showBarrierSelf || showBarrierCleric ||
                      (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body);

        ClericBarrier =
            AnimStore.SpawnAnimBody(Player, TouAssets.ClericBarrier.LoadAsset(), false, -1.1f, -0.35f, 1.5f)!;
        ClericBarrier.GetComponent<SpriteAnim>().SetSpeed(2f);
    }

    public override void Update()
    {
        if (Player == null || Cleric == null)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (!MeetingHud.Instance && ClericBarrier?.gameObject != null)
        {
            ClericBarrier?.SetActive(!Player.IsConcealed() && IsVisible && ShowBarrier);
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        if (ClericBarrier?.gameObject != null)
        {
            ClericBarrier.gameObject.Destroy();
        }
    }
}