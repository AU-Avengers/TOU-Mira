using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace TownOfUs.Modules.Localization;

[RegisterInIl2Cpp]
public class TmpMiraTranslator(IntPtr cppPtr) : MonoBehaviour(cppPtr), IMiraTranslation
{
    public string stringName;

    public string defaultStr;

    public bool parseStr;

    public bool resetWithoutDefault;

    public void ResetText()
    {
        if (resetWithoutDefault && defaultStr.IsNullOrWhiteSpace())
        {
            return;
        }

        TextMeshPro component = GetComponent<TextMeshPro>();
        string text = parseStr ? TouLocale.GetParsed(stringName, defaultStr) : TouLocale.Get(stringName, defaultStr);
        if (component != null)
        {
            component.text = text;
            component.ForceMeshUpdate();
        }
        else
        {
            TextMeshProUGUI component2 = GetComponent<TextMeshProUGUI>();
            component2.text = text;
            component2.ForceMeshUpdate();
        }
    }

    public void Start()
    {
        TouLocalizationProvider.ActiveTexts.Add(this);
        ResetText();
    }

    public void OnDestroy()
    {
        if (TranslationController.InstanceExists)
        {
            try
            {
                TouLocalizationProvider.ActiveTexts.Remove(this);
            }
            catch
            {
                // Ignored
            }
        }
    }
}