using AmongUs.GameOptions;
using Reactor.Utilities.Attributes;
using UnityEngine;
using TMPro;
using TownOfUs.Utilities;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
public class WikiHyperlink(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    private TextMeshPro tmp;
    private Camera worldCamera;

    public void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
        worldCamera = Camera.main;
    }

    public void Update()
    {
        if (!tmp) return;
        if (!worldCamera) return;
        
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Vector3 mousePos = Input.mousePosition;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmp, mousePos, worldCamera);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = tmp.textInfo.linkInfo[linkIndex];
                
                var role = MiscUtils.AllRoles.FirstOrDefault(x => x.GetType().FullName == linkInfo.GetLinkID()) ?? RoleManager.Instance.GetRole(RoleTypes.Crewmate); // i hate il2cpp
                var modifier = MiscUtils.AllModifiers.FirstOrDefault(x => x.GetType().FullName == linkInfo.GetLinkID());

                dynamic wikiEntry;
                if (role is IWikiDiscoverable wikiRole)
                {
                    wikiEntry = wikiRole;
                }
                else if (modifier is IWikiDiscoverable wikiModifier)
                {
                    wikiEntry = wikiModifier;
                }
                else if (SoftWikiEntries.RoleEntries.TryGetValue(role, out var softRoleWiki))
                {
                    wikiEntry = softRoleWiki;
                }
                else
                {
                    return;
                }


                if (HudManager.Instance.Chat.IsOpenOrOpening)
                {
                    HudManager.Instance.Chat.Close();
                }

                try
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }
                catch
                {
                    // ignored
                }
                
                var wiki = IngameWikiMinigame.Create();
                wiki.Begin(null);    
                wiki.OpenFor(wikiEntry);
            }
        }
    }
}
