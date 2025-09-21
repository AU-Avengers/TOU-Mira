using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
public sealed class GuesserMenu(IntPtr cppPtr) : Minigame(cppPtr)
{
    private UiElement? backButton;
    private int currentPage;
    private UiElement? defaultButtonSelected;
    private Action<BaseModifier>? onModifierClick;
    private Action<RoleBehaviour>? onRoleClick;
    private ShapeshifterPanel? panelPrefab;
    private List<ShapeshifterPanel>? potentialVictims;
    private float xOffset = 1.95f;
    private float xStart = -0.8f;
    private float yOffset = -0.65f;
    private float yStart = 2.15f;

    public void OnDisable()
    {
        ControllerManager.Instance.CloseOverlayMenu(name);
    }

    public static GuesserMenu Create()
    {
        var shapeShifterRole = RoleManager.Instance.GetRole(RoleTypes.Shapeshifter);

        var ogMenu = shapeShifterRole.TryCast<ShapeshifterRole>()!.ShapeshifterMenu;
        var newMenu = Instantiate(ogMenu);
        var customMenu = newMenu.gameObject.AddComponent<GuesserMenu>();

        customMenu.panelPrefab = newMenu.PanelPrefab;
        customMenu.xStart = newMenu.XStart;
        customMenu.yStart = newMenu.YStart;
        customMenu.xOffset = newMenu.XOffset;
        customMenu.yOffset = newMenu.YOffset;
        customMenu.defaultButtonSelected = newMenu.DefaultButtonSelected;
        customMenu.backButton = newMenu.BackButton;

        var back = customMenu.backButton.GetComponent<PassiveButton>();
        back.OnClick.RemoveAllListeners();
        back.OnClick.AddListener((UnityAction)(Action)customMenu.Close);

        customMenu.CloseSound = newMenu.CloseSound;
        customMenu.logger = newMenu.logger;
        customMenu.OpenSound = newMenu.OpenSound;

        newMenu.DestroyImmediate();

        customMenu.transform.SetParent(Camera.main.transform, false);
        customMenu.transform.localPosition = new Vector3(0f, 0f, -60f);

        var nextButton = Instantiate(customMenu.backButton, customMenu.transform).gameObject;
        nextButton.transform.localPosition = new Vector3(-1.85f, -2.185f, -60f);
        nextButton.transform.localScale = new Vector3(0.65f, 0.65f, 1);
        nextButton.name = "RightArrowButton";
        nextButton.GetComponent<SpriteRenderer>().sprite = MiraAssets.NextButton.LoadAsset();
        nextButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().DestroyImmediate();

        var passiveButton = nextButton.gameObject.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            customMenu.currentPage++;
            if (customMenu.currentPage > Mathf.CeilToInt(customMenu.potentialVictims!.Count / 15f) - 1)
            {
                customMenu.currentPage = 0;
            }

            customMenu.ShowPage();
        }));

        var backButton = Instantiate(nextButton, customMenu.transform).gameObject;
        nextButton.transform.localPosition = new Vector3(1.85f, -2.185f, -60f);
        backButton.name = "LeftArrowButton";
        backButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        backButton.GetComponent<SpriteRenderer>().flipX = true;
        backButton.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            customMenu.currentPage--;
            if (customMenu.currentPage < 0)
            {
                customMenu.currentPage = Mathf.CeilToInt(customMenu.potentialVictims!.Count / 15f) - 1;
            }

            customMenu.ShowPage();
        }));
        customMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        customMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        return customMenu;
    }

    public Il2CppSystem.Collections.Generic.List<UiElement> ShowPage()
    {
        foreach (var panel in potentialVictims!)
        {
            panel.gameObject.SetActive(false);
        }

        var list = potentialVictims.Skip(currentPage * 15).Take(15).ToList();
        var list2 = new Il2CppSystem.Collections.Generic.List<UiElement>();

        foreach (var panel in list)
        {
            panel.gameObject.SetActive(true);
            list2.Add(panel.Button);
        }

        return list2;
    }

    [HideFromIl2Cpp]
    public void Begin(Func<RoleBehaviour, bool> roleMatch, Action<RoleBehaviour> roleClickHandler,
        Func<BaseModifier, bool>? modifierMatch = null, Action<BaseModifier>? modifierClickHandler = null)
    {
        MinigameStubs.Begin(this, null);

        onRoleClick = roleClickHandler;
        onModifierClick = modifierClickHandler;
        potentialVictims = [];

        var roles = MiscUtils.GetPotentialRoles().Where(roleMatch).ToList();
        
        // This code adds Amne, Surv, Jester, and Merc based on Guardian Angel and Executioner Death settings
        var allRoles = MiscUtils.AllRoles.Where(roleMatch).Where(x => x is IGuessable && !roles.Contains(x)).ToList();

        if (allRoles.Count > 0)
        {
            foreach (var addedRole in allRoles)
            {
                if (addedRole is IGuessable guessable && guessable.CanBeGuessed)
                {
                    roles.Add(addedRole);
                }
            }
        }
        
        var newRoleList = roles.OrderBy(x =>
            LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.SortGuessingByAlignmentToggle.Value
                ? x.GetRoleAlignment().ToDisplayString() + x.GetRoleName()
                : x.GetRoleName()).ToList();

        for (var i = 0; i < newRoleList.Count; i++)
        {
            var role = newRoleList[i];
            var num = i % 3;
            var num2 = i / 3 % 5;

            var shapeshifterPanel = Instantiate(panelPrefab, transform);
            shapeshifterPanel!.transform.localPosition =
                new Vector3(xStart + num * xOffset, yStart + num2 * yOffset, -1f);
            shapeshifterPanel.SetRole(i, role, () => { onRoleClick(role); });
            shapeshifterPanel.gameObject.transform.FindChild("Nameplate").FindChild("Highlight")
                .FindChild("ShapeshifterIcon").gameObject.SetActive(false);

            potentialVictims.Add(shapeshifterPanel);
        }

        if (modifierMatch != null && onModifierClick != null)
        {
            var modifiers = MiscUtils.AllModifiers.Where(modifierMatch).OrderBy(x => x.ModifierName).ToList();

            for (var i = 0; i < modifiers.Count; i++)
            {
                var index = newRoleList.Count + i;
                var modifier = modifiers[i];
                var num = index % 3;
                var num2 = index / 3 % 5;

                var shapeshifterPanel = Instantiate(panelPrefab, transform);
                shapeshifterPanel!.transform.localPosition =
                    new Vector3(xStart + num * xOffset, yStart + num2 * yOffset, -1f);
                shapeshifterPanel.SetModifier(index, modifier, () => { onModifierClick(modifier); });
                shapeshifterPanel.gameObject.transform.FindChild("Nameplate").FindChild("Highlight")
                    .FindChild("ShapeshifterIcon").gameObject.SetActive(false);

                potentialVictims.Add(shapeshifterPanel);
            }
        }

        var list2 = ShowPage();

        ControllerManager.Instance.OpenOverlayMenu(name, backButton, defaultButtonSelected, list2);

        MeetingHud.Instance.playerStates.Do(x => x.gameObject.SetActive(false));
    }

    public override void Close()
    {
        MinigameStubs.Close(this);

        MeetingHud.Instance.playerStates.Do(x => x.gameObject.SetActive(true));
    }
}