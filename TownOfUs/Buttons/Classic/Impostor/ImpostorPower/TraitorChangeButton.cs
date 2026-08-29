using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class TraitorChangeButton : TownOfUsRoleButton<TraitorRole>
{
    public override string Name => MiraLocaleManager.Get("TouRoleTraitorChangeRole", "Change Role");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.01f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.TraitorSelect;
    public bool NoRolesAvailable;
    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && !NoRolesAvailable;
    }

    public override bool ZeroIsInfinite { get; set; } = true;

    public override void ClickHandler()
    {
        if (!CanClick() || Minigame.Instance || PlayerControl.LocalPlayer.HasDied())
        {
            return;
        }

        OnClick();
    }

    protected override void OnClick()
    {
        if (Role.ChosenRoles.Count == 0)
        {
            var excluded = MiscUtils.AllRegisteredRoles
                .Where(x => x is ISpawnChange { NoSpawn: true } || x.Role is RoleTypes.Impostor || x.IsDead || x is ITownOfUsRole
                {
                    RoleAlignment: RoleAlignment.ImpostorPower
                }).Select(x => x.Role).ToList();
            var impRoles = MiscUtils.GetRolesToAssign(ModdedRoleTeams.Impostor, x => !excluded.Contains(x.Role))
                .Select(x => x.RoleType).ToList();

            var roleList = MiscUtils.GetPotentialRoles()
                .Where(role => role is not ITraitorIgnore ignore || !ignore.IsIgnored)
                .Where(role => impRoles.Contains((ushort)role.Role))
                .Where(role => role is not TraitorRole)
                .ToList();

            if (TutorialManager.InstanceExists)
            {
                impRoles = MiscUtils.GetRegisteredRoles(ModdedRoleTeams.Impostor)
                    .Where(x => !excluded.Contains(x.Role))
                    .Select(x => (ushort)x.Role).ToList();
                roleList = MiscUtils.AllRegisteredRoles
                    .Where(role => role is not ITraitorIgnore ignore || !ignore.IsIgnored)
                    .Where(role => impRoles.Contains((ushort)role.Role))
                    .Where(role => role is not TraitorRole)
                    .ToList();
            }

            if (OptionGroupSingleton<TraitorOptions>.Instance.RemoveExistingRoles)
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.IsImpostor() && !player.AmOwner)
                    {
                        var role = player.GetRoleWhenAlive();
                        if (role)
                        {
                            impRoles.Remove((ushort)role!.Role);
                        }
                    }
                }
            }

            roleList.Shuffle();
            roleList.Shuffle();
            var random = roleList.Random();
            roleList.Shuffle();

            for (var i = 0; i < 3; i++)
            {
                var selected = roleList.Random();
                if (selected == null)
                {
                    continue;
                }

                Role.ChosenRoles.Add(selected);
                roleList.Remove(selected);
            }

            Role.RandomRole = random;
        }

        if (!Minigame.Instance)
        {
            if (Role.ChosenRoles.Count == 0)
            {
                NoRolesAvailable = true;
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}No roles are available for the taking.</color></b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Traitor.LoadAsset());

                notif1.AdjustNotification();
                return;
            }
            var traitorMenu = TraitorSelectionMinigame.Create();
            traitorMenu.Open(
                Role.ChosenRoles,
                role =>
                {
                    Role.SelectedRole = role;
                    Role.UpdateRole();
                    traitorMenu.Close();
                },
                Role.RandomRole?.Role
            );
        }
    }
}