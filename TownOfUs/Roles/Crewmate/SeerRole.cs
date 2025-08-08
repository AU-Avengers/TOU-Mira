using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SeerRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => TouLocale.Get(TouNames.Seer, "Seer");
    public string RoleDescription => "Compare The Alignment Of Two Players";
    public string RoleLongDescription => "Compare two players to see if they have matching or conflicting alignments";
    public Color RoleColor => TownOfUsColors.Seer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Seer,
        IntroSound = TouAudio.QuestionSound
    };

    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return $"The {RoleName} is a Crewmate Investigative role that can compare two players to see if they have matching or conflicting alignments.\n\nCrewmates appear enemies to Impostors and Neutrals.\nImpostors appear enemies to Crewmates and Neutrals.\nNeutrals appear enemies to Crewmates and Impostors."
               + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Compare",
            "Compare the alignments of two players.\nIf they don't match, you will be informed of such.</b>",
            TouCrewAssets.SeerSprite),
    ];

    [MethodRpc((uint)TownOfUsRpc.Compare, SendImmediately = true)]
    public static void RpcCompare(PlayerControl seer, byte player1, byte player2)
    {
        if (seer.Data.Role is not SeerRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcCompare - Invalid Seer");
            return;
        }

        var t1 = GetTarget(player1);
        var t2 = GetTarget(player2);

        if (t1 == null || t2 == null)
        {
            if (seer.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(Color.red));
                ShowNotification($"<b>You need to pick two targets.</b>");
            }
            return;
        }

        if (t1 == seer || t2 == seer)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));
            ShowNotification($"<b>You can't use yourself to compare!</b>");
            return;
        }
        
        var play1 = MiscUtils.PlayerById(player1)!;
        var play2 = MiscUtils.PlayerById(player2)!;

        if (play1.TryGetModifier<InvulnerabilityModifier>(out var invic) && invic.AttackAllInteractions)
        {
            if (seer.AmOwner) play1.RpcCustomMurder(seer);
            return;
        }

        if (play2.TryGetModifier<InvulnerabilityModifier>(out var invic2) && invic2.AttackAllInteractions)
        {
            if (seer.AmOwner) play2.RpcCustomMurder(seer);
            return;
        }

        if (play1.HasModifier<VeteranAlertModifier>())
        {
            if (seer.AmOwner) play1.RpcCustomMurder(seer);
            return;
        }

        if (play2.HasModifier<VeteranAlertModifier>())
        {
            if (seer.AmOwner) play2.RpcCustomMurder(seer);
            return;
        }

        if (seer.AmOwner)
        {
            var button = CustomButtonSingleton<SeerCompareButton>.Instance;
            button.ResetCooldownAndOrEffect();
        }


        var playerA = play1.CachedPlayerData.PlayerName;
        var playerB = play2.CachedPlayerData.PlayerName;

        void ShowNotification(string message)
        {
            if (seer.AmOwner)
            {
                var notif = Helpers.CreateAndShowNotification(
                    message, Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Seer.LoadAsset());
                notif.Text.SetOutlineThickness(0.35f);
            }
        }

        bool enemies = Enemies(play1, play2);
        bool Enemies(PlayerControl p1, PlayerControl p2)
        {
            if (p1 == null || p2 == null) return false;
            if (p1.Data?.Role == null || p2.Data?.Role == null) return false;

            var friendlyNB = OptionGroupSingleton<SeerOptions>.Instance.BenignShowFriendlyToAll;
            var friendlyNE = OptionGroupSingleton<SeerOptions>.Instance.EvilShowFriendlyToAll;

            if (p1.IsCrewmate() && p2.IsCrewmate()) return false;
            if (p1.IsImpostor() && p2.IsImpostor()) return false;
            if (p1.Is(RoleAlignment.NeutralBenign) && p2.Is(RoleAlignment.NeutralBenign)) return false;
            if (p1.Is(RoleAlignment.NeutralEvil) && p2.Is(RoleAlignment.NeutralEvil)) return false;
            if (p1.Is(RoleAlignment.NeutralOutlier) && p2.Is(RoleAlignment.NeutralOutlier)) return false;

            if (p1.Is(RoleAlignment.NeutralBenign) || p2.Is(RoleAlignment.NeutralBenign))
                return !friendlyNB;
            if (p1.Is(RoleAlignment.NeutralEvil) || p2.Is(RoleAlignment.NeutralEvil))
                return !friendlyNE;

            // You sense that Atony and Cursed Soul appear to be enemies!
            return true;
        }


        if (enemies)
        {
            Coroutines.Start(MiscUtils.CoFlash(Palette.ImpostorRed));
            ShowNotification($"<b>{Palette.ImpostorRed.ToTextColor()}{playerA} and {playerB} seem to have conflicting alignments!</color></b>");
        }
        else
        {
            Coroutines.Start(MiscUtils.CoFlash(Palette.CrewmateBlue));
            ShowNotification($"<b>{Palette.CrewmateBlue.ToTextColor()}{playerA} and {playerB} seem to have matching alignments!</color></b>");
        }

        static MonoBehaviour? GetTarget(byte id)
        {
            var data = GameData.Instance.GetPlayerById(id);
            if (!data)
            {
                return null;
            }

            var body = Helpers.GetBodyById(id);
            if (data.IsDead && body)
            {
                return body;
            }

            var pc = data.Object;
            if (!pc)
            {
                return null;
            }

            return pc;
        }
    }
}