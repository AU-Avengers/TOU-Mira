using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace TownOfUs.Modules.Components;

/// <summary>
/// <inheritdoc/>
/// <para/>
/// Specifically used for selecting two players.
/// </summary>
/// <param name="il2CppPtr"><inheritdoc/></param>
[RegisterInIl2Cpp]
public class DoublePlayerMenu(IntPtr il2CppPtr) : CustomPlayerMenu(il2CppPtr)
{
    public PlayerControl? target1;
    private LoadableAsset<Sprite>? hoverSelectSprite;
    private LoadableAsset<Sprite>? hoverDeselectSprite;
    private Color? activeColor;
    private Color? hoverSelectColor;
    private Color? hoverDeselectColor;
    // These are the Highlight, Icon, and IsSelected variable respectively.
    private Action<SpriteRenderer, SpriteRenderer, bool>? onMouseOverAction;
    private Action<SpriteRenderer, SpriteRenderer, bool>? onMouseOutAction;

    public static DoublePlayerMenu Create()
    {
        var shapeShifterRole = RoleManager.Instance.GetRole(RoleTypes.Shapeshifter);

        var ogMenu = shapeShifterRole.TryCast<ShapeshifterRole>()!.ShapeshifterMenu;
        var newMenu = Instantiate(ogMenu);
        var customMenu = newMenu.gameObject.AddComponent<DoublePlayerMenu>();

        customMenu.panelPrefab = newMenu.PanelPrefab;
        customMenu.xStart = newMenu.XStart;
        customMenu.yStart = newMenu.YStart;
        customMenu.xOffset = newMenu.XOffset;
        customMenu.yOffset = newMenu.YOffset;
        customMenu.backButton = newMenu.BackButton;
        var back = customMenu.backButton.GetComponent<PassiveButton>();
        back.OnClick.RemoveAllListeners();
        back.OnClick.AddListener((UnityAction)(() =>
        {
            Instance.Close();
        }));

        customMenu.CloseSound = newMenu.CloseSound;
        customMenu.logger = newMenu.logger;
        customMenu.OpenSound = newMenu.OpenSound;

        newMenu.DestroyImmediate();

        customMenu.transform.SetParent(Camera.main!.transform, false);
        customMenu.transform.localPosition = new Vector3(0f, 0f, -50f);

        return customMenu;
    }

    public static DoublePlayerMenu Create(
        Color? activeColor,
        LoadableAsset<Sprite>? hoverSelectSprite = null,
        Color? hoverSelectColor = null,
        LoadableAsset<Sprite>? hoverDeselectSprite = null,
        Color? hoverDeselectColor = null)
    {
        var customMenu = Create();

        customMenu.activeColor = activeColor;

        customMenu.hoverSelectSprite = hoverSelectSprite;
        customMenu.hoverSelectColor = hoverSelectColor;

        customMenu.hoverDeselectSprite = hoverDeselectSprite ?? hoverSelectSprite;
        customMenu.hoverDeselectColor = hoverDeselectColor ?? hoverSelectColor;

        return customMenu;
    }

    /// <summary>
    /// Begins/opens the custom player menu.
    /// </summary>
    /// <param name="playerMatch">Function to determine if player should show in the custom menu.</param>
    /// <param name="onClick"><see cref="PassiveButton.OnClick"/> action for player.</param>
    /// <param name="onMouseOut">Function that can optionally be run when the mouse is moved outside a player panel.</param>
    /// <param name="onMouseOver">Function that can optionally be run when the mouse is moved over a player panel.</param>
    [HideFromIl2Cpp]
    public void Begin(Func<PlayerControl, bool> playerMatch, Action<PlayerControl, PlayerControl> onClick, Action<SpriteRenderer, SpriteRenderer, bool>? onMouseOut = null, Action<SpriteRenderer, SpriteRenderer, bool>? onMouseOver = null)
    {
        Begin(
            playerMatch,
            plr =>
            {
                if (plr == null) // Close the menu
                {
                    ForceClose();
                    target1 = null;
                    return;
                }

                if (target1 == null) // Set first choice
                {
                    target1 = plr;
                    var targetPanel = this.GetVictimPanel(target1.Data);
                    SetNameplateAppearance(targetPanel, hoverDeselectSprite, hoverDeselectColor, activeColor, true);
                    return;
                }
                if (target1.PlayerId == plr.PlayerId) // Unselect first choice
                {
                    var targetPanel = this.GetVictimPanel(target1.Data);
                    SetNameplateAppearance(targetPanel, hoverSelectSprite, hoverSelectColor, Color.clear);
                    target1 = null;
                    return;
                }

                onClick(target1, plr);
            }
        );
        onMouseOverAction = onMouseOver;
        onMouseOutAction = onMouseOut;
        foreach (var victim in potentialVictims)
        {
            SetNameplateAppearance(victim, hoverSelectSprite, hoverSelectColor, Color.clear);
        }
    }

    [HideFromIl2Cpp]
    private void SetNameplateAppearance(ShapeshifterPanel panel,
        LoadableAsset<Sprite>? sprite, Color? overColor, Color? unselectedColor, bool isSelected = false)
    {
        var nameplate = panel.gameObject.transform.FindChild("Nameplate");
        var highlight = nameplate.FindChild("Highlight").GetComponent<SpriteRenderer>();
        var icon = highlight.transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            icon.sprite = sprite.LoadAsset();
        }
        var button = nameplate.GetComponent<ButtonRolloverHandler>();
        if (overColor is { } oColor)
        {
            button.OverColor = oColor;
        }
        if (unselectedColor is { } uColor)
        {
            button.UnselectedColor = uColor;
        }

        if (onMouseOverAction != null)
        {
            panel.Button.OnMouseOver.RemoveAllListeners();
            panel.Button.OnMouseOver = new UnityEvent();
            panel.Button.OnMouseOver.AddListener((UnityAction)(() => onMouseOverAction(highlight, icon, isSelected)));
            if (isSelected)
            {
                onMouseOverAction(highlight, icon, isSelected);
            }
        }
        if (onMouseOutAction != null)
        {
            panel.Button.OnMouseOut.RemoveAllListeners();
            panel.Button.OnMouseOut = new UnityEvent();
            panel.Button.OnMouseOut.AddListener((UnityAction)(() => onMouseOutAction(highlight, icon, isSelected)));
            if (!isSelected)
            {
                onMouseOutAction(highlight, icon, isSelected);
            }
        }
    }
}
