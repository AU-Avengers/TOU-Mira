using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace TownOfUs.Modules.Wiki;

[RegisterInIl2Cpp]
public sealed class InGameWikiEntry(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    public Il2CppReferenceField<SpriteRenderer> EntryIconRenderer;
    public Il2CppReferenceField<TextMeshPro> EntryNameTmp;
    public Il2CppReferenceField<TextMeshPro> EntryTeamTmp;
    public Il2CppReferenceField<SpriteRenderer> EntryColorRenderer;
    public Il2CppReferenceField<TextMeshPro> EntryAmountTmp;
    public Il2CppReferenceField<TextMeshPro> EntrySourceTmp;
    [HideFromIl2Cpp] public string EntryTitle { get; set; }
    [HideFromIl2Cpp] public string EntryTeam { get; set; }
    [HideFromIl2Cpp] public string EntrySource { get; set; }

    public void SetData(Sprite sprite, string title, string team, Color color, string amount, string source)
    {
        EntryTitle = title;
        EntryTeam = team;
        EntrySource = source;
        gameObject.name = $"{title.ToLower(TownOfUsPlugin.Culture)} - {team.ToLower(TownOfUsPlugin.Culture)} - {source.ToLower(TownOfUsPlugin.Culture)}";
        EntryIconRenderer.Value.sprite = sprite;
        EntryIconRenderer.Value.SetSizeLimit(0.75f);
        EntryNameTmp.Value.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{title}</font>";
        EntryTeamTmp.Value.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Masked\">{team}</font>";
        EntryTeamTmp.Value.SetOutlineColor(Color.black);
        EntryTeamTmp.Value.SetOutlineThickness(0.35f);
        EntryColorRenderer.Value.color = color;
        EntryAmountTmp.Value.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{amount}</font>";
        EntryAmountTmp.Value.m_maxWidth = EntryAmountTmp.Value.maxWidth + 0.1f;
        EntrySourceTmp.Value.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{source}</font>";
    }
}