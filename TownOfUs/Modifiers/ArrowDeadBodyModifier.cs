using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using Reactor.Utilities.Extensions;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers;

[MiraIgnore]
public abstract class ArrowDeadBodyModifier(DeadBody deadBody, Color color, float updateInterval) : BaseModifier
{
    private readonly float _updateInterval = updateInterval;

    private ArrowBehaviour? _arrow;
    private DateTime _time = DateTime.UnixEpoch;
    public override string ModifierName => "DeadBody Arrow";
    public override bool Unique => false;
    public override bool HideOnUi => true;
    public DeadBody DeadBody { get; set; } = deadBody;

    //public override string GetHudString()
    //{
    //    var player = MiscUtils.PlayerById(DeadBody.ParentId);

    //    return ModifierName + $"\nOwner: {Player.Data.PlayerName}\nTarget: {player.Data.PlayerName}</color>";
    //}

    public override void OnActivate()
    {
        _arrow = MiscUtils.CreateArrow(Player.transform, color);
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
        if (DeadBody == null || DeadBody.gameObject == null || !DeadBody.gameObject.activeSelf)
        {
            ModifierComponent!.RemoveModifier(this);
            return;
        }

        if (_updateInterval <= 0 || _time <= DateTime.UtcNow.AddSeconds(-_updateInterval))
        {
            if (_arrow != null)
            {
                _arrow.target = DeadBody.transform.position;
                _arrow.Update();
            }

            _time = DateTime.UtcNow;
        }
    }
}