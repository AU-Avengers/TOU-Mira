using HarmonyLib;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace TownOfUs.Modules;

[RegisterInIl2Cpp]
public sealed class MeetingMenu(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    public delegate bool Exemption(PlayerVoteArea voteArea);

    public delegate void OnClick(PlayerVoteArea voteArea, MeetingHud meeting);

    public delegate void ButtonSetupAction(PlayerVoteArea voteArea, GameObject button);

    public static MeetingMenu CreateMenu(
        RoleBehaviour owner,
        OnClick onClick,
        MeetingAbilityType abilityType,
        LoadableAsset<Sprite> activeSprite,
        LoadableAsset<Sprite> disabledSprite = null!,
        Exemption exemption = null!,
        Color? activeColor = null!,
        Color? disabledColor = null!,
        Color? hoverColor = null!,
        Vector3? position = null!,
        Exemption startActive = null!,
        ButtonSetupAction buttonSetup = null!)
    {
        return CreateMenu(owner, onClick, string.Empty, abilityType, activeSprite, disabledSprite, exemption,
              activeColor, disabledColor, hoverColor, position, startActive, buttonSetup);
    }

    public static MeetingMenu CreateMenu(
        RoleBehaviour owner,
        OnClick onClick,
        string abilityName,
        MeetingAbilityType abilityType,
        LoadableAsset<Sprite> activeSprite,
        LoadableAsset<Sprite> disabledSprite = null!,
        Exemption exemption = null!,
        Color? activeColor = null!,
        Color? disabledColor = null!,
        Color? hoverColor = null!,
        Vector3? position = null!,
        Exemption startActive = null!,
        ButtonSetupAction buttonSetup = null!)
    {
        var obj = new GameObject();
        var menu = obj.AddComponent<MeetingMenu>();

        menu._obj = obj;
        menu.Owner = owner;
        menu.Click = onClick ?? throw new ArgumentException("onClick should exist");
        menu.IsExempt = exemption;
        menu.ActiveSprite = activeSprite;
        menu.DisabledSprite = disabledSprite;
        menu.ActiveColor = activeColor ?? Color.green;
        menu.DisabledColor = disabledColor ?? Color.white;
        menu.HoverColor = hoverColor ?? Color.red;
        menu.Type = abilityType;
        menu.Position = position ?? new Vector3(-0.95f, 0.03f, -3f);
        menu.AbilityName = abilityName;
        menu.StartActive = startActive;
        menu.ButtonSetup = buttonSetup;

        Instances.Add(menu);

        return menu;
    }

    public static MeetingMenu CreateMenu(
        RoleBehaviour owner,
        LoadableAsset<Sprite> activeSprite,
        LoadableAsset<Sprite> disabledSprite = null!,
        Exemption exemption = null!,
        Color? activeColor = null!,
        Color? disabledColor = null!,
        Color? hoverColor = null!,
        Vector3? position = null!,
        Exemption startActive = null!,
        ButtonSetupAction buttonSetup = null!)
    {
        return CreateMenu(owner, (_, _) => { }, string.Empty, MeetingAbilityType.None, activeSprite, disabledSprite, exemption,
              activeColor ?? Color.white, disabledColor, hoverColor ?? Color.white, position, startActive, buttonSetup);
    }

    public static List<MeetingMenu> Instances { get; } = [];

    private GameObject _obj;

    public RoleBehaviour Owner { get; private set; }
    public OnClick Click { get; private set; }
    public Exemption IsExempt { get; private set; }
    public Exemption StartActive { get; private set; }
    public ButtonSetupAction ButtonSetup { get; private set; }
    public LoadableAsset<Sprite> ActiveSprite { get; private set; }
    public LoadableAsset<Sprite> DisabledSprite { get; private set; }
    public string AbilityName { get; private set; }
    private Color ActiveColor { get; set; }
    private Color DisabledColor { get; set; }
    private Color HoverColor { get; set; }
    public MeetingAbilityType Type { get; private set; }
    public Vector3 Position { get; private set; }
    public Dictionary<byte, bool> Actives { get; } = [];
    public Dictionary<byte, GameObject> Buttons { get; } = [];
    private Dictionary<byte, SpriteRenderer> ButtonSprites { get; } = [];

    public void Destroy()
    {
        HideButtons();
        if (_obj)
        {
            Destroy(_obj);
        }
        Instances.Remove(this);
    }

    public void HideButtons()
    {
        Buttons.Keys.ToList().ForEach(HideSingle);
        Buttons.Clear();
        Actives.Clear();
        ButtonSprites.Clear();
    }

    public static void HideButtonsForAll()
    {
        Instances.Do(x => x.HideButtons());
    }

    public void HideSingle(byte targetId)
    {
        Actives[targetId] = false;

        if (!Buttons.TryGetValue(targetId, out var button) || !button)
        {
            return;
        }

        button.SetActive(false);
        button.Destroy();
        Buttons[targetId] = null!;
        ButtonSprites[targetId] = null!;
    }

    public static void HideSingleForAll(byte targetId)
    {
        Instances.Do(x => x.HideSingle(targetId));
    }

    private void GenButton(PlayerVoteArea voteArea, MeetingHud __instance)
    {
        Actives.Add(voteArea.PlayerId, false);

        if (IsExempt?.Invoke(voteArea) == true)
        {
            Buttons.Add(voteArea.PlayerId, null!);
            ButtonSprites.Add(voteArea.PlayerId, null!);
            return;
        }

        var targetBox = Instantiate(
            voteArea.Buttons.transform.Find("CancelButton").gameObject,
            voteArea.transform);
        targetBox.name = $"MeetingButton{Owner.GetRoleName().Replace(" ", "")}{voteArea.PlayerId}";
        targetBox.transform.localPosition = Position;
        var renderer = targetBox.GetComponent<SpriteRenderer>();

        if (Type != MeetingAbilityType.Click && StartActive != null)
        {
            var active = StartActive(voteArea);

            Actives[voteArea.PlayerId] = active;

            renderer.sprite = active ? ActiveSprite : DisabledSprite;
        }
        else
        {
            renderer.sprite = (Type == MeetingAbilityType.Toggle ? DisabledSprite : ActiveSprite);
        }

        var button = targetBox.GetComponent<PassiveButton>();
        button.OverrideOnClickListeners(() => Click(voteArea, __instance));
        button.OverrideOnMouseOverListeners(() => renderer.color = HoverColor);
        button.OverrideOnMouseOutListeners(() => renderer.color =
            Type != MeetingAbilityType.Click && Actives[voteArea.PlayerId]
                ? ActiveColor
                : DisabledColor);
        var collider = targetBox.GetComponent<BoxCollider2D>();
        collider.size = renderer.sprite.bounds.size;
        collider.offset = Vector2.zero;
        targetBox.transform.GetChild(0).gameObject.DeepDestroy();

        var buttonText = Instantiate(
            __instance.MeetingAbilityButton.buttonLabelText.gameObject,
            targetBox.transform);
        buttonText.transform.localPosition = new Vector3(0, -0.2f, 0f);
        var tmpText = buttonText.GetComponent<TextMeshPro>();
        tmpText.color = Color.white;
        tmpText.text = AbilityName;
        //tmpText.ForceMeshUpdate();
        tmpText.fontSize = 2.5f;
        tmpText.fontSizeMax = 2.5f;
        tmpText.fontSizeMin = 2.5f;
        tmpText.m_enableWordWrapping = false;
        Buttons.Add(voteArea.PlayerId, targetBox);
        ButtonSprites.Add(voteArea.PlayerId, renderer);

        ButtonSetup?.Invoke(voteArea, targetBox);
    }

    public void GenButtons(MeetingHud meeting, bool usable = true)
    {
        if (meeting == null)
        {
            return;
        }

        HideButtons();

        // Message($"MeetingMenu.GenButtons '{Owner.Player.Data.PlayerName}' AmOwner: {Owner.Player.AmOwner}");
        if (!usable || !Owner.Player.AmOwner)
        {
            return;
        }

        Actives.Clear();
        Buttons.Clear();
        ButtonSprites.Clear();
        meeting.playerStates.ToList().ForEach(x => GenButton(x, meeting));
    }

    public void Update()
    {
        if (!MeetingHud.Instance || Type != MeetingAbilityType.Toggle)
        {
            return;
        }

        foreach (var pair in ButtonSprites)
        {
            if (!pair.Value)
            {
                continue;
            }

            pair.Value.sprite = (Actives[pair.Key] ? ActiveSprite : DisabledSprite);
            pair.Value.color = Actives[pair.Key] ? ActiveColor : DisabledColor;
        }
    }

    public static void ClearAll()
    {
        Instances.ProcessAndClear(x => x.Destroy());
    }
}

public enum MeetingAbilityType : byte
{
    None,
    Toggle,
    Click
}