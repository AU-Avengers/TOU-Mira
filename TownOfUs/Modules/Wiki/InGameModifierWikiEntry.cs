using Il2CppInterop.Runtime.InteropTypes.Fields;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using TMPro;
using TownOfUs.Modifiers;
using UnityEngine;

namespace TownOfUs.Modules.Wiki;

public sealed class InGameModifierWikiEntry : MonoBehaviour
{
    public Il2CppReferenceField<SpriteRenderer> EntryIconRenderer;
    public Il2CppReferenceField<TextMeshPro> EntryNameTmp;
    public Il2CppReferenceField<TextMeshPro> EntryTeamTmp;
    public Il2CppReferenceField<SpriteRenderer> EntryColorRenderer;
    public Il2CppReferenceField<TextMeshPro> EntryAmountTmp;
    public Il2CppReferenceField<TextMeshPro> EntrySourceTmp;
    public Il2CppReferenceField<ButtonRolloverHandler> RolloverHandler;
    public Il2CppReferenceField<SpriteRenderer> ButtonRenderer; public BaseModifier Modifier { get; set; } public string EntryTitle { get; set; } public string EntryTeam { get; set; } public string EntrySource { get; set; }

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
        EntryAmountTmp.Value.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{txt}</font>";
        EntryNameTmp.Value.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{EntryTitle}</font>";
        if (amount == 0)
        {
            var baseColor = new Color32(210, 210, 210, 255);
            ButtonRenderer.Value.color = baseColor;
            RolloverHandler.Value.OutColor = baseColor;
            RolloverHandler.Value.UnselectedColor = baseColor;
            RolloverHandler.Value.OverColor = new Color32(196, 196, 196, 255);
        }
        else
        {
            var baseColor = Color.white;
            ButtonRenderer.Value.color = baseColor;
            RolloverHandler.Value.OutColor = baseColor;
            RolloverHandler.Value.UnselectedColor = baseColor;
            RolloverHandler.Value.OverColor = new Color32(202, 202, 202, 255);
        }
    }

    public void SetInitialData(BaseModifier mod, Sprite sprite, string team, Color color, string source)
    {
        Modifier = mod;
        EntryTeam = team;
        EntrySource = source;
        EntryIconRenderer.Value.sprite = sprite;
        EntryIconRenderer.Value.SetSizeLimit(0.75f);
        SetData();
        EntryTeamTmp.Value.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Masked\">{team}</font>";
        EntryTeamTmp.Value.SetOutlineColor(Color.black);
        EntryTeamTmp.Value.SetOutlineThickness(0.35f);
        EntryColorRenderer.Value.color = color;
        EntrySourceTmp.Value.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{source}</font>";
        EntryAmountTmp.Value.m_maxWidth = EntryAmountTmp.Value.maxWidth + 0.1f;
    }
}