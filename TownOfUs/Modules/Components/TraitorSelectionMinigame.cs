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
using Random = UnityEngine.Random;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity")]
public sealed class TraitorSelectionMinigame(IntPtr cppPtr) : Minigame(cppPtr)
{
    public Transform? RolesHolder;
    public GameObject? RolePrefab;
    public TextMeshPro? StatusText;

    private readonly Color _bgColor = new Color32(6, 0, 0, 215);
    private RoleTypes? _selectedRole;
    private List<RoleBehaviour> availableRoles = [];
    private Action<RoleBehaviour> clickHandler;
    public static int CurrentCard { get; set; }

    private void Awake()
    {
        if (Instance)
        {
            Instance.Close();
        }

        RolesHolder = transform.FindChild("Roles");
        RolePrefab = transform.FindChild("RoleCardHolder").gameObject;
        StatusText = transform.FindChild("Status").gameObject.GetComponent<TextMeshPro>();

        StatusText.font = HudManager.Instance.TaskPanel.taskText.font;
        StatusText.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
        StatusText.text = "Select a role.";
        StatusText.gameObject.SetActive(false);
    }

    public static TraitorSelectionMinigame Create()
    {
        var gameObject = Instantiate(TouAssets.RoleSelectionGame.LoadAsset(), HudManager.Instance.transform);
        gameObject.GetComponent<Minigame>().DestroyImmediate();
        gameObject.SetActive(false);

        return gameObject.AddComponent<TraitorSelectionMinigame>();
    }

    [HideFromIl2Cpp]
    public void Open(List<RoleBehaviour> roles, Action<RoleBehaviour> onClick, RoleTypes? defaultRole = null)
    {
        availableRoles = roles;
        clickHandler = onClick;
        _selectedRole = defaultRole ?? roles.Random()!.Role;

        Coroutines.Start(CoOpen(this));
    }

    private static IEnumerator CoOpen(TraitorSelectionMinigame minigame)
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
        MinigameStubs.Close(this);
    }

    private void Begin()
    {
        HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, _bgColor));

        StatusText!.gameObject.SetActive(true);

        var z = 0;

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

            var card = CreateCard(roleName, teamName, roleImg, z, role.TeamColor);
            card.OnClick.RemoveAllListeners();
            card.OnClick.AddListener((UnityAction)(() => { clickHandler.Invoke(role); }));

            z++;
        }

        var randomCard = CreateCard("Random", "Random\nImpostor", TouRoleIcons.RandomImp.LoadAsset(), z,
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

    private PassiveButton CreateCard(string roleName, string teamName, Sprite? sprite, float z, Color color)
    {
        var newRoleObj = Instantiate(RolePrefab, RolesHolder);
        var actualCard = newRoleObj!.transform.GetChild(0);
        var roleText = actualCard.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();
        var roleImage = actualCard.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        var teamText = actualCard.transform.GetChild(2).gameObject.GetComponent<TextMeshPro>();
        var passiveButton = actualCard.GetComponent<PassiveButton>();
        var buttonRollover = actualCard.GetComponent<ButtonRolloverHandler>();

        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            newRoleObj.transform.localPosition = new Vector3(newRoleObj.transform.localPosition.x,
                newRoleObj.transform.localPosition.y, newRoleObj.transform.localPosition.z - 10);
        }));
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            newRoleObj.transform.localPosition = new Vector3(newRoleObj.transform.localPosition.x,
                newRoleObj.transform.localPosition.y, newRoleObj.transform.localPosition.z + 10);
        }));

        var randZ = -10f + z * 5f + Random.RandomRange(-1.5f, 1.5f);
        newRoleObj.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -randZ));
        newRoleObj.transform.localPosition =
            new Vector3(newRoleObj.transform.localPosition.x, newRoleObj.transform.localPosition.y, z);

        roleText.text = roleName;
        teamText.text = teamName;

        if (sprite != null)
        {
            roleImage.sprite = sprite;
        }
        roleImage.SetSizeLimit(2.8f);

        buttonRollover.OverColor = color;
        roleText.color = color;
        teamText.color = color;

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
            yield return CoAnimateCardIn(child);
            Coroutines.Start(MiscUtils.BetterBloop(child, finalSize: 0.55f, duration: 0.22f, intensity: 0.16f));
            yield return new WaitForSeconds(0.1f);
        }

        CurrentCard = -1;
    }

    private static IEnumerator CoAnimateCardIn(Transform card)
    {
        var randY = (CurrentCard * CurrentCard * 0.5f - CurrentCard) * 0.1f + Random.RandomRange(-0.15f, 0f);
        var randZ = -10f + CurrentCard * 5f + Random.RandomRange(-1.5f, 0f);
        if (CurrentCard == 0)
        {
            randY = 0f;
            randZ = -2f;
        }

        card.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -randZ));
        card.transform.localPosition = new Vector3(card.transform.localPosition.x, card.transform.localPosition.y - 5f,
            card.transform.localPosition.z);
        card.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 14f));
        card.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        card.parent.gameObject.SetActive(true);
        for (var timer = 0f; timer < 0.4f; timer += Time.deltaTime)
        {
            var num = timer / 0.4f;
            card.localPosition =
                new Vector3(card.localPosition.x, Mathf.SmoothStep(-5f, randY, num), card.localPosition.z);
            card.transform.localRotation =
                Quaternion.Euler(new Vector3(0, 0, Mathf.SmoothStep(-randZ + 2.5f, -randZ, num)));
            yield return null;
        }

        CurrentCard++;

        card.localPosition = new Vector3(card.localPosition.x, randY, card.localPosition.z);
        card.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -randZ));
    }
}