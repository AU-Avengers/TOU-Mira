using System.Collections;
using AmongUs.GameOptions;
using LibCpp2IL;
using MiraAPI.Events;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Patches;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TownOfUs.Utilities;

public static class Extensions
{
    public static bool IsGhostDead(this NetworkedPlayerInfo data)
    {
        return data.Role is IGhostRole ghostRole ? !ghostRole.GhostActive : data.IsDead;
    }

    public static ITownOfUsRole? GetTownOfUsRole(this PlayerControl player)
    {
        var role = player.Data?.Role as ITownOfUsRole;

        return role;
    }

    public static T? GetRole<T>(this PlayerControl player) where T : RoleBehaviour
    {
        var role = player.Data?.Role as T;

        return role;
    }

    public static bool IsRole<T>(this PlayerControl player) where T : RoleBehaviour
    {
        return player.Data?.Role is T;
    }

    public static bool IsLover(this PlayerControl player)
    {
        return player?.HasModifier<LoverModifier>() == true;
    }

    public static bool IsImpostor(this PlayerControl player)
    {
        return player?.Data && player?.Data?.Role && player?.Data?.Role.IsImpostor() == true;
    }

    public static bool IsImpostor(this RoleBehaviour role)
    {
        return role is ICustomRole customRole
            ? customRole.Team is ModdedRoleTeams.Impostor
            : role.TeamType is RoleTeamTypes.Impostor;
    }

    public static bool IsTraitor(this PlayerControl player)
    {
        return player?.Data && player?.Data?.Role && player?.Data?.Role.IsImpostor() == true &&
               (player?.HasModifier<TraitorCacheModifier>() == true || player?.Data?.Role is TraitorRole);
    }

    public static bool IsCrewmate(this PlayerControl player)
    {
        return player.Data && player.Data.Role && player.Data.Role.IsCrewmate();
    }

    public static bool IsCrewmate(this RoleBehaviour role)
    {
        return role is ICustomRole customRole
            ? customRole.Team == ModdedRoleTeams.Crewmate
            : role.TeamType == RoleTeamTypes.Crewmate;
    }

    public static bool IsNeutral(this PlayerControl player)
    {
        return player.Data && player.Data.Role && player.Data.Role.IsNeutral();
    }

    public static bool IsNeutral(this RoleBehaviour role)
    {
        return role is ICustomRole { Team: ModdedRoleTeams.Custom };
    }

    public static bool Is(this PlayerControl player, RoleTypes roleType)
    {
        return player.Data.Role.Role == roleType;
    }

    public static bool Is(this PlayerControl player, RoleAlignment roleAlignment)
    {
        if (player.Data.Role.GetRoleAlignment() == roleAlignment)
        {
            return true;
        }

        return false;
    }

    public static bool Is(this PlayerControl player, ModdedRoleTeams team)
    {
        if (player.Data.Role is ITownOfUsRole role && role.Team == team)
        {
            return true;
        }

        if (team == ModdedRoleTeams.Impostor)
        {
            return player.IsImpostor();
        }

        if (team == ModdedRoleTeams.Crewmate)
        {
            return player.IsCrewmate();
        }

        return false;
    }

    public static bool IsJailed(this PlayerControl player)
    {
        return player.HasModifier<JailedModifier>() && !player.HasDied();
    }

    public static bool IsHysteria(this PlayerControl player)
    {
        var mod = player.GetModifier<HypnotisedModifier>();

        return mod?.HysteriaActive == true;
    }

    public static bool HasDied(this PlayerControl player)
    {
        return !player || !player.Data || player.Data.IsDead || player.Data.Disconnected;
    }

    public static IEnumerator CoClean(this DeadBody body)
    {
        var renderer = body.bodyRenderers[^1];
        yield return MiscUtils.PerformTimedAction(1f, t => renderer.color = renderer.color.SetAlpha(1 - t));
        body.gameObject.Destroy();
    }

    public static void OverrideOnClickListeners(this PassiveButton passive, Action action, bool enabled = true)
    {
        if (!passive)
        {
            return;
        }

        passive.OnClick?.RemoveAllListeners();
        passive.OnClick = new Button.ButtonClickedEvent();
        passive.OnClick.AddListener(action);
        passive.enabled = enabled;
    }

    public static void OverrideOnMouseOverListeners(this PassiveButton passive, Action action, bool enabled = true)
    {
        if (!passive)
        {
            return;
        }

        passive.OnMouseOver?.RemoveAllListeners();
        passive.OnMouseOver = new UnityEvent();
        passive.OnMouseOver.AddListener(action);
        passive.enabled = enabled;
    }

    public static void OverrideOnMouseOutListeners(this PassiveButton passive, Action action, bool enabled = true)
    {
        if (!passive)
        {
            return;
        }

        passive.OnMouseOut?.RemoveAllListeners();
        passive.OnMouseOut = new UnityEvent();
        passive.OnMouseOut.AddListener(action);
        passive.enabled = enabled;
    }

    public static void DestroyAll(this IEnumerable<Component> collection)
    {
        foreach (var item in collection)
        {
            if (item == null)
            {
                continue;
            }

            Object.Destroy(item);

            if (item.gameObject == null)
            {
                return;
            }

            Object.Destroy(item.gameObject);
        }
    }

    public static void SetRole(this ShapeshifterPanel panel, int index, RoleBehaviour roleBehaviour, Action onClick)
    {
        panel.shapeshift = onClick;
        panel.PlayerIcon.SetFlipX(false);
        panel.PlayerIcon.ToggleName(false);
        panel.PlayerIcon.gameObject.SetActive(false);

        SpriteRenderer[] componentsInChildren = panel.GetComponentsInChildren<SpriteRenderer>();

        foreach (var spriteRend in componentsInChildren)
        {
            spriteRend.material.SetInt(PlayerMaterial.MaskLayer, index + 2);
        }

        panel.PlayerIcon.SetMaskLayer(index + 2);
        panel.PlayerIcon.cosmetics.SetMaskType(PlayerMaterial.MaskType.ComplexUI);

        foreach (var hand in panel.PlayerIcon.Hands)
        {
            hand.sharedMaterial = CosmeticsLayer.GetBodyMaterial(PlayerMaterial.MaskType.ComplexUI);
            hand.gameObject.active = false;
        }

        foreach (var sprite in panel.PlayerIcon.OtherBodySprites)
        {
            sprite.sharedMaterial = CosmeticsLayer.GetBodyMaterial(PlayerMaterial.MaskType.ComplexUI);
            sprite.gameObject.active = false;
        }

        var roleIcon = Object.Instantiate(panel.Background, panel.transform).gameObject;
        roleIcon.name = "RoleIcon";
        roleIcon.GetComponent<PassiveButton>().Destroy();
        roleIcon.gameObject.transform.localScale = new Vector3(0.21f, 0.9f, 1);
        roleIcon.gameObject.transform.localPosition += new Vector3(-0.964f, 0, -2);

        var roleImg = TouRoleIcons.RandomImp.LoadAsset();

        if (roleBehaviour.IsCrewmate())
        {
            roleImg = TouRoleIcons.RandomCrew.LoadAsset();
        }
        else if (roleBehaviour.IsNeutral())
        {
            roleImg = TouRoleIcons.RandomNeut.LoadAsset();
        }

        if (roleBehaviour is ICustomRole customRole3 && customRole3.Configuration.Icon != null)
        {
            roleImg = customRole3.Configuration.Icon.LoadAsset();
        }
        else if (roleBehaviour.RoleIconSolid != null)
        {
            roleImg = roleBehaviour.RoleIconSolid;
        }

        roleIcon.gameObject.GetComponent<SpriteRenderer>().sprite = roleImg;
        roleIcon.gameObject.SetActive(true);

        var color = roleBehaviour is ICustomRole customRole ? customRole.RoleColor : roleBehaviour.TeamColor;

        var alignment = MiscUtils.GetParsedRoleAlignment(roleBehaviour, true);

        var finalString =
            $"<size=88%>{roleBehaviour.GetRoleName()}</size>\n<size=70%><color=white>{alignment}</color></size>";

        panel.LevelNumberText.transform.parent.gameObject.SetActive(false);
        panel.NameText.color = color;
        panel.NameText.text = finalString;
        panel.NameText.alignment = TextAlignmentOptions.Right;
        panel.NameText.transform.localPosition += Vector3.left * 0.05f;
    }

    public static void SetModifier(this ShapeshifterPanel panel, int index, BaseModifier modifier, Action onClick)
    {
        panel.shapeshift = onClick;
        panel.PlayerIcon.SetFlipX(false);
        panel.PlayerIcon.ToggleName(false);
        panel.PlayerIcon.gameObject.SetActive(false);

        SpriteRenderer[] componentsInChildren = panel.GetComponentsInChildren<SpriteRenderer>();

        foreach (var spriteRend in componentsInChildren)
        {
            spriteRend.material.SetInt(PlayerMaterial.MaskLayer, index + 2);
        }

        panel.PlayerIcon.SetMaskLayer(index + 2);
        panel.PlayerIcon.cosmetics.SetMaskType(PlayerMaterial.MaskType.ComplexUI);

        foreach (var hand in panel.PlayerIcon.Hands)
        {
            hand.sharedMaterial = CosmeticsLayer.GetBodyMaterial(PlayerMaterial.MaskType.ComplexUI);
            hand.gameObject.active = false;
        }

        foreach (var sprite in panel.PlayerIcon.OtherBodySprites)
        {
            sprite.sharedMaterial = CosmeticsLayer.GetBodyMaterial(PlayerMaterial.MaskType.ComplexUI);
            sprite.gameObject.active = false;
        }

        var roleIcon = Object.Instantiate(panel.Background, panel.transform).gameObject;
        roleIcon.name = "RoleIcon";
        roleIcon.GetComponent<PassiveButton>().Destroy();
        roleIcon.gameObject.transform.localScale = new Vector3(0.21f, 0.9f, 1);
        roleIcon.gameObject.transform.localPosition += new Vector3(-0.964f, 0, -2);

        var modImg = TouRoleIcons.RandomAny.LoadAsset();

        var faction = modifier.GetModifierFaction();
        if (faction.ToDisplayString().Contains("Crewmate"))
        {
            modImg = TouRoleIcons.RandomCrew.LoadAsset();
        }
        else if (faction.ToDisplayString().Contains("Neutral"))
        {
            modImg = TouRoleIcons.RandomNeut.LoadAsset();
        }
        else if (faction.ToDisplayString().Contains("Impostor"))
        {
            modImg = TouRoleIcons.RandomImp.LoadAsset();
        }

        if (modifier.ModifierIcon != null)
        {
            modImg = modifier.ModifierIcon.LoadAsset();
        }

        roleIcon.gameObject.GetComponent<SpriteRenderer>().sprite = modImg;
        roleIcon.gameObject.SetActive(true);

        var teamName = MiscUtils.GetParsedModifierFaction(faction, true);
        var finalString =
            $"<size=88%>{modifier.ModifierName}<color=white> ({TouLocale.Get("Modifier")})</size>\n<size=70%>{teamName}</color></size>";
        var color = MiscUtils.GetModifierColour(modifier);

        panel.LevelNumberText.transform.parent.gameObject.SetActive(false);
        panel.NameText.color = color;
        panel.NameText.text = finalString;
        panel.NameText.alignment = TextAlignmentOptions.Right;
        panel.NameText.transform.localPosition += Vector3.left * 0.05f;
    }

    [MethodRpc((uint)TownOfUsRpc.ChangeRole)]
    public static void RpcChangeRole(this PlayerControl player, ushort newRoleType, bool recordRole = true)
    {
        ChangeRole(player, newRoleType, recordRole);
    }

    public static void ChangeRole(this PlayerControl player, ushort newRoleType, bool recordRole = true)
    {
        player.roleAssigned = false;

        var data = player.Data;

        if (data.Role)
        {
            data.Role.Deinitialize(player);
        }

        // Object.Destroy(data.Role.gameObject);
        var newRole = RoleManager.Instance.GetRole((RoleTypes)newRoleType);
        var roleBehaviour = Object.Instantiate(newRole, data.gameObject.transform);
        var oldRole = player.Data.Role;

        roleBehaviour.Initialize(player);

        if (player.AmOwner && HudManager.Instance)
        {
            HudManager.Instance.SetHudActive(player, roleBehaviour, true);

            if (MeetingHud.Instance || ExileController.Instance)
            {
                HudManager.Instance.SetHudActive(player, roleBehaviour, false);
            }
        }

        player.Data.Role = roleBehaviour;
        player.Data.RoleType = roleBehaviour.Role;

        if (!roleBehaviour.IsDead)
        {
            player.Data.RoleWhenAlive = new Il2CppSystem.Nullable<RoleTypes>(roleBehaviour.Role);
        }

        if (recordRole)
        {
            GameHistory.RegisterRole(player, roleBehaviour);
        }

        roleBehaviour.AdjustTasks(player);

        player.MyPhysics.ResetMoveState();

        player.Data.Role.SpawnTaskHeader(player);

        if (TutorialManager.InstanceExists && HudManager.Instance)
        {
            HudManagerPatches.ZoomButton.SetActive(true);
        }

        var changeRoleEvent = new ChangeRoleEvent(player, oldRole, newRole);
        MiraEventManager.InvokeEvent(changeRoleEvent);
    }

    [MethodRpc((uint)TownOfUsRpc.PlayerExile)]
    public static void RpcPlayerExile(this PlayerControl player)
    {
        player.Exiled();
    }

    [MethodRpc((uint)TownOfUsRpc.SetPos)]
    public static void RpcSetPos(this PlayerControl player, Vector2 pos)
    {
        player.transform.position = pos;
        player.NetTransform.SnapTo(pos);
    }

    public static void GhostFade(this PlayerControl player)
    {
        player.Visible = true;
        var color = new Color(1f, 1f, 1f, 0f);

        var maxDistance = ShipStatus.Instance.MaxLightRadius *
                          GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;

        if (PlayerControl.LocalPlayer == null)
        {
            return;
        }

        var distance = (PlayerControl.LocalPlayer.GetTruePosition() - player.GetTruePosition()).magnitude;

        var distPercent = distance / maxDistance;
        distPercent = Mathf.Max(0, distPercent - 1);

        var velocity = player.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude;
        color.a = 0.07f + velocity / player.MyPhysics.GhostSpeed * 0.13f;
        color.a = Mathf.Lerp(color.a, 0, distPercent);

        if (player.GetAppearanceType() != TownOfUsAppearances.PlayerOnly)
        {
            var fade = new VisualAppearance(player.GetDefaultModifiedAppearance(), TownOfUsAppearances.PlayerOnly)
            {
                HatId = string.Empty,
                SkinId = string.Empty,
                VisorId = string.Empty,
                PlayerName = string.Empty,
                PetId = string.Empty,
                RendererColor = color,
                NameColor = Color.clear,
                ColorBlindTextColor = Color.clear,
                NameVisible = false
            };

            player?.RawSetAppearance(fade);
        }

        player!.cosmetics.currentBodySprite.BodySprite.color = color;
        if (player.cosmetics.GetLongBoi() != null)
        {
            player.cosmetics.GetLongBoi().headSprite.color = color;
            player.cosmetics.GetLongBoi().neckSprite.color = color;
            player.cosmetics.GetLongBoi().foregroundNeckSprite.color = color;
        }
    }

    public static void SpawnAtRandomVent(this PlayerControl player)
    {
        List<Vent> vents;

        var cleanVentTasks = player.myTasks.ToArray().Where(x => x.TaskType == TaskTypes.VentCleaning).ToList();

        if (cleanVentTasks != null)
        {
            var ids = cleanVentTasks.Where(x => !x.IsComplete)
                .ToList()
                .ConvertAll(x => x.FindConsoles().ToArray()[0].ConsoleId);

            vents = ShipStatus.Instance.AllVents.Where(x => !ids.Contains(x.Id)).ToList();
        }
        else
        {
            vents = ShipStatus.Instance.AllVents.ToList();
        }

        var startingVent = vents[Random.RandomRangeInt(0, vents.Count)];

        var pos = new Vector2(startingVent.transform.position.x, startingVent.transform.position.y + 0.3636f);

        player.RpcSetPos(pos);
    }

    public static void Shuffle<T>(this List<T> list)
    {
        for (var i = list.Count - 1; i > 0; --i)
        {
            var j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public static T TakeFirst<T>(this List<T> list)
    {
        return list.RemoveAndReturn(0);
    }

    public static List<T> Pad<T>(this List<T> list, int max, T item)
    {
        if (list.Count < max)
        {
            var diff = max - list.Count;

            for (var i = 0; i < diff; i++)
            {
                list.Add(item);
            }
        }

        return list;
    }

    [MethodRpc((uint)TownOfUsRpc.CatchGhost)]
    public static void RpcCatchGhost(this PlayerControl player)
    {
        if (player.Data.Role is IGhostRole ghost)
        {
            ghost.Clicked();
            if (player.AmOwner)
            {
                HudManagerPatches.ZoomButton.SetActive(true);
            }
        }
    }

    public static void SetOutline(this Vent vent, bool on, bool mainTarget, Color color)
    {
        vent.myRend.material.SetFloat(ShaderID.Outline, on ? 1 : 0);
        vent.myRend.material.SetColor(ShaderID.OutlineColor, color);
        vent.myRend.material.SetColor(ShaderID.AddColor, mainTarget ? color : Color.clear);
    }

    public static float GetKillCooldown(this PlayerControl player)
    {
        return UnderdogModifier.GetKillCooldown(player);
    }

    /// <summary>
    ///     Gets the closest player that matches the given criteria that also isn't hidden by other roles.
    /// </summary>
    /// <param name="playerControl">The player object.</param>
    /// <param name="includeImpostors">Whether impostors should be included in the search.</param>
    /// <param name="distance">The radius to search within.</param>
    /// <param name="ignoreColliders">Whether colliders should be ignored when searching.</param>
    /// <param name="predicate">Optional predicate to test if the object is valid.</param>
    /// <returns>The closest player if there is one, false otherwise.</returns>
    public static PlayerControl? GetClosestLivingPlayer(
        this PlayerControl playerControl,
        bool includeImpostors,
        float distance,
        bool ignoreColliders = false,
        Predicate<PlayerControl>? predicate = null)
    {
        var filteredPlayers = Helpers.GetClosestPlayers(playerControl, distance, ignoreColliders)
            .Where(playerInfo => !playerInfo.Data.Disconnected &&
                                 playerInfo.PlayerId != playerControl.PlayerId &&
                                 ((playerInfo.TryGetModifier<DisabledModifier>(out var mod) && mod.IsConsideredAlive) ||
                                  !playerInfo.HasModifier<DisabledModifier>()) &&
                                 !playerInfo.Data.IsDead &&
                                 (includeImpostors || !playerInfo.Data.Role.IsImpostor))
            .ToList();

        return predicate != null ? filteredPlayers.Find(predicate) : filteredPlayers.FirstOrDefault();
    }

    public static GameObject CreateHackedIcon(this ActionButton button)
    {
        var hackedSprite = new GameObject("HackedSprite");
        hackedSprite.transform.SetParent(button.transform);
        hackedSprite.transform.localPosition = new Vector3(0, 0, -10f);
        hackedSprite.gameObject.layer = button.gameObject.layer;

        var render = hackedSprite.AddComponent<SpriteRenderer>();
        render.sprite = TouAssets.Hacked.LoadAsset();

        hackedSprite.SetHackActive(false);

        return hackedSprite;
    }

    public static void SetHackActive(this GameObject hackedSprite, bool isActive)
    {
        if (hackedSprite && hackedSprite.gameObject)
        {
            hackedSprite.gameObject.SetActive(isActive);
            hackedSprite.GetComponent<SpriteRenderer>().enabled = isActive;
        }
    }
}