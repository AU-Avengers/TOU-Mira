using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class SpellslingerHexButton : TownOfUsRoleButton<SpellslingerRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleSpellslingerHex", "Hex");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<SpellslingerOptions>.Instance.HexCooldown + MapCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<SpellslingerOptions>.Instance.MaxHexes;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.HexSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && !SpellslingerRole.EveryoneHexed();
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance, predicate: x => !x.HasModifier<SpellslingerHexedModifier>());
    }
    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Target.RpcAddModifier<SpellslingerHexedModifier>(PlayerControl.LocalPlayer);

        if (SpellslingerRole.EveryoneHexed())
        {
            CustomButtonSingleton<SpellslingerHexButton>.Instance.SetActive(false, Role);
            CustomButtonSingleton<SpellslingerHexBombButton>.Instance.SetActive(true, Role);
        }
    }
}
