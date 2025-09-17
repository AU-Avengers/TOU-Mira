using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class PoliticianCampaignButton : TownOfUsRoleButton<PoliticianRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRolePlumberFlush", "Flush");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override float Cooldown => OptionGroupSingleton<PoliticianOptions>.Instance.CampaignCooldown + MapCooldown;
    public override Color TextOutlineColor => TownOfUsColors.Politician;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.CampaignButtonSprite;

    public override bool CanUse()
    {
        return base.CanUse() && Role is { CanCampaign: true };
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: x => !x.HasModifier<PoliticianCampaignedModifier>());
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Target?.RpcAddModifier<PoliticianCampaignedModifier>(PlayerControl.LocalPlayer);
    }
}