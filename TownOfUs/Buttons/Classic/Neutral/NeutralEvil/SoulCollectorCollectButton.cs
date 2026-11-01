using MiraAPI.Hud;
using MiraAPI.Utilities;
using TownOfUs.Modules.Components;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class SoulCollectorCollectButton : TownOfUsRoleButton<SoulCollectorRole, SoulCollectibleBody>, ILegacyCapable
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.SoulCollectorCollect", "Collect");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.SoulCollector;
    public override float Cooldown => Math.Clamp(MapCooldown, 25f, 120f);
    public override bool ZeroIsInfinite { get; set; } = true;
    public override int MaxUses => 3;

    public override ButtonUsesMode UsesMode => ButtonUsesMode.PerRound;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyNeutAssets.ReapSprite : TouNeutAssets.CollectSprite;
    public override bool UsableInDeath => true;

    public override SoulCollectibleBody? GetTarget()
    {
        return GetNearestDeadBody(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.MaxReportDistance / 4f);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Target.WasCollected = true;
    }

    public static SoulCollectibleBody? GetNearestDeadBody(PlayerControl playerControl, float radius)
    {
        return GetNearestDeadBodies(playerControl.GetTruePosition(), radius, Helpers.CreateFilter(Constants.NotShipMask)).Find(component => component && !component.WasCollected);
    }
    public static List<SoulCollectibleBody> GetNearestDeadBodies(Vector2 source, float radius, ContactFilter2D filter)
    {
        var results = new Il2CppSystem.Collections.Generic.List<Collider2D>();
        Physics2D.OverlapCircle(source, radius, filter, results);
        return
        [
            .. results.ToArray()
                .Where(collider2D => collider2D.CompareTag("DeadBody"))
                .Select(collider2D => collider2D.GetComponent<SoulCollectibleBody>())
        ];
    }
}