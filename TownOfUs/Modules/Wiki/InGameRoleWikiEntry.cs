using MiraAPI.Roles;
using MiraAPI.Utilities;
using TMPro;
using UnityEngine;

namespace TownOfUs.Modules.Wiki;

public sealed class InGameRoleWikiEntry : MonoBehaviour
{
    public SpriteRenderer EntryIconRenderer;
    public TextMeshPro EntryNameTmp;
    public TextMeshPro EntryTeamTmp;
    public SpriteRenderer EntryColorRenderer;
    public TextMeshPro EntryAmountTmp;
    public TextMeshPro EntrySourceTmp;
    public ButtonRolloverHandler RolloverHandler;
    public SpriteRenderer ButtonRenderer; public RoleBehaviour RoleBehaviour { get; set; } public ICustomRole? CustomRole { get; set; } public string EntryTitle { get; set; } public string EntryTeam { get; set; } public string EntrySource { get; set; } public bool HasNoCount { get; set; }

    public void SetData()
    {
        int amount = 0;
        if (!HasNoCount)
        {
            int chance;
            if (CustomRole != null)
            {
                amount = (int)CustomRole.GetCount()!;
                chance = (int)CustomRole.GetChance()!;
            }
            else
            {
                var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
                var roleOptions = currentGameOptions.RoleOptions;

                amount = roleOptions.GetNumPerGame(RoleBehaviour.Role);
                chance = roleOptions.GetChancePerGame(RoleBehaviour.Role);
            }

            var txt = amount != 0
                ? $"{TouLocale.Get("Amount", "Amount")}: {amount} - {TouLocale.Get("Chance", "Chance")}: {chance}%"
                : $"{TouLocale.Get("Amount", "Amount")}: 0";
            EntryAmountTmp.text =
                $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{txt}</font>";
        }
        EntryTitle = RoleBehaviour.GetRoleName();
        gameObject.name =
            $"{EntryTitle.ToLower(TownOfUsPlugin.Culture)} - {EntryTeam.ToLower(TownOfUsPlugin.Culture)} - {EntrySource.ToLower(TownOfUsPlugin.Culture)}";

        EntryNameTmp.text =
            $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{EntryTitle}</font>";
        if (!HasNoCount && amount == 0)
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

    public void SetInitialData(RoleBehaviour role, Sprite sprite, string team, Color color, string source)
    {
        RoleBehaviour = role;
        var roleEntry = SoftWikiEntries.RoleEntries.GetValueOrDefault(RoleBehaviour)!;
        roleEntry.EntryName = TranslationController.Instance.GetString(RoleBehaviour.StringName);
        roleEntry.GetAdvancedDescription = TranslationController.Instance.GetString(RoleBehaviour.BlurbNameLong);
        if (roleEntry.GetAdvancedDescription.Contains("STRMISS"))
        {
            var baseName = ($"{RoleBehaviour.StringName}").Replace("Role", "");
            if (Enum.TryParse<StringNames>($"RolesHelp_{baseName}_01", out var helpName))
            {
                roleEntry.GetAdvancedDescription = TranslationController.Instance.GetString(helpName);
            }
        }
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

    public void SetInitialData(RoleBehaviour role, ICustomRole customRole, Sprite sprite, string team, Color color, string source)
    {
        RoleBehaviour = role;
        CustomRole = customRole;
        HasNoCount = CustomRole.Configuration.MaxRoleCount == 0 || CustomRole.Configuration.HideSettings;
        if (HasNoCount)
        {
            EntryNameTmp.transform.localPosition -= new Vector3(0, 0.1f, 0);
            EntryAmountTmp.text = string.Empty;
        }
        if (SoftWikiEntries.RoleEntries.ContainsKey(RoleBehaviour))
        {
            SoftWikiEntries.RoleEntries.GetValueOrDefault(RoleBehaviour)!.EntryName = CustomRole.RoleName;
            SoftWikiEntries.RoleEntries.GetValueOrDefault(RoleBehaviour)!.GetAdvancedDescription =
                CustomRole.RoleDescription + MiscUtils.AppendOptionsText(RoleBehaviour.GetType());
        }
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