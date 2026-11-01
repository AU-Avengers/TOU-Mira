using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class SoulCollectorReapButton : TownOfUsRoleButton<SoulCollectorRole, PlayerControl>, ILegacyCapable
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.SoulCollectorReap", "Reap");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.SoulCollector;
    public override float Cooldown => Math.Clamp(MapCooldown, 25f, 120f);
    public override float EffectDuration => 1f;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override int MaxUses => 3;

    public override ButtonUsesMode UsesMode => ButtonUsesMode.PerRound;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyNeutAssets.ReapSprite : TouNeutAssets.ReapSprite;
    public override bool UsableInDeath => true;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: x => !x.HasModifier<ReapedModifier>());
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }
        var newTargets = Helpers.GetClosestPlayers(PlayerControl.LocalPlayer,
                1 * ShipStatus.Instance.MaxLightRadius).Select(x => new KeyValuePair<byte, string>(x.PlayerId, x.Data.PlayerName))
            .ToDictionary(x => x.Key, x => x.Value);

        SoulCollectorRole.RpcReapPlayers(PlayerControl.LocalPlayer, newTargets);
    }

    public override void OnEffectEnd()
    {
        // Nothing!
    }
}