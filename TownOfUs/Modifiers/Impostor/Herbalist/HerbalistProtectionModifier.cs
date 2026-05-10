using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using PowerTools;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor.Herbalist;

public sealed class HerbalistProtectionModifier(PlayerControl herbalist) : BaseShieldModifier
{
    public override string ModifierName => "Barrier";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Cleric;
    public override string ShieldDescription => "You are shielded by a Cleric!\nNo one can interact with you.";
    public override float Duration => OptionGroupSingleton<HerbalistOptions>.Instance.ProtectDuration;
    public override bool AutoStart => true;
    public bool ShowBarrier { get; set; }

    public override bool HideOnUi
    {
        get
        {
            return !LocalSettingsTabSingleton<TownOfUsLocalRoleSettings>.Instance.ShowShieldHudToggle.Value ||
                   !OptionGroupSingleton<HerbalistOptions>.Instance.ShowBarrier;
        }
    }

    public override bool VisibleSymbol
    {
        get
        {
            var showBarrierSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId && OptionGroupSingleton<HerbalistOptions>.Instance.ShowBarrier;
            return showBarrierSelf;
        }
    }

    public PlayerControl Herbalist { get; } = herbalist;
    public GameObject ClericBarrier { get; set; }


    public override void OnActivate()
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        var showBarrierSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId && OptionGroupSingleton<HerbalistOptions>.Instance.ShowBarrier;

        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x =>
            x.ParentId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);
        var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x =>
            x.PlayerId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);

        ShowBarrier = showBarrierSelf || Herbalist.AmOwner ||
                      (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body);

        ClericBarrier =
            AnimStore.SpawnAnimBody(Player, TouAssets.ClericBarrier.LoadAsset(), false, -1.1f, -0.35f, 1.5f)!;
        ClericBarrier.GetComponent<SpriteAnim>().SetSpeed(2f);
    }

    public override void Update()
    {
        if (!Player || Herbalist == null)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (!MeetingHud.Instance && ClericBarrier)
        {
            ClericBarrier.SetActive(!Player.IsConcealed() && IsVisible && ShowBarrier);
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
        if (ClericBarrier)
        {
            ClericBarrier.Destroy();
        }
    }
}