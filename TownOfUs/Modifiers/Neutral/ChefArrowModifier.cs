using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Neutral;

public sealed class ChefArrowModifier(DeadBody deadBody, Color color) : TimedModifier
{
    private ArrowBehaviour? _arrow;
    public override string ModifierName => "Death Notifier";
    public override float Duration => OptionGroupSingleton<ChefOptions>.Instance.ChefArrowDuration.Value;
    public override bool AutoStart => false;
    public override bool RemoveOnComplete => true;
    public override bool HideOnUi => true;
    public DeadBody DeadBody { get; set; } = deadBody;

    public override void OnActivate()
    {
        base.OnActivate();
        if (OptionGroupSingleton<ChefOptions>.Instance.ChefArrowDuration.Value > 0f)
        {
            StartTimer();
        }

        _arrow = MiscUtils.CreateArrow(Player.transform, color);
        _arrow.target = DeadBody.transform.position;

        Coroutines.Start(MiscUtils.CoFlash(color));
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (DeadBody == null)
        {
            ModifierComponent!.RemoveModifier(this);
            return;
        }

        if (_arrow != null)
        {
            _arrow.target = DeadBody.transform.position;
            _arrow.Update();
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        if (!_arrow.IsDestroyedOrNull())
        {
            _arrow?.gameObject.Destroy();
            _arrow?.Destroy();
        }
    }
}