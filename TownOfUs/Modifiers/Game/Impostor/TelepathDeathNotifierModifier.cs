using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Modifiers.Impostor;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class TelepathDeathNotifierModifier(PlayerControl telepath) : TimedModifier
{
    private ArrowBehaviour? _arrow;
    public override string ModifierName => "Death Notifier";
    public override float Duration => OptionGroupSingleton<TelepathOptions>.Instance.TelepathArrowDuration.Value;
    public override bool HideOnUi => true;
    public PlayerControl Telepath { get; set; } = telepath;

    public override void OnActivate()
    {
        base.OnActivate();

        var deadPlayer = GameData.Instance.AllPlayers.ToArray()
            .FirstOrDefault(x => x.PlayerId == Player.PlayerId && x.IsDead);
        if (deadPlayer == null)
        {
            return;
        }

        _arrow = MiscUtils.CreateArrow(Telepath.transform, Color.white);
        _arrow.target = deadPlayer.Object.GetTruePosition();
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