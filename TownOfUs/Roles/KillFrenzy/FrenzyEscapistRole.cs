using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules.Anims;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.KillFrenzy;

public sealed class FrenzyEscapistRole(IntPtr cppPtr)
    : FrenzyRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    [HideFromIl2Cpp] public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.KillFrenzy;

    public bool CanSpawnOnCurrentMode() => MiscUtils.CurrentGamemode() is TouGamemode.KillFrenzy;

    [HideFromIl2Cpp]
    Func<bool> ICustomRole.VisibleInSettings => () => MiscUtils.CurrentGamemode() is TouGamemode.KillFrenzy;
    public bool WinConditionMet()
    {
        var wwCount = CustomRoleUtils.GetActiveRolesOfType<FrenzyEscapistRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > wwCount)
        {
            return false;
        }

        return wwCount >= Helpers.GetAlivePlayers().Count - wwCount;
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
    [HideFromIl2Cpp] public Vector2? MarkedLocation { get; set; }
    [HideFromIl2Cpp] public GameObject? EscapeMark { get; set; }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not FrenzyEscapistRole || Player.HasDied())
        {
            return;
        }

        if (EscapeMark != null)
        {
            EscapeMark.SetActive(Player.AmOwner || PlayerControl.LocalPlayer.HasDied());
            if (MarkedLocation == null)
            {
                EscapeMark.gameObject.Destroy();
                EscapeMark = null;
            }
        }
    }

    public string LocaleKey => "Escapist";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.FrenzyKiller;

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<FrenzyGhostRole>(),
        FreeplayFolder = "Kill Frenzy",
        Icon = TouRoleIcons.Escapist,
        IntroSound = TouAudio.TimeLordIntroSound,
        OptionsScreenshot = TouBanners.EscapistRoleBanner,
        CanUseVent = false,
    };



    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Mark", "Mark"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}MarkWikiDescription"),
                    TouImpAssets.MarkSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Recall", "Recall"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}RecallWikiDescription"),
                    TouImpAssets.RecallSprite)
            };
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
        EscapeMark?.gameObject.Destroy();
    }

    [MethodRpc((uint)TownOfUsRpc.FrenzyRecall)]
    public static void RpcRecall(PlayerControl player)
    {
        if (player.Data.Role is not FrenzyEscapistRole)
        {
            Error("RpcRecall - Invalid escapist");
            return;
        }
    }

    [MethodRpc((uint)TownOfUsRpc.FrenzyMarkLocation)]
    public static void RpcMarkLocation(PlayerControl player, Vector2 pos)
    {
        if (player.Data.Role is not FrenzyEscapistRole henry)
        {
            Error("RpcRecall - Invalid escapist");
            return;
        }

        henry.MarkedLocation = pos;
        henry.EscapeMark = AnimStore.SpawnAnimAtPlayer(player, TouAssets.EscapistMarkPrefab.LoadAsset());
        henry.EscapeMark.transform.localPosition = new Vector3(pos.x, pos.y + 0.3f, 0.1f);
        henry.EscapeMark.SetActive(false);
    }
}