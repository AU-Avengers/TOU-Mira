using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Components;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class SpellslingerHexBombButton : TownOfUsRoleButton<SpellslingerRole>
{
    private bool _bombed;
    public override string Name => TouLocale.Get("TouRoleSpellslingerHexBomb", "Hex Bomb");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.HexBombSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        _bombed = false;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && SpellslingerRole.EveryoneHexed();
    }

    public override bool CanClick()
    {
        return base.CanClick() && !_bombed;
    }

    protected override void OnClick()
    {
        if (ShipStatus.Instance.Systems.ContainsKey(SystemTypes.LifeSupp))
        {
            var lifeSuppSystemType = ShipStatus.Instance.Systems[SystemTypes.LifeSupp].Cast<LifeSuppSystemType>();
            if (lifeSuppSystemType != null)
            {
                lifeSuppSystemType.Countdown = 10000f;
            }
        }

        foreach (var systemType2 in ShipStatus.Instance.Systems.Values)
        {
            var sabo = systemType2.TryCast<ICriticalSabotage>();
            if (sabo == null)
            {
                continue;
            }

            sabo.ClearSabotage();
        }
        
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, HexBombSabotageSystem.SabotageId);

        _bombed = true;
    }
}
