using HarmonyLib;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Utilities;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace TownOfUs.Modules;

// Code Review: This works, and isn't too bad but could definitely be improved by yet again using a MonoBehaviour.
public sealed class MeetingMenu : IDisposable
{
    public delegate bool Exemption(PlayerVoteArea voteArea);

    public delegate void OnClick(PlayerVoteArea voteArea, MeetingHud meeting);

    public MeetingMenu(
        RoleBehaviour owner,
        OnClick onClick,
        MeetingAbilityType abilityType,
        LoadableAsset<Sprite> activeSprite,
        LoadableAsset<Sprite> disabledSprite = null!,
        Exemption exemption = null!,
        Color? activeColor = null!,
        Color? disabledColor = null!,
        Color? hoverColor = null!,
        Vector3? position = null!)
    {
        Owner = owner;
        Click = onClick ?? throw new ArgumentException("onClick should exist");
        IsExempt = exemption;
        ActiveSprite = activeSprite;
        DisabledSprite = disabledSprite;
        ActiveColor = activeColor ?? Color.green;
        DisabledColor = disabledColor ?? Color.white;
        HoverColor = hoverColor ?? Color.red;
        Type = abilityType;
        Position = position ?? new Vector3(-0.95f, 0.03f, -3f);

        Instances.Add(this);
    }

    public static List<MeetingMenu> Instances { get; set; } = [];

    public RoleBehaviour Owner { get; }
    public OnClick Click { get; }
    public Exemption IsExempt { get; }
    public LoadableAsset<Sprite> ActiveSprite { get; }
    public LoadableAsset<Sprite> DisabledSprite { get; }
    private Color ActiveColor { get; }
    private Color DisabledColor { get; }
    private Color HoverColor { get; }
    public MeetingAbilityType Type { get; }
    public Vector3 Position { get; set; }
    public Dictionary<byte, bool> Actives { get; } = [];
    public Dictionary<byte, GameObject> Buttons { get; } = [];
    private Dictionary<byte, SpriteRenderer> ButtonSprites { get; } = [];

    public void Dispose()
    {
        HideButtons();
    }

    public void HideButtons()
    {
        Buttons.Keys.ToList().ForEach(HideSingle);
        Buttons.Clear();
        Actives.Clear();
        ButtonSprites.Clear();
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

    private void GenButton(PlayerVoteArea voteArea, MeetingHud __instance)
    {
        Actives.Add(voteArea.TargetPlayerId, false);

        if (IsExempt(voteArea))
        {
            Buttons.Add(voteArea.TargetPlayerId, null!);
            ButtonSprites.Add(voteArea.TargetPlayerId, null!);
            return;
        }

        var targetBox = UObject.Instantiate(
            voteArea.Buttons.transform.Find("CancelButton").gameObject,
            voteArea.transform);
        targetBox.name = $"MeetingButton{Owner.GetRoleName().Replace(" ", "")}{voteArea.TargetPlayerId}";
        targetBox.transform.localPosition = Position;
        var renderer = targetBox.GetComponent<SpriteRenderer>();
        renderer.sprite = (Type == MeetingAbilityType.Toggle ? DisabledSprite : ActiveSprite).LoadAsset();
        var button = targetBox.GetComponent<PassiveButton>();
        button.OverrideOnClickListeners(() => Click(voteArea, __instance));
        button.OverrideOnMouseOverListeners(() => renderer.color = HoverColor);
        button.OverrideOnMouseOutListeners(() => renderer.color =
            Type == MeetingAbilityType.Toggle && Actives[voteArea.TargetPlayerId]
                ? ActiveColor
                : DisabledColor);
        var collider = targetBox.GetComponent<BoxCollider2D>();
        collider.size = renderer.sprite.bounds.size;
        collider.offset = Vector2.zero;
        targetBox.transform.GetChild(0).gameObject.Destroy();
        Buttons.Add(voteArea.TargetPlayerId, targetBox);
        ButtonSprites.Add(voteArea.TargetPlayerId, renderer);
    }

    public void GenButtons(MeetingHud meeting, bool usable)
    {
        HideButtons();

        // Logger<TownOfUsPlugin>.Message($"MeetingMenu.GenButtons '{Owner.Player.Data.PlayerName}' AmOwner: {Owner.Player.AmOwner}");
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

            pair.Value.sprite = (Actives[pair.Key] ? ActiveSprite : DisabledSprite).LoadAsset();
            pair.Value.color = Actives[pair.Key] ? ActiveColor : DisabledColor;
        }
    }

    public static void ClearAll()
    {
        Instances.Do(x => x.Dispose());
    }
}

public enum MeetingAbilityType : byte
{
    Toggle,
    Click
}