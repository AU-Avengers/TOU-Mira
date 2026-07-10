using UnityEngine;
using TMPro;

namespace TownOfUs.Modules.Wiki;

public class InGameModifierWikiEntry : MonoBehaviour
{
    public SpriteRenderer EntryIconRenderer;
    public TextMeshPro EntryNameTmp;
    public TextMeshPro EntryTeamTmp;
    public SpriteRenderer EntryColorRenderer;
    public TextMeshPro EntryAmountTmp;
    public TextMeshPro EntrySourceTmp;
    public ButtonRolloverHandler RolloverHandler;
    public SpriteRenderer ButtonRenderer;
}
