using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class TraitorChangeButton : TownOfUsRoleButton<TraitorRole>
{
    public override string Name => TouLocale.Get("TouRoleTraitorChangeRole", "Change Role");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 1f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.TraitorSelect;

    public override void ClickHandler()
    {
        if (!CanClick() || Minigame.Instance != null || PlayerControl.LocalPlayer.HasDied())
        {
            return;
        }

        OnClick();
    }

    protected override void OnClick()
    {
        if (Role.ChosenRoles.Count == 0)
        {
            var excluded = MiscUtils.AllRoles
                .Where(x => x is ISpawnChange { NoSpawn: true } || x is ITownOfUsRole
                {
                    RoleAlignment: RoleAlignment.ImpostorPower
                }).Select(x => x.Role).ToList();
            var impRoles = MiscUtils.GetRolesToAssign(ModdedRoleTeams.Impostor, x => !excluded.Contains(x.Role))
                .Select(x => x.RoleType).ToList();

            var roleList = MiscUtils.GetPotentialRoles()
                .Where(role => role is ICustomRole)
                .Where(role => role is not ITraitorIgnore ignore || !ignore.IsIgnored)
                .Where(role => impRoles.Contains(RoleId.Get(role.GetType())))
                .Where(role => role is not TraitorRole)
                .ToList();

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

        if (Minigame.Instance == null)
        {
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