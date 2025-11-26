using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
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
    public PlayerControl? GazeTarget { get; set; }
    public PlayerControl? IntuitTarget { get; set; }

    public override void Initialize(PlayerControl player)
    {
        GazeTarget = null;
        IntuitTarget = null;
        RoleBehaviourStubs.Initialize(this, player);
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }
    public void SeerCompare(PlayerControl seer)
    {
        if (GazeTarget == null || IntuitTarget == null)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));
            ShowNotification($"<b>You need to pick two targets.</b>");
            return;
        }

        if (GazeTarget == seer || IntuitTarget == seer)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));
            ShowNotification($"<b>You can't use yourself to compare!</b>");
            return;
        }
        var gazeButton = CustomButtonSingleton<SeerGazeButton>.Instance;
        gazeButton.ResetCooldownAndOrEffect();
        var intuitButton = CustomButtonSingleton<SeerIntuitButton>.Instance;
        intuitButton.ResetCooldownAndOrEffect();
        var playerA = GazeTarget.CachedPlayerData.PlayerName;
        var playerB = IntuitTarget.CachedPlayerData.PlayerName;

        void ShowNotification(string message)
        {
            var notif = Helpers.CreateAndShowNotification(message, Color.white, new Vector3(0f, 1f, -20f),
                spr: TouRoleIcons.Seer.LoadAsset());
            notif.AdjustNotification();
        }

        bool enemies = Enemies(GazeTarget, IntuitTarget);
        bool Enemies(PlayerControl p1, PlayerControl p2)
        {
            if (p1 == null || p2 == null) return false;
            if (p1.Data?.Role == null || p2.Data?.Role == null) return false;

            var friendlyNb = OptionGroupSingleton<SeerOptions>.Instance.BenignShowFriendlyToAll;
            var friendlyNe = OptionGroupSingleton<SeerOptions>.Instance.EvilShowFriendlyToAll;
            var friendlyNo = OptionGroupSingleton<SeerOptions>.Instance.OutlierShowFriendlyToAll;

            if (p1.IsCrewmate() && p2.IsCrewmate()) return false;
            if (p1.IsImpostor() && p2.IsImpostor()) return false;
            if (p1.Data.Role.Role == p2.Data.Role.Role) return false; // Two werewolves are friendly to one another
            if (p1.Is(RoleAlignment.NeutralBenign) && p2.Is(RoleAlignment.NeutralBenign)) return false;
            if (p1.Is(RoleAlignment.NeutralEvil) && p2.Is(RoleAlignment.NeutralEvil)) return false;
            if (p1.Is(RoleAlignment.NeutralOutlier) && p2.Is(RoleAlignment.NeutralOutlier)) return false;

            if (p1.Is(RoleAlignment.NeutralBenign) || p2.Is(RoleAlignment.NeutralBenign))
                return !friendlyNb;
            if (p1.Is(RoleAlignment.NeutralEvil) || p2.Is(RoleAlignment.NeutralEvil))
                return !friendlyNe;
            if (p1.Is(RoleAlignment.NeutralOutlier) || p2.Is(RoleAlignment.NeutralOutlier))
                return !friendlyNo;

            // You sense that Atony and Cursed Soul appear to be enemies!
            return true;
        }


        if (enemies)
        {
            Coroutines.Start(MiscUtils.CoFlash(Palette.ImpostorRed));
            ShowNotification($"<b>{Palette.ImpostorRed.ToTextColor()}{playerA} and {playerB} appear as enemies!</color></b>");
        }
        else
        {
            Coroutines.Start(MiscUtils.CoFlash(Palette.CrewmateBlue));
            ShowNotification($"<b>{Palette.CrewmateBlue.ToTextColor()}{playerA} and {playerB} appear friendly to each other!</color></b>");
        }
        IntuitTarget = null;
        GazeTarget = null;
    }
}