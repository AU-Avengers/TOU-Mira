using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.HnsImpostor;
using TownOfUs.Options.Roles.HnsImpostor;
using TownOfUs.Roles.HnsImpostor;
using UnityEngine;

namespace TownOfUs.Buttons.HnsImpostor;

public sealed class CamouflagerCamoButton : TownOfUsRoleButton<HnsCamouflagerRole>
{
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override string Name => TouLocale.GetParsed("HnsRoleCamouflagerCamo", "Camo");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<HnsCamouflagerOptions>.Instance.CamoCooldown + MapCooldown, 5f, 120f);
    public override float EffectDuration => OptionGroupSingleton<HnsCamouflagerOptions>.Instance.CamoDuration;
    public override int MaxUses => (int)OptionGroupSingleton<HnsCamouflagerOptions>.Instance.MaxCamoUses;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.HypnotiseButtonSprite;

    protected override void OnClick()
    {
        foreach (var player in Helpers.GetAlivePlayers())
        {
            player.RpcAddModifier<HnsGlobalCamouflageModifier>(PlayerControl.LocalPlayer);
        }
    }

    public override void OnEffectEnd()
    {
        // Nothing happens here lol
    }
}