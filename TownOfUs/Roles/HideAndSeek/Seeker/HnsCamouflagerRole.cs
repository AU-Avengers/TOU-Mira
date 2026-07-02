using System.Collections;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.HideAndSeek.Seeker;

public sealed class HnsCamouflagerRole
    : ImpostorRole, ITownOfUsRole, IWikiDiscoverable
{
    public static PlayerBodyTypes HiderBodyType = PlayerBodyTypes.Normal;
    public static PlayerBodyTypes SeekerBodyType = PlayerBodyTypes.Seeker;
    public string LocaleKey => "Camouflager";
    public string RoleName => TouLocale.Get($"HnsRole{LocaleKey}");
    public string RoleDescription => "...";
    public string RoleLongDescription => TouLocale.GetParsed($"HnsRole{LocaleKey}TabDescription");
    public string RoleHintText => TouLocale.GetParsed($"HnsRole{LocaleKey}TabHint");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"HnsRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"HnsRole{LocaleKey}Camo", "Camo"),
                    TouLocale.GetParsed($"HnsRole{LocaleKey}CamoWikiDescription"),
                    TouImpAssets.HypnotiseButtonSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSeeker;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Hypnotist.LoadAsset(), "TouMira.Role.Impostor.Hypnotist", 1.45f),
        /*HideSettings = MiscUtils.CurrentGamemode() is not TouGamemode.HideAndSeek,*/
        FreeplayFolder = "Hide n Seek",
        Icon = TouRoleIcons.Hypnotist,
        RoleHintType = RoleHintType.TaskHint
    };

    public override void AppendTaskHint(System.Text.StringBuilder taskStringBuilder)
    {
        taskStringBuilder.AppendLine($"\n{RoleHintText}\n{RoleLongDescription}");
    }

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        // ignore
    }
 public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.HideAndSeek;

    public bool CanSpawnOnCurrentMode() => MiscUtils.CurrentGamemode() is TouGamemode.HideAndSeek;

    Func<bool> ICustomRole.VisibleInSettings => () => MiscUtils.CurrentGamemode() is TouGamemode.HideAndSeek;

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        AmongUsClient.Instance.StartCoroutine(CoSetUpBodyType());
    }

    public IEnumerator CoSetUpBodyType()
    {
        yield return new WaitForSeconds(7f);
        SeekerBodyType = GameManager.Instance.GetBodyType(Player);
        var randomHider = PlayerControl.AllPlayerControls.FirstOrDefault(x => x != Player);
        HiderBodyType = randomHider != null ? GameManager.Instance.GetBodyType(randomHider) : PlayerBodyTypes.Normal;
    }
}