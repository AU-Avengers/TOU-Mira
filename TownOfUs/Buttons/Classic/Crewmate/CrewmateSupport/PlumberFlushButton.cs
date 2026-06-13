using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class PlumberFlushButton : TownOfUsVentRoleButton<PlumberRole>, ILegacyCapable
{
    public override string Name => TouLocale.GetParsed("TouRolePlumberFlush", "Flush");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Plumber;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<PlumberOptions>.Instance.FlushCooldown + MapCooldown, 5f, 120f);
    public override float EffectDuration => PlayerControl.AllPlayerControls.ToArray().Any(x => x.inVent) ? OptionGroupSingleton<PlumberOptions>.Instance.FlushDuration : 0.001f;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyCrewAssets.FlushSprite : TouCrewAssets.FlushSprite;

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error($"{Name}: Target is null");
            return;
        }

        PlumberRole.RpcPlumberFlush(PlayerControl.LocalPlayer);

        var block = CustomButtonSingleton<PlumberBlockButton>.Instance;

        block?.SetTimer(block.Cooldown);
    }
}