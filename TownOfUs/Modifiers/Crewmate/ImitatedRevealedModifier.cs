// using MiraAPI.Roles;

using AmongUs.GameOptions;
using MiraAPI.Roles;
using TownOfUs.Modules;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ImitatedRevealedModifier(RoleBehaviour role)
    : BaseRevealModifier
{

    public override RoleBehaviour? ShownRole { get; set; } = role;
    public override bool RevealRole { get; set; } = true;
    public override string ModifierName => "Role Revealed";
    public override void OnActivate()
    {
        base.OnActivate();
        var roleWhenAlive = Player.GetRoleWhenAlive();
        if (roleWhenAlive is ICrewVariant crewType)
        {
            roleWhenAlive = crewType.CrewVariant;
        }

        if (roleWhenAlive is ImitatorRole || roleWhenAlive is SurvivorRole || roleWhenAlive.IsSimpleRole)
        {
            roleWhenAlive = RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ImitatorRole>());
        }
        SetNewInfo(true, null, null, roleWhenAlive);
    }

    public override void OnMeetingStart()
    {
        var roleWhenAlive = Player.GetRoleWhenAlive();
        if (roleWhenAlive is ICrewVariant crewType)
        {
            roleWhenAlive = crewType.CrewVariant;
        }

        if (roleWhenAlive is ImitatorRole || roleWhenAlive is SurvivorRole || roleWhenAlive.IsSimpleRole)
        {
            roleWhenAlive = RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ImitatorRole>());
        }
        SetNewInfo(true, null, null, roleWhenAlive);
        if (ShownRole == null)
        {
            return;
        }
        foreach (var voteArea in MeetingHud.Instance.playerStates)
        {
            if (Player.PlayerId == voteArea.TargetPlayerId)
            {
                Sprite? roleImg = null;

                if (ShownRole is ICustomRole customRole && customRole.Configuration.Icon != null)
                {
                    roleImg = customRole.Configuration.Icon.LoadAsset();
                }
                else if (ShownRole.RoleIconSolid != null)
                {
                    roleImg = ShownRole.RoleIconSolid;
                }

                if (roleImg != null)
                {
                    var newIcon = UnityEngine.Object.Instantiate(voteArea.GAIcon, voteArea.transform);
                    newIcon.gameObject.SetActive(true);
                    newIcon.sprite = roleImg;
                    newIcon.enabled = true;
                    newIcon.name = "RoleIcon";
                    newIcon.SetSizeLimit(1.44f);
                    newIcon.transform.localPosition = new Vector3(-1.25f, -0.15f, -3f);
                    // newIcon.transform.localPosition = new Vector3(-1.3f, 0f, -3f);
                    newIcon.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
                }

                break;
            }
        }
    }
}