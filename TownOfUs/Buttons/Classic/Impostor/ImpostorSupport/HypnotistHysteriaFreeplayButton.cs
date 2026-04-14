using System.Collections;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class HypnotistHysteriaFreeplayButton : TownOfUsRoleButton<HypnotistRole>
{
    public override string Name => TouLocale.GetParsed("TouRoleHypnotistMassHysteria", "Hysteria");
    public override BaseKeybind Keybind => Keybinds.TertiaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 3f;
    public override float InitialCooldown =>3f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.HysteriaCleanSprite;

    public override bool ZeroIsInfinite { get; set; } = true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && TutorialManager.InstanceExists;
    }

    protected override void OnClick()
    {
        Coroutines.Start(CoHysteria());
    }

    public static IEnumerator CoHysteria()
    {
        if (!PlayerControl.LocalPlayer.HasModifier<HypnotisedModifier>())
        {
            PlayerControl.LocalPlayer.AddModifier<HypnotisedModifier>(PlayerControl.LocalPlayer);
        }
        yield return new WaitForSeconds(0.02f);
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
}