using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Colors;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MedicRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    private MeetingMenu meetingMenu;
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public PlayerControl? Shielded { get; set; }
    public bool IsProtecting { get; set; }
    public bool ProtectedAttackedThisRound { get; set; }

    public void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not MedicRole)
        {
            return;
        }

        var dced = IsProtecting && Shielded == null;
        if (Shielded != null && Shielded.HasDied() || dced)
        {
            Clear(dced);
        }
    }

    public DoomableType DoomHintType => DoomableType.Protective;
    public string IdPart => "Medic";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Shield", "Shield"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Shield.WikiDescription"),
                    TouCrewAssets.MedicSprite)
            ];
        }
    }

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Shield", "Shield"),
                    MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Shield.WikiDescription"),
                    TouCrewAssets.MedicSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Medic;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Medic.LoadAsset(), "TouMira.Role.Crewmate.Medic", 1.45f),
        IntroSound = TouAudio.ScientistIntroSound,
        OptionsScreenshot = TouBanners.MedicRoleBanner,
        Icon = TouRoleIcons.Medic
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Shielded != null)
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"\n<b>{ProtectionString.Replace("<player>", Shielded.Data.PlayerName)}</b>");
        }

        return stringB;
    }

    public static string ProtectionString = MiraLocaleManager.Get("TownOfUsMira.Role.MedicTabProtecting");

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ProtectionString = MiraLocaleManager.Get("TownOfUsMira.Role.MedicTabProtecting");

        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(
                this,
                (_, _) => { },
                MeetingAbilityType.Click,
                TouAssets.LighterSprite,
                null!,
                IsExempt,
                hoverColor: Color.white)
            {
                Position = new Vector3(1.1f, -0.18f, -3f)
            };
        }
    }
    public static bool IsExempt(PlayerVoteArea voteArea)
    {
        return false;
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        var meeting = MeetingHud.Instance;
        if (Player.AmOwner && meeting != null)
        {
            meetingMenu.GenButtons(meeting,
                true);

            foreach (var button in meetingMenu.Buttons)
            {
                if (button.Value == null)
                {
                    continue;
                }

                button.Value.transform.localScale *= 0.8f;

                var player = MiscUtils.PlayerById(button.Key);

                if (player == null)
                {
                    continue;
                }

                var colorType = GetColorTypeForPlayer(player.Data.DefaultOutfit.ColorId);

                var renderer = button.Value.GetComponent<SpriteRenderer>();

                if (renderer == null)
                {
                    continue;
                }

                renderer.sprite = colorType switch
                {
                    "lighter" => TouAssets.LighterSprite.LoadAsset(),
                    _ => TouAssets.DarkerSprite.LoadAsset()
                };
            }
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner)
        {
            meetingMenu.HideButtons();
        }
    }

    public void Clear(bool playerLeft = false)
    {
        if (playerLeft)
        {
            IsProtecting = false;
            Shielded = null;
            if (Player.AmOwner)
            {
                var button = CustomButtonSingleton<MedicShieldButton>.Instance;
                button.ResetCooldownAndOrEffect();
                button.SetUses(button.UsesLeft + 1);
            }
        }
        SetShieldedPlayer(null);
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        Clear();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        Clear();

        if (Player.AmOwner)
        {
            meetingMenu?.Dispose();
            meetingMenu = null!;
        }
    }

    public void SetShieldedPlayer(PlayerControl? player)
    {
        IsProtecting = false;
        if (Shielded?.TryGetModifier<MedicShieldModifier>(out var mod) == true)
        {
            mod.RemoveMedic(Player);
        }
        Shielded = player;
        if (Shielded != null)
        {
            IsProtecting = true;
            if (Shielded.TryGetModifier<MedicShieldModifier>(out var mod2))
            {
                mod2.SetNewMedic(Player);
            }
            else
            {
                Shielded.AddModifier<MedicShieldModifier>(Player);
            }
        }
    }

    public void Report(byte deadPlayerId)
    {
        var areReportsEnabled = OptionGroupSingleton<MedicOptions>.Instance.ShowReports;

        if (!areReportsEnabled)
        {
            return;
        }

        var matches = GameHistory.KilledPlayers.Where(x => x.VictimId == deadPlayerId).ToArray();

        DeadPlayer? killer = null;

        if (matches.Length > 0)
        {
            killer = matches[0];
        }

        if (killer == null)
        {
            return;
        }

        // Message($"CmdReportDeadBody");
        var br = new BodyReport
        {
            Killer = MiscUtils.PlayerById(killer.KillerId),
            Reporter = Player,
            Body = MiscUtils.PlayerById(killer.VictimId),
            KillAge = (float)(DateTime.UtcNow - killer.KillTime).TotalMilliseconds
        };

        var reportMsg = BodyReport.ParseMedicReport(br);

        if (string.IsNullOrWhiteSpace(reportMsg))
        {
            return;
        }

        var title = $"<color=#{TownOfUsColors.Medic.ToHtmlStringRGBA()}>{MiraLocaleManager.Get("TownOfUsMira.Role.MedicMessageTitle")}</color>";
        var reported = Player;
        if (br.Body != null)
        {
            reported = br.Body;
        }

        MiscUtils.AddFakeChat(reported.Data, title, reportMsg, false, true);
    }

    public static string GetColorTypeForPlayer(int colorId)
    {
        return PaletteManager.GetBrightness(colorId) is CustomColorBrightness.Darker ? "darker" : "lighter";
    }

    public static void DangerAnim()
    {
        Coroutines.Start(MiscUtils.CoFlash(new Color(0f, 0.5f, 0f, 1f)));
    }

    public static void DangerAnimNonMedic()
    {
        Coroutines.Start(MiscUtils.CoFlash(OptionGroupSingleton<GameMechanicOptions>.Instance.AnonymousShields ? TownOfUsColors.NeutralWiki :new Color(0f, 0.5f, 0f, 1f)));
    }

    public void LobbyStart()
    {
        CustomButtonSingleton<MedicShieldButton>.Instance.CanChangeTarget = true;
    }

    [MethodRpc((uint)TownOfUsRpc.MedicShield)]
    public static void RpcMedicShield(PlayerControl medic, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(medic);
            return;
        }
        if (medic.Data.Role is not MedicRole)
        {
            Error("RpcMedicShield - Invalid medic");
            return;
        }

        var role = medic.GetRole<MedicRole>();

        role?.SetShieldedPlayer(target);
    }

    [MethodRpc((uint)TownOfUsRpc.ClearMedicShield)]
    public static void RpcClearMedicShield(PlayerControl medic)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(medic);
            return;
        }
        ClearMedicShield(medic);
    }

    public static void ClearMedicShield(PlayerControl medic)
    {
        if (medic.Data.Role is not MedicRole)
        {
            Error("ClearMedicShield - Invalid medic");
            return;
        }

        var role = medic.GetRole<MedicRole>();

        role?.SetShieldedPlayer(null);
    }

    [MethodRpc((uint)TownOfUsRpc.MedicShieldAttacked)]
    public static void RpcMedicShieldAttacked(PlayerControl source, PlayerControl medic, PlayerControl shielded)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }
        if (medic.Data.Role is not MedicRole role)
        {
            Error("RpcMedicShieldAttacked - Invalid medic");
            return;
        }

        if (PlayerControl.LocalPlayer.PlayerId == source.PlayerId)
        {
            Coroutines.Start(MiscUtils.CoFlash(new Color(0f, 0.5f, 0f, 1f)));
        }

        var shieldNotify = OptionGroupSingleton<MedicOptions>.Instance.WhoGetsNotification;

        if (shielded.AmOwner && shieldNotify == MedicOption.Shielded)
        {
            DangerAnim();
        }

        if (medic.AmOwner && shieldNotify == MedicOption.Medic)
        {
            DangerAnim();
        }

        if (source.AmOwner)
        { 
            DangerAnimNonMedic();
        }

        if (shieldNotify == MedicOption.Everyone && !source.AmOwner)
        {
            DangerAnimNonMedic();
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.MedicShieldProtect, !role.ProtectedAttackedThisRound ? "New Attack" : "Already Attacked", medic, shielded);
        MiraEventManager.InvokeEvent(touAbilityEvent);
        role.ProtectedAttackedThisRound = true;

        var shieldBreaks = OptionGroupSingleton<MedicOptions>.Instance.ShieldBreaks;

        if (shieldBreaks)
        {
            role.SetShieldedPlayer(null);
        }
        else if (shielded.TryGetModifier<MedicShieldModifier>(out var mod))
        {
            mod.ShiftNextMedic(medic);
        }
    }
}