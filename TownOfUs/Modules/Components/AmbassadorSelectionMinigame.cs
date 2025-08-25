using System.Collections;
using System.Diagnostics.CodeAnalysis;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity")]
public sealed class AmbassadorSelectionMinigame(IntPtr cppPtr) : Minigame(cppPtr)
{
    public Transform? RolesHolder;
    public GameObject? RolePrefab;
    public TextMeshPro? StatusText;
    public TextMeshPro? RoleName;
    public SpriteRenderer? RoleIcon;
    public TextMeshPro? RoleTeam;
    public GameObject? RedRing;
    public GameObject? WarpRing;

    private readonly Color _bgColor = new Color32(24, 0, 0, 215);
    private RoleTypes? _selectedRole;
    private List<RoleBehaviour> availableRoles = [];
    private Action<RoleBehaviour> clickHandler;
    public static int CurrentCard { get; set; }
    public static int RoleCount { get; set; }

    private void Awake()
    {
        if (Instance)
        {
            Instance.Close();
        }

        RolesHolder = transform.FindChild("Roles");
        RolePrefab = transform.FindChild("RoleCardHolder").gameObject;
        StatusText = transform.FindChild("Status").gameObject.GetComponent<TextMeshPro>();
        RoleName = transform.FindChild("Status").FindChild("RoleName").gameObject.GetComponent<TextMeshPro>();
        RoleTeam = transform.FindChild("Status").FindChild("RoleTeam").gameObject.GetComponent<TextMeshPro>();
        RoleIcon = transform.FindChild("Status").FindChild("RoleImage").gameObject.GetComponent<SpriteRenderer>();
        RedRing = transform.FindChild("Status").FindChild("RoleRing").gameObject;
        WarpRing = transform.FindChild("Status").FindChild("RingWarp").gameObject;
        RoleTeam = transform.FindChild("Status").FindChild("RoleTeam").gameObject.GetComponent<TextMeshPro>();

        StatusText.font = HudManager.Instance.TaskPanel.taskText.font;
        StatusText.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
        StatusText.text = "Choose a role for retraining.";
        StatusText.gameObject.SetActive(false);
        
        RoleName.font = HudManager.Instance.TaskPanel.taskText.font;
        RoleName.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
        RoleName.text = "Random";
        RoleName.gameObject.SetActive(false);
        
        RoleTeam.font = HudManager.Instance.TaskPanel.taskText.font;
        RoleTeam.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
        RoleTeam.text = "Random Impostor";
        RoleTeam.gameObject.SetActive(false);
        
        RoleIcon.sprite = TouRoleIcons.RandomImp.LoadAsset();
        RoleIcon.gameObject.SetActive(false);
        RedRing.SetActive(false);
        WarpRing.SetActive(false);
    }

    public static AmbassadorSelectionMinigame Create()
    {
        var gameObject = Instantiate(TouAssets.AltRoleSelectionGame.LoadAsset(), HudManager.Instance.transform);
        gameObject.GetComponent<Minigame>().DestroyImmediate();
        gameObject.SetActive(false);

        return gameObject.AddComponent<AmbassadorSelectionMinigame>();
    }

    [HideFromIl2Cpp]
    public void Open(List<RoleBehaviour> roles, Action<RoleBehaviour> onClick, RoleTypes? defaultRole = null)
    {
        availableRoles = roles;
        clickHandler = onClick;
        _selectedRole = defaultRole ?? roles.Random()!.Role;
        // Adds random as an option
        RoleCount = availableRoles.Count + 1;

        Coroutines.Start(CoOpen(this));
    }

    private static IEnumerator CoOpen(AmbassadorSelectionMinigame minigame)
    {
        while (ExileController.Instance != null)
        {
            yield return new WaitForSeconds(0.65f);
        }

        minigame.gameObject.SetActive(true);
        minigame.Begin();
    }

    public override void Close()
    {
        HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(_bgColor, Color.clear));
        CurrentCard = -1;
        RoleCount = -1;
        MinigameStubs.Close(this);
    }

    private void Begin()
    {
        HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, _bgColor));

        StatusText!.gameObject.SetActive(true);
        RoleName!.gameObject.SetActive(true);
        RoleTeam!.gameObject.SetActive(true);
        RoleIcon!.gameObject.SetActive(true);
        RedRing!.SetActive(true);
        WarpRing!.SetActive(true);
        RoleIcon!.SetSizeLimit(2.8f);
        
        foreach (var role in availableRoles)
        {
            var teamName = role.GetRoleAlignment().ToDisplayString();

            if (role is ITownOfUsRole touRole)
            {
                teamName = touRole.RoleAlignment.ToDisplayString();
            }

            var roleName = role.NiceName;
            var roleImg = TouRoleIcons.RandomAny.LoadAsset();

            if (role is ICustomRole customRole)
            {
                if (customRole.Configuration.Icon != null)
                {
                    roleImg = customRole.Configuration.Icon.LoadAsset();
                }
                else if (customRole.Team == ModdedRoleTeams.Crewmate)
                {
                    roleImg = TouRoleIcons.RandomCrew.LoadAsset();
                }
                else if (customRole.Team == ModdedRoleTeams.Impostor)
                {
                    roleImg = TouRoleIcons.RandomImp.LoadAsset();
                }
                else
                {
                    roleImg = TouRoleIcons.RandomNeut.LoadAsset();
                }
            }
            else
            {
                if (role.RoleIconSolid != null)
                {
                    roleImg = role.RoleIconSolid;
                }
            }

            var card = CreateCard(roleName, teamName, roleImg, role.TeamColor);
            card.OnClick.RemoveAllListeners();
            card.OnClick.AddListener((UnityAction)(() => { clickHandler.Invoke(role); }));
        }

        var randomCard = CreateCard("Random", "Random Impostor", TouRoleIcons.RandomImp.LoadAsset(),
            TownOfUsColors.Impostor);
        randomCard.OnClick.RemoveAllListeners();
        randomCard.OnClick.AddListener((UnityAction)(() =>
        {
            clickHandler.Invoke(RoleManager.Instance.GetRole(_selectedRole!.Value));
        }));

        Coroutines.Start(CoAnimateCards());
        TransType = TransitionType.None;
        Begin(null);
    }

    private PassiveButton CreateCard(string roleName, string teamName, Sprite? sprite, Color color)
    {

        var newRoleObj = Instantiate(RolePrefab, RolesHolder);
        var actualCard = newRoleObj!.transform.GetChild(0);
        var roleText = actualCard.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();
        var roleImage = actualCard.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        var teamText = actualCard.transform.GetChild(2).gameObject.GetComponent<TextMeshPro>();
        var selection = actualCard.transform.GetChild(3).gameObject;
        var passiveButton = actualCard.GetComponent<PassiveButton>();
        var buttonRollover = actualCard.GetComponent<ButtonRolloverHandler>();

        selection.SetActive(false);
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            selection.SetActive(true);
            RoleName!.text = roleName;
            RoleTeam!.text = teamName;
            if (sprite != null) RoleIcon!.sprite = sprite;
            RoleIcon!.SetSizeLimit(2.8f);
        }));
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            selection.SetActive(false);
        }));

        float angle = (2 * Mathf.PI / RoleCount) * CurrentCard;
        float x = 1.9f * Mathf.Cos(angle);
        float y = 0.1f + 1.9f * Mathf.Sin(angle);
        
        newRoleObj.transform.localPosition =
            new Vector3(x, y, -1f);
        newRoleObj.name = roleName + " Selection";
        
        roleText.text = roleName;
        teamText.text = teamName;

        roleImage.sprite = (sprite != null) ? sprite : TouRoleIcons.RandomImp.LoadAsset();
        roleImage.SetSizeLimit(2.8f);

        buttonRollover.OverColor = color;
        roleText.color = color;
        teamText.color = color;
        ++CurrentCard;
        newRoleObj.gameObject.SetActive(true);

        return passiveButton;
    }

    [HideFromIl2Cpp]
    private IEnumerator CoAnimateCards()
    {
        foreach (var o in RolesHolder!.transform)
        {
            var card = o.Cast<Transform>();
            if (card == null)
            {
                continue;
            }

            var child = card.GetChild(0);
            Coroutines.Start(MiscUtils.BetterBloop(child, finalSize: 0.5f - (RoleCount * 0.0075f), duration: 0.1f, intensity: 0.11f));
            yield return new WaitForSeconds(0.01f);
        }

        CurrentCard = -1;
        RoleCount = -1;
    }
}