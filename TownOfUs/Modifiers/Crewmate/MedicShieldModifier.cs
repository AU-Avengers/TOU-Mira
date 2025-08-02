using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class MedicShieldModifier(PlayerControl medic) : BaseShieldModifier
{
    public override string ModifierName => $"{TouLocale.Get(TouNames.Medic, "Medic")} Shield";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Medic;
    public override string ShieldDescription => $"You are shielded by a {TouLocale.Get(TouNames.Medic, "Medic")} !\nYou may not die to other players";
    public PlayerControl Medic { get; } = medic;
    public GameObject? MedicShield { get; set; }
    public bool ShowShield { get; set; }

    public override bool HideOnUi
    {
        get
        {
            var showShielded = OptionGroupSingleton<MedicOptions>.Instance.ShowShielded;
            return !TownOfUsPlugin.ShowShieldHud.Value || (showShielded is MedicOption.Medic or MedicOption.Nobody);
        }
    }

    public override bool VisibleSymbol
    {
        get
        {
            var showShielded = OptionGroupSingleton<MedicOptions>.Instance.ShowShielded;
            var showShieldedEveryone = showShielded == MedicOption.Everyone;
            var showShieldedSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                                   showShielded is MedicOption.Shielded or MedicOption.ShieldedAndMedic;
            return showShieldedSelf || showShieldedEveryone;
        }
    }

    public override void OnActivate()
    {
        var touAbilityEvent = new TouAbilityEvent(AbilityType.MedicShield, Medic, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var showShielded = OptionGroupSingleton<MedicOptions>.Instance.ShowShielded;

        var showShieldedEveryone = showShielded == MedicOption.Everyone;
        var showShieldedSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                               showShielded is MedicOption.Shielded or MedicOption.ShieldedAndMedic;
        var showShieldedMedic = PlayerControl.LocalPlayer.PlayerId == Medic.PlayerId &&
                                showShielded is MedicOption.Medic or MedicOption.ShieldedAndMedic;

        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x =>
            x.ParentId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);
        var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x =>
            x.PlayerId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);
        
        ShowShield = showShieldedEveryone || showShieldedSelf || showShieldedMedic || (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body);
        
        MedicShield = AnimStore.SpawnAnimBody(Player, TouAssets.MedicShield.LoadAsset(), false, -1.1f, -0.1f, 1.5f)!;
    }

    public override void OnDeactivate()
    {
        if (MedicShield?.gameObject != null)
        {
            MedicShield.gameObject.Destroy();
        }
    }

    public override void Update()
    {
        if (Player == null || Medic == null)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }
        
        if (!MeetingHud.Instance && MedicShield?.gameObject != null)
        {
            MedicShield?.SetActive(!Player.IsConcealed() && IsVisible && ShowShield);
        }
    }
}