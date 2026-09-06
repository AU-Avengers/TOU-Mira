using MiraAPI.Events;
using MiraAPI.GameOptions;
using PowerTools;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ClericBarrierModifier(PlayerControl cleric) : BaseShieldModifier
{
    public override string ModifierName => MiraLocaleManager.Get("TownOfUsMira.Modifier.ClericBarrier");
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Cleric;
    public override string ShieldDescription => MiraLocaleManager.Get("TownOfUsMira.Modifier.ClericBarrierDescription");
    public override float Duration => OptionGroupSingleton<ClericOptions>.Instance.BarrierDuration;
    public override bool AutoStart => true;
    public bool ShowBarrier { get; set; }
    public override bool HideOnUi
    {
        get
        {
            return !LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.ShowShieldHudToggle.Value ||
                   !OptionGroupSingleton<ClericOptions>.Instance.ShowBarrier;
        }
    }

    public override bool VisibleSymbol
    {
        get
        {
            var showBarrierSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId && OptionGroupSingleton<ClericOptions>.Instance.ShowBarrier;
            return showBarrierSelf;
        }
    }

    public PlayerControl Cleric { get; } = cleric;
    public GameObject ClericBarrier { get; set; }


    public override void OnActivate()
    {
        var touAbilityEvent = new TouAbilityEvent(AbilityType.ClericBarrier, Cleric, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        var showBarrierSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId && OptionGroupSingleton<ClericOptions>.Instance.ShowBarrier;

        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x =>
            x.ParentId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);
        var fakePlayer = !TutorialManager.InstanceExists ? MiscUtils.GetFakePlayer(PlayerControl.LocalPlayer.PlayerId) : null;

        ShowBarrier = showBarrierSelf || PlayerControl.LocalPlayer.PlayerId == Cleric.PlayerId ||
                      (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body);

        ClericBarrier =
            AnimStore.SpawnAnimBody(Player, TouAssets.ClericBarrier.LoadAsset(), false, -1.1f, -0.35f, 1.5f)!;
        ClericBarrier.GetComponent<SpriteAnim>().SetSpeed(2f);
    }

    public override void Update()
    {
        if (!Player || Cleric == null)
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