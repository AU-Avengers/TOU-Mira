using System.Collections;
using AmongUs.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;
using TMPro;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
public class WikiHyperlink(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    private TextMeshPro tmp;
    private Camera worldCamera;
    private bool _hovered;

    public int HyperlinkIndex;
    public string HyperlinkString;
    public string HoverHyperlinkString;

    public void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
        worldCamera = Camera.main;
    }

    public void Update()
    {
        if (!tmp)
        {
            return;
        }

        if (!worldCamera)
        {
            return;
        }

        Vector3 mousePos = Input.mousePosition;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmp, mousePos, worldCamera);
        var hovering = linkIndex == HyperlinkIndex;
        if (hovering && !_hovered)
        {
            tmp.text = tmp.text.Replace(HyperlinkString, HoverHyperlinkString);
        }
        else if (!hovering && _hovered)
        {
            tmp.text = tmp.text.Replace(HoverHyperlinkString, HyperlinkString);
        }

        _hovered = hovering;

        if (hovering && Input.GetMouseButtonDown(0)) // Left click
        {
            OpenHyperlink(tmp.textInfo.linkInfo[linkIndex]);
        }
    }

    public static void OpenHyperlink(TMP_LinkInfo linkInfo)
    {
        string id = linkInfo.GetLinkID().Split(':')[0]; // The id is {RoleClassFullName}:{linkIdx}
        Debug($"Looking for wiki entry: {id}");
        if (id.StartsWith("AmongUs.Roles.", StringComparison.InvariantCulture))
        {
            id = id["AmongUs.Roles.".Length..];
        }

        var role = MiscUtils.AllRoles.FirstOrDefault(x => x.GetType().FullName == id) ??
                   MiscUtils.AllRegisteredRoles.FirstOrDefault(x => x.Role.ToString() == id) ??
                   RoleManager.Instance.GetRole(RoleTypes.Crewmate); // i hate il2cpp
        var modifier = MiscUtils.AllOverallWikiModifiers.FirstOrDefault(x => x.GetType().FullName == id);

        if (LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.UseVanillaRoleGuide.Value)
        {
            CloseChatAndMinigame();
            MatchInfoGuide.Instance.Open();
            Coroutines.Start(CoLoadAdvancedWiki(role, modifier));
        }
        else
        {
            if (role is IWikiDiscoverable wikiRole)
            {
                OpenWikiEntry(wikiRole);
            }
            else if (modifier is IWikiDiscoverable wikiModifier)
            {
                OpenWikiEntry(wikiModifier);
            }
            else if (SoftWikiEntries.RoleEntries.TryGetValue(role, out var softRoleWiki))
            {
                OpenWikiEntry(softRoleWiki);
            }
            else if (modifier != null && SoftWikiEntries.ModifierEntries.TryGetValue(modifier, out var softModWiki))
            {
                OpenWikiEntry(softModWiki);
            }
        }
    }

    private static void CloseChatAndMinigame()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening)
        {
            HudManager.Instance.Chat.Close();
        }

        if (Minigame.Instance)
        {
            Minigame.Instance.Close();
        }
    }

    private static void OpenWikiEntry(IWikiDiscoverable wikiDiscoverable)
    {
        CloseChatAndMinigame();
        var wiki = IngameWikiMinigame.Create();
        wiki.Begin(null);
        wiki.OpenFor(wikiDiscoverable);
    }

    private static void OpenWikiEntry(SoftWikiInfo softWikiInfo)
    {
        CloseChatAndMinigame();
        var wiki = IngameWikiMinigame.Create();
        wiki.Begin(null);
        wiki.OpenFor(softWikiInfo);
    }

    public static IEnumerator CoLoadAdvancedWiki(RoleBehaviour role, BaseModifier? modifier)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (modifier != null)
        {
            RoleGuidePatches.DisplayAdvancedWiki(MatchInfoGuide.Instance, modifier);
        }
        else
        {
            RoleGuidePatches.DisplayAdvancedWiki(MatchInfoGuide.Instance, role);
        }
    }
}