using System.Collections;
using AmongUs.GameOptions;
using InnerNet;
using MiraAPI.GameModes;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Patches;
using TownOfUs.Roles.TownOfPolus.Crewmate;
using TownOfUs.Roles.TownOfPolus.Impostor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.GameModes;

public class TownOfPolusMode : AbstractGameMode
{
    public override string Name => "Town of Polus";
    public override string Description => "Polus.gg's Town of Polus mode, reimplemented in Mira.";
    public override Color Color => new Color32(157, 146, 198, 255);
    public override bool ShowGameModeIntroCutscene => true;
    public override void AssignRoles(out bool runOriginal, LogicRoleSelectionNormal instance)
    {
        runOriginal = false;
        Il2CppSystem.Collections.Generic.List<ClientData> list = new();
        AmongUsClient.Instance.GetAllClients(list);
        List<NetworkedPlayerInfo> list2  = list.ToArray()
            .Where(c => c.Character != null && c.Character.Data != null && !c.Character.Data.Disconnected &&
                        !c.Character.Data.IsDead).OrderBy(c => c.Id).Select(c => c.Character.Data)
            .ToList();
        foreach (NetworkedPlayerInfo networkedPlayerInfo in GameData.Instance.AllPlayers)
        {
            if (networkedPlayerInfo.Object != null && networkedPlayerInfo.Object.isDummy)
            {
                list2.Add(networkedPlayerInfo);
            }
        }
        IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        int adjustedNumImpostors = currentGameOptions.GetAdjustedNumImpostors(list2.Count);
        AssignRolesForTeam(list2, currentGameOptions, RoleTeamTypes.Impostor, adjustedNumImpostors, (RoleTypes)RoleId.Get<PolusImpostorRole>());
        AssignRolesForTeam(list2, currentGameOptions, RoleTeamTypes.Crewmate, int.MaxValue, (RoleTypes)RoleId.Get<PolusCrewmateRole>());
    }
    public static void AssignRolesForTeam(
        List<NetworkedPlayerInfo> players,
        IGameOptions opts,
        RoleTeamTypes team,
        int teamMax,
        RoleTypes defaultRole)
    {
        int num = 0;
        var source = RoleManager.Instance.AllRoles.ToArray()
            .Where(role => role.TeamType == team && !RoleManager.IsGhostRole(role.Role) &&
                           CustomRoleUtils.CanSpawnOnCurrentMode(role));
        List<RoleTypes> list = new List<RoleTypes>();
        IRoleOptionsCollection roleOptions = opts.RoleOptions;

        var assignmentData = source.Where(x => !x.IsDead).Select(role =>
            new RoleManager.RoleAssignmentData(
                role,
                roleOptions.GetNumPerGame(role.Role),
                roleOptions.GetChancePerGame(role.Role))).ToList();
        var source2 = CustomRoleUtils.GetPossibleRoles(assignmentData, x => x.Chance == 100);
        var guaranteedRoles = source.Where(x => source2.Contains(((ushort)x.Role, 100)));
        foreach (RoleManager.RoleAssignmentData roleAssignmentData in guaranteedRoles.Select((x) =>
                     new RoleManager.RoleAssignmentData(x, roleOptions.GetNumPerGame(x.Role), 100)))
        {
            while (true)
            {
                RoleManager.RoleAssignmentData roleAssignmentData2 = roleAssignmentData;
                int count = roleAssignmentData2.Count;
                roleAssignmentData2.Count = count - 1;
                if (count <= 0)
                {
                    break;
                }

                list.Add(roleAssignmentData.Role.Role);
            }
        }

        AssignRolesFromList(players, teamMax, list, ref num);

        var list2 = source.Where(x => !x.IsDead).Select(role =>
            new RoleManager.RoleAssignmentData(
                role,
                roleOptions.GetNumPerGame(role.Role),
                roleOptions.GetChancePerGame(role.Role))).ToList();

        list.Clear();
        foreach (RoleManager.RoleAssignmentData roleAssignmentData3 in list2)
        {
            for (int i = 0; i < roleAssignmentData3.Count; i++)
            {
                if (HashRandom.Next(101) < roleAssignmentData3.Chance)
                {
                    list.Add(roleAssignmentData3.Role.Role);
                }
            }
        }

        AssignRolesFromList(players, teamMax, list, ref num);

        while (list.Count < players.Count && list.Count + num < teamMax)
        {
            list.Add(defaultRole);
        }

        AssignRolesFromList(players, teamMax, list, ref num);
    }
    public static void AssignRolesFromList(List<NetworkedPlayerInfo> players, int teamMax, List<RoleTypes> roleList, ref int rolesAssigned)
    {
        while (roleList.Count > 0 && players.Count > 0 && rolesAssigned < teamMax)
        {
            int index = HashRandom.FastNext(roleList.Count);
            RoleTypes roleType = roleList[index];
            roleList.RemoveAt(index);
            int index2 = HashRandom.FastNext(players.Count);
            players[index2].Object.RpcSetRole(roleType, false);
            players.RemoveAt(index2);
            rolesAssigned++;
        }
    }

    public override IEnumerator IntroCutscene(IntroCutscene __instance)
    {
        Logger.GlobalInstance.Info("IntroCutscene :: CoBegin() :: Starting intro cutscene", null);
        SoundManager.Instance.PlaySound(__instance.IntroStinger, false, 1f, null);
        if (!LegacyAssets.IsLegacy)
        {
            var hudInstance = HudManager.Instance;
            Debug("Applying vanilla assets patch...");
            var killBtn = hudInstance.KillButton;
            var reportBtn = hudInstance.ReportButton;
            var saboBtn = hudInstance.SabotageButton;
            var useBtn = hudInstance.UseButton;
            var petBtn = hudInstance.PetButton;
            var ventBtn = hudInstance.ImpostorVentButton;
            killBtn.defaultKillSprite = LegacyVanillaAssets.KillSprite.LoadAsset();
            killBtn.graphic.sprite = LegacyVanillaAssets.KillSprite.LoadAsset();
            reportBtn.graphic.sprite = LegacyVanillaAssets.ReportSprite.LoadAsset();
            saboBtn.graphic.sprite = LegacyVanillaAssets.SabotageSprite.LoadAsset();
            useBtn.graphic.sprite = LegacyVanillaAssets.UseSprite.LoadAsset();
            petBtn.graphic.sprite = LegacyVanillaAssets.PetSprite.LoadAsset();
            ventBtn.graphic.sprite = LegacyVanillaAssets.VentSprite.LoadAsset();
            killBtn.RemoveLabel();
            reportBtn.RemoveLabel();
            saboBtn.RemoveLabel();
            useBtn.RemoveLabel();
            petBtn.RemoveLabel();
            ventBtn.RemoveLabel();
        };
        foreach (var button in CustomButtonManager.Buttons.Where(x => x is ILegacyButton))
        {
            button.RemoveLabel();
        }
        
        Logger.GlobalInstance.Info("IntroCutscene :: CoBegin() :: Game Mode: Town of Polus (TOU Mira)", null);
        __instance.LogPlayerRoleData();
        __instance.HideAndSeekPanels.SetActive(false);
        __instance.CrewmateRules.SetActive(false);
        __instance.ImpostorRules.SetActive(false);
        __instance.ImpostorName.gameObject.SetActive(false);
        __instance.ImpostorTitle.gameObject.SetActive(false);
        Func<NetworkedPlayerInfo, bool> method = pcd =>
            !PlayerControl.LocalPlayer.Data.Role.IsImpostor ||
            pcd.Role.TeamType == PlayerControl.LocalPlayer.Data.Role.TeamType;
        var list = global::IntroCutscene.SelectTeamToShow(method);
        if (list == null || list.Count < 1)
        {
            Logger.GlobalInstance.Error("IntroCutscene :: CoBegin() :: teamToShow is EMPTY or NULL", null);
        }
        var role = PlayerControl.LocalPlayer.Data.Role;
        __instance.ImpostorText.text = role.Blurb;
        yield return ShowTeam(__instance, list, 3f);
        ShipStatus.Instance.StartSFX();
        Object.Destroy(__instance.gameObject);
    }
    public static IEnumerator ShowTeam(IntroCutscene __instance, Il2CppSystem.Collections.Generic.List<PlayerControl> teamToShow, float duration)
    {
        if (__instance.overlayHandle == null)
        {
            __instance.overlayHandle = DestroyableSingleton<DualshockLightManager>.Instance.AllocateLight();
        }
        yield return ShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
        var role = PlayerControl.LocalPlayer.Data.Role;
        if (!role.IsImpostor())
        {
            __instance.BeginCrewmate(teamToShow);
        }
        else
        {
            __instance.BeginImpostor(teamToShow);
        }
        __instance.overlayHandle.color = role.TeamColor;
        __instance.BackgroundBar.material.SetColor("_Color", role.TeamColor);
        __instance.TeamTitle.text = role.GetRoleName();
        __instance.TeamTitle.color = role.TeamColor;
        if (role is PolusCrewmateRole)
        {
            int adjustedNumImpostors = GameManager.Instance.LogicOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount);
            if (adjustedNumImpostors == 1)
            {
                __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsS);
            }
            else
            {
                __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsP, adjustedNumImpostors);
            }
            __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[FF1919FF]", "<color=#FF1919FF>");
            __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[]", "</color>");
        }
        else
        {
            __instance.ImpostorText.text = role.Blurb;
        }
        Color c = __instance.TeamTitle.color;
        Color fade = Color.black;
        Color impColor = Color.white;
        Vector3 titlePos = __instance.TeamTitle.transform.localPosition;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float num = Mathf.Min(1f, timer / duration);
            __instance.Foreground.material.SetFloat("_Rad", __instance.ForegroundRadius.ExpOutLerp(num * 2f));
            fade.a = Mathf.Lerp(1f, 0f, num * 3f);
            __instance.FrontMost.color = fade;
            c.a = Mathf.Clamp(FloatRange.ExpOutLerp(num, 0f, 1f), 0f, 1f);
            __instance.TeamTitle.color = c;
            __instance.RoleText.color = c;
            impColor.a = Mathf.Lerp(0f, 1f, (num - 0.3f) * 3f);
            __instance.ImpostorText.color = impColor;
            titlePos.y = 2.7f - num * 0.3f;
            __instance.TeamTitle.transform.localPosition = titlePos;
            __instance.overlayHandle.color.SetAlpha(Mathf.Min(1f, timer * 2f));
            yield return null;
        }
        timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            float num2 = timer / 1f;
            fade.a = Mathf.Lerp(0f, 1f, num2 * 3f);
            __instance.FrontMost.color = fade;
            __instance.overlayHandle.color.SetAlpha(1f - fade.a);
            yield return null;
        }
        yield break;
    }
}