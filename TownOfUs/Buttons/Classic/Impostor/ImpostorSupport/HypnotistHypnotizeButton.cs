using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class HypnotistHypnotizeButton : TownOfUsRoleButton<HypnotistRole, PlayerControl>,
    IAftermathablePlayerButton
{
    public override string Name => TouLocale.GetParsed("TouRoleHypnotistHypnotize", "Hypnotize");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<HypnotistOptions>.Instance.HypnotiseCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.HypnotiseButtonSprite;

    public override bool ZeroIsInfinite { get; set; } = true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && role is HypnotistRole hypno && !hypno.HysteriaActive;
    }

    public override bool CanUse()
    {
        return base.CanUse() && !Role.HysteriaActive;
    }

    public void AftermathHandler()
    {
        PlayerControl.LocalPlayer.RpcAddModifier<HypnotisedModifier>(PlayerControl.LocalPlayer);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        if (TutorialManager.InstanceExists)
        {
            Coroutines.Start(CoHysteria());
        }
        else
        {
            Target.RpcAddModifier<HypnotisedModifier>(PlayerControl.LocalPlayer);
        }
    }

    public static IEnumerator CoHysteria()
    {
        if (!PlayerControl.LocalPlayer.HasModifier<HypnotisedModifier>())
        {
            PlayerControl.LocalPlayer.RpcAddModifier<HypnotisedModifier>(PlayerControl.LocalPlayer);
        }
        yield return null;
        yield return null;
        if (PlayerControl.LocalPlayer.TryGetModifier<HypnotisedModifier>(out var hystMod))
        {
            if (hystMod.HysteriaActive)
            {
                hystMod.UnHysteria();
            }
            else
            {
                hystMod.Hysteria();
            }
        }
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance, false,
            player => !player.HasModifier<HypnotisedModifier>());
    }
}