using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace TownOfUs.Roles.Crewmate;

public sealed class JailorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    private GameObject? executeButton;
    private TMP_Text? usesText;
    public override bool IsAffectedByComms => false;

    public int Executes { get; set; } = (int)OptionGroupSingleton<JailorOptions>.Instance.MaxExecutes;

    public PlayerControl Jailed => PlayerControl.AllPlayerControls.ToArray()
        .FirstOrDefault(x => x.GetModifier<JailedModifier>()?.JailorId == Player.PlayerId)!;

    public DoomableType DoomHintType => DoomableType.Relentless;
    public string RoleName => TouLocale.Get(TouNames.Jailor, "Jailor");
    public string RoleDescription => "Jail And Execute The <color=#FF0000FF>Impostors</color>";
    public string RoleLongDescription => "Execute evildoers in meetings but avoid crewmates";
    public Color RoleColor => TownOfUsColors.Jailor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;
    public bool IsPowerCrew => Executes > 0; // Stop end game checks if the Jailor can still execute someone

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        Icon = TouRoleIcons.Jailor,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Impostor)
    };

    public void LobbyStart()
    {
        Clear();
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        if (PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
        {
            stringB.AppendLine(CultureInfo.InvariantCulture, $"You can execute crewmates.");
        }

        return stringB;
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Power role that can jail other players. During a meeting, the Jailor can choose to execute their jailed player. (Unless the Jailor is an Imitator)"
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Jail",
            "Jail a player. During the meeting everyone will see who is jailed. You can privately talk with your detained player using the instructions that are in the private chatbox",
            TouCrewAssets.JailSprite),
        new("Execute (Meeting)",
            "Execute the detained player. If the player is a crewmate the Jailor will lose the ability to Jail.",
            TouAssets.ExecuteCleanSprite)
    ];

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        Executes = (int)OptionGroupSingleton<JailorOptions>.Instance.MaxExecutes;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        Clear();
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        Clear();

        if (Player.HasDied())
        {
            return;
        }

        if (Player.AmOwner)
        {
            if (Jailed!.HasDied())
            {
                return;
            }

            var title = $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>Jailor Feedback</color>";
            MiscUtils.AddFakeChat(Jailed.Data, title, "Communicate with your jailee in the <b>RED</b> private chatbox next to the <b>REGULAR</b> chatbox.", false,
                true);
        }

        if (MeetingHud.Instance)
        {
            AddMeetingButtons(MeetingHud.Instance);
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        executeButton?.Destroy();
        usesText?.Destroy();
    }

    public void Clear()
    {
        executeButton?.Destroy();
        usesText?.Destroy();
    }

    private void AddMeetingButtons(MeetingHud __instance)
    {
        if (Jailed == null || Jailed?.HasDied() == true)
        {
            return;
        }

        if (!Player.AmOwner)
        {
            return;
        }

        if (Executes <= 0 || Jailed?.HasDied() == true)
        {
            return;
        }

        if (Player.HasModifier<ImitatorCacheModifier>())
        {
            return;
        }

        foreach (var voteArea in __instance.playerStates)
        {
            if (Jailed?.PlayerId == voteArea.TargetPlayerId)
                // if (!(jailorRole.Jailed.IsLover() && PlayerControl.LocalPlayer.IsLover()))
            {
                GenButton(voteArea);
            }
        }
    }


    private void GenButton(PlayerVoteArea voteArea)
    {
        var confirmButton = voteArea.Buttons.transform.GetChild(0).gameObject;

        var newButtonObj = Instantiate(confirmButton, voteArea.transform);
        //newButtonObj.transform.position = confirmButton.transform.position - new Vector3(0.75f, 0f, -2.1f);
        newButtonObj.transform.position = confirmButton.transform.position - new Vector3(0.75f, 0f, 0f);
        newButtonObj.transform.localScale *= 0.8f;
        newButtonObj.layer = 5;
        newButtonObj.transform.parent = confirmButton.transform.parent.parent;

        executeButton = newButtonObj;

        var renderer = newButtonObj.GetComponent<SpriteRenderer>();
        renderer.sprite = TouAssets.ExecuteSprite.LoadAsset();

        var passive = newButtonObj.GetComponent<PassiveButton>();
        passive.OnClick = new Button.ButtonClickedEvent();
        passive.OnClick.AddListener(Execute());

        var usesTextObj = Instantiate(voteArea.NameText, voteArea.transform);
        usesTextObj.transform.localPosition = new Vector3(-0.22f, 0.16f, newButtonObj.transform.position.z - 0.1f);
        usesTextObj.text = $"{Executes}";
        usesTextObj.transform.localScale = usesTextObj.transform.localScale * 0.65f;

        usesText = usesTextObj;
    }

    [HideFromIl2Cpp]
    private Action Execute()
    {
        void Listener()
        {
            if (Player.HasDied())
            {
                return;
            }

            Clear();

            Executes--;
            var text = $"{Jailed.Data.PlayerName} cannot be executed! They must be Invulnerable!";
            if (!Jailed.HasModifier<InvulnerabilityModifier>())
            {
                if (Jailed.Is(ModdedRoleTeams.Crewmate) &&
                    !(PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) &&
                      !allyMod.GetsPunished) && !(Jailed.TryGetModifier<AllianceGameModifier>(out var allyMod2) &&
                                                  !allyMod2.GetsPunished))
                {
                    Executes = 0;

                    CustomButtonSingleton<JailorJailButton>.Instance.ExecutedACrew = true;
                    text = $"{Jailed.Data.PlayerName} was a crewmate! You can no longer jail anyone!";
                }
                else
                {
                    Coroutines.Start(MiscUtils.CoFlash(Color.green));
                    text = $"{Jailed.Data.PlayerName} was successfully executed!";
                }

                Player.RpcCustomMurder(Jailed, createDeadBody: false, teleportMurderer: false);
                DeathHandlerModifier.RpcUpdateDeathHandler(Jailed, "Executed", DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse, $"By {Player.Data.PlayerName}", lockInfo: DeathHandlerOverride.SetTrue);
            }

            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{text}</b>", Color.white, spr: TouRoleIcons.Jailor.LoadAsset());

            notif1.Text.SetOutlineThickness(0.35f);
            notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
        }

        return Listener;
    }
}