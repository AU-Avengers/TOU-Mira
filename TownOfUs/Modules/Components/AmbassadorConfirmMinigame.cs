using System.Collections;
using System.Diagnostics.CodeAnalysis;
using MiraAPI.Patches.Stubs;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TownOfUs.Modules.Components;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity")]
public sealed class AmbassadorConfirmMinigame : Minigame
{
    public TextMeshPro TitleText;
    public SpriteRenderer RoleIcon;
    public TextMeshPro RetrainText;
    public GameObject Divider;
    public GameObject Box;
    public GameObject DenyButton;
    public GameObject AcceptButton;
    private RoleBehaviour NewRole;

    private readonly Color _bgColor = new Color32(24, 0, 0, 215);
    private Action<bool> clickHandler;

    private void Awake()
    {
        if (Instance)
        {
            Instance.Close();
        }

        TitleText = transform.Find("Status").Find("Title").gameObject.GetComponent<TextMeshPro>();
        RoleIcon = transform.Find("Status").Find("RoleImage").gameObject.GetComponent<SpriteRenderer>();
        RetrainText = transform.Find("Status").Find("RetrainText").gameObject.GetComponent<TextMeshPro>();
        Divider = transform.Find("Status").Find("Divider").gameObject;
        Box = transform.Find("Status").Find("Box").gameObject;
        DenyButton = transform.Find("Status").Find("DenyButton").gameObject;
        AcceptButton = transform.Find("Status").Find("AcceptButton").gameObject;

        TitleText.font = HudManager.Instance.TaskPanel.taskText.font;
        TitleText.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
        TitleText.text = "Ambassador Retrain";

        RetrainText.font = HudManager.Instance.TaskPanel.taskText.font;
        RetrainText.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
        RetrainText.text =
            $"Are you sure you want to be retrained into {NewRole.GetRoleName()}?\nThis change is permanent.";

        RoleIcon.sprite = NewRole.RoleIconWhite ?? TouRoleIcons.Impostor.LoadAsset();

        RoleIcon.SetSizeLimit(2.8f);

        TitleText.gameObject.SetActive(false);
        RoleIcon.gameObject.SetActive(false);
        RetrainText.gameObject.SetActive(false);
        Divider.SetActive(false);
        Box.SetActive(false);
        DenyButton.SetActive(false);
        AcceptButton.SetActive(false);
    }

    public static AmbassadorConfirmMinigame Create()
    {
        var gameObject = Instantiate(TouAssets.ConfirmMinigame.LoadAsset(), HudManager.Instance.transform);
        gameObject.GetComponent<Minigame>().DestroyImmediate();
        gameObject.SetActive(false);

        return gameObject.AddComponent<AmbassadorConfirmMinigame>();
    }

    public void Open(RoleBehaviour role, Action<bool> onClick)
    {
        clickHandler = onClick;
        NewRole = role;

        AmongUsClient.Instance.StartCoroutine(CoOpen(this));
    }

    private static IEnumerator CoOpen(AmbassadorConfirmMinigame minigame)
    {
        while (ExileController.Instance)
        {
            yield return new WaitForSeconds(0.65f);
        }

        minigame.gameObject.SetActive(true);
        minigame.Begin();
    }

    public override void Close()
    {
        HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(_bgColor, Color.clear));
        MinigameStubs.Close(this);
    }

    private void Begin()
    {
        HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, _bgColor));

        TitleText.gameObject.SetActive(true);
        RoleIcon.gameObject.SetActive(true);
        RetrainText.gameObject.SetActive(true);
        Divider.SetActive(true);
        Box.SetActive(true);
        DenyButton.SetActive(true);
        AcceptButton.SetActive(true);

        DenyButton.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
        DenyButton.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            clickHandler.Invoke(false);
        }));

        AcceptButton.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
        AcceptButton.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            clickHandler.Invoke(true);
        }));

        TransType = TransitionType.None;
        Begin(null);
    }
}