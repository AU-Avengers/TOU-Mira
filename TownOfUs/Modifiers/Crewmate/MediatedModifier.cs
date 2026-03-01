using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers.Types;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class MediatedModifier(byte mediumId) : TimedModifier
{
    private ArrowBehaviour? _arrow;

    private MediumRole? _medium;
    private PlayerControl? _mediumPlayer;
    public override string ModifierName => "Mediated";
    public override bool HideOnUi => true;
    public byte MediumId { get; } = mediumId;
    public override float Duration => OptionGroupSingleton<MediumOptions>.Instance.MediateDuration.Value + 1f;

    public override void OnMeetingStart()
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnActivate()
    {
        _medium = GameData.Instance.GetPlayerById(MediumId).Role as MediumRole;
        _mediumPlayer = _medium?.Player;

        if (_mediumPlayer == null || _medium == null || !Player.Data.IsDead)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.MediumMediate, _mediumPlayer, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        _medium.MediatedPlayers.Add(this);

        switch ((MediumVisibility)OptionGroupSingleton<MediumOptions>.Instance.ArrowVisibility.Value)
        {
            case MediumVisibility.Both:
                var ownerTransform = Player.AmOwner ? _mediumPlayer.transform : Player.transform;
                _arrow = MiscUtils.CreateArrow(ownerTransform, TownOfUsColors.Medium);
                break;

            case MediumVisibility.ShowMedium when Player.AmOwner:
                _arrow = MiscUtils.CreateArrow(_mediumPlayer.transform, TownOfUsColors.Medium);
                break;

            case MediumVisibility.ShowMediate when _mediumPlayer.AmOwner:
                _arrow = MiscUtils.CreateArrow(Player.transform, TownOfUsColors.Medium);
                break;
        }

        var hidden =
            (AppearanceVisibility)OptionGroupSingleton<MediumOptions>.Instance.PlayerVisibility.Value is
            AppearanceVisibility.None or AppearanceVisibility.Living;
        if (_mediumPlayer.AmOwner && hidden)
        {
            Player.SetCamouflage();
        }

        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Medium, alpha: 0.5f));
    }

    public override void OnDeactivate()
    {
        if (_mediumPlayer == null)
        {
            return;
        }

        if (_medium != null)
        {
            _medium.MediatedPlayers.Remove(this);
        }

        if (_mediumPlayer.AmOwner)
        {
            CustomButtonSingleton<MediumMediateButton>.Instance.SetTimerPaused(false);
            CustomButtonSingleton<MediumMediateButton>.Instance.ResetCooldownAndOrEffect();

            var hidden =
                (AppearanceVisibility)OptionGroupSingleton<MediumOptions>.Instance.PlayerVisibility.Value is
                AppearanceVisibility.None or AppearanceVisibility.Living;

            if (hidden)
            {
                Player.SetCamouflage(false);
            }
        }

        if (_arrow != null)
        {
            _arrow.gameObject.Destroy();
        }
    }

    public override void FixedUpdate()
    {
        if (!Player.Data.IsDead)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (_mediumPlayer != null && _mediumPlayer.AmOwner)
        {
            Player.Visible = true;
        }

        if (_arrow != null && _arrow.target != _arrow.transform.parent.position)
        {
            _arrow.target = _arrow.transform.parent.position;
        }

        base.FixedUpdate();
    }
}
