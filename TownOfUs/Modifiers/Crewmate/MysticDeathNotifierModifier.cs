using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

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

        if (OptionGroupSingleton<MysticOptions>.Instance.MysticHnsPopUp.Value)
        {
            var popup = GameManagerCreator.Instance.HideAndSeekManagerPrefab.DeathPopupPrefab;
            var item = Object.Instantiate(popup, HudManager.Instance.transform.parent);
            item.Show(Player, 0);
        }

        _arrow = MiscUtils.CreateArrow(Mystic.transform, Color.white);
        _arrow.target = Player.GetTruePosition();

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