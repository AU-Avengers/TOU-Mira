using AmongUs.GameOptions;
using Il2CppSystem.Text;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.GameModes;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Roles.TownOfPolus;

public class PolusGhostImpRole(IntPtr cppPtr) : ImpostorGhostRole(cppPtr), ITownOfUsRole
{
    RoleOptionsGroup ICustomRole.RoleOptionsGroup => TouRoleGroups.TownOfPolusImpostor;
    public string IdPrefix => "TownOfUsMira.TownOfPolus.Role";
    public virtual string IdPart => "Impostor";
    public virtual string RoleName => Player != null ? Player.GetRoleWhenAlive().GetRoleName() : MiraLocaleManager.Get("ImpostorKeyword");
    public virtual string RoleDescription => Player != null ? Player.GetRoleWhenAlive().Blurb : MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.ImpDescriptionDead");

    public virtual string RoleLongDescription
    {
        get
        {
            if (Player == null)
            {
                return MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.ImpDescriptionDead");
            }

            var role = Player.GetRoleWhenAlive();
            return role is PolusBaseImpRole polusRole ? polusRole.RoleDescriptionDead : role.BlurbLong;
        }
    }
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }

        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        var text =
            $"{RoleColor.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.TabText").Replace("<roleName>", RoleName).Replace("<description>", "<color=#FF0000>" + RoleLongDescription + "</color>")}</color>" +
            "\n<color=#FFFFFF>" + MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.FakeTaskTabText") + "</color>";
        orCreateTask.Text = text;
        orCreateTask.name = "TownOfUsMira.TownOfPolus.Role.Text";
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }

    public virtual Color RoleColor => Player != null ? Player.GetRoleWhenAlive().TeamColor : Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.Impostor;

    public virtual CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.None,
        HideSettings = true,
        MaxRoleCount = 0,
        DefaultRoleCount = 0,
        DefaultChance = 0,
        CanModifyChance = false,
        ShowInFreeplay = false,
        CanUseVent = false,
        AssociatedGameMode = typeof(TownOfPolusMode),
        FreeplayFolder = "Town of Polus",
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(PolusGgAssets.IconImpostor.LoadAsset(), "TownOfPolus.Role.Impostor.Impostor", 1.45f),
        Icon = PolusGgAssets.IconImpostor
    };
    public override bool IsDead => true; // needed because we inherit from RoleBehaviour
    public override bool IsAffectedByComms => false;
    private Minigame _hauntMenu = null!;
    public void Awake()
    {
        var crewGhost = RoleManager.Instance.GetRole(RoleTypes.ImpostorGhost).Cast<ImpostorGhostRole>();
        _hauntMenu = crewGhost.HauntMenu;
        Ability = crewGhost.Ability;
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console2 = usable.TryCast<Console>()!;
        return console2 == null || console2.AllowImpostor;
    }

    // reimplement haunt minigame
    public override void UseAbility()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening)
        {
            return;
        }

        if (Minigame.Instance)
        {
            if (Minigame.Instance.TryCast<HauntMenuMinigame>())
            {
                Minigame.Instance.Close();
            }

            return;
        }

        var minigame = Instantiate(_hauntMenu, HudManager.Instance.AbilityButton.transform, false);
        minigame.transform.SetLocalZ(-5f);
        minigame.Begin(null);
        HudManager.Instance.AbilityButton.SetDisabled();
    }

    public override void AppendTaskHint(StringBuilder taskStringBuilder)
    {
        // remove default task hint
    }
}