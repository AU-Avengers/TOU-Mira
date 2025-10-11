using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class MagicMirrorModifier(PlayerControl mirrorcaster) : BaseShieldModifier
{
    public override string ModifierName => $"Magic Mirror";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Mirrorcaster;

    public override string ShieldDescription =>
        $"You are protected by the Mirrorcaster!\nYou may not die to other players";

    public PlayerControl Mirrorcaster { get; } = mirrorcaster;
    public GameObject? MedicShield { get; set; }
    public bool ShowShield { get; set; }

    public override bool HideOnUi => true;

    public override bool VisibleSymbol => Mirrorcaster.AmOwner;

    public override void OnActivate()
    {
        var touAbilityEvent = new TouAbilityEvent(AbilityType.MagicMirror, Mirrorcaster, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x =>
            x.ParentId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);
        var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x =>
            x.PlayerId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);

        ShowShield = Mirrorcaster.AmOwner ||
                     (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body);

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
        if (Player == null || Mirrorcaster == null)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (!MeetingHud.Instance && MedicShield?.gameObject != null)
        {
            MedicShield?.SetActive(!Player.IsConcealed() && IsVisible && ShowShield);
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
}