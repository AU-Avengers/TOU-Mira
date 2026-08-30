using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class EscapistRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    [HideFromIl2Cpp] public Vector2? MarkedLocation { get; set; }
    [HideFromIl2Cpp] public GameObject EscapeMark { get; set; }

    public void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not EscapistRole || Player.HasDied())
        {
            return;
        }

        if (EscapeMark)
        {
            EscapeMark.SetActive(PlayerControl.LocalPlayer.IsImpostorAligned() || (PlayerControl.LocalPlayer.HasDied() &&
                                                                            OptionGroupSingleton<GeneralOptions>
                                                                                .Instance.TheDeadKnow));
            if (MarkedLocation == null)
            {
                EscapeMark.Destroy();
                EscapeMark = null!;
            }
        }
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TransporterRole>());
    public DoomableType DoomHintType => DoomableType.Protective;
    public string IdPart => "Escapist";
    public string RoleName => MiraLocaleManager.Get($"TouRole{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TouRole{IdPart}IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TouRole{IdPart}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouRole{IdPart}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Escapist.LoadAsset(), "TouMira.Role.Impostor.Escapist", 1.45f),
        Icon = TouRoleIcons.Escapist,
        IntroSound = TouAudio.TimeLordIntroSound,
        OptionsScreenshot = TouBanners.EscapistRoleBanner,
        CanUseVent = OptionGroupSingleton<EscapistOptions>.Instance.CanVent
    };



    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TouRole{IdPart}Mark", "Mark"),
                    MiraLocaleManager.Get($"TouRole{IdPart}MarkWikiDescription"),
                    TouImpAssets.MarkSprite),
                new(MiraLocaleManager.Get($"TouRole{IdPart}Recall", "Recall"),
                    MiraLocaleManager.Get($"TouRole{IdPart}RecallWikiDescription"),
                    TouImpAssets.RecallSprite)
            ];
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
        EscapeMark?.gameObject.DeepDestroy();
    }

    [MethodRpc((uint)TownOfUsRpc.Recall)]
    public static void RpcRecall(PlayerControl player)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (player.Data.Role is not EscapistRole)
        {
            Error("RpcRecall - Invalid escapist");
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.EscapistRecall, player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    [MethodRpc((uint)TownOfUsRpc.MarkLocation)]
    public static void RpcMarkLocation(PlayerControl player, Vector2 pos)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (player.Data.Role is not EscapistRole henry)
        {
            Error("RpcRecall - Invalid escapist");
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.EscapistMark, player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        henry.MarkedLocation = pos;
        henry.EscapeMark = AnimStore.SpawnAnimAtPlayer(player, TouAssets.EscapistMarkPrefab.LoadAsset());
        henry.EscapeMark.transform.localPosition = new Vector3(pos.x, pos.y + 0.3f, 0.1f);
        henry.EscapeMark.SetActive(false);
    }
}