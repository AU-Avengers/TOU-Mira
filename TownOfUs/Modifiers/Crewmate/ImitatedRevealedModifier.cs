// using MiraAPI.Roles;
using TownOfUs.Modules;
// using TownOfUs.Utilities;
// using UnityEngine;
// using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ImitatedRevealedModifier(RoleBehaviour role)
    : RevealModifier((int)ChangeRoleResult.Nothing, true, role)
{
    public override string ModifierName => "Role Revealed";
    public override void OnActivate()
    {
        base.OnActivate();
        ChangeRoleResult = ChangeRoleResult.Nothing;
        var roleWhenAlive = Player.GetRoleWhenAlive();
        if (roleWhenAlive is ICrewVariant crewType)
        {
            roleWhenAlive = crewType.CrewVariant;
        }
        SetNewInfo(true, null, null, roleWhenAlive);
    }

    // TODO: Fix Imitator not showing role icons on already imitated players. Is this required? No, but it makes it more visually appealing.
    public override void OnMeetingStart()
    {
        var roleWhenAlive = Player.GetRoleWhenAlive();
        if (roleWhenAlive is ICrewVariant crewType)
        {
            roleWhenAlive = crewType.CrewVariant;
        }
        SetNewInfo(true, null, null, roleWhenAlive);
        RevealRole = true;
        NameColor = roleWhenAlive.TeamColor;
        ShownRole = roleWhenAlive;
        /*if (ShownRole == null)
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
                    var newIcon = Object.Instantiate(voteArea.FairyIcon, voteArea.transform);
                    newIcon.gameObject.SetActive(true);
                    newIcon.sprite = roleImg;
                    newIcon.SetSizeLimit(1.44f);
                }

                break;
            }
        }*/
    }
}