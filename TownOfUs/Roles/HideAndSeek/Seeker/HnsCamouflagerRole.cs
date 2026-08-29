using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameModes;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.HideAndSeek.Seeker;

public sealed class HnsCamouflagerRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public static PlayerBodyTypes HiderBodyType = PlayerBodyTypes.Normal;
    public static PlayerBodyTypes SeekerBodyType = PlayerBodyTypes.Seeker;
    public string IdPart => "Camouflager";
    public string RoleName => MiraLocaleManager.Get($"HnsRole{IdPart}");
    public string RoleDescription => "...";
    public string RoleLongDescription => MiraLocaleManager.Get($"HnsRole{IdPart}TabDescription");
    public string RoleHintText => MiraLocaleManager.Get($"HnsRole{IdPart}TabHint");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"HnsRole{IdPart}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"HnsRole{IdPart}Camo", "Camo"),
                    MiraLocaleManager.Get($"HnsRole{IdPart}CamoWikiDescription"),
                    TouImpAssets.HypnotiseButtonSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSeeker;

    public CustomRoleConfiguration Configuration => new(this)
    {
        AssociatedGameMode = typeof(HideAndSeekMode),
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Hypnotist.LoadAsset(), "TouMira.Role.Impostor.Hypnotist", 1.45f),
        /*HideSettings = MiscUtils.CurrentGamemode() is not TouGamemode.HideAndSeek,*/
        FreeplayFolder = "Hide n Seek",
        Icon = TouRoleIcons.Hypnotist,
        RoleHintType = RoleHintType.TaskHint
    };

    public override void AppendTaskHint(Il2CppSystem.Text.StringBuilder taskStringBuilder)
    {
        taskStringBuilder.AppendLine($"\n{RoleHintText}\n{RoleLongDescription}");
    }

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        // ignore
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        Coroutines.Start(CoSetUpBodyType());
    }

    [HideFromIl2Cpp]
    public IEnumerator CoSetUpBodyType()
    {
        yield return new WaitForSeconds(7f);
        SeekerBodyType = GameManager.Instance.GetBodyType(Player);
        var randomHider = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x != Player);
        HiderBodyType = randomHider != null ? GameManager.Instance.GetBodyType(randomHider) : PlayerBodyTypes.Normal;
    }
}