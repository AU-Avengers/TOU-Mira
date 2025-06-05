using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class InquisitorInquireButton : TownOfUsRoleButton<InquisitorRole, PlayerControl>
{
    public override string Name => "Inquire";
    public override string Keybind => Keybinds.SecondaryAction;
    public override int MaxUses => (int)OptionGroupSingleton<InquisitorOptions>.Instance.MaxUses;
    public override Color TextOutlineColor => TownOfUsColors.Inquisitor;
    public override float Cooldown => OptionGroupSingleton<InquisitorOptions>.Instance.InquireCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.Observe;

    public override bool Enabled(RoleBehaviour? role) => base.Enabled(role) && !OptionGroupSingleton<InquisitorOptions>.Instance.CantInquire;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, predicate: x => !x.HasModifier<InquisitorInquiredModifier>());
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }
        ModifierUtils.GetPlayersWithModifier<InquisitorInquiredModifier>().Do(x => x.RemoveModifier<InquisitorInquiredModifier>());

        Target.AddModifier<InquisitorInquiredModifier>();
        
        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{TownOfUsColors.Inquisitor.ToTextColor()}You will know if {Target.Data.PlayerName} is a heretic during the next meeting.</color></b>", Color.white, spr: TouRoleIcons.Inquisitor.LoadAsset());

        notif1.Text.SetOutlineThickness(0.35f);
            notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
    }
}
