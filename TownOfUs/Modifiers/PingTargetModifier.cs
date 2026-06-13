using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using PowerTools;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace TownOfUs.Modifiers;

[MiraIgnore]
public abstract class PingTargetModifier(PlayerControl owner, Color color, float updateInterval) : TimedModifier
{
    private readonly float _updateInterval = updateInterval;
    public override float Duration => 1f;
    public override bool AutoStart => false;

    private PingBehaviour? _arrow;
    private DateTime _time = DateTime.UnixEpoch;
    public PingBehaviour? Arrow => _arrow;
    public SpriteAnim AnimHandler;
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
        _arrow = MiscUtils.CreatePing(Owner.transform, color);
        AnimHandler = _arrow.gameObject.GetComponent<SpriteAnim>();
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        if (!_arrow.IsDestroyedOrNull())
        {
            _arrow?.gameObject.DeepDestroy();
            _arrow?.Destroy();
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!Player)
        {
            ModifierComponent!.RemoveModifier(this);
            return;
        }

        if (_updateInterval <= 0 || _time <= DateTime.UtcNow.AddSeconds(-_updateInterval))
        {
            if (_arrow != null)
            {
                _arrow.target = Player.transform.position;
                _arrow.Update();
                if (AnimHandler && !AnimHandler.IsPlaying())
                {
                    AnimHandler.Play();
                }
            }

            _time = DateTime.UtcNow;
        }
    }
}