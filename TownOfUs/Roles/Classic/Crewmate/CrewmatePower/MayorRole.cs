using System.Collections;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using PowerTools;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Modules.RainbowMod;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MayorRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable, IUnguessable, ILoyalCrewmate
{
    public bool CanBeTraitor => false;
    public bool CanBeCrewpostor => false;
    public bool CanBeEgotist => true;
    public bool CanBeOtherEvil => true;
    public bool IsDraftable => false;
    public static GameObject MayorPlayer;
    public bool Revealed { get; set; }
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string IdPart => "Mayor";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Mayor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Mayor.LoadAsset(), "TouMira.Role.Crewmate.Mayor", 1.45f),
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
        var stringB = new StringBuilder();
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"{RoleColor.ToTextColor()}{MiraLocaleManager.Get("YouAreA")}<b> {this.GetRoleName()}.</b></color>");
        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"<size=60%>{MiraLocaleManager.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(RoleAlignment, true)}</b></size>");
        stringB.Append("<size=70%>");
        if (PlayerControl.LocalPlayer.HasModifier<EgotistModifier>())
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"{MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescriptionEgo")}");
        }
        else
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"{this.GetRoleLongDescription()}");
        }
        if (!Revealed)
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>{UnrevealedString}</b>");
        }

        return stringB;
    }

    public bool IsGuessable => false;
    public RoleBehaviour AppearAs => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<PoliticianRole>());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } = [];


    public static string UnrevealedString = MiraLocaleManager.Get("TownOfUsMira.Role.MayorUnrevealedTabText");
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        UnrevealedString = MiraLocaleManager.Get("TownOfUsMira.Role.MayorUnrevealedTabText");
        if (!Player.HasModifier<MayorRevealModifier>())
        {
            Player.AddModifier<MayorRevealModifier>(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MayorRole>()));
        }

        if (MeetingHud.Instance && !DisabledAnimation)
        {
            var targetVoteArea = MeetingHud.Instance.playerStates.First(x => x.PlayerId == player.PlayerId);
            Coroutines.Start(CoAnimateReveal(targetVoteArea));
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        var meeting = MeetingHud.Instance;
        if (meeting == null)
        {
            return;
        }

        var targetVoteArea = meeting.playerStates.First(x => x.PlayerId == Player.PlayerId);
        if (Revealed && !DisabledAnimation)
        {
            Coroutines.Start(CoAnimatePostReveal(targetVoteArea));
        }
    }

    [MethodRpc((uint)TownOfUsRpc.AnimateNewReveal)]
    public static void RpcAnimateNewReveal(PlayerControl plr)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(plr);
            return;
        }
        if (plr.Data.Role is MayorRole mayor)
        {
            mayor.Revealed = true;
        }

        if (DisabledAnimation)
        {
            return;
        }

        var targetVoteArea = MeetingHud.Instance.playerStates.First(x => x.PlayerId == plr.PlayerId);
        Coroutines.Start(CoAnimateReveal(targetVoteArea));
    }

    private static IEnumerator CoAnimateReveal(PlayerVoteArea voteArea)
    {
        if (Minigame.Instance)
        {
            Minigame.Instance.Close();
            Minigame.Instance.Close();
        }

        // hide meeting menu buttons (such as for guessers) for everyone but the mayor
        if (voteArea.PlayerId != PlayerControl.LocalPlayer.PlayerId)
        {
            MeetingMenu.Instances.Do(x => x.HideSingle(voteArea.PlayerId));
        }

        MayorPlayer = Instantiate(TouAssets.MayorRevealPrefab.LoadAsset(), voteArea.transform);
        MayorPlayer.transform.localPosition = new Vector3(-0.8f, 0, 0);
        MayorPlayer.transform.localScale = new Vector3(0.375f, 0.375f, 1f);
        MayorPlayer.gameObject.layer = MayorPlayer.transform.GetChild(0).gameObject.layer = voteArea.gameObject.layer;

        var animationRend = MayorPlayer.GetComponent<SpriteRenderer>();
        animationRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
        var r = animationRend.gameObject.GetComponent<RainbowBehaviour>()
             ?? animationRend.gameObject.AddComponent<RainbowBehaviour>();
        r.AddRend(animationRend, voteArea.PlayerIcon.ColorId);
        var handRend = MayorPlayer.transform.FindRecursive("Hands").GetComponent<SpriteRenderer>();
        if (!handRend)
        {
            handRend = MayorPlayer.transform.FindRecursive("Hand").GetComponent<SpriteRenderer>();
        }

        if (handRend)
        {
            handRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
            var r2 = handRend.gameObject.GetComponent<RainbowBehaviour>()
                  ?? handRend.gameObject.AddComponent<RainbowBehaviour>();
            r2.AddRend(handRend, voteArea.PlayerIcon.ColorId);
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
        var player = MiscUtils.PlayerById(voteArea.PlayerId);
        if (player!.Data.Role is MayorRole mayor)
        {
            mayor.Revealed = true;
        }

        yield return new WaitForSeconds(bodysAnim.m_currAnim.length - 0.25f);
        DestroyReveal(voteArea);
        MayorPlayer = Instantiate(TouAssets.MayorPostRevealPrefab.LoadAsset(), voteArea.transform);
        MayorPlayer.transform.localPosition = new Vector3(-0.8f, 0, 0);
        MayorPlayer.transform.localScale = new Vector3(0.375f, 0.375f, 1f);
        MayorPlayer.gameObject.layer = MayorPlayer.transform.GetChild(0).gameObject.layer = voteArea.gameObject.layer;

        animationRend = MayorPlayer.GetComponent<SpriteRenderer>();
        animationRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
        r = animationRend.gameObject.GetComponent<RainbowBehaviour>()
         ?? animationRend.gameObject.AddComponent<RainbowBehaviour>();
        r.AddRend(animationRend, voteArea.PlayerIcon.ColorId);
        handRend = MayorPlayer.transform.FindRecursive("Hands").GetComponent<SpriteRenderer>();
        if (!handRend)
        {
            handRend = MayorPlayer.transform.FindRecursive("Hand").GetComponent<SpriteRenderer>();
        }

        if (handRend)
        {
            handRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
            var r2 = animationRend.gameObject.GetComponent<RainbowBehaviour>()
                  ?? animationRend.gameObject.AddComponent<RainbowBehaviour>();
            r2.AddRend(animationRend, voteArea.PlayerIcon.ColorId);
        }

        voteArea.PlayerIcon.gameObject.SetActive(false);
        MayorPlayer.gameObject.SetActive(true);
        MayorPlayer.transform.GetChild(0).gameObject.SetActive(true);
        MayorPlayer.transform.GetChild(1).gameObject.SetActive(true);
    }

    private static IEnumerator CoAnimatePostReveal(PlayerVoteArea voteArea)
    {
        MayorPlayer = Instantiate(TouAssets.MayorPostRevealPrefab.LoadAsset(), voteArea.transform);
        MayorPlayer.transform.localPosition = new Vector3(-0.8f, 0, 0);
        MayorPlayer.transform.localScale = new Vector3(0.375f, 0.375f, 1f);
        MayorPlayer.gameObject.layer = MayorPlayer.transform.GetChild(0).gameObject.layer = voteArea.gameObject.layer;


        var animationRend = MayorPlayer.GetComponent<SpriteRenderer>();
        animationRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
        var r = animationRend.gameObject.GetComponent<RainbowBehaviour>()
             ?? animationRend.gameObject.AddComponent<RainbowBehaviour>();
        r.AddRend(animationRend, voteArea.PlayerIcon.ColorId);
        var handRend = MayorPlayer.transform.FindRecursive("Hands").GetComponent<SpriteRenderer>();
        if (!handRend)
        {
            handRend = MayorPlayer.transform.FindRecursive("Hand").GetComponent<SpriteRenderer>();
        }

        if (handRend)
        {
            handRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;
            var r2 = handRend.gameObject.GetComponent<RainbowBehaviour>()
                  ?? handRend.gameObject.AddComponent<RainbowBehaviour>();
            r2.AddRend(handRend, voteArea.PlayerIcon.ColorId);
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