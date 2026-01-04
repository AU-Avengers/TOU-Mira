using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using TownOfUs.Modules.Anims;
using UnityEngine;

namespace TownOfUs.Modifiers;

/// <summary>
/// Visual-only version of the First Death Shield, used when a player is morphed/mimicked as someone who has
/// <see cref="FirstDeadShield"/>
/// </summary>
public sealed class FirstDeadShieldDisguiseVisual(PlayerControl target) : TimedModifier, IAnimated
{
    public override string ModifierName => "First Death Shield (Disguise)";
    public override bool HideOnUi => true;
    public override float Duration => float.MaxValue;
    public override bool AutoStart => true;
    public override bool RemoveOnComplete => false;

     public bool IsVisible { get; set; } = true;

    public PlayerControl Target { get; } = target;

    private GameObject? _shield;

    public void SetVisible()
    {
    }

    public override void OnActivate()
    {
        _shield = AnimStore.SpawnAnimBody(Player, TouAssets.FirstRoundShield.LoadAsset(), false, -1.1f, -0.225f, 1.5f)!;
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
         if (_shield != null)
        {
             UnityEngine.Object.Destroy(_shield);
        }
    }

    public override void Update()
    {
        if (Player == null || Target == null)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (!MeetingHud.Instance && _shield?.gameObject != null)
        {
             // Show only while the target *actually* has the shield.
             // Morph/Mimic are implemented as ConcealedModifier, but they are still visible to others.
             // Only hide the shield for "true conceal" (e.g. swoop/invis), vents, disabled, etc.
             var trulyConcealed =
                 Player.GetModifiers<ConcealedModifier>().Any(x => !x.VisibleToOthers) ||
                 !Player.Visible ||
                 (Player.TryGetModifier<DisabledModifier>(out var disabled) && !disabled.IsConsideredAlive) ||
                 Player.inVent;

             _shield.SetActive(!trulyConcealed && IsVisible && Target.HasModifier<FirstDeadShield>());
        }
        else if (MeetingHud.Instance)
        {
            if (_shield?.gameObject != null)
            {
                _shield.SetActive(false);
            }

            ModifierComponent?.RemoveModifier(this);
        }
    }
}


