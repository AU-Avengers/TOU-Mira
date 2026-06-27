using Il2CppInterop.Runtime.InteropTypes.Fields;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TMPro;
using UnityEngine;

namespace TownOfUs.Modules.Wiki;

public sealed class InGameRoleWikiEntry : MonoBehaviour
{
    public Il2CppReferenceField<SpriteRenderer> EntryIconRenderer;
    public Il2CppReferenceField<TextMeshPro> EntryNameTmp;
    public Il2CppReferenceField<TextMeshPro> EntryTeamTmp;
    public Il2CppReferenceField<SpriteRenderer> EntryColorRenderer;
    public Il2CppReferenceField<TextMeshPro> EntryAmountTmp;
    public Il2CppReferenceField<TextMeshPro> EntrySourceTmp;
    public Il2CppReferenceField<ButtonRolloverHandler> RolloverHandler;
    public Il2CppReferenceField<SpriteRenderer> ButtonRenderer; public RoleBehaviour RoleBehaviour { get; set; } public ICustomRole? CustomRole { get; set; } public string EntryTitle { get; set; } public string EntryTeam { get; set; } public string EntrySource { get; set; } public bool HasNoCount { get; set; }

    public void SetData()
    {
        var amount = 0;
        var chance = 0;
        if (!HasNoCount)
        {
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
            EntryAmountTmp.Value.text =
                $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{txt}</font>";
        }
        EntryTitle = RoleBehaviour.GetRoleName();
        gameObject.name =
            $"{EntryTitle.ToLower(TownOfUsPlugin.Culture)} - {EntryTeam.ToLower(TownOfUsPlugin.Culture)} - {EntrySource.ToLower(TownOfUsPlugin.Culture)}";

        EntryNameTmp.Value.text =
            $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{EntryTitle}</font>";
        if (!HasNoCount && amount == 0)
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

    public void SetInitialData(RoleBehaviour role, ICustomRole customRole, Sprite sprite, string team, Color color, string source)
    {
        RoleBehaviour = role;
        CustomRole = customRole;
        HasNoCount = CustomRole.Configuration.MaxRoleCount == 0 || CustomRole.Configuration.HideSettings;
        if (HasNoCount)
        {
            EntryNameTmp.Value.transform.localPosition -= new Vector3(0, 0.1f, 0);
            EntryAmountTmp.Value.text = string.Empty;
        }
        if (SoftWikiEntries.RoleEntries.ContainsKey(RoleBehaviour))
        {
            SoftWikiEntries.RoleEntries.GetValueOrDefault(RoleBehaviour)!.EntryName = CustomRole.RoleName;
            SoftWikiEntries.RoleEntries.GetValueOrDefault(RoleBehaviour)!.GetAdvancedDescription =
                CustomRole.RoleDescription + MiscUtils.AppendOptionsText(RoleBehaviour.GetType());
        }
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