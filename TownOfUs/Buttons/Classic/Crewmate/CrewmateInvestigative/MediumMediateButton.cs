using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Buttons.Crewmate;

public sealed class MediumMediateButton : TownOfUsRoleButton<MediumRole>
{
    public override string Name => TouLocale.GetParsed("TouRoleMediumMediate", "Mediate");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Medium;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<MediumOptions>.Instance.MediateCooldown.Value + MapCooldown, 0.001f, 120f);
    public override float EffectDuration => OptionGroupSingleton<MediumOptions>.Instance.MediateDuration.Value;

    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.MediateSprite;

    public override bool ZeroIsInfinite { get; set; } = true;

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        if (EffectActive)
        {
            Timer = Cooldown;
            EffectActive = false;
        }
        else if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = Cooldown;
        }
    }

    protected override void OnClick()
    {
        if (EffectActive)
        {
            if (Role.Spirit != null)
            {
                MediumRole.RpcRemoveMediumSpirit(PlayerControl.LocalPlayer, Role.Spirit);
            }
            return;
        }

        var deadPlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(plr => plr.Data.IsDead && !plr.Data.Disconnected &&
                          Object.FindObjectsOfType<DeadBody>().Any(x => x.ParentId == plr.PlayerId)
                          && !plr.HasModifier<MediatedModifier>()).ToList();

        if (deadPlayers.Count == 0)
        {
            MediumRole.RpcMediate(PlayerControl.LocalPlayer);
            return;
        }

        var targets = (MediateRevealedTargets)OptionGroupSingleton<MediumOptions>.Instance.WhoIsRevealed.Value switch
        {
            MediateRevealedTargets.NewestDead => [deadPlayers[0]],
            MediateRevealedTargets.AllDead => deadPlayers,
            MediateRevealedTargets.OldestDead => [deadPlayers[^1]],
            MediateRevealedTargets.RandomDead => deadPlayers.Randomize(),
            _ => []
        };

        MediumRole.RpcMultiMediate(PlayerControl.LocalPlayer, targets);
    }

    public override void OnEffectEnd()
    {
        if (Role.Spirit == null)
        {
            return;
        }
        MediumRole.RpcRemoveMediumSpirit(PlayerControl.LocalPlayer, Role.Spirit);
    }

    public override bool CanUse()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() || PlayerControl.LocalPlayer
                .GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return ((Timer <= 0 && !EffectActive) || (EffectActive && Timer <= EffectDuration - OptionGroupSingleton<MediumOptions>.Instance.LivingSeeSpiritTimer.Value - 1f));
    }
}