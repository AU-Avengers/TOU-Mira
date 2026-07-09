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
    [HideFromIl2Cpp]
    public void Begin(Func<PlayerControl, bool> playerMatch, Action<PlayerControl, PlayerControl> onClick)
    {
        Begin(
            playerMatch,
            plr =>
            {
                if (plr == null)
                {
                    return;
                }

                if (target1 == null) // Set first choice
                {
                    target1 = plr;
                    var targetPanel = this.GetVictimPanel(target1.Data);
                    SetNameplateAppearance(targetPanel, hoverDeselectSprite, hoverDeselectColor, activeColor);
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
        foreach (var victim in potentialVictims)
        {
            SetNameplateAppearance(victim, hoverSelectSprite, hoverSelectColor, Color.clear);
        }
    }

    private static void SetNameplateAppearance(ShapeshifterPanel panel,
        LoadableAsset<Sprite>? sprite, Color? overColor, Color? unselectedColor)
    {
        var nameplate = panel.gameObject.transform.FindChild("Nameplate");
        if (sprite != null)
        {
            nameplate.FindChild("Highlight").FindChild("ShapeshifterIcon")
                .GetComponent<SpriteRenderer>().sprite = sprite.LoadAsset();
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
    }
}
