using System.Collections;
using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using PowerTools;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MayorRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable, IUnguessable
{
    public static GameObject MayorPlayer;

    private MeetingMenu meetingMenu;
    public bool Revealed { get; set; }
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string RoleName => TouLocale.Get(TouNames.Mayor, "Mayor");
    public string RoleDescription => "Reveal Yourself To Save The Crew";
    public string RoleLongDescription => "Lead the crew to victory!";
    public Color RoleColor => TownOfUsColors.Mayor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Mayor,
        HideSettings = true,
        MaxRoleCount = 0,
        DefaultRoleCount = 0,
        DefaultChance = 0,
        CanModifyChance = false
    };

    public bool IsPowerCrew => true;
    public static bool DisabledAnimation { get; set; }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        if (!Revealed)
        {
            stringB.AppendLine(CultureInfo.InvariantCulture, $"<b>Reveal yourself whenever you wish.</b>");
        }

        if (PlayerControl.LocalPlayer.HasModifier<EgotistModifier>())
        {
            stringB.AppendLine(CultureInfo.InvariantCulture, $"<b>The Impostors know your true motives.</b>");
        }

        return stringB;
    }

    public bool IsGuessable => false;
    public RoleBehaviour AppearAs => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<PoliticianRole>());

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Power role that gains three votes and is revealed to all players, also changing their look in meetings.";
    }

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.HasModifier<ToBecomeTraitorModifier>())
        {
            Player.GetModifier<ToBecomeTraitorModifier>()!.Clear();
        }
        if (MeetingHud.Instance && !DisabledAnimation)
        {
            var targetVoteArea = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == player.PlayerId);
            Coroutines.Start(CoAnimateReveal(targetVoteArea));
        }

        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(
                this,
                Click,
                MeetingAbilityType.Click,
                TouAssets.RevealButtonSprite,
                null!,
                IsExempt)
            {
                Position = new Vector3(-0.35f, 0f, -3f)
            };
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        var targetVoteArea = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == Player.PlayerId);
        if (Revealed && !DisabledAnimation)
        {
            Coroutines.Start(CoAnimatePostReveal(targetVoteArea));
        }

        if (Player.AmOwner && !Revealed)
            // Logger<TownOfUsPlugin>.Message($"PoliticianRole.OnMeetingStart '{Player.Data.PlayerName}' {Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>()}");
        {
            meetingMenu.GenButtons(MeetingHud.Instance,
                Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>());
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

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (Player.AmOwner)
        {
            meetingMenu?.Dispose();
            meetingMenu = null!;
        }
    }

    public void Click(PlayerVoteArea voteArea, MeetingHud __)
    {
        if (!Player.AmOwner)
        {
            return;
        }

        meetingMenu.HideButtons();
        RpcAnimateNewReveal(Player);
    }

    [MethodRpc((uint)TownOfUsRpc.AnimateNewReveal, SendImmediately = true)]
    public static void RpcAnimateNewReveal(PlayerControl plr)
    {
        if (plr.Data.Role is MayorRole mayor)
        {
            mayor.Revealed = true;
        }

        if (DisabledAnimation)
        {
            return;
        }
        var targetVoteArea = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == plr.PlayerId);
        Coroutines.Start(CoAnimateReveal(targetVoteArea));
    }


    public bool IsExempt(PlayerVoteArea voteArea)
    {
        return voteArea?.TargetPlayerId != Player.PlayerId;
    }

    public static bool MayorVisibilityFlag(PlayerControl player)
    {
        return player.IsRole<MayorRole>() && player.GetRole<MayorRole>()!.Revealed;
    }

    private static IEnumerator CoAnimateReveal(PlayerVoteArea voteArea)
    {
        if (Minigame.Instance != null)
        {
            Minigame.Instance.Close();
            Minigame.Instance.Close();
        }

        // hide meeting menu buttons (such as for guessers) for everyone but the mayor
        if (voteArea.TargetPlayerId != PlayerControl.LocalPlayer.PlayerId)
        {
            MeetingMenu.Instances.Do(x => x.HideSingle(voteArea.TargetPlayerId));
        }

        MayorPlayer = Instantiate(TouAssets.MayorRevealPrefab.LoadAsset(), voteArea.transform);
        MayorPlayer.transform.localPosition = new Vector3(-0.8f, 0, 0);
        MayorPlayer.transform.localScale = new Vector3(0.375f, 0.375f, 1f);
        MayorPlayer.gameObject.layer = MayorPlayer.transform.GetChild(0).gameObject.layer = voteArea.gameObject.layer;

        var animationRend = MayorPlayer.GetComponent<SpriteRenderer>();
        animationRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
        var handRend = MayorPlayer.transform.FindRecursive("Hands").GetComponent<SpriteRenderer>();
        if (!handRend)
        {
            handRend = MayorPlayer.transform.FindRecursive("Hand").GetComponent<SpriteRenderer>();
        }

        if (handRend)
        {
            handRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
        }

        voteArea.PlayerIcon.gameObject.SetActive(false);
        MayorPlayer.gameObject.SetActive(true);
        MayorPlayer.transform.GetChild(0).gameObject.SetActive(true);
        MayorPlayer.transform.GetChild(1).gameObject.SetActive(true);

        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Mayor, 0.15f, 0.15f));

        var bodysAnim = MayorPlayer.GetComponent<SpriteAnim>();
        var outfitAnim = MayorPlayer.transform.GetChild(0).GetComponent<SpriteAnim>();
        var handAnim = MayorPlayer.transform.GetChild(1).GetComponent<SpriteAnim>();
        bodysAnim.SetSpeed(1.02f);
        outfitAnim.SetSpeed(1.02f);
        handAnim.SetSpeed(1.02f);
        TouAudio.PlaySound(TouAudio.MayorRevealSound);
        yield return new WaitForSeconds(0.1f);
        var player = MiscUtils.PlayerById(voteArea.TargetPlayerId);
        if (player!.Data.Role is MayorRole mayor)
        {
            mayor.Revealed = true;
        }

        yield return new WaitForSeconds(bodysAnim.m_currAnim.length - 0.25f);
    }

    private static IEnumerator CoAnimatePostReveal(PlayerVoteArea voteArea)
    {
        MayorPlayer = Instantiate(TouAssets.MayorPostRevealPrefab.LoadAsset(), voteArea.transform);
        MayorPlayer.transform.localPosition = new Vector3(-0.8f, 0, 0);
        MayorPlayer.transform.localScale = new Vector3(0.375f, 0.375f, 1f);
        MayorPlayer.gameObject.layer = MayorPlayer.transform.GetChild(0).gameObject.layer = voteArea.gameObject.layer;

        var animationRend = MayorPlayer.GetComponent<SpriteRenderer>();
        animationRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
        var handRend = MayorPlayer.transform.FindRecursive("Hands").GetComponent<SpriteRenderer>();
        if (!handRend)
        {
            handRend = MayorPlayer.transform.FindRecursive("Hand").GetComponent<SpriteRenderer>();
        }

        if (handRend)
        {
            handRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
        }

        voteArea.PlayerIcon.gameObject.SetActive(false);
        MayorPlayer.gameObject.SetActive(true);
        MayorPlayer.transform.GetChild(0).gameObject.SetActive(true);
        MayorPlayer.transform.GetChild(1).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.01f);
    }

    public static void DestroyReveal(PlayerVoteArea voteArea)
    {
        if (MayorPlayer != null)
        {
            MayorPlayer.gameObject.SetActive(false);
            voteArea.PlayerIcon.gameObject.SetActive(true);
            Destroy(MayorPlayer);
            MayorPlayer = null!;
        }
    }
}