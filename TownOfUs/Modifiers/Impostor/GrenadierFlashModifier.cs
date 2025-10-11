using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using Color = UnityEngine.Color;

namespace TownOfUs.Modifiers.Impostor;

public sealed class GrenadierFlashModifier(PlayerControl grenadier) : DisabledModifier, IDisposable
{
    private readonly Color blindVision = new(0.83f, 0.83f, 0.83f, 1f);
    private readonly Color dimVision = new(0.83f, 0.83f, 0.83f, 0.2f);

    private readonly Color normalVision = new(0.83f, 0.83f, 0.83f, 0f);

    private ScreenFlash? flash;
    public override string ModifierName => "Flashed";
    public override bool HideOnUi => true;
    public override float Duration => OptionGroupSingleton<GrenadierOptions>.Instance.GrenadeDuration + 0.5f;
    public override bool AutoStart => true;
    public override bool CanUseAbilities => true;
    public PlayerControl Grenadier => grenadier;

    public void Dispose()
    {
        flash?.Dispose();
    }

    public override void OnActivate()
    {
        base.OnActivate();
        var touAbilityEvent = new TouAbilityEvent(AbilityType.GrenadierFlash, Grenadier, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        flash = new ScreenFlash();

        if (Player.AmOwner && !Grenadier.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}You were flashed by a Grenadier!</color></b>", Color.white,
                spr: TouRoleIcons.Grenadier.LoadAsset());

            notif1.AdjustNotification();
            notif1.transform.localPosition = new Vector3(0f, 1f, -150f);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!Player.IsImpostor() && PlayerControl.LocalPlayer.IsImpostor())
        {
            if (TimeRemaining <= Duration - 0.5f && TimeRemaining >= 0.5f)
            {
                Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, Color.black);
            }
            else
            {
                Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor,
                    Palette.VisorColor);
            }
        }

        if (PlayerControl.LocalPlayer.PlayerId == Player.PlayerId)
        {
            if (TimeRemaining > Duration - 0.5f)
            {
                var fade = (TimeRemaining - Duration) * -2.0f;

                if (ShouldPlayerBeBlinded(Player))
                {
                    SetFlash(Color.Lerp(normalVision, blindVision, fade));
                }
                else if (ShouldPlayerBeDimmed(Player))
                {
                    SetFlash(Color.Lerp(normalVision, dimVision, fade));
                }
                else
                {
                    SetFlash(normalVision);
                }
            }
            else if (TimeRemaining <= Duration - 0.5f && TimeRemaining >= 0.5f)
            {
                if (ShouldPlayerBeBlinded(Player))
                {
                    SetFlash(blindVision);
                }
                else if (ShouldPlayerBeDimmed(Player))
                {
                    SetFlash(dimVision);
                }
                else
                {
                    SetFlash(normalVision);
                }
            }
            else if (TimeRemaining < 0.5f)
            {
                var fade2 = TimeRemaining * -2.0f + 1.0f;

                if (ShouldPlayerBeBlinded(Player))
                {
                    SetFlash(Color.Lerp(blindVision, normalVision, fade2));
                }
                else if (ShouldPlayerBeDimmed(Player))
                {
                    SetFlash(Color.Lerp(dimVision, normalVision, fade2));
                }
                else
                {
                    SetFlash(normalVision);
                }
            }
            else
            {
                SetFlash(normalVision);

                TimeRemaining = 0.0f;
            }

            if (MeetingHud.Instance)
            {
                SetFlash(normalVision);

                TimeRemaining = 0.0f;
            }
        }
    }

    public override void OnDeactivate()
    {
        if (Player.AmOwner)
        {
            SetFlash(normalVision);

            flash?.Destroy();
        }

        if (!Player.IsImpostor() && PlayerControl.LocalPlayer.IsImpostor())
        {
            Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, Palette.VisorColor);
        }
    }

    public override void OnMeetingStart()
    {
        ModifierComponent?.RemoveModifier(this);
    }

    private void SetFlash(Color color)
    {
        if (flash != null)
        {
            flash.SetColour(color);
            flash.SetActive(true);

            if (color == normalVision)
            {
                flash.SetActive(false);
            }
        }
    }

    private static bool ShouldPlayerBeDimmed(PlayerControl player)
    {
        return (player.IsImpostor() || player.HasDied()) && !MeetingHud.Instance;
    }

    private static bool ShouldPlayerBeBlinded(PlayerControl player)
    {
        return !player.IsImpostor() && !player.HasDied() && !MeetingHud.Instance;
    }
}