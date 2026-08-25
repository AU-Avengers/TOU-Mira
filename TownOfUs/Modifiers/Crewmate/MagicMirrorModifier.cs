using MiraAPI.Events;
using MiraAPI.GameOptions;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class MagicMirrorModifier(PlayerControl mirrorcaster) : BaseShieldModifier
{
    public override string ModifierName => TouLocale.Get("TouModifierMagicMirror");
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Mirrorcaster;

    public override string ShieldDescription =>
        TouLocale.Get("TouModifierMagicMirrorDescription");

    public PlayerControl Mirrorcaster { get; } = mirrorcaster;
    public GameObject MedicShield { get; set; }
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
            var fakePlayer = !TutorialManager.InstanceExists ? MiscUtils.GetFakePlayer(PlayerControl.LocalPlayer.PlayerId) : null;

        ShowShield = Mirrorcaster.AmOwner ||
                     (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body);

        MedicShield = AnimStore.SpawnAnimBody(Player, TouAssets.MagicMirror.LoadAsset(), false, -1.1f, -0.1f, 1.5f)!;
    }

    public override void OnDeactivate()
    {
        if (MedicShield)
        {
            MedicShield.Destroy();
        }
    }

    public override void Update()
    {
        if (!Player || Mirrorcaster == null)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (!MeetingHud.Instance && MedicShield)
        {
            MedicShield.SetActive(!Player.IsConcealed() && IsVisible && ShowShield);
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