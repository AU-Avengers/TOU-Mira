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
    public override string Name => TouLocale.GetParsed("TouRolePoliticianCampaign", "Campaign");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<PoliticianOptions>.Instance.CampaignCooldown + MapCooldown, 5f, 120f);
    public override Color TextOutlineColor => TownOfUsColors.Politician;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.CampaignButtonSprite;

    public override bool CanUse()
    {
        return base.CanUse() && Role is { CanCampaign: true };
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: x => !x.HasModifier<PoliticianCampaignedModifier>(y => y.Politician.AmOwner));
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