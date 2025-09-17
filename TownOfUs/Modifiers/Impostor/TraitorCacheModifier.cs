using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor;

public sealed class TraitorCacheModifier : BaseModifier, ICachedRole
{
    public override string ModifierName => "Traitor";
    public override bool HideOnUi => true;
    public bool ShowCurrentRoleFirst => true;

    public bool Visible => Player.AmOwner || PlayerControl.LocalPlayer.HasDied() ||
                           GuardianAngelTouRole.GASeesRoleVisibilityFlag(Player);

    public RoleBehaviour CachedRole => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TraitorRole>());

    public override void OnActivate()
    {
        if (Player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}You are a new role, and you are only guessable as Traitor now!</color></b>",
                Color.white, spr: TouRoleIcons.Traitor.LoadAsset());

            notif1.Text.SetOutlineThickness(0.35f);
            notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.TraitorChangeRole, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }
}