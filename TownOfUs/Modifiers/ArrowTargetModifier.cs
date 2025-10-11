using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using Reactor.Utilities.Extensions;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers;

[MiraIgnore]
public abstract class ArrowTargetModifier(PlayerControl owner, Color color, float updateInterval) : BaseModifier
{
    private readonly float _updateInterval = updateInterval;

    private ArrowBehaviour? _arrow;
    private DateTime _time = DateTime.UnixEpoch;
    public ArrowBehaviour? Arrow => _arrow;
    public override string ModifierName => "Arrow Target";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public PlayerControl Owner { get; set; } = owner;

    //public override string GetHudString()
    //{
    //    return ModifierName + $"\nOwner: {Owner.Data.PlayerName}\nTarget: {Player.Data.PlayerName}</color>";
    //}

    public override void OnActivate()
    {
        _arrow = MiscUtils.CreateArrow(Owner.transform, color);
    }

    public override void OnDeath(DeathReason reason)
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

    public override void FixedUpdate()
    {
        if (_updateInterval <= 0 || _time <= DateTime.UtcNow.AddSeconds(-_updateInterval))
        {
            if (_arrow != null)
            {
                _arrow.target = Player.transform.position;
                _arrow.Update();
            }

            _time = DateTime.UtcNow;
        }

        if (Player == null)
        {
            ModifierComponent!.RemoveModifier(this);
            return;
        }
    }
}