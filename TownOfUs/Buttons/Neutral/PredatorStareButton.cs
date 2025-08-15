using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class PredatorStareButton : TownOfUsRoleButton<PredatorRole, PlayerControl>
{
    public override string Name => "Stare";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Predator;
    public override float Cooldown => OptionGroupSingleton<PredatorOptions>.Instance.PredatorStareCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<PredatorOptions>.Instance.PredatorStareDuration;
    public override int MaxUses => (int)OptionGroupSingleton<PredatorOptions>.Instance.StareUses;
    // Using Hunter Stalk Button texture as a placeholder
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.StalkButtonSprite;
    public int ExtraUses { get; set; }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Stare: Target is null");
            return;
        }

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>If {Target.Data.PlayerName} uses an ability, you will be able to kill them instantly.</b>",
            // Using Hunter Role Icon texture as a placeholder
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Hunter.LoadAsset());
        notif1.Text.SetOutlineThickness(0.35f);

        Target.RpcAddModifier<PredatorStaringModifier>(PlayerControl.LocalPlayer);
        OverrideName("Staring");
    }

    public override void OnEffectEnd()
    {
        OverrideName("Stare");
    }

    public override PlayerControl? GetTarget()
    {
        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsLover());
        }
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}