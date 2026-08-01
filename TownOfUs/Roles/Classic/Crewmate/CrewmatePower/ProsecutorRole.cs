using System.Collections;
using System.Text;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;
using UnityEngine.Events;

namespace TownOfUs.Roles.Crewmate;

public sealed class ProsecutorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    [HideFromIl2Cpp] public PlayerVoteArea? ProsecuteButton { get; private set; }

    public bool HasProsecuted { get; private set; }

    public byte ProsecuteVictim { get; set; } = byte.MaxValue;

    public bool SelectingProsecuteVictim { get; set; }
    public bool HideProsButton { get; set; }

    public int ProsecutionsCompleted { get; set; }

    public void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not ProsecutorRole)
        {
            return;
        }

        var meeting = MeetingHud.Instance;

        if (!Player.AmOwner || meeting == null || ProsecuteButton == null)
        {
            return;
        }

        ProsecuteButton.gameObject.SetActive(!HideProsButton && meeting.state == MeetingHud.VoteStates.NotVoted &&
                                             !SelectingProsecuteVictim);

        if (!ProsecuteButton.gameObject.active)
        {
            return;
        }

        if (meeting.state == MeetingHud.VoteStates.Discussion &&
            meeting.discussionTimer < GameOptionsManager.Instance.currentNormalGameOptions.DiscussionTime)
        {
            ProsecuteButton.SetDisabled();
        }
        else
        {
            ProsecuteButton.SetEnabled();
        }

        ProsecuteButton.voteComplete = meeting.SkipVoteButton.voteComplete;
    }

    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string LocaleKey => "Prosecutor";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(TouLocale.GetParsed($"TouRole{LocaleKey}ProsecuteWiki", "Prosecute"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}ProsecuteWikiDescription"),
                    TouRoleIcons.Prosecutor)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Prosecutor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public bool IsPowerCrew =>
        ProsecutionsCompleted <
        (int)OptionGroupSingleton<ProsecutorOptions>.Instance
            .MaxProsecutions; // Disable end game checks if prosecutes are available

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Prosecutor.LoadAsset(), "TouMira.Role.Crewmate.Prosecutor", 1.45f),
        MaxRoleCount = 1,
        Icon = TouRoleIcons.Prosecutor,
        OptionsScreenshot = TouBanners.ProsecutorRoleBanner,
        IntroSound = TouAudio.ProsIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = ITownOfUsRole.SetNewTabText(this);
        if (PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
        {
            text.AppendLine(TownOfUsPlugin.Culture, $"{TouLocale.GetParsed($"TouRole{LocaleKey}CanProsecuteCrew")}");
        }

        var prosecutes = OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions - ProsecutionsCompleted;
        text.AppendLine(TownOfUsPlugin.Culture, $"{TouLocale.GetParsed($"TouRole{LocaleKey}ProsecutionsRemaining").Replace("<count>", prosecutes.ToString(TownOfUsPlugin.Culture))}");
        return text;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.HasModifier<ImitatorCacheModifier>())
        {
            ProsecutionsCompleted = (int)OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions;
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        var meeting = MeetingHud.Instance;
        if (!Player.AmOwner || meeting == null ||
            ProsecutionsCompleted >= OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions)
        {
            return;
        }

        var skip = meeting.SkipVoteButton;
        ProsecuteButton = Instantiate(skip, skip.transform.parent);
        ProsecuteButton.Parent = meeting;
        ProsecuteButton.SetTargetPlayerId(251);
        ProsecuteButton.transform.localPosition = skip.transform.localPosition + new Vector3(0f, -0.17f, 0f);

        ProsecuteButton.gameObject.GetComponentInChildren<TextTranslatorTMP>().Destroy();
        ProsecuteButton.gameObject.GetComponentInChildren<TextMeshPro>().text =
            TouLocale.GetParsed($"TouRole{LocaleKey}Prosecute").ToUpperInvariant();
        ProsecuteButton.gameObject.name = "button_prosecuteButton";

        foreach (var plr in meeting.playerStates.AddItem(skip))
        {
            plr.gameObject.GetComponentInChildren<PassiveButton>().OnClick
                .AddListener((UnityAction)(() => ProsecuteButton.ClearButtons()));
        }

        skip.transform.localPosition += new Vector3(0f, 0.20f, 0f);
    }

    public void Cleanup()
    {
        HideProsButton = false;
        ProsecuteButton = null;
        SelectingProsecuteVictim = false;
        ProsecuteVictim = byte.MaxValue;

        if (HasProsecuted)
        {
            ProsecutionsCompleted++;
        }

        HasProsecuted = false;
    }

    [MethodRpc((uint)TownOfUsRpc.Prosecute)]
    public static void RpcProsecute(PlayerControl plr, byte Victim)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(plr);
            return;
        }
        if (plr.Data.Role is not ProsecutorRole prosecutorRole)
        {
            return;
        }

        if (prosecutorRole.ProsecutionsCompleted >=
            OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions)
        {
            return;
        }

        prosecutorRole.HasProsecuted = true;
        prosecutorRole.ProsecuteVictim = Victim;
    }

    [MethodRpc((uint)TownOfUsRpc.ShowProsAnimation)]
    public static void RpcShowProsAnimation(PlayerControl player)
    {
        if (LobbyBehaviour.Instance || !MeetingHud.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }

        Coroutines.Start(CoShowProsAnimation());
    }
    private static IEnumerator CoShowProsAnimation()
    {
        TouAudio.PlaySound(TouAudio.ProsecuteSound);
        var prosAnim = Instantiate(TouAssets.ProsecuteAnimation.LoadAsset(), MeetingHud.Instance.transform);
        prosAnim.transform.localPosition = new Vector3(0f, 0f, -80f);
        prosAnim.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
        var handPoint = prosAnim.transform.GetChild(0);
        var playerMat = PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        var matSetUp = false;
        foreach (var renderer in handPoint.GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.material = playerMat;
            if (!matSetUp)
            {
                matSetUp = true;
                renderer.material.SetColor(ShaderID.BodyColor, new Color32(255, 172, 121, 255));
                renderer.material.SetColor(ShaderID.BackColor, new Color32(180, 80, 80, 255));
                renderer.material.SetColor(ShaderID.VisorColor, Palette.VisorColor);
                playerMat = renderer.material;
            }
        }
        var body = prosAnim.transform.GetChild(1);
        foreach (var renderer in body.GetComponentsInChildren<SpriteRenderer>())
        {
            if (renderer.transform.name.Contains("_Colored"))
            {
                renderer.material = playerMat;
            }
        }
        var gavel = prosAnim.transform.GetChild(2);
        foreach (var renderer in gavel.GetComponentsInChildren<SpriteRenderer>())
        {
            if (renderer.transform.name.Contains("_Colored"))
            {
                renderer.material = playerMat;
            }
        }
        var killBg = prosAnim.transform.GetChild(3);
        killBg.localScale = new Vector3(2f, 0f, 1f);
        yield return Effects.Wait(0.25f);
        yield return new WaitForLerp(0.25f, new Action<float>(t =>
        {
            var adj = t / 200;
            handPoint.localPosition += new Vector3(adj, 0f, 0f);
            body.localPosition += new Vector3(adj, 0f, 0f);
            gavel.localPosition += new Vector3(adj, 0f, 0f);
        }));
        yield return new WaitForLerp(0.16666667f, new Action<float>(t =>
        {
            killBg.localScale = new Vector3(2f, Mathf.Clamp(killBg.localScale.y + t, 0, 1.1f), 1f);
        }));
        var adj = 0f;
        yield return new WaitForLerp(2.23f, new Action<float>(t =>
        {
            adj = t / 300;
            handPoint.localPosition += new Vector3(adj, 0f, 0f);
            body.localPosition += new Vector3(adj, 0f, 0f);
            gavel.localPosition += new Vector3(adj, 0f, 0f);
        }));
        yield return new WaitForLerp(0.2f, new Action<float>(t =>
        {
            var newStart = adj;
            handPoint.localPosition += new Vector3(newStart + t, 0f, 0f);
            body.localPosition += new Vector3(newStart + t, 0f, 0f);
            gavel.localPosition += new Vector3(newStart + t, 0f, 0f);
            killBg.localScale = new Vector3(1f, 1.1f - t, 1f);
        }));
        
        Destroy(prosAnim);
    }
}