using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
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
    public string LocaleKey => "Seer";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public static string ReworkString => OptionGroupSingleton<SeerOptions>.Instance.SalemSeer.Value ? "Alt" : string.Empty;
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}{ReworkString}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}{ReworkString}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}{ReworkString}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            var abilityName = TouLocale.GetParsed($"TouRole{LocaleKey}Reveal", "Reveal");
            var abilityDesc = TouLocale.GetParsed($"TouRole{LocaleKey}RevealWikiDescription");
            if (OptionGroupSingleton<SeerOptions>.Instance.SalemSeer.Value)
            {
                abilityName = TouLocale.GetParsed($"TouRole{LocaleKey}Compare", "Compare");
                abilityDesc = TouLocale.GetParsed($"TouRole{LocaleKey}CompareWikiDescription");
            }
            return new List<CustomButtonWikiDescription>
            {
                new(abilityName, abilityDesc, TouCrewAssets.SeerSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Seer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Seer,
        IntroSound = TouAudio.QuestionSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }
    public static void SeerCompare(PlayerControl seer, byte player1, byte player2)
    {
        var t1 = GetTarget(player1);
        var t2 = GetTarget(player2);

        if (t1 == null || t2 == null)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));
            ShowNotification($"<b>You need to pick two targets.</b>");
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
            play1.RpcCustomMurder(seer);
            return;
        }

        if (play2.TryGetModifier<InvulnerabilityModifier>(out var invic2) && invic2.AttackAllInteractions)
        {
            play2.RpcCustomMurder(seer);
            return;
        }

        if (play1.HasModifier<VeteranAlertModifier>())
        {
            play1.RpcCustomMurder(seer);
            return;
        }

        if (play2.HasModifier<VeteranAlertModifier>())
        {
            play2.RpcCustomMurder(seer);
            return;
        }
        var button = CustomButtonSingleton<SeerCompareButton>.Instance;
        button.ResetCooldownAndOrEffect();


        var playerA = play1.CachedPlayerData.PlayerName;
        var playerB = play2.CachedPlayerData.PlayerName;

        void ShowNotification(string message)
        {
            var notif = Helpers.CreateAndShowNotification(message, Color.white, new Vector3(0f, 1f, -20f),
                spr: TouRoleIcons.Seer.LoadAsset());
            notif.AdjustNotification();
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