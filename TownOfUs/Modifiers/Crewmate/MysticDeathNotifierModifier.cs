using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class MysticDeathNotifierModifier(PlayerControl mystic) : TimedModifier
{
    private static readonly Color FlashColor = Palette.CrewmateRoleBlue;

    private ArrowBehaviour? _arrow;
    public override string ModifierName => "Death Notifier";
    public override float Duration => OptionGroupSingleton<MysticOptions>.Instance.MysticArrowDuration;
    public override bool HideOnUi => true;
    public PlayerControl Mystic { get; set; } = mystic;

    public override void OnActivate()
    {
        base.OnActivate();

        var deadPlayer = GameData.Instance.AllPlayers.ToArray()
            .FirstOrDefault(x => x.PlayerId == Player.PlayerId && x.IsDead);
        if (deadPlayer == null)
        {
            return;
        }

        _arrow = MiscUtils.CreateArrow(Mystic.transform, Color.white);
        _arrow.target = deadPlayer.Object.GetTruePosition();

        Coroutines.Start(MiscUtils.CoFlash(FlashColor));
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