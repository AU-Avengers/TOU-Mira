using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using TownOfUs.Options.Modifiers.HnsCrewmate;

namespace TownOfUs.Modifiers.HnsGame.Crewmate;

public sealed class HnsFrozenModifier : TimedModifier
{
    public override string ModifierName => "Frozen";
    public override bool HideOnUi => true;
    public override float Duration => OptionGroupSingleton<HnsFrostyOptions>.Instance.ChillDuration;

    private float SpeedCache { get; set; }
    private DateTime ApplicationTime { get; set; }

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }

    public override void OnActivate()
    {
        ApplicationTime = DateTime.UtcNow;
        SpeedCache = Player.MyPhysics.Speed;
        Player.MyPhysics.Speed *= OptionGroupSingleton<HnsFrostyOptions>.Instance.ChillStartSpeed;
    }

    public override void OnDeactivate()
    {
        Player.MyPhysics.Speed = SpeedCache;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        var timeSpan = DateTime.UtcNow - ApplicationTime;
        var duration = Duration * 1000f;
        Player.MyPhysics.Speed = SpeedCache * 1 - (duration - (float)timeSpan.TotalMilliseconds) *
            (1 - OptionGroupSingleton<HnsFrostyOptions>.Instance.ChillStartSpeed) / duration;
    }
}