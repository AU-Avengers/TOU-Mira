using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class MonarchKnightButton : TownOfUsRoleButton<MonarchRole, PlayerControl>
{
    public override string Name => "Knight";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Monarch;
    public override float Cooldown => OptionGroupSingleton<MonarchOptions>.Instance.KnightCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<MonarchOptions>.Instance.KnightDelay;
    public override int MaxUses => (int)OptionGroupSingleton<MonarchOptions>.Instance.MaxKnights;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.KnightSprite;
    public PlayerControl? _knightedTarget;
    private bool _isProcessingClick;

    public bool Usable { get; set; } =
        OptionGroupSingleton<MonarchOptions>.Instance.FirstRoundUse || TutorialManager.InstanceExists;

    public override bool CanUse()
    {
        if (TimeLordRewindSystem.IsRewinding)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasDied() && !Usable)
        {
            return false;
        }

        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (!PlayerControl.LocalPlayer.CanMove ||
            PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        var newTarget = GetTarget();
        if (newTarget != Target)
        {
            SetOutline(false);
        }

        Target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);

        return PlayerControl.LocalPlayer.moveable &&
               (EffectActive || (!EffectActive && Target != null && (!LimitedUses || UsesLeft > 0) && Timer <= 0));
    }

    public override bool CanClick()
    {
        return CanUse();
    }

    public override void ClickHandler()
    {
        if (_isProcessingClick)
        {
            return;
        }

        _isProcessingClick = true;


        try
        {
            if (!CanClick() || PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() ||
                PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
            {
                return;
            }

            OnClick();
        }
        finally
        {
            Coroutines.Start(ResetProcessingFlag());
        }
    }

    private System.Collections.IEnumerator ResetProcessingFlag()
    {
        yield return new WaitForSeconds(0.2f);
        _isProcessingClick = false;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: x => !x.HasModifier<KnightedModifier>());
    }

    protected override void OnClick()
    {
        if (EffectActive)
        {
            var notif2 = Helpers.CreateAndShowNotification(
                $"<b>Knighting has been cancelled.</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Monarch.LoadAsset());
            notif2.Text.SetOutlineThickness(0.35f);
            _knightedTarget = null;
            ResetCooldownAndOrEffect();
            EffectActive = false;
            Timer = 0.001f;
            return;
        }

        if (Target == null)
        {
            return;
        }

        OverrideName("Knighting");

        _knightedTarget = Target;
        var notif = Helpers.CreateAndShowNotification(
            $"<b>You chose to knight {_knightedTarget.CachedPlayerData.PlayerName}. They will be knighted in {OptionGroupSingleton<MonarchOptions>.Instance.KnightDelay} second(s)!</b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Monarch.LoadAsset());
        notif.Text.SetOutlineThickness(0.35f);

        if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = Cooldown;
        }
    }

    public override void OnEffectEnd()
    {
        OverrideName("Knight");

        if (_knightedTarget == null) return;

        if (LimitedUses)
        {
            UsesLeft--;
            Button?.SetUsesRemaining(UsesLeft);
            TownOfUsColors.UseBasic = false;
            if (TextOutlineColor != Color.clear)
            {
                SetTextOutline(TextOutlineColor);
                if (Button != null)
                {
                    Button.usesRemainingSprite.color = TextOutlineColor;
                }
            }

            TownOfUsColors.UseBasic = LocalSettingsTabSingleton<TownOfUsLocalRoleSettings>.Instance
                .UseCrewmateTeamColorToggle.Value;
        }

        MonarchRole.RpcKnight(PlayerControl.LocalPlayer, _knightedTarget);
        _knightedTarget = null;
    }

}
