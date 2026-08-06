using Reactor.Utilities.Extensions;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.TownOfPolus.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.TownOfPolus.Crewmate;

public sealed class PolusEngineerFixButton : TownOfUsRoleButton<PolusEngineerRole>, ILegacyButton
{
    public override string Name => string.Empty;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.PolusEngineer;
    public override float Cooldown => Math.Clamp(MapCooldown, 0.01f, 120f);
    public override int MaxUses => 1;
    public override LoadableAsset<Sprite> Sprite => PolusGgAssets.ButtonFix;
    public override bool ShouldPauseInVent => false;

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        Button?.cooldownTimerText.gameObject.SetActive(false);
    }
    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
    }

    public override bool CanUse()
    {
        var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();

        return base.CanUse() && system is { AnyActive: true };
    }

    protected override void OnClick()
    {
        var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();

        if (system is not { AnyActive: true })
        {
            ResetCooldownAndOrEffect();
        }

        if (system is { AnyActive: true })
        {
            List<LoadableAsset<AudioClip>> audio = [TouAudio.EngiFix1, TouAudio.EngiFix2, TouAudio.EngiFix3];
            TouAudio.PlaySound(audio.Random()!, 4f);
            EngineerTouRole.EngineerFix(PlayerControl.LocalPlayer);

            if (LimitedUses)
            {
                UsesLeft--;
                Button?.SetUsesRemaining(UsesLeft);
                TownOfUsColors.UseBasic = false;
                if (TextOutlineColor != Color.clear)
                {
                    SetTextOutline(TextOutlineColor);
                    Button?.usesRemainingSprite.color = TextOutlineColor;
                }
            }
        }
    }
}