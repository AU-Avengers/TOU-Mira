using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class MedicShieldModifier(PlayerControl medic) : BaseShieldModifier
{
    public override string ModifierName => TouLocale.Get("TouMedicShield", "Medic");
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Medic;

    public override string ShieldDescription =>
        $"You are shielded by a {TouLocale.Get("TouRoleMedic", "Medic")} !\nYou may not die to other players";

    public PlayerControl Medic { get; private set; } = medic;
    public List<PlayerControl> AllMedics { get; } = new();
    public GameObject MedicShield { get; set; }
    public bool ShowShield { get; set; }

    public void ShiftNextMedic(PlayerControl? shielded)
    {
        var count = AllMedics.Count;
        if (count > 1)
        {
            var currentIndex = AllMedics.IndexOf(shielded ?? Medic);
            if (currentIndex + 1 < count)
            {
                SetNewMedic(AllMedics[currentIndex + 1]);
            }
            else
            {
                SetNewMedic(AllMedics[0]);
            }
        }
    }
    public void SetNewMedic(PlayerControl newMedic)
    {
        Medic = newMedic;
        if (!AllMedics.Contains(newMedic))
        {
            AllMedics.Add(newMedic);
        }
    }

    public void RemoveMedic(PlayerControl med)
    {
        AllMedics.Remove(med);
        if (Medic == med && AllMedics.HasAny())
        {
            SetNewMedic(AllMedics[0]);
        }
    }

    public override bool HideOnUi
    {
        get
        {
            var showShielded = OptionGroupSingleton<MedicOptions>.Instance.ShowShielded;
            return !LocalSettingsTabSingleton<TownOfUsLocalRoleSettings>.Instance.ShowShieldHudToggle.Value ||
                   (showShielded is MedicOption.Medic or MedicOption.Nobody);
        }
    }

    public override bool VisibleSymbol
    {
        get
        {
            var showShielded = OptionGroupSingleton<MedicOptions>.Instance.ShowShielded;
            var showShieldedEveryone = showShielded == MedicOption.Everyone;
            var showShieldedSelf = Player.AmOwner &&
                                   showShielded is MedicOption.Shielded or MedicOption.ShieldedAndMedic;
            return showShieldedSelf || showShieldedEveryone;
        }
    }

    public override void OnActivate()
    {
        var touAbilityEvent = new TouAbilityEvent(AbilityType.MedicShield, Medic, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
        AllMedics.Add(Medic);

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var showShielded = OptionGroupSingleton<MedicOptions>.Instance.ShowShielded;

        var showShieldedEveryone = showShielded == MedicOption.Everyone;
        var showShieldedSelf = Player.AmOwner &&
                               showShielded is MedicOption.Shielded or MedicOption.ShieldedAndMedic;
        var showShieldedMedic = AllMedics.Contains(PlayerControl.LocalPlayer) &&
                                showShielded is MedicOption.Medic or MedicOption.ShieldedAndMedic;

        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x =>
            x.ParentId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);
        var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x =>
            x.PlayerId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);

        ShowShield = showShieldedEveryone || showShieldedSelf || showShieldedMedic ||
                     (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body);

        MedicShield = AnimStore.SpawnAnimBody(Player, TouAssets.MedicShield.LoadAsset(), false, -1.1f, -0.1f, 1.5f)!;
    }

    public override void OnDeactivate()
    {
        if (MedicShield)
        {
            MedicShield.Destroy();
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!Player || !AllMedics.HasAny())
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (!MeetingHud.Instance && MedicShield)
        {
            MedicShield.SetActive(!Player.IsConcealed() && IsVisible && ShowShield);
        }
    }
}