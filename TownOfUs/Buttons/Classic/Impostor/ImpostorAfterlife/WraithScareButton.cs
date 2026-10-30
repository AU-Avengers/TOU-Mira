using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class WraithScareButton : TownOfUsRoleButton<WraithRole, PlayerControl>
{
    public override string Name => TouLocale.GetParsed("TouRoleWraithScare", "Scare");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<WraithOptions>.Instance.ScareCooldown.Value;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.PhantomSpookSprite;
    public override int MaxUses => (int)OptionGroupSingleton<WraithOptions>.Instance.ScareUses.Value;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool ShouldPauseInVent => false;
    public override bool UsableInDeath => true;

    protected override void OnClick()
    {
        // ignored
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance);
    }
}