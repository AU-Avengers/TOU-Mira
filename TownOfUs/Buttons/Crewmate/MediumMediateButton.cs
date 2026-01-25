using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Crewmate;
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
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<MediumOptions>.Instance.MediateCooldown + MapCooldown, 0.001f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.MediateSprite;

    public override bool ZeroIsInfinite { get; set; } = true;

    protected override void OnClick()
    {
        var deadPlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(plr => plr.Data.IsDead && !plr.Data.Disconnected &&
                          Object.FindObjectsOfType<DeadBody>().Any(x => x.ParentId == plr.PlayerId)
                          && !plr.HasModifier<MediatedModifier>()).ToList();

        if (deadPlayers.Count == 0)
        {
            return;
        }

        var targets = OptionGroupSingleton<MediumOptions>.Instance.WhoIsRevealed switch
        {
            MediateRevealedTargets.NewestDead => [deadPlayers[0]],
            MediateRevealedTargets.AllDead => deadPlayers,
            MediateRevealedTargets.OldestDead => [deadPlayers[^1]],
            MediateRevealedTargets.RandomDead => deadPlayers.Randomize(),
            _ => []
        };

        foreach (var plr in targets)
        {
            MediumRole.RpcMediate(PlayerControl.LocalPlayer, plr);
        }
    }
}