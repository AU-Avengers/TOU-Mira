using HarmonyLib;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Modifiers;
// This is intended to be used for roles such as Snitch or Mayor
// for getting the role, use this: RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ImitatorRole>())
[MiraIgnore]
public abstract class RevealModifier(int roleChangeResult, bool revealRole, RoleBehaviour role) : TimedModifier
{
    public override string ModifierName => "Revealed";
    public override float Duration => 1f;
    public override bool AutoStart => false;
    public override bool HideOnUi => true;
    public override bool RemoveOnComplete => false;
    public string ExtraRoleText { get; set; } = string.Empty;
    public string ExtraNameText { get; set; } = string.Empty;
    public Color? NameColor { get; set; }
    public bool RevealRole { get; set; } = revealRole;
    
    public ChangeRoleResult ChangeRoleResult { get; set; } = (ChangeRoleResult)roleChangeResult;
    
    public RoleBehaviour? ShownRole { get; set; }
    
    public bool Visible { get; set; } = true;

    public override string GetDescription() => "You are Revealed!";

    public void SetNewInfo(bool revealRole, string? nameTxt = null, string? roleTxt = null, RoleBehaviour? role2 = null, Color? nameColor = null)
    {
        RevealRole = revealRole;
        ExtraRoleText = roleTxt ?? ExtraRoleText; // Set to string.Empty to remove the text
        ExtraNameText = nameTxt ?? ExtraNameText; // Set to string.Empty to remove the text
        NameColor = nameColor;
        ShownRole = role2;
    }

    public override void OnActivate()
    {
        base.OnActivate();
        if (RevealRole && ShownRole == null)
        {
            ShownRole = role != null ? role : Player.Data.Role;
        }
        if (MeetingHud.Instance && RevealRole)
        {
            var targetVoteArea = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == Player.PlayerId);
            if (targetVoteArea.TargetPlayerId != PlayerControl.LocalPlayer.PlayerId)
            {
                MeetingMenu.Instances.Do(x => x.HideSingle(targetVoteArea.TargetPlayerId));
            }
        }
    }
}

public enum ChangeRoleResult
{
    UpdateInfo,
    RemoveModifier,
    Nothing
}