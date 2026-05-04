using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
public class InventoryTabPaginationBehaviour(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public TextMeshPro title;
    public PassiveButton backButton;
    public PassiveButton nextButton;
    public InventoryTab tab;

    public int CurrentTab { get; private set; }
    public int MaxTab { get; private set; }

    public Func<string> GetTextFunction { get; private set; }

    public void Setup(InventoryTab inventoryTab, int maxTab, Func<string> getTextFunction)
    {
        tab = inventoryTab;
        MaxTab = maxTab;
        GetTextFunction = getTextFunction;

        if (!nextButton)
        {
            nextButton = Instantiate(PlayerCustomizationMenu.Instance.BackButton, inventoryTab.transform).GetComponent<PassiveButton>();
            nextButton.GetComponent<AspectPosition>().DestroyImmediate();
            nextButton.GetComponent<CloseButtonConsoleBehaviour>().DestroyImmediate();
            nextButton.name = "nextButton";
            nextButton.transform.localPosition = new Vector3(2.91f, -0.23f, -55f);
            nextButton.transform.localScale = new Vector3(0.5f, 0.5f, 1);
            nextButton.OnClick = new Button.ButtonClickedEvent();
            nextButton.OnClick.AddListener((UnityAction)NextPage);
            nextButton.GetComponent<SpriteRenderer>().sprite = MiraAssets.NextButton.LoadAsset();
        }

        if (!backButton)
        {
            backButton = Instantiate(nextButton, inventoryTab.transform);
            backButton.name = "backButton";
            backButton.transform.localPosition = new Vector3(-1.19f, -0.23f, -55f);
            backButton.OnClick = new Button.ButtonClickedEvent();
            backButton.OnClick.AddListener((UnityAction)PreviousPage);
            backButton.GetComponent<SpriteRenderer>().flipX = true;
        }

        if (!title)
        {
            title = inventoryTab.transform.Find("Text").GetComponent<TextMeshPro>();
            title.GetComponent<TextTranslatorTMP>().DestroyImmediate();
            title.text = GetTextFunction();
            title.alignment = TextAlignmentOptions.Center;
            title.transform.localPosition =  new Vector3(0.86f, -0.23f, -55f);
            title.fontSize = title.fontSizeMax = 6;
            title.rectTransform.sizeDelta = new Vector2(3.5f, 1f);
        }
    }

    private void Update()
    {
        if (title)
        {
            title.text = GetTextFunction();
        }
    }

    public void NextPage()
    {
        CurrentTab++;
        if (CurrentTab > MaxTab)
        {
            CurrentTab = 0;
        }

        if (tab)
        {
            tab.enabled = false;
            tab.enabled = true;
        }
    }

    public void PreviousPage()
    {
        CurrentTab--;
        if (CurrentTab < 0) CurrentTab = MaxTab;

        if (tab)
        {
            tab.enabled = false;
            tab.enabled = true;
        }
    }
}