using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using TMPro;
using TownOfUs.Modifiers;
using UnityEngine;

namespace TownOfUs.Modules.Wiki;

public sealed class InGameModifierWikiEntry : MonoBehaviour
{
    public SpriteRenderer EntryIconRenderer;
    public TextMeshPro EntryNameTmp;
    public TextMeshPro EntryTeamTmp;
    public SpriteRenderer EntryColorRenderer;
    public TextMeshPro EntryAmountTmp;
    public TextMeshPro EntrySourceTmp;
    public ButtonRolloverHandler RolloverHandler;
    public SpriteRenderer ButtonRenderer; public BaseModifier Modifier { get; set; } public string EntryTitle { get; set; } public string EntryTeam { get; set; } public string EntrySource { get; set; }

    public void SetData()
    {
        var amount = Modifier is GameModifier gameMod ? gameMod.GetAmountPerGame() : 0;
        var chance = Modifier is GameModifier gameMod2 ? gameMod2.GetAssignmentChance() : 0;
        if (Modifier is TouBaseGameModifier touMod)
        {
            amount = touMod.CustomAmount;
            chance = touMod.CustomChance;
        }

        var txt = amount != 0
            ? $"{TouLocale.Get("Amount", "Amount")}: {amount} - {TouLocale.Get("Chance", "Chance")}: {chance}%"
            : $"{TouLocale.Get("Amount", "Amount")}: 0";

        EntryTitle = Modifier.ModifierName;
        gameObject.name = $"{EntryTitle.ToLower(TownOfUsPlugin.Culture)} - {EntryTeam.ToLower(TownOfUsPlugin.Culture)} - {EntrySource.ToLower(TownOfUsPlugin.Culture)}";
        EntryAmountTmp.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{txt}</font>";
        EntryNameTmp.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{EntryTitle}</font>";
        if (amount == 0)
        {
            var baseColor = new Color32(210, 210, 210, 255);
            ButtonRenderer.color = baseColor;
            RolloverHandler.OutColor = baseColor;
            RolloverHandler.UnselectedColor = baseColor;
            RolloverHandler.OverColor = new Color32(196, 196, 196, 255);
        }
        else
        {
            var baseColor = Color.white;
            ButtonRenderer.color = baseColor;
            RolloverHandler.OutColor = baseColor;
            RolloverHandler.UnselectedColor = baseColor;
            RolloverHandler.OverColor = new Color32(202, 202, 202, 255);
        }
    }

    public void SetInitialData(BaseModifier mod, Sprite sprite, string team, Color color, string source)
    {
        Modifier = mod;
        EntryTeam = team;
        EntrySource = source;
        EntryIconRenderer.sprite = sprite;
        EntryIconRenderer.SetSizeLimit(0.75f);
        SetData();
        EntryTeamTmp.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Masked\">{team}</font>";
        EntryTeamTmp.SetOutlineColor(Color.black);
        EntryTeamTmp.SetOutlineThickness(0.35f);
        EntryColorRenderer.color = color;
        EntrySourceTmp.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{source}</font>";
        EntryAmountTmp.m_maxWidth = EntryAmountTmp.maxWidth + 0.1f;
    }
}